using FluentValidation;
using Gic.Match3.Domain.Helpers;
using Gic.Match3.Domain.Models;
using Gic.Match3.Domain.Services;
using Gic.Match3.Domain.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Gic.Match3.Tests.Unit.Helpers
{
    /// <summary>
    /// Verifies the custom reflection-based Dependency Injection logic.
    /// Ensures all services and validators in the Domain assembly are correctly registered.
    /// </summary>
    public class ServiceExtensionsTests
    {
        private readonly IServiceCollection _services;
        private readonly GameRulesOptions _options;

        public ServiceExtensionsTests()
        {
            _services = new ServiceCollection();
            _options = new GameRulesOptions { BrickSize = 3, MaxBricks = 1, AllowedSymbols = ['@', '#'] };
        }

        [Fact]
        public void AddServices_ShouldRegisterGameRulesOptionsAsSingleton()
        {
            // Act
            _services.AddServices(_options);
            var provider = _services.BuildServiceProvider();

            // Assert
            var registeredOptions = provider.GetService<GameRulesOptions>();
            registeredOptions.Should().NotBeNull();
            registeredOptions.Should().BeSameAs(_options); // Singleton check
        }

        [Fact]
        public void AddServices_ShouldRegisterMatchServiceAsTransient()
        {
            // Act
            _services.AddServices(_options);
            var serviceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IMatchService));

            // Assert
            serviceDescriptor.Should().NotBeNull();
            serviceDescriptor!.ImplementationType.Should().Be(typeof(MatchService));
            serviceDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact]
        public void AddServices_ShouldRegisterValidatorsAsTransient()
        {
            // Act
            _services.AddServices(_options);

            // Check for a specific validator that we know exists in the assembly
            var validatorDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IValidator<Brick>));

            // Assert
            validatorDescriptor.Should().NotBeNull();
            validatorDescriptor!.ImplementationType.Should().Be(typeof(BrickValidator));
            validatorDescriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
        }

        [Fact]
        public void AddServices_ShouldRegisterAllExpectedValidators()
        {
            // Act
            _services.AddServices(_options);
            var provider = _services.BuildServiceProvider();

            // Assert - Verify a few key validators can be resolved
            provider.GetService<IValidator<Brick>>().Should().BeOfType<BrickValidator>();
            provider.GetService<IValidator<FrameCommandDto>>().Should().BeOfType<FrameCommandValidator>();
            provider.GetService<IValidator<GameInitDto>>().Should().BeOfType<GameInitializationValidator>();
        }
    }
}
