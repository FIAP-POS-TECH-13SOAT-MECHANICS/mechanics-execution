using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Services;
using Mechanics.Infra.Messaging.Consumers;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.Auth.Consumers;

public class CustomerCreatedConsumer(ILogger<CustomerCreatedConsumer> logger, UserAppService service) : IEventConsumer<CustomerCreatedEvent>
{
    public async Task ConsumeAsync(CustomerCreatedEvent message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating user for customer '{CustomerId}'", message.CustomerId);

        var request = new CreateUserForCustomerRequest(message);
        var response = await service.Create(request, cancellationToken);

        logger.LogInformation("User for customer '{CustomerId}' created with ID '{UserId}'",
            message.CustomerId, response.CreatedId);
    }
}

public class CustomerCreatedEvent
{
    public required string FullName { get; init; }
    public required string CpfNumber { get; init; }
    public required string Email { get; init; }
    public required Guid CustomerId { get; init; }
    public required Guid RoleId { get; init; }
    public required string RoleName { get; init; }
}
