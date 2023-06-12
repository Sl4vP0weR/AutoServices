namespace Microsoft.Extensions.DependencyInjection.AutoServices;

public sealed class AutoServices : ICloneable
{
    public IServiceCollection Services { get; init; }
    public ServiceLifetime DefaultLifetime { get; set; }

    public readonly HashSet<Type> Markers = new()
    {
        typeof(IAutoService),
        typeof(ServiceAttribute)
    };

    private AutoServices(IServiceCollection services, IEnumerable<Type> markers = null)
    {
        Services = services;

        Markers = new(markers ?? Markers);
    }

    public static AutoServices Create(IServiceCollection services) => new(services);
    public static AutoServices Create(IServiceCollection services, AutoServices parent) =>
        new(services, parent.Markers);

    public AutoServices WithMarker<T>() => WithMarker(typeof(T));

    public AutoServices WithMarker(Type type)
    {
        Markers.Add(type);

        return this;
    }

    public AutoServices WithMarkers(params Type[] types)
    {
        foreach (var type in types)
            WithMarker(type);

        return this;
    }

    public AutoServices WithDefaultLifetime(ServiceLifetime defaultLifetime)
    {
        DefaultLifetime = defaultLifetime;

        return this;
    }

    /// <summary>
    /// Retrieves <see cref="ServiceLifetime"/> from <paramref name="service"/>'s <see cref="ServiceAttribute.Lifetime"/>, 
    /// otherwise returns <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    /// <param name="service">The type of service to retrive lifetime from.</param>
    public ServiceLifetime GetLifetime(Type service)
    {
        var attribute = service.GetCustomAttribute<ServiceAttribute>();

        return attribute?.Lifetime ?? DefaultLifetime;
    }

    /// <summary>
    /// Adds service of type <paramref name="service"/> with <paramref name="implementation"/> (if null <paramref name="service"/> will be used) to the current <see cref="IServiceCollection"/>.
    /// Will be used <see cref="ServiceLifetime"/> retrieved from <see cref="GetLifetime"/>.
    /// </summary>
    /// <param name="service">The type of service to add.</param>
    /// <param name="implementation">The type of implementation to add. If null, <paramref name="service"/> will be used.</param>
    public AutoServices AddService(Type service, Type? implementation = null)
    {
        service ??= service.GetGenericDefinition();
        implementation = implementation is null ? service : implementation.GetGenericDefinition();

        var descriptor = new ServiceDescriptor(service, implementation, GetLifetime(implementation));
        Services.Add(descriptor);

        return this;
    }

    /// <summary>
    /// Finds services in <paramref name="assembly"/> associated with markers and adds them to the current <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="assembly">The assembly to search for services. If null, the calling assembly will be used.</param>
    public AutoServices AddFromAssembly(Assembly assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        IEnumerable<Type> types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex) { types = ex.Types; }

        var attributeType = typeof(ServiceAttribute);

        // types that associated with marker
        types = types.Where(IsMarked).ToList();

        AddServices(types, types);

        return this;
    }

    /// <summary>
    /// Returns true if <paramref name="type"/> is marked by any marker.
    /// </summary>
    /// <param name="type" />
    public bool IsMarked(Type type)
    {
        if (type.CustomAttributes.Any(x => Markers.Contains(x.AttributeType)))
            return true;

        return Markers.Any(x => x.IsAssignableFromNonInstance(type));
    }

    /// <summary>
    /// Adds services of types <paramref name="services"/> with corresponding <paramref name="implementations"/> (if null, <paramref name="services"/> will be used) to the current <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The types of services to add.</param>
    /// <param name="implementations">The types of implementations to add.</param>
    public AutoServices AddServices(IEnumerable<Type> services, IEnumerable<Type> implementations)
    {
        implementations = implementations.Where(impl => !impl.IsInterface && !impl.IsAbstract).ToList();
        Parallel.ForEach(services, service =>
        {
            var impls = implementations.Where(service.IsAssignableFromNonInstance).ToList(); // find all implementations of interface

            Parallel.ForEach(impls, impl =>
            {
                AddService(service, impl);

                Debug.WriteLine($"Registered service {service.Name} with implementation {impl.Name}.");
            });
        });

        return this;
    }

    object ICloneable.Clone() => Clone();
    public AutoServices Clone()
    {
        var clone = new AutoServices(Services, Markers);

        return clone;
    }
}