namespace Microsoft.Extensions.DependencyInjection.AutoServices;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class LifetimeAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; init; }
    public LifetimeAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }
}