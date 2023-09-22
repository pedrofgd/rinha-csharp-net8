using System.Net;
using System.Net.Http.Json;
using IntegrationTests.Helpers;
using WebEndpoints;

namespace IntegrationTests;

[Collection("IntegrationTests")]
public class ContagemPessoasTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string Suffix = "/contagem-pessoas";

    private HttpClient _httpClient;

    public ContagemPessoasTests(TestWebApplicationFactory<Program> fixture)
    {
        _httpClient = fixture.CreateClient();

        TestDatabaseContext.ClearDatabase().Wait();
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
                    Stack = new[] { "C#", "Npgsql" }
                });
        }

        var sut = await _httpClient.GetAsync(Suffix);
        
        Assert.Equal(HttpStatusCode.OK, sut.StatusCode);
        var contagem = int.Parse(await sut.Content.ReadAsStringAsync());
        Assert.Equal(total, contagem);
    }
}