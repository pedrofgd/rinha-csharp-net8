using System.Net;
using System.Net.Http.Json;
using IntegrationTests.Helpers;
using Newtonsoft.Json;
using WebEndpoints;

namespace IntegrationTests;

[Collection("IntegrationTests")]
public class ObterPessoasPorTermoTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string Suffix = "/pessoas";
    private static readonly string[] StackItems = { "C#", "Npgsql" };
    
    private readonly HttpClient _httpClient;

    public ObterPessoasPorTermoTests(TestWebApplicationFactory<Program> fixture)
    {
        _httpClient = fixture.CreateClient();

        TestDatabaseContext.ClearDatabase().Wait();
    }

    [Theory]
    [InlineData("P")]
    [InlineData("edro")]
    public async Task ObterPessoasPorTermoNomeSucesso(string termo)
    {
        const string nome = "Pedro";
        await _httpClient.PostAsJsonAsync(Suffix, new PessoaDto { Nome = nome, Apelido = "ap1", Nascimento = "2001-12-15", Stack = null });
        await _httpClient.PostAsJsonAsync(Suffix, new PessoaDto { Nome = nome, Apelido = "ap2", Nascimento = "2001-12-15", Stack = null });
        await _httpClient.PostAsJsonAsync(Suffix, new PessoaDto { Nome = "AAA", Apelido = "ap3", Nascimento = "2001-12-15", Stack = null });

        var sut = await _httpClient.GetAsync($"{Suffix}?t={termo}");
        
        Assert.Equal(HttpStatusCode.OK, sut.StatusCode);
        var pessoas = JsonConvert.DeserializeObject<ICollection<PessoaDto>>(await sut.Content.ReadAsStringAsync());
        Assert.NotNull(pessoas);
        Assert.Equal(2, pessoas.Count);
    }
    
    [Theory]
    [InlineData("pedrofgd")]
    [InlineData("fgd")]
    public async Task ObterPessoasPorTermoApelidoSucesso(string termo)
    {
        await _httpClient.PostAsJsonAsync(Suffix, new PessoaDto { Nome = "Pedro", Apelido = "pedrofgd1", Nascimento = "2001-12-15", Stack = null });
        await _httpClient.PostAsJsonAsync(Suffix, new PessoaDto { Nome = "Pedro", Apelido = "pedrofgd2", Nascimento = "2001-12-15", Stack = null });
        await _httpClient.PostAsJsonAsync(Suffix, new PessoaDto { Nome = "Pedro", Apelido = "aaa", Nascimento = "2001-12-15", Stack = null });

        var sut = await _httpClient.GetAsync($"{Suffix}?t={termo}");
        
        Assert.Equal(HttpStatusCode.OK, sut.StatusCode);
        var pessoas = JsonConvert.DeserializeObject<ICollection<PessoaDto>>(await sut.Content.ReadAsStringAsync());
        Assert.NotNull(pessoas);
        Assert.Equal(2, pessoas.Count);
    }
    
    [Theory]
    [InlineData("C")]
    [InlineData("Npg")]
    public async Task ObterPessoasPorTermoStackSucesso(string termo)
    {
        await _httpClient.PostAsJsonAsync(Suffix, new PessoaDto { Nome = "Pedro", Apelido = "ap1", Nascimento = "2001-12-15", Stack = StackItems });
        await _httpClient.PostAsJsonAsync(Suffix, new PessoaDto { Nome = "Pedro", Apelido = "ap2", Nascimento = "2001-12-15", Stack = StackItems });
        await _httpClient.PostAsJsonAsync(Suffix, new PessoaDto { Nome = "Pedro", Apelido = "ap3", Nascimento = "2001-12-15", Stack = null});

        var sut = await _httpClient.GetAsync($"{Suffix}?t={termo}");
        
        Assert.Equal(HttpStatusCode.OK, sut.StatusCode);
        var pessoas = JsonConvert.DeserializeObject<ICollection<PessoaDto>>(await sut.Content.ReadAsStringAsync());
        Assert.NotNull(pessoas);
        Assert.Equal(2, pessoas.Count);
    }

    [Fact]
    public async Task ObterPessoasPorTermoRetornaVazioSeNaoEncontrado()
    {
        var sut = await _httpClient.GetAsync($"{Suffix}?t=AAA");
        
        Assert.Equal(HttpStatusCode.OK, sut.StatusCode);
        var pessoas = JsonConvert.DeserializeObject<ICollection<PessoaDto>>(await sut.Content.ReadAsStringAsync());
        Assert.NotNull(pessoas);
        Assert.Empty(pessoas);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?t=")]
    public async Task ObterPessoaPorTermoFalhaSeBuscaNaoInformada(string busca)
    {
        var sut = await _httpClient.GetAsync($"{Suffix}/{busca}");
        
        Assert.Equal(HttpStatusCode.BadRequest, sut.StatusCode);
    }
}