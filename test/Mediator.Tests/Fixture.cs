using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

#pragma warning disable CS0162 // Unreachable code detected

public static class Fixture
{
    public static bool CreateServiceScope;

    public static (IServiceProvider sp, IMediator mediator) GetMediator(
        Action<IServiceCollection>? configureServices = null,
        bool? createScope = null
    )
    {
        var services = new ServiceCollection();

        services.AddMediator();

        configureServices?.Invoke(services);

        IServiceProvider sp = services.BuildServiceProvider(
            new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true }
        );

        var shouldCreateScope = createScope.HasValue ? createScope.Value : CreateServiceScope;
        if (shouldCreateScope)
            sp = sp.CreateScope().ServiceProvider;

        var mediator = sp.GetRequiredService<IMediator>();
        var publisher = sp.GetRequiredService<INotificationPublisher>();

        if (Mediator.NotificationPublisherName == nameof(TaskWhenAllPublisher))
            Assert.IsType<TaskWhenAllPublisher>(publisher);
        else if (Mediator.NotificationPublisherName == nameof(ForeachAwaitPublisher))
            Assert.IsType<ForeachAwaitPublisher>(publisher);
        else
            Assert.IsType<ForeachAwaitPublisher>(publisher);

        return (sp, mediator!);
    }

    public static (IServiceProvider sp, IMediator mediator) GetMediatorCustomContainer(
        Action<IServiceCollection>? configureServices = null,
        bool? createScope = null
    )
    {
        var services = new ServiceCollection();
        services.AddSingleton<IServiceScopeFactory, CustomServiceProviderScope>(sp => new CustomServiceProviderScope(
            (CustomServiceProvider)sp,
            true
        ));

        services.AddMediator();

        configureServices?.Invoke(services);

        IServiceProvider sp = new CustomServiceProvider(services);

        var shouldCreateScope = createScope.HasValue ? createScope.Value : CreateServiceScope;
        if (shouldCreateScope)
            sp = ((CustomServiceProvider)sp).CreateScope().ServiceProvider;

        var mediator = sp.GetRequiredService<IMediator>();
        return (sp, mediator!);
    }

    private sealed class CustomServiceProvider : IServiceProvider
    {
        private readonly IServiceCollection _services;

        public CustomServiceProvider(IServiceCollection services)
        {
            _services = services;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
                return this;
            if (serviceType == typeof(IServiceProviderIsService))
                return null;

            if (serviceType.IsConstructedGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                serviceType = serviceType.GetGenericArguments().Single();
                var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(serviceType));
                _services
                    .Where(s => s.ServiceType.IsAssignableTo(serviceType))
                    .Select(s => GetService(s.ServiceType))
                    .ToList()
                    .ForEach(i => list!.GetType().GetMethod("Add")!.Invoke(list, new[] { i }));
                return list;
            }

            var descriptor =
                _services.FirstOrDefault(s => s.ServiceType == serviceType)
                ?? _services.FirstOrDefault(s => s.ServiceType.IsAssignableTo(serviceType));

            if (descriptor is null)
            {
                // Special case to handle open generic registrations..
                if (serviceType.IsGenericType)
                {
                    descriptor = _services.FirstOrDefault(s =>
                        s.ServiceType.IsGenericType
                        && !s.ServiceType.IsConstructedGenericType
                        && s.ServiceType == serviceType.GetGenericTypeDefinition()
                        && s.ImplementationType is not null
                    );
                    if (descriptor is not null)
                    {
                        var genericType = descriptor.ImplementationType!.MakeGenericType(
                            serviceType.GenericTypeArguments
                        );
                        return ActivatorUtilities.CreateInstance(this, genericType);
                    }
                }
                throw new Exception($"Service not found: {serviceType}");
            }

            var result =
                descriptor.ImplementationInstance
                ?? descriptor.ImplementationFactory?.Invoke(this)
                ?? ActivatorUtilities.CreateInstance(this, descriptor.ImplementationType!);

            if (result is null)
                throw new Exception("Service not found");

            return result;
        }

        public CustomServiceProviderScope CreateScope() => new CustomServiceProviderScope(this, true);

        internal object? GetService(Type serviceType, CustomServiceProviderScope serviceProviderEngineScope)
        {
            return this.GetService(serviceType);
        }
    }

    private sealed class CustomServiceProviderScope
        : IServiceScope,
            IServiceProvider,
            IAsyncDisposable,
            IServiceScopeFactory
    {
        public CustomServiceProviderScope(CustomServiceProvider provider, bool isRootScope)
        {
            ResolvedServices = new Dictionary<(Type Type, int Slot), object?>();
            RootProvider = provider;
            IsRootScope = isRootScope;
        }

        internal Dictionary<(Type Type, int Slot), object?> ResolvedServices { get; }

        public bool IsRootScope { get; }

        internal CustomServiceProvider RootProvider { get; }

        public object? GetService(Type serviceType)
        {
            return RootProvider.GetService(serviceType, this);
        }

        public IServiceProvider ServiceProvider => this;

        public IServiceScope CreateScope() => RootProvider.CreateScope();

        public void Dispose() { }

        public ValueTask DisposeAsync()
        {
            return default;
        }
    }
}

#pragma warning restore CS0162 // Unreachable code detected
