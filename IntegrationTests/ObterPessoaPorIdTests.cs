using System.Net;
using System.Net.Http.Json;
using IntegrationTests.Helpers;
using Newtonsoft.Json;
using WebEndpoints;

namespace IntegrationTests;

[Collection("IntegrationTests")]
public class ObterPessoaPorIdTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string Suffix = "/pessoas";
    
    private readonly HttpClient _httpClient;

    public ObterPessoaPorIdTests(TestWebApplicationFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();
        
        TestDatabaseContext.ClearDatabase(factory).Wait();
    }

    [Fact]
    public async Task ObterPessoaPorIdSucesso()
    {
        var pessoa = new PessoaDto
        {
            Nome = "Pedro",
            Apelido = "pedrofgd",
            Nascimento = "2001-12-25",
            Stack = new[] { "C#" }
        };
        var pessoaCriada = await _httpClient.PostAsJsonAsync(Suffix, pessoa);
        var pessoaId = pessoaCriada.Headers.Location!.ToString().Split("/").Last();
        var sut = await _httpClient.GetAsync($"{Suffix}/{pessoaId}");
        
        Assert.Equal(HttpStatusCode.OK, sut.StatusCode);
        var pessoaEncontrada = JsonConvert.DeserializeObject<PessoaDto>(await sut.Content.ReadAsStringAsync());
        Assert.NotNull(pessoaEncontrada);
        Assert.Equal(Guid.Parse(pessoaId), pessoaEncontrada.Id);
        Assert.Equal(pessoa.Nome, pessoaEncontrada.Nome);
        Assert.Equal(pessoa.Apelido, pessoaEncontrada.Apelido);
        Assert.Equal(pessoa.Nascimento, pessoaEncontrada.Nascimento);
        Assert.NotNull(pessoaEncontrada.Stack);
        Assert.Equal(pessoa.Stack, pessoaEncontrada.Stack);
    }

    [Fact]
    public async Task ObterPessoaPorIdNotFound()
    {
        var sut = await _httpClient.GetAsync($"{Suffix}/{Guid.NewGuid()}");
        
        Assert.Equal(HttpStatusCode.NotFound, sut.StatusCode);
    }
}