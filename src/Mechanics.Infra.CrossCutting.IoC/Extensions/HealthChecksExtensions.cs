using Mechanics.Infra.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Mechanics.Infra.CrossCutting.IoC.Extensions;

public static class HealthChecksExtensions
{
    public static IHealthChecksBuilder AddDbHealthCheck(this IHealthChecksBuilder builder) =>
        builder.AddDbContextCheck<AppDbContext>(nameof(AppDbContext));
}
