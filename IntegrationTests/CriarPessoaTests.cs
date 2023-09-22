using System.Net;
using System.Net.Http.Json;
using IntegrationTests.Helpers;
using Newtonsoft.Json;
using WebEndpoints;

namespace IntegrationTests;

[Collection("IntegrationTests")]
public class CriarPessoaTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string Suffix = "/pessoas";
    
    private readonly HttpClient _httpClient;

    public CriarPessoaTests(TestWebApplicationFactory<Program> factory)
    {
        _httpClient = factory.CreateClient();

        TestDatabaseContext.ClearDatabase().Wait();
    }

    public static IEnumerable<object[]> DadosValidos()
    {
        yield return new object[] { new PessoaDto { Nome = "Pedro", Apelido = "pedrofgd", Nascimento = "2001-12-25", Stack = new[] { "C#", "Npgsql" } } };
        yield return new object[] { new PessoaDto { Nome = "Pedro", Apelido = "pedrofgd2", Nascimento = "2001-12-25", Stack = null } };
    }
    
    public static IEnumerable<object?[]> StringNullOuEmBranco()
    {
        yield return new object[] { "" };
        yield return new object[] { " " };
        yield return new object?[] { null };
    }

    [Theory]
    [MemberData(nameof(DadosValidos))]
    public async Task CriarPessoaSucesso(PessoaDto dto)
    {
        var sut = await _httpClient.PostAsJsonAsync(Suffix, dto);
        
        Assert.Equal(HttpStatusCode.Created, sut.StatusCode);
        Assert.NotNull(sut.Headers.Location);
        var id = sut.Headers.Location.ToString().Split("/").Last();
        var obterPessoaPorId = await _httpClient.GetAsync($"{Suffix}/{id}");
        var pessoaSalva = JsonConvert.DeserializeObject<PessoaDto>(await obterPessoaPorId.Content.ReadAsStringAsync());
        Assert.NotNull(pessoaSalva);
        Assert.Equal(dto.Nome, pessoaSalva.Nome);
        Assert.Equal(dto.Apelido, pessoaSalva.Apelido);
        Assert.Equal(dto.Nascimento, pessoaSalva.Nascimento);
        Assert.Equal(dto.Stack!, pessoaSalva.Stack!);
    }

    [Theory]
    [MemberData(nameof(StringNullOuEmBranco))]
    public async Task CriarPessoaFalhaComNomeInvalido(string nomeInvalido)
    {
        var dto = new PessoaDto { Nome = nomeInvalido, Apelido = "pedrofgd", Nascimento = "2001-06-25", Stack = null };
        
        var sut = await _httpClient.PostAsJsonAsync(Suffix, dto);
        
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sut.StatusCode);
    }
    
    [Fact]
    public async Task CriarPessoaFalhaComNomeMuitoLongo()
    {
        var nomeLongo = "Nome".PadRight(101, 'a');
        var dto = new PessoaDto { Nome = nomeLongo, Apelido = "pedrofgd", Nascimento = "2001-06-25", Stack = null };
        
        var sut = await _httpClient.PostAsJsonAsync(Suffix, dto);
        
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sut.StatusCode);
    }

    [Theory]
    [MemberData(nameof(StringNullOuEmBranco))]
    public async Task CriarPessoaFalhaComApelidoInvalido(string apelidoInvalido)
    {
        var dto = new PessoaDto { Nome = "Pedro", Apelido = apelidoInvalido, Nascimento = "2001-06-25", Stack = null };
        
        var sut = await _httpClient.PostAsJsonAsync(Suffix, dto);
        
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sut.StatusCode);
    }

    [Fact]
    public async Task CriarPessoaFalhaComApelidoMuitoLongo()
    {
        var apelidoLongo = "apelido".PadRight(33, '0');
        var dto = new PessoaDto { Nome = "Pedro", Apelido = apelidoLongo, Nascimento = "2001-06-25", Stack = null };
        
        var sut = await _httpClient.PostAsJsonAsync(Suffix, dto);
        
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sut.StatusCode);
    }

    [Fact]
    public async Task CriarPessoaFalhaComApelidoDuplicado()
    {
        const string apelido = "pedrofgd";
        var dto = new PessoaDto { Nome = "Pedro", Apelido = apelido, Nascimento = "2001-06-25", Stack = null };
        await _httpClient.PostAsJsonAsync(Suffix, dto);
        
        var sut = await _httpClient.PostAsJsonAsync(Suffix, dto);
        
        Assert.Equal(HttpStatusCode.BadRequest, sut.StatusCode);
    }

    [Theory]
    [InlineData("25/12/2001")]
    [InlineData("2001-13-25")]
    [InlineData("2001-12-32")]
    [MemberData(nameof(StringNullOuEmBranco))]
    public async Task CriarPessoaFalhaComNascimentoInvalido(string nascimentoInvalido)
    {
        var dto = new PessoaDto { Nome = "Pedro", Apelido = "pedrofgd", Nascimento = nascimentoInvalido, Stack = null };
        
        var sut = await _httpClient.PostAsJsonAsync(Suffix, dto);
        
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sut.StatusCode);
    }

    [Fact]
    public async Task CriarPessoaFalhaComItemStackMuitoLongo()
    {
        var itemStack = ".NET".PadRight(33, '0');
        var dto = new PessoaDto { Nome = "Pedro", Apelido = "pedrofgd", Nascimento = "2001-12-25", Stack = new[] { itemStack } };
        
        var sut = await _httpClient.PostAsJsonAsync(Suffix, dto);
        
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sut.StatusCode);
    }
}