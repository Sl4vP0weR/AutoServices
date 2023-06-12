namespace AutoServices.Tests;

[Service(Lifetime = ServiceLifetime.Transient)]
internal class AutoServiceTransient : ITestAutoService
{
}
