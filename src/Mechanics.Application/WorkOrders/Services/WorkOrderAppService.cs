using AutoMapper;
using Mechanics.Application.Budgets.Events;
using Mechanics.Application.Identity.Services;
using Mechanics.Application.Observability;
using Mechanics.Application.Utils;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Utils.PagedList;
using Mechanics.Application.Vehicles.Services;
using Mechanics.Application.WorkOrders.Events;
using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Domain.Base.Exceptions;
using Mechanics.Domain.Products;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data;
using Mechanics.Infra.Messaging.Publishers;
using Mechanics.Infra.Security.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Mechanics.Application.WorkOrders.Services;

public class WorkOrderAppService(
    AppDbContext db,
    IMapper mapper,
    ILogger<WorkOrderAppService> logger,
    IIdentityApiService identityApiService,
    IWorkOrdersApiService workOrdersApiService,
    IEventPublisher eventPublisher)
    : IAppService, IWorkOrderAppService
{
    /// <summary>
    ///     Cria uma nova WorkOrder.
    /// </summary>
    public async Task<CreateItemResponse> Create(WorkOrderCreatedEvent request, CancellationToken cancellationToken = default)
    {
        try
        {
            var vehicle = await workOrdersApiService.GetVehicleByIdAsync(request.VehicleId, cancellationToken);
            EntityNotFoundException.ThrowIfNull(vehicle, request.VehicleId);

            var existingOrders = await db.WorkOrders.Where(w => w.CustomerId == vehicle.OwnerId)
                .ToListAsync(cancellationToken);
            var accessKey = WorkOrder.GenerateNewAccessKey(existingOrders);

            var now = DateTime.Now;
            var wo = new WorkOrder
            {
                Id = request.WorkOrderId,
                CustomerId = vehicle.OwnerId,
                VehicleId = request.VehicleId,
                Status = WorkOrderStatus.Received,
                CreationDate = now,
                LastUpdate = now,
                ReportedProblem = request.ReportedProblem,
            };

            await db.WorkOrders.AddAsync(wo, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            AppMetrics.WorkOrdersCreated.Add(1, new TagList
            {
                { "status", "created" },
            });

            logger.LogInformation(
                "Work order created | {work_order.id} | {vehicle.id} | {work_order.status}",
                wo.Id,
                wo.VehicleId,
                wo.Status.ToString());

            return new CreateItemResponse { CreatedId = wo.Id };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while creating work order");
            throw;
        }
    }

    public async Task Assign(Guid workOrderId, Guid assignedToUserId, Guid performedByUserId, string? comment = null,
        CancellationToken cancellationToken = default)
    {
        var wo = await db.WorkOrders.FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);
        EntityNotFoundException.ThrowIfNull(wo, workOrderId);

        var assignedUser = await identityApiService.GetUserById(assignedToUserId, cancellationToken);
        EntityNotFoundException.ThrowIfNull(assignedUser, assignedToUserId);

        if (assignedUser.Role.Name != RoleNames.Mechanic)
            throw new BusinessException("Assigned user must be a mechanic.");

        wo.AssignedToUserId = assignedToUserId;
        wo.LastUpdate = DateTime.Now;

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            Action = "Assigned",
            Details = comment is null ? $"Assigned to {assignedToUserId}" : $"Assigned to {assignedToUserId}. Comment: {comment}",
            PerformedByUserId = performedByUserId,
        };
        await db.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        if (wo.Status == WorkOrderStatus.Received)
        {
            await ChangeStatus(workOrderId, WorkOrderStatus.UnderDiagnosis, performedByUserId,
                comment: "Auto-transition to UnderDiagnosis due assignment", cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    ///     Obtém detalhes de uma WorkOrder por id.
    /// </summary>
    public async Task<GetWorkOrderResponse?> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var wo = await db.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        return wo is null ? null : mapper.Map<GetWorkOrderResponse>(wo);
    }

    /// <summary>
    ///     Lista WorkOrders com filtros opcionais e paginação.
    /// </summary>
    public async Task<GetWorkOrdersResponse> GetList(GetWorkOrdersRequest request, CancellationToken cancellationToken = default)
    {
        var hasCustomer = request.CustomerId.HasValue;
        var hasVehicle = request.VehicleId.HasValue;

        var query = db.WorkOrders
            .AsNoTracking()
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .Where(w => !hasCustomer || w.CustomerId == request.CustomerId!.Value)
            .Where(w => !hasVehicle || w.VehicleId == request.VehicleId!.Value)
            .Where(w => request.IncludeCompleted || w.Status != WorkOrderStatus.Completed && w.Status != WorkOrderStatus.Delivered)
            .OrderByDescending(w => w.Status).ThenBy(w => w.CreationDate)
            .AsSplitQuery();

        var (items, count) = await query.GetPaginatedList(request, cancellationToken);

        var mapped = mapper.Map<IEnumerable<GetWorkOrderResponse>>(items);
        return new GetWorkOrdersResponse(mapped, count);
    }

    /// <summary>
    ///     Solicita aprovação do orçamento para a ordem.
    /// </summary>
    public async Task RequestApproval(Guid workOrderId, Guid performedByUserId, CancellationToken cancellationToken = default)
    {
        var woExists = await db.WorkOrders.AnyAsync(w => w.Id == workOrderId, cancellationToken);
        EntityNotFoundException.ThrowIfNotFound<WorkOrder>(woExists, workOrderId);

        // TODO enviar ordem e todos os produtos para Billing
        // await budgetService.CreateAndSendBudget(workOrderId, performedByUserId, cancellationToken);

        var wo = await db.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

        EntityNotFoundException.ThrowIfNull(wo, workOrderId);

        // Calcula o orçamento (produtos + serviços)
        var budgetItems = new List<Budgets.Events.BudgetItem>();
        var total = decimal.Zero;

        // Adiciona produtos
        if (wo.Products?.Any() == true)
        {
            var productIds = wo.Products.Select(p => p.ProductId).ToList();
            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            foreach (var workOrderProduct in wo.Products)
            {
                var product = products.FirstOrDefault(p => p.Id == workOrderProduct.ProductId);
                if (product != null)
                {
                    var subtotal = product.UnitPrice * workOrderProduct.Quantity;
                    total += subtotal;
                    budgetItems.Add(new Budgets.Events.BudgetItem
                    {
                        Id = product.Id,
                        Name = product.Name,
                        UnitPrice = product.UnitPrice,
                        Quantity = workOrderProduct.Quantity,
                        Subtotal = subtotal,
                    });
                }
            }
        }

        // Adiciona serviços
        if (wo.ServiceCatalog?.Any() == true)
        {
            foreach (var service in wo.ServiceCatalog)
            {
                var subtotal = service.BasePrice;
                total += subtotal;
                budgetItems.Add(new Budgets.Events.BudgetItem
                {
                    Id = service.Id,
                    Name = service.Name,
                    UnitPrice = service.BasePrice,
                    Quantity = 1,
                    Subtotal = subtotal,
                });
            }
        }

        // Publica evento BudgetCreatedEvent
        var budgetEvent = new BudgetCreatedEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.Now,
            WorkOrderId = wo.Id,
            CustomerId = wo.CustomerId,
            VehicleId = wo.VehicleId,
            Total = total,
            ExpiresAt = DateTimeOffset.Now.AddDays(7), // Orçamento válido por 7 dias
            Items = budgetItems.AsReadOnly(),
        };

        try
        {
            await eventPublisher.PublishAsync(budgetEvent, cancellationToken);
            logger.LogInformation("Published BudgetCreatedEvent for WorkOrder {WorkOrderId} with total {Total}", wo.Id, total);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish BudgetCreatedEvent for WorkOrder {WorkOrderId}", wo.Id);
            throw; // Relança pois a criação do orçamento é crítica
        }
    }

    /// <summary>
    ///     Altera o status da WorkOrder seguindo regras do fluxo principal.
    ///     Received -> UnderDiagnosis -> PendingApproval -> InProgress -> Completed -> Delivered
    /// </summary>
    public async Task ChangeStatus(Guid workOrderId, WorkOrderStatus newStatus, Guid performedByUserId,
        string? comment = null, CancellationToken cancellationToken = default)
    {
        var wo = await db.WorkOrders.FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);
        EntityNotFoundException.ThrowIfNull(wo, workOrderId);

        var previous = wo.Status;

        if (previous == newStatus)
            throw new BusinessException($"Work order is already in {newStatus}.");

        if (!IsTransitionAllowed(previous, newStatus))
            throw new BusinessException($"Invalid status transition from {previous} to {newStatus}.");

        if (newStatus == WorkOrderStatus.InProgress)
        {
            // TODO consultar ordem para confirmar que orçamento está aprovado
            /*var approved = wo.ApprovedAt != null;

            if (!approved)
                throw new BusinessException("Order must be approved before starting.");*/
        }

        var timeInPreviousStatus = DateTime.Now - wo.LastUpdate;

        wo.Status = newStatus;
        wo.LastStatusChangeBy = performedByUserId;
        if (newStatus == WorkOrderStatus.InProgress)
            wo.ApprovedAt ??= DateTime.Now;
        if (newStatus == WorkOrderStatus.Delivered)
            wo.DeliveredAt = DateTime.Now;

        wo.LastUpdate = DateTime.Now;

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            Action = "StatusChanged",
            Details = comment is null ? $"From {previous} to {newStatus}" : $"From {previous} to {newStatus}. Comment: {comment}",
            PerformedByUserId = performedByUserId,
        };
        await db.WorkOrderHistories.AddAsync(hist, cancellationToken);

        var tags = new TagList
        {
            { "previous_status", previous.ToString() },
            { "new_status", newStatus.ToString() },
        };

        AppMetrics.TimeInStatusTotalSeconds.Add(
            Math.Round(timeInPreviousStatus.TotalSeconds, 2),
            tags);

        AppMetrics.TimeInStatusSamples.Add(1, tags);
        AppMetrics.StatusTransitions.Add(1, tags);

        AppMetrics.StatusDurationSeconds.Record(
            Math.Round(timeInPreviousStatus.TotalSeconds, 2),
            new TagList
            {
                { "status", previous.ToString() },
            });

        logger.LogInformation(
            "Work order status changed | {work_order.id} | {work_order.previous_status} → {work_order.new_status} | {work_order.time_in_status_seconds}s",
            wo.Id,
            previous.ToString(),
            newStatus.ToString(),
            Math.Round(timeInPreviousStatus.TotalSeconds, 2));

        await db.SaveChangesAsync(cancellationToken);

        // Publica evento de alteração de status para notificar outros serviços
        try
        {
            var statusChangedEvent = new WorkOrderStatusChangedEvent
            {
                WorkOrderId = wo.Id,
                LastStatusChangeBy = performedByUserId,
                OldStatus = previous.ToString(),
                NewStatus = newStatus.ToString(),
                LastUpdate = DateTimeOffset.Now,
            };
            await eventPublisher.PublishAsync(statusChangedEvent, cancellationToken);
            logger.LogInformation("Published WorkOrderStatusChangedEvent for WorkOrder {WorkOrderId}", wo.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish WorkOrderStatusChangedEvent for WorkOrder {WorkOrderId}", wo.Id);
            // Não relança a exceção para não interromper o fluxo
        }

        // notifica cliente sobre a mudança de status
        var customer = await workOrdersApiService.GetCustomerByIdAsync(wo.CustomerId, cancellationToken);
        if (customer != null)
        {
            try
            {
                // TODO publicar evento de alteração de status da OS
            }
            catch (Exception ex)
            {
                AppMetrics.EmailsFailed.Add(1, new TagList { { "template", "status_changed" } });
                logger.LogWarning(ex, "Failed to send status changed email for WorkOrder {WorkOrderId}", wo.Id);
            }
        }
    }

    /// <summary>
    ///     Atualiza produtos, serviços e observações da ordem.
    /// </summary>
    public async Task UpdateDetails(Guid workOrderId, UpdateWorkOrderRequest request, Guid performedByUserId,
        CancellationToken cancellationToken = default)
    {
        var wo = await db.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

        EntityNotFoundException.ThrowIfNull(wo, workOrderId);

        var addedProducts = await ApplyProductsToWorkOrderAsync(wo, request.Products?.ToList(), cancellationToken);
        var addedServices = await ApplyServicesToWorkOrderAsync(wo, request.ServiceIds, cancellationToken);

        var observationChanged = request.Observations is not null && wo.Observations != request.Observations;
        if (observationChanged)
            wo.Observations = request.Observations;

        if (addedProducts == 0 && addedServices == 0 && !observationChanged)
            return;

        wo.LastUpdate = DateTime.Now;

        var detailsParts = new List<string>();
        if (addedProducts > 0) detailsParts.Add($"AddedProducts:{addedProducts}");
        if (addedServices > 0) detailsParts.Add($"AddedServices:{addedServices}");
        if (observationChanged) detailsParts.Add("ObservationsUpdated");

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            Action = "DetailsUpdated",
            Details = string.Join("; ", detailsParts),
            PerformedByUserId = performedByUserId,
        };
        await db.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     Helper: aplica produtos retorna quantidade adicionada.
    /// </summary>
    private async Task<int> ApplyProductsToWorkOrderAsync(WorkOrder wo, IReadOnlyList<WorkOrderProductRequest>? products,
        CancellationToken cancellationToken)
    {
        if (products is null || !products.Any())
            return 0;

        var productIds = products.Select(p => p.ProductId).ToArray();
        var existingIds = await db.Products
            .Where(product => productIds.Contains(product.Id))
            .Select(product => product.Id)
            .ToListAsync(cancellationToken);

        var notFoundId = productIds.Except(existingIds).FirstOrDefault();
        EntityNotFoundException.ThrowIfNotFound<Product>(notFoundId == Guid.Empty, notFoundId);

        var productsToChange = wo.Products?.ToDictionary(p => p.ProductId, p => p) ?? new Dictionary<Guid, WorkOrderProduct>();

        var added = 0;
        foreach (var product in products)
        {
            var existing = productsToChange.TryGetValue(product.ProductId, out var existingProduct);
            if (existing)
            {
                if (existingProduct!.Quantity != product.Quantity)
                    existingProduct.Quantity = product.Quantity;

                continue;
            }

            productsToChange.Add(product.ProductId, new WorkOrderProduct
            {
                ProductId = product.ProductId,
                Quantity = product.Quantity,
            });
            added++;
        }

        wo.Products = productsToChange.Values.ToList();
        return added;
    }

    /// <summary>
    ///     Helper: aplica serviços retorna quantidade adicionada.
    /// </summary>
    private async Task<int> ApplyServicesToWorkOrderAsync(WorkOrder wo, IEnumerable<Guid>? serviceIds,
        CancellationToken cancellationToken)
    {
        if (serviceIds is null) return 0;
        var ids = serviceIds as IList<Guid> ?? serviceIds.ToArray();
        if (!ids.Any()) return 0;

        var services = await db.ServiceCatalog.Where(s => ids.Contains(s.Id)).ToListAsync(cancellationToken);
        if (services.Count == 0) return 0;

        wo.ServiceCatalog ??= new List<ServiceCatalog>();

        var added = 0;
        foreach (var s in services.Where(s => wo.ServiceCatalog.All(x => x.Id != s.Id)))
        {
            wo.ServiceCatalog.Add(s);
            added++;
        }

        return added;
    }

    /// <summary>
    ///     Obtém o tempo médio total estimado para execução dos serviços associados a uma WorkOrder.
    /// </summary>
    /// <param name="id">Identificador da WorkOrder.</param>
    /// <param name="cancellationToken">Token para cancelamento da operação.</param>
    public async Task<GetWorkOrderAverageTimeResponse> GetAverageServiceTime(Guid id,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await db.WorkOrders
            .AsNoTracking()
            .Include(w => w.ServiceCatalog)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        EntityNotFoundException.ThrowIfNull(workOrder, id);

        return new GetWorkOrderAverageTimeResponse
        {
            WorkOrderId = workOrder.Id,
            TotalAverageTime = workOrder.ServiceCatalog?.Sum(s => s.AverageTime) ?? 0,
        };
    }

    private static bool IsTransitionAllowed(WorkOrderStatus from, WorkOrderStatus to) =>
        (from, to) switch
        {
            (WorkOrderStatus.Received, WorkOrderStatus.UnderDiagnosis) => true,
            (WorkOrderStatus.UnderDiagnosis, WorkOrderStatus.PendingApproval) => true,
            (WorkOrderStatus.PendingApproval, WorkOrderStatus.InProgress) => true,
            (WorkOrderStatus.InProgress, WorkOrderStatus.Completed) => true,
            (WorkOrderStatus.Completed, WorkOrderStatus.Delivered) => true,
            _ => false,
        };
}
