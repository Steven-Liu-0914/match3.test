using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using Gic.Match3.Domain.Models;
using Gic.Match3.Domain.Services;

namespace Gic.Match3.Domain.Helpers;

public static class ServiceExtensions
{
    /// <summary>
    /// Entry point for registering all Domain-level dependencies.
    /// </summary>
    public static IServiceCollection AddServices(this IServiceCollection services, GameRulesOptions options)
    {
        services.AddSingleton(options);

        var domainAssembly = typeof(IBaseService).Assembly;

        AddBaseServices(services, domainAssembly);
        AddValidators(services, domainAssembly);

        return services;
    }

    /// <summary>
    /// Scans the assembly for classes implementing IBaseService and maps them to their specific interfaces.
    /// </summary>
    private static void AddBaseServices(IServiceCollection services, Assembly assembly)
    {
        var serviceTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IBaseService).IsAssignableFrom(t));

        foreach (var implementationType in serviceTypes)
        {
            // Find the domain-specific interface
            // while excluding the original base interface IBaseService.
            var serviceInterface = implementationType.GetInterfaces()
                .FirstOrDefault(i => i != typeof(IBaseService) && typeof(IBaseService).IsAssignableFrom(i));

            if (serviceInterface != null)
            {
                // Use Transient so that a brand new copy of the service is created every time it is needed.
                // This is the safest choice because it prevents different parts of the game from accidentally sharing or overwriting each other's background data.
                services.AddTransient(serviceInterface, implementationType);
            }
        }
    }

    /// <summary>
    /// Manually scans for any class that implements IValidator<T> and registers it.
    /// This replaces the need for external dependency-injection helper packages.
    /// </summary>
    private static void AddValidators(IServiceCollection services, Assembly assembly)
    {
        var validatorTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Implementation = t, Interface = i })
            .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == typeof(IValidator<>));

        foreach (var validator in validatorTypes)
        {
            // Register as Transient: e.g., IValidator<Brick> -> BrickValidator
            // Use Transient here so that every time we need to check the rules, we get a fresh, clean rule-checker.
            // This guarantees that old errors or data from a previous check don't accidentally carry over to the next one.
            services.AddTransient(validator.Interface, validator.Implementation);
        }
    }
}