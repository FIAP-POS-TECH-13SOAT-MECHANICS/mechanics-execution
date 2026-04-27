using AutoMapper;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Vehicles.Requests;
using Mechanics.Application.Vehicles.Services;
using Mechanics.Domain.Base.Validation;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Tests.Unit.Tests.Vehicles;

[TestClass]
[TestCategory("Vehicles")]
public class VehicleAppServiceTests

{
    public TestContext TestContext { get; set; }
    private readonly IMapper _mapper = AutoMapperFactory.CreateMap("Vehicles");

    #region cadastrar veículo

    [TestMethod("Cadastro de veículo")]
    public async Task It_ShouldCreateVehicle()
    {
        var ownerId = Guid.NewGuid();
        await using var context = new DbContextTestBuilder()
            .WithData([CustomerMocks.CreateCustomerPf(ownerId)])
            .Build();
        var service = new VehicleAppService(context, _mapper);
        var request = VehicleMocks.BuildCreateRequest(ownerId);

        var response = await service.Create(request, CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.CreatedId);
        var created = await context.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == response.CreatedId, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(created);
        Assert.AreEqual(request.Manufacturer.Trim().ToUpper(), created.Manufacturer);
        Assert.AreEqual(request.Model.Trim().ToUpper(), created.Model);
        Assert.AreEqual(request.Color!.Value, created.Color);
        Assert.AreEqual("2020", created.Year);
        Assert.AreEqual(request.LicensePlate, created.LicensePlate.Number);
        Assert.AreEqual(request.Chassis, created.Chassis);
        Assert.AreEqual(request.OwnerId, created.OwnerId);
    }

    [TestMethod("Cadastro de veículo com placa inválida")]
    public async Task It_ShouldThrow_WhenLicensePlateIsInvalid()
    {
        var ownerId = Guid.NewGuid();
        await using var context = new DbContextTestBuilder()
            .WithData([CustomerMocks.CreateCustomerPf(ownerId)])
            .Build();
        var service = new VehicleAppService(context, _mapper);
        var request = VehicleMocks.BuildInvalidPlateCreateRequest(ownerId);

        await Assert.ThrowsExactlyAsync<DomainValidationException>(async () =>
        {
            await service.Create(request, CancellationToken.None);
        });
    }

    #endregion

    #region atualizar veículo

    [TestMethod("Alteração de veículo")]
    public async Task It_ShouldUpdateVehicle()
    {
        var targetOwnerId = Guid.NewGuid();
        var id = Guid.NewGuid();
        await using var context = new DbContextTestBuilder()
            .WithData([CustomerMocks.CreateCustomerPf(targetOwnerId)])
            .WithData(builder =>
            {
                var ownerId = Guid.NewGuid();
                builder.Customers.Add(CustomerMocks.CreateCustomerPf(ownerId));
                builder.Vehicles.Add(VehicleMocks.CreateVehicle(id, ownerId));
            })
            .Build();
        var service = new VehicleAppService(context, _mapper);
        var request = VehicleMocks.BuildUpdateRequest(targetOwnerId);

        var response = await service.Update(id, request, CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsInstanceOfType<UpdateItemResponse>(response);
        var updated = await context.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updated);
        Assert.AreEqual(request.Manufacturer!.Trim().ToUpper(), updated.Manufacturer);
        Assert.AreEqual(request.Model!.Trim().ToUpper(), updated.Model);
        Assert.AreEqual(request.Color!.Value, updated.Color);
        Assert.AreEqual("2021", updated.Year);
        Assert.AreEqual(request.LicensePlate, updated.LicensePlate.Number);
        Assert.AreEqual(request.Chassis, updated.Chassis);
        Assert.AreEqual(targetOwnerId, updated.OwnerId);
    }

    #endregion

    #region listar veículos

    [TestMethod("Consulta por ID")]
    public async Task It_ShouldGetVehicleById()
    {
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var existing = VehicleMocks.CreateVehicle(id, ownerId, plate: "DEF2G34", chassis: "9BWZZZ377VT004252");
        await using var context = new DbContextTestBuilder()
            .WithData(builder =>
            {
                builder.Add(CustomerMocks.CreateCustomerPf(ownerId));
                builder.Add(existing);
            })
            .Build();
        var service = new VehicleAppService(context, _mapper);

        var response = await service.Get(id, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(response);
        Assert.AreEqual(id, response.Id);
        Assert.AreEqual(existing.Manufacturer, response.Manufacturer);
        Assert.AreEqual(existing.Model, response.Model);
        Assert.AreEqual(existing.Color, response.Color);
        Assert.AreEqual(existing.Year, response.Year);
        Assert.AreEqual(existing.LicensePlate.ToString(), response.LicensePlate);
        Assert.AreEqual(existing.Chassis, response.Chassis);
        Assert.AreEqual(existing.OwnerId, response.OwnerId);
    }

    [TestMethod("Listar por proprietário")]
    public async Task It_ShouldListByOwner()
    {
        var targetOwnerId = Guid.NewGuid();
        await using var context = new DbContextTestBuilder()
            .WithData([CustomerMocks.CreateCustomerPf(targetOwnerId)])
            .WithData([
                VehicleMocks.CreateVehicle(Guid.NewGuid(), targetOwnerId, plate: "AAA1A11", chassis: "9BWZZZ377VT004261"),
                VehicleMocks.CreateVehicle(Guid.NewGuid(), targetOwnerId, plate: "BBB2B22", chassis: "9BWZZZ377VT004262"),
            ])
            .WithData(builder =>
            {
                var ownerId = Guid.NewGuid();
                builder.Customers.Add(CustomerMocks.CreateCustomerPf(ownerId));
                builder.Vehicles.Add(VehicleMocks.CreateVehicle(Guid.NewGuid(), ownerId, plate: "CCC3C33",
                    chassis: "9BWZZZ377VT004263"));
            })
            .Build();
        var service = new VehicleAppService(context, _mapper);
        var request = new GetVehiclesRequest { OwnerId = targetOwnerId, Page = 1, ItemsPerPage = 10 };

        var list = await service.GetList(request, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(list);
        Assert.AreEqual(2, list.Items.Count());
        Assert.AreEqual(2, list.TotalCount);
    }

    [TestMethod("Listar por placa")]
    public async Task It_ShouldListByLicensePlate()
    {
        const string targetPlate = "JKA5K67";
        var ownerId = Guid.NewGuid();
        await using var context = new DbContextTestBuilder()
            .WithData([CustomerMocks.CreateCustomerPf(ownerId)])
            .WithData([
                VehicleMocks.CreateVehicle(Guid.NewGuid(), ownerId, plate: targetPlate, chassis: "9BWZZZ377VT004271"),
                VehicleMocks.CreateVehicle(Guid.NewGuid(), ownerId, plate: "XYZ1Z23", chassis: "9BWZZZ377VT004272"),
            ])
            .Build();
        var service = new VehicleAppService(context, _mapper);
        var request = new GetVehiclesRequest { LicensePlate = targetPlate, Page = 1, ItemsPerPage = 10 };

        var list = await service.GetList(request, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Items.Count());
        Assert.AreEqual(1, list.TotalCount);
        Assert.AreEqual(targetPlate, list.Items.First().LicensePlate);
    }

    [TestMethod("Listar por chassi")]
    public async Task It_ShouldListByChassis()
    {
        var ownerId = Guid.NewGuid();
        const string targetChassis = "9BWZZZ377VT004281";
        await using var context = new DbContextTestBuilder()
            .WithData([CustomerMocks.CreateCustomerPf(ownerId)])
            .WithData([
                VehicleMocks.CreateVehicle(Guid.NewGuid(), ownerId, plate: "JKA5K67", chassis: targetChassis),
                VehicleMocks.CreateVehicle(Guid.NewGuid(), ownerId, plate: "XYZ1Z23", chassis: "9BWZZZ377VT004272"),
            ])
            .Build();
        var service = new VehicleAppService(context, _mapper);
        var request = new GetVehiclesRequest { Chassis = targetChassis, Page = 1, ItemsPerPage = 10 };

        var list = await service.GetList(request, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Items.Count());
        Assert.AreEqual(1, list.TotalCount);
        Assert.AreEqual(targetChassis, list.Items.First().Chassis);
    }

    #endregion
}
