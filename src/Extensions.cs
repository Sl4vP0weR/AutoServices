namespace Microsoft.Extensions.DependencyInjection.AutoServices;

public static class Extensions
{
    /// <summary>
    /// Creates <see cref="AutoServices"/> instance using <paramref name="services"/> for configuration.
    /// </summary>
    /// <param name="services" />
    public static AutoServices ConfigureAutoServices(this IServiceCollection services) => AutoServices.Create(services);

    /// <summary>
    /// Retrieves generic type definition from <paramref name="type"/> and returns it if exists, otherwise returns <paramref name="type"/>.
    /// </summary>
    /// <param name="type"></param>
    public static Type GetGenericDefinition(this Type type) =>
        !type.IsGenericTypeDefinition && type.IsGenericType ?
        type.GetGenericTypeDefinition() :
        type;

    public static IList<T> AsList<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable is IList<T> list)
            return list;

        return enumerable.ToList();
    }    

    public static bool IsAssignableFromNonInstance(this Type @this, Type from)
    {
        from = from.GetGenericDefinition();
        @this = @this.GetGenericDefinition();

        if (@this.IsInterface)
        {
            var interfaces = from.GetInterfaces().Select(GetGenericDefinition);

            if (interfaces.Any(@this.Equals))
                return true;
        }
        else
        {
            var @base = @this;
            while (@base is not null)
            {
                if (@base == from)
                    return true;

                @base = @base.BaseType;
            }
        }

        return @this.IsAssignableFrom(from);
    }
}