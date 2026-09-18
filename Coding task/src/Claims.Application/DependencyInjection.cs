using Claims.Application.Claims;
using Claims.Application.Covers;
using Claims.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IPremiumCalculator, PremiumCalculator>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<ICoverService, CoverService>();
        return services;
    }
}
