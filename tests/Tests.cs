namespace AutoServices.Tests;

using AutoServiceGeneric = AutoServiceGeneric<AutoServiceSingleton>;

public class Tests
{
    private HostApplicationBuilder builder;
    private IHost host;
    private IServiceProvider services;

    [SetUp]
    public void StartHost()
    {
        builder = Host.CreateApplicationBuilder();

        builder.Services
            .ConfigureAutoServices()
            .AddFromAssembly();

        host = builder.Build();
        services = host.Services;
    }

    [Test]
    [TestCase(typeof(ITestAutoService<AutoServiceSingleton>))]
    [TestCase(typeof(AutoServiceSingleton))]
    [TestCase(typeof(AutoServiceTransient))]
    [TestCase(typeof(AutoServiceScoped))]
    [TestCase(typeof(AutoServiceGeneric))]
    [TestCase(typeof(AutoServiceStruct))]
    public void Registered(Type type)
    {
        Assert.IsNotNull(services.GetService(type), "Service `{0}` is not registered.", type);
    }

    [Test]
    [TestCase(typeof(AutoServiceSingleton), ExpectedResult = true)]
    [TestCase(typeof(AutoServiceTransient), ExpectedResult = false)]
    [TestCase(typeof(AutoServiceScoped),    ExpectedResult = true)]
    public bool SingleScopeEquals(Type type)
    {
        var a = services.GetService(type);
        var b = services.GetService(type);

        return a == b;
    }

    [Test]
    [TestCase(typeof(AutoServiceSingleton), ExpectedResult = true)]
    [TestCase(typeof(AutoServiceTransient), ExpectedResult = false)]
    [TestCase(typeof(AutoServiceScoped),    ExpectedResult = false)]
    public bool DifferentScopesEquals(Type type)
    {
        var scopeA = services.CreateScope().ServiceProvider;
        var scopeB = services.CreateScope().ServiceProvider;

        var a = scopeA.GetService(type);
        var b = scopeB.GetService(type);

        return a == b;
    }
}