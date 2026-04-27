using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Notification.Services;
using Mechanics.Application.Utils;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Base.Validation;
using Mechanics.Infra.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Services;

public class AuthAppService(AppDbContext dbContext, IEmailService emailService) : IAppService
{
    public async Task<UpdateItemResponse?> CreatePassword(CreatePasswordRequest request, CancellationToken cancellationToken)
    {
        var normalizedCpf = new string(request.CpfNumber.Where(char.IsDigit).ToArray());
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.CpfNumber == normalizedCpf, cancellationToken);
        if (user is null || user.GetPasswordCreationCode() != request.PasswordCreationCode)
            return null;

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        user.SecurityStamp = Guid.NewGuid().ToString();

        await dbContext.SaveChangesAsync(cancellationToken);

        await emailService.UserPasswordChanged(user, cancellationToken);

        return new UpdateItemResponse { UpdatedItemId = user.Id };
    }

    public async Task ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var normalizedCpf = new string(request.CpfNumber.Where(char.IsDigit).ToArray());
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.CpfNumber == normalizedCpf, cancellationToken);
        if (user is null)
            return;

        await emailService.SendUserPasswordCreationCode(user, user.GetPasswordCreationCode(), cancellationToken);
    }

    public async Task ChangePassword(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FindAsync([userId], cancellationToken: cancellationToken);
        ArgumentNullException.ThrowIfNull(user);

        var verificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        Validator.BuildAndThrow(builder =>
            builder.AddValidation(verificationResult != PasswordVerificationResult.Failed, nameof(request.CurrentPassword),
                "Invalid current password."));

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();

        await dbContext.SaveChangesAsync(cancellationToken);

        await emailService.UserPasswordChanged(user, cancellationToken);
    }
}