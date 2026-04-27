using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Services;
using Mechanics.Application.Notification.Services;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Base.Validation;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Mechanics.Tests.Unit.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
public class AuthAppServiceTests
{
    public TestContext TestContext { get; set; }
    private readonly IEmailService _mailService = Mock.Of<IEmailService>();

    #region recuperação de senha

    [TestMethod("Deve enviar e-mail se o usuário existir")]
    public async Task It_ShouldSendPasswordResetEmail_WithValidUserName()
    {
        var user = UserMocks.CreateUser("12345678909", "TEST_5eCre+Key");
        await using var context = new DbContextTestBuilder()
            .WithData([user])
            .Build();
        var emailServiceStub = new Mock<IEmailService>();
        emailServiceStub.Setup(handler =>
                handler.SendUserPasswordCreationCode(user, user.GetPasswordCreationCode(),
                    TestContext.CancellationTokenSource.Token))
            .Verifiable(Times.Once());
        var appService = new AuthAppService(context, emailServiceStub.Object);
        var request = new ResetPasswordRequest { CpfNumber = "12345678909" };

        await appService.ResetPassword(request, TestContext.CancellationTokenSource.Token);

        emailServiceStub.Verify();
    }

    [TestMethod("Não deve tentar enviar e-mail se o usuário não existir")]
    public async Task It_ShouldNotSendPasswordResetEmail_WithValidUserName()
    {
        await using var context = new DbContextTestBuilder().Build();
        var emailServiceStub = new Mock<IEmailService>();
        emailServiceStub.Setup(handler =>
                handler.SendUserPasswordCreationCode(It.IsAny<User>(), It.IsAny<string>(),
                    TestContext.CancellationTokenSource.Token))
            .Verifiable(Times.Never());
        var appService = new AuthAppService(context, emailServiceStub.Object);
        var request = new ResetPasswordRequest { CpfNumber = "11144477735" };

        await appService.ResetPassword(request, TestContext.CancellationTokenSource.Token);

        emailServiceStub.Verify();
    }

    [TestMethod("Deve alterar a senha se o código for válido")]
    public async Task It_ShouldChangePassword_WithValidCode()
    {
        var user = UserMocks.CreateUser("12345678909", "TEST_5eCre+Key");
        await using var context = new DbContextTestBuilder()
            .WithData([CloneHelper.DeepClone(user)])
            .Build();
        var emailServiceStub = new Mock<IEmailService>();
        emailServiceStub.Setup(handler =>
                handler.UserPasswordChanged(user, TestContext.CancellationTokenSource.Token))
            .Verifiable(Times.Once());
        var appService = new AuthAppService(context, emailServiceStub.Object);
        var request = new CreatePasswordRequest
        {
            CpfNumber = "12345678909",
            Password = "TEST2_5eCre+Key1",
            PasswordCreationCode = user.GetPasswordCreationCode(),
        };

        var result = await appService.CreatePassword(request, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(result);
        var updatedItem = await context.Users.FindAsync([user.Id], TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updatedItem);
        Assert.AreNotEqual(user.PasswordHash, updatedItem.PasswordHash);
        Assert.AreNotEqual(user.SecurityStamp, updatedItem.SecurityStamp);
        emailServiceStub.Verify();
    }

    [TestMethod("Deve manter a senha atual se o código for inválido")]
    public async Task It_ShouldNotChangePassword_WithInvalidCode()
    {
        var user = UserMocks.CreateUser("12345678909", "TEST_5eCre+Key");
        await using var context = new DbContextTestBuilder()
            .WithData([CloneHelper.DeepClone(user)])
            .Build();
        var emailServiceStub = new Mock<IEmailService>();
        emailServiceStub.Setup(handler =>
                handler.UserPasswordChanged(user, TestContext.CancellationTokenSource.Token))
            .Verifiable(Times.Never());
        var appService = new AuthAppService(context, emailServiceStub.Object);
        var request = new CreatePasswordRequest
        {
            CpfNumber = "12345678909",
            Password = "TEST2_5eCre+Key2",
            PasswordCreationCode = "3ef19994-2a12-4618-9ca4-a9110b15ca9c",
        };

        var result = await appService.CreatePassword(request, TestContext.CancellationTokenSource.Token);

        Assert.IsNull(result);
        var updatedItem = await context.Users.FindAsync([user.Id], TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updatedItem);
        Assert.AreEqual(user.PasswordHash, updatedItem.PasswordHash);
        Assert.AreEqual(user.SecurityStamp, updatedItem.SecurityStamp);
        emailServiceStub.Verify();
    }

    #endregion

    #region Alteração de senha

    [TestMethod("Deve alterar a senha se a senha atual estiver correta")]
    public async Task It_ShouldChangePassword_WithValidCurrentPassword()
    {
        var user = UserMocks.CreateUser("ce1b15a2-32e2-4eec-a468-f79da7426dc6", "TEST_5eCre+Key1");
        await using var context = new DbContextTestBuilder()
            .WithData([CloneHelper.DeepClone(user)])
            .Build();
        var emailServiceStub = new Mock<IEmailService>();
        emailServiceStub.Setup(handler =>
                handler.UserPasswordChanged(user, TestContext.CancellationTokenSource.Token))
            .Verifiable(Times.Once());
        var appService = new AuthAppService(context, emailServiceStub.Object);
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "TEST_5eCre+Key1",
            NewPassword = "TEST2_5eCre+Key2",
        };

        await appService.ChangePassword(user.Id, request, TestContext.CancellationTokenSource.Token);

        var updatedItem = await context.Users.FindAsync([user.Id], TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updatedItem);
        Assert.AreNotEqual(user.PasswordHash, updatedItem.PasswordHash);
        Assert.AreEqual(PasswordVerificationResult.Success,
            new PasswordHasher<User>().VerifyHashedPassword(updatedItem, updatedItem.PasswordHash, request.NewPassword));
        Assert.AreNotEqual(user.SecurityStamp, updatedItem.SecurityStamp);
        emailServiceStub.Verify();
    }

    [TestMethod("Deve lançar exceção se a senha atual estiver incorreta")]
    public async Task It_ShouldThrowException_WithInvalidCurrentPassword()
    {
        var user = UserMocks.CreateUser("ce1b15a2-32e2-4eec-a468-f79da7426dc6", "TEST_5eCre+Key1");
        await using var context = new DbContextTestBuilder()
            .WithData([CloneHelper.DeepClone(user)])
            .Build();
        var emailServiceStub = new Mock<IEmailService>();
        emailServiceStub.Setup(handler =>
                handler.UserPasswordChanged(user, TestContext.CancellationTokenSource.Token))
            .Verifiable(Times.Never());
        var appService = new AuthAppService(context, emailServiceStub.Object);
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "TEST_5eCre+Key0",
            NewPassword = "TEST2_5eCre+Key2",
        };

        await Assert.ThrowsExactlyAsync<DomainValidationException>(async () =>
            await appService.ChangePassword(user.Id, request, TestContext.CancellationTokenSource.Token));

        var updatedItem = await context.Users.FindAsync([user.Id], TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updatedItem);
        Assert.AreEqual(user.PasswordHash, updatedItem.PasswordHash);
        Assert.AreEqual(user.SecurityStamp, updatedItem.SecurityStamp);
        emailServiceStub.Verify();
    }

    #endregion
}
