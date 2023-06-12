namespace AutoServices.Tests;

[Service(Lifetime = ServiceLifetime.Scoped)]
internal class AutoServiceScoped : ITestAutoService
{
}
