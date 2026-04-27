using Mechanics.Application.Customers.Requests;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Vehicles.Requests;
using Mechanics.Application.Vehicles.Responses;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Vehicles;
using Mechanics.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mechanics.Tests.Integration.Tests.Vehicles;

[TestClass]
[TestCategory("Integration")]
[TestCategory("Vehicles")]
public class VehiclesControllerTests
{
    public TestContext TestContext { get; set; }

    private readonly JsonSerializerOptions _serializarOptions = new(JsonSerializerDefaults.Web)
        { Converters = { new JsonStringEnumConverter() } };

    [TestMethod("Cadastro de veículo")]
    public async Task It_ShouldCreateVehicle()
    {
        // Arrange
        var (client, ownerId) = await GetClientAndOwner("67273958026");
        var request = new CreateVehicleRequest
        {
            Manufacturer = "Ford",
            Model = "Fiesta",
            Color = VehicleColor.Black,
            Year = "2020",
            LicensePlate = "ABC1D23",
            Chassis = "9BW9ZZ377VT004251",
            OwnerId = ownerId,
        };

        // Act
        var httpResponse = await client.PostAsJsonAsync("api/vehicles", request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
        var content = await httpResponse.Content.ReadFromJsonAsync<CreateItemResponse>(TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(content);
        Assert.AreNotEqual(Guid.Empty, content.CreatedId);
    }

    [TestMethod("Atualização de veículo")]
    public async Task It_ShouldUpdateVehicle()
    {
        var (client, ownerId) = await GetClientAndOwner("30925409057");
        var createRequest = new CreateVehicleRequest
        {
            Manufacturer = "FORD",
            Model = "FIESTA",
            Color = VehicleColor.Black,
            Year = "2020",
            LicensePlate = "ABC1F25",
            Chassis = "9BW9ZZ377VT004252",
            OwnerId = ownerId,
        };
        var createResponse = await client.PostAsJsonAsync("api/vehicles", createRequest, TestContext.CancellationTokenSource.Token);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateItemResponse>(TestContext.CancellationTokenSource.Token);
        var vehicleId = created!.CreatedId;

        var updateRequest = new UpdateVehicleRequest
        {
            Model = "FIESTA TITANIUM",
            Color = VehicleColor.White,
            Year = "2021",
        };
        var updateUri = $"api/vehicles/{vehicleId}";
        var updateResponse = await client.PutAsJsonAsync(updateUri, updateRequest, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.NoContent, updateResponse.StatusCode);
        var getResponse = await client.GetAsync($"api/vehicles/{vehicleId}", TestContext.CancellationTokenSource.Token);
        var vehicle = await getResponse.Content.ReadFromJsonAsync<GetVehicleResponse>(
            _serializarOptions, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(vehicle);
        Assert.AreEqual(vehicleId, vehicle.Id);
        Assert.AreEqual(createRequest.Manufacturer, vehicle.Manufacturer);
        Assert.AreEqual(updateRequest.Model, vehicle.Model);
        Assert.AreEqual(updateRequest.Color, vehicle.Color);
        Assert.AreEqual(updateRequest.Year, vehicle.Year);
        Assert.AreEqual(createRequest.LicensePlate, vehicle.LicensePlate);
        Assert.AreEqual(createRequest.Chassis, vehicle.Chassis);
        Assert.AreEqual(ownerId, vehicle.OwnerId);
    }

    private async Task<(HttpClient client, Guid ownerId)> GetClientAndOwner(string document)
    {
        var factory = TestProperties.Factory;
        var client = factory.GetAuthenticatedClient(RoleNames.Administrator);

        var customerRequest = new CreateIndividualCustomerRequest
        {
            FullName = "Owner Test",
            Email = $"owner_{Guid.NewGuid():N}@example.com",
            CpfNumber = document,
        };

        var createdCustomer =
            await client.PostAsJsonAsync("api/customers/individual", customerRequest, TestContext.CancellationTokenSource.Token);
        var customerResponse =
            await createdCustomer.Content.ReadFromJsonAsync<CreateItemResponse>(TestContext.CancellationTokenSource.Token);
        return (client, customerResponse!.CreatedId);
    }
}
