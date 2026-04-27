using AutoMapper;
using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Services;
using Mechanics.Application.Customers.Requests;
using Mechanics.Application.Notification.Services;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Data.Seeds;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Mechanics.Tests.Unit.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
[TestCategory("Users")]
public class UserAppServiceTests
{
    public TestContext TestContext { get; set; }
    private readonly IMapper _mapper = AutoMapperFactory.CreateMap("Auth");
    private readonly IEmailService _mailService = Mock.Of<IEmailService>();

    #region cadastrar usuário

    [TestMethod("Cria usuário quando RoleId é válido.")]
    public async Task It_ShouldCreateUser_WhenRoleIdIsValid()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        await using var context = new DbContextTestBuilder()
            .WithData(ctx => ctx.Roles.Add(new Role { Id = roleId, Name = RoleNames.Administrator }))
            .Build();
        var handler = new UserAppService(context, _mapper, _mailService);
        var request = UserMocks.BuildCreateRequest(roleId);

        // Act
        var response = await handler.Create(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.CreatedId);
        var created = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == response.CreatedId, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(created);
        Assert.AreEqual(request.FullName, created.FullName);
        Assert.AreEqual(request.CpfNumber, created.CpfNumber);
        Assert.AreEqual(request.RoleId, created.RoleId);
    }

    [TestMethod("Cria usuário para cliente.")]
    public async Task It_ShouldCreateUserForCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customerUserRole = RoleSeeds.GetSeeds().First(r => r.Name == RoleNames.CustomerUser);

        await using var context = new DbContextTestBuilder()
            .WithData(ctx => ctx.Roles.Add(customerUserRole))
            .Build();

        var handler = new UserAppService(context, _mapper, _mailService);
        var individualRequest = new CreateIndividualCustomerRequest
        {
            FullName = "Joao Cliente",
            Email = "joao@cliente.com",
            CpfNumber = "341.041.040-60",
        };
        var request = new CreateUserForCustomerRequest(customerId, individualRequest);

        // Act
        var response = await handler.Create(request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.CreatedId);
        var created = await context.Users.AsNoTracking()
            .FirstAsync(u => u.Id == response.CreatedId, TestContext.CancellationTokenSource.Token);
        Assert.AreEqual(customerId, created.CustomerId);
        Assert.AreEqual(customerUserRole.Id, created.RoleId);
        Assert.AreEqual("JOAO CLIENTE", created.FullName);
    }

    #endregion

    #region buscar usuário

    [TestMethod("Retorna usuário quando o Id existe.")]
    public async Task It_ShouldReturnUser_WhenIdExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = UserMocks.CreateUser(userId, "Maria da Silva", "46387073006", RoleNames.Mechanic);

        await using var context = new DbContextTestBuilder()
            .WithData(ctx => ctx.Users.Add(user))
            .Build();

        var handler = new UserAppService(context, _mapper, _mailService);

        // Act
        var response = await handler.Get(userId, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(userId, response.Id);
        Assert.AreEqual(user.FullName, response.FullName);
        Assert.AreEqual(user.CpfNumber, response.CpfNumber);
        Assert.IsNotNull(response.Role);
        Assert.AreEqual(RoleNames.Mechanic, response.Role.Name);
    }

    [TestMethod("Retorna null quando o Id não existe.")]
    public async Task It_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Arrange
        await using var context = new DbContextTestBuilder().Build();
        var handler = new UserAppService(context, _mapper, _mailService);
        var userId = Guid.NewGuid();

        // Act
        var response = await handler.Get(userId, CancellationToken.None);

        // Assert
        Assert.IsNull(response);
    }

    [TestMethod("Retorna usuário pelo ID e CustomerId.")]
    public async Task It_ShouldReturnUser_WhenIdAndCustomerIdMatch()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = UserMocks.CreateUser(userId, "Joao Cliente", "12345678909", RoleNames.CustomerUser);
        typeof(User).GetProperty(nameof(User.CustomerId))?.SetValue(user, customerId);

        await using var context = new DbContextTestBuilder()
            .WithData(ctx => ctx.Users.Add(user))
            .Build();

        var handler = new UserAppService(context, _mapper, _mailService);

        // Act
        var response = await handler.GetByIdAndCustomerId(customerId, userId, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(userId, response.Id);
    }

    [TestMethod("Retorna null quando o Id existe mas CustomerId não coincide.")]
    public async Task It_ShouldReturnNull_WhenCustomerIdDoesNotMatch()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = UserMocks.CreateUser(userId, "Joao Cliente", "12345678909", RoleNames.CustomerUser);
        typeof(User).GetProperty(nameof(User.CustomerId))?.SetValue(user, Guid.NewGuid()); // Outro customerId

        await using var context = new DbContextTestBuilder()
            .WithData(ctx => ctx.Users.Add(user))
            .Build();

        var handler = new UserAppService(context, _mapper, _mailService);

        // Act
        var response = await handler.GetByIdAndCustomerId(customerId, userId, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNull(response);
    }

    #endregion

    #region listar usuários

    [TestMethod("Retorna lista de usuários.")]
    public async Task It_ShouldReturnUsersList()
    {
        // Arrange
        List<User> users =
        [
            UserMocks.CreateUser(Guid.NewGuid(), "Joao Silva", "54744567002", RoleNames.Mechanic),
            UserMocks.CreateUser(Guid.NewGuid(), "Jose Silva", "92969731045", RoleNames.Administrator),
        ];
        await using var context = new DbContextTestBuilder().WithData(users).Build();
        var handler = new UserAppService(context, _mapper, _mailService);
        var request = new GetUsersRequest { Page = 1, ItemsPerPage = 10 };

        // Act
        var response = await handler.GetList(request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(users.Count, response.Items.Count());
        Assert.AreEqual(users.Count, response.TotalCount);
    }

    [TestMethod("Retorna listas paginadas de usuários.")]
    public async Task It_ShouldReturnUsersPaginatedList()
    {
        // Arrange
        var userId1 = new Guid("ba4a5773-9269-4228-821b-13ea7be6ae8a");
        var userId2 = new Guid("bc272ac4-c97a-4997-9871-7e4f4c023797");
        List<User> users =
        [
            UserMocks.CreateUser(userId1, "Joao Silva", "59600903093", RoleNames.Mechanic),
            UserMocks.CreateUser(userId2, "Jose Silva", "16026321039", RoleNames.Administrator),
        ];
        await using var context = new DbContextTestBuilder().WithData(users).Build();
        var handler = new UserAppService(context, _mapper, _mailService);
        var request1 = new GetUsersRequest { Page = 1, ItemsPerPage = 1 };
        var request2 = new GetUsersRequest { Page = 2, ItemsPerPage = 1 };

        // Act
        var response1 = await handler.GetList(request1, TestContext.CancellationTokenSource.Token);
        var response2 = await handler.GetList(request2, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response1);
        Assert.AreEqual(1, response1.Items.Count());
        Assert.AreEqual(2, response1.TotalCount);
        Assert.Contains(user => user.Id == userId1, response1.Items);
        Assert.IsNotNull(response2);
        Assert.AreEqual(1, response2.Items.Count());
        Assert.AreEqual(2, response2.TotalCount);
        Assert.Contains(user => user.Id == userId2, response2.Items);
    }

    [TestMethod("Retorna lista paginada filtrando usuários pelo nome.")]
    public async Task It_ShouldReturnUsers_WhenFilterByName()
    {
        // Arrange
        List<User> users =
        [
            UserMocks.CreateUser(Guid.NewGuid(), "Joao Silva", "62697237011", RoleNames.Mechanic),
            UserMocks.CreateUser(Guid.NewGuid(), "Jose Silva", "61991950004", RoleNames.Administrator),
        ];
        await using var context = new DbContextTestBuilder().WithData(users).Build();
        var handler = new UserAppService(context, _mapper, _mailService);
        var request = new GetUsersRequest { Page = 1, ItemsPerPage = 10, Name = "joao" };

        // Act
        var response = await handler.GetList(request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(1, response.Items.Count());
        Assert.AreEqual(1, response.TotalCount);
        Assert.Contains(user => user.Id == users[0].Id, response.Items);
    }

    [TestMethod("Retorna lista de usuários por CustomerId.")]
    public async Task It_ShouldReturnUsers_ByCustomerId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var user1 = UserMocks.CreateUser(Guid.NewGuid(), "User 1", "41594921008", RoleNames.CustomerUser);
        typeof(User).GetProperty(nameof(User.CustomerId))?.SetValue(user1, customerId);

        var user2 = UserMocks.CreateUser(Guid.NewGuid(), "User 2", "56559792036", RoleNames.CustomerUser);
        typeof(User).GetProperty(nameof(User.CustomerId))?.SetValue(user2, Guid.NewGuid()); // Outro customer

        await using var context = new DbContextTestBuilder()
            .WithData(new List<User> { user1, user2 })
            .Build();

        var handler = new UserAppService(context, _mapper, _mailService);
        var request = new GetUsersRequest { Page = 1, ItemsPerPage = 10 };

        // Act
        var response = await handler.GetListByCustomerId(customerId, request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(1, response.TotalCount);
        Assert.AreEqual("USER 1", response.Items.First().FullName);
    }

    #endregion

    #region atualizar usuário

    [TestMethod("Atualiza role do usuário quando outros dados são null.")]
    public async Task It_ShouldUpdateUserRole_WhenOtherDataIsNull()
    {
        var administratorRole = RoleSeeds.GetSeeds().First(role => role.Name == RoleNames.Administrator);
        var user = UserMocks.CreateUser(Guid.NewGuid(), "Maria da Silva", "42386508080", RoleNames.Attendant);
        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Users.Add(user);
                ctx.Roles.Add(administratorRole);
            })
            .Build();
        var userId = (await context.Users.FirstAsync(TestContext.CancellationTokenSource.Token)).Id;
        var handler = new UserAppService(context, _mapper, _mailService);
        var request = UserMocks.BuildUpdateRequest(administratorRole.Id);

        var response = await handler.Update(userId, request, CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsInstanceOfType<UpdateItemResponse>(response);
        var updated = await context.Users.AsNoTracking().FirstOrDefaultAsync(TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updated);
        Assert.AreEqual(user.FullName, updated.FullName);
        Assert.AreEqual(user.CpfNumber, updated.CpfNumber);
        Assert.AreEqual(administratorRole.Id, updated.RoleId);
    }

    [TestMethod("Retorna null quando o usuário não existe.")]
    public async Task It_ShouldReturnNull_WhenUserDoesNotExist()
    {
        await using var context = new DbContextTestBuilder().Build();
        var handler = new UserAppService(context, _mapper, _mailService);
        var request = UserMocks.BuildUpdateRequest(new Guid("f2d59afa-6e85-4557-8ff1-733343ba83f8"));

        var response = await handler.Update(Guid.NewGuid(), request, TestContext.CancellationTokenSource.Token);

        Assert.IsNull(response);
    }

    [TestMethod("Ignora alteração de Role ao atualizar usuário por CustomerId.")]
    public async Task It_ShouldIgnoreRoleUpdate_WhenUpdatingByCustomerId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var customerUserRole = RoleSeeds.GetSeeds().First(r => r.Name == RoleNames.CustomerUser);
        var adminRole = RoleSeeds.GetSeeds().First(r => r.Name == RoleNames.Administrator);

        var user = UserMocks.CreateUser(userId, "User Before", "12345678909", RoleNames.CustomerUser);
        typeof(User).GetProperty(nameof(User.CustomerId))?.SetValue(user, customerId);

        await using var context = new DbContextTestBuilder()
            .WithData(new List<User> { user })
            .Build();

        var handler = new UserAppService(context, _mapper, _mailService);
        var request = new UpdateUserRequest
        {
            FullName = "User After",
            RoleId = adminRole.Id // Tentativa de mudar para Admin
        };

        // Act
        var response =
            await handler.UpdateByIdAndCustomerId(customerId, userId, request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        var updated = await context.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
        Assert.AreEqual("USER AFTER", updated.FullName);
        Assert.AreEqual(customerUserRole.Id, updated.RoleId); // Role deve permanecer a mesma
    }

    [TestMethod("Retorna null ao atualizar quando CustomerId não coincide.")]
    public async Task It_ShouldReturnNull_WhenUpdatingWithWrongCustomerId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = UserMocks.CreateUser(userId, "User Before", "12345678909", RoleNames.CustomerUser);
        typeof(User).GetProperty(nameof(User.CustomerId))?.SetValue(user, Guid.NewGuid()); // Outro customer

        await using var context = new DbContextTestBuilder()
            .WithData(new List<User> { user })
            .Build();

        var handler = new UserAppService(context, _mapper, _mailService);
        var request = new UpdateUserRequest { FullName = "User After" };

        // Act
        var response =
            await handler.UpdateByIdAndCustomerId(customerId, userId, request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNull(response);
    }

    #endregion
}
