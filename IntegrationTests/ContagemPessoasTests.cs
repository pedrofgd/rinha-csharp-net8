using System.Net;
using System.Net.Http.Json;
using IntegrationTests.Helpers;
using WebEndpoints;

namespace IntegrationTests;

[Collection("IntegrationTests")]
public class ContagemPessoasTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string Suffix = "/contagem-pessoas";
    private static readonly string[] StackItems = { "C#", "Npgsql" };

    private readonly HttpClient _httpClient;

    public ContagemPessoasTests(TestWebApplicationFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();

        TestDatabaseContext.ClearDatabase(factory).Wait();
    }

    [Fact]
    public async Task ContarPessoasSucesso()
    {
        const int total = 10;
        for (var i = 0; i < total; i++)
        {
            await _httpClient.PostAsJsonAsync("/pessoas",
                new PessoaDto
                {
                    Nome = "Pedro",
                    Apelido = $"ap{i}",
                    Nascimento = "2001-12-15",
                    Stack = StackItems
                });
        }

        var sut = await _httpClient.GetAsync(Suffix);
        
        Assert.Equal(HttpStatusCode.OK, sut.StatusCode);
        var contagem = int.Parse(await sut.Content.ReadAsStringAsync());
        Assert.Equal(total, contagem);
    }
}