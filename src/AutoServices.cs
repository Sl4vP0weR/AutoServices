namespace Microsoft.Extensions.DependencyInjection.AutoServices;

public sealed class AutoServices
{
    public IServiceCollection Services { get; init; }
    public ServiceLifetime DefaultLifetime { get; set; }
    private AutoServices(IServiceCollection services)
    {
        Services = services;
    }

    public static AutoServices Create(IServiceCollection services) => new(services);

    /// <summary>
    /// Retrieves <see cref="ServiceLifetime"/> from <paramref name="service"/>'s <see cref="LifetimeAttribute.Lifetime"/>, 
    /// otherwise returns <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    /// <param name="service"></param>
    public ServiceLifetime GetLifetime(Type service)
    {
        var attribute = service.GetCustomAttribute<LifetimeAttribute>();
        if (attribute is not null)
            return attribute.Lifetime;

        return DefaultLifetime;
    }

    /// <summary>
    /// Adds service of type <paramref name="service"/> with <paramref name="implementation"/>(if null <paramref name="service"/> will be used) to the current <see cref="IServiceCollection"/><br/>
    /// Will be used <see cref="ServiceLifetime"/> retrieved from <see cref="GetLifetime"/>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="implementation"></param>
    public AutoServices AddService(Type service, Type implementation = null)
    {
        service ??= service.GetGenericDefinition();
        implementation = implementation is null ? service : implementation.GetGenericDefinition();

        var descriptor = new ServiceDescriptor(service, implementation, GetLifetime(implementation));
        Services.Add(descriptor);

        return this;
    }

    /// <summary>
    /// Finds services in <paramref name="assembly"/> associated with <paramref name="markerType"/> and adds them to the current <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="markerType"></param>
    public AutoServices AddFromAssembly(Assembly assembly = null, Type markerType = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        markerType ??= typeof(IAutoService);

        IEnumerable<Type> types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex) { types = ex.Types; }

        // types that associated with marker
        types = types 
            .Where(markerType.IsAssignableFromNonInstance)
            .AsList(); 

        AddServices(types, types);

        return this;
    }

    public AutoServices AddServices(IEnumerable<Type> services, IEnumerable<Type> implementations)
    {
        implementations = implementations.ToList();
        foreach (var service in services)
        {
            var impls = implementations.Where(service.IsAssignableFromNonInstance).ToList(); // find all implementations of interface

            foreach (var impl in impls)
            {
                if (impl.IsInterface || impl.IsAbstract)
                    continue;

                AddService(service, impl);

                Debug.WriteLine($"Registerd service {service.Name} with implementation {impl.Name}.");
            }
        }

        return this;
    }
}