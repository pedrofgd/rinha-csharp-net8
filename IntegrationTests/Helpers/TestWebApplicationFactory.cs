using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests.Helpers;

// ReSharper disable once ClassNeverInstantiated.Global
public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
}