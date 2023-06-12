namespace Microsoft.Extensions.DependencyInjection.AutoServices;

/// <summary>
/// Attribute that marks types as the services for AutoServices registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class ServiceAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; init; }
}