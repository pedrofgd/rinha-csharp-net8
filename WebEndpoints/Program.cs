using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebEndpoints;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

ConnectionFactory.InitPostgres(configuration.GetConnectionString("PostgreSQL"));

var app = builder.Build();

var pessoas = app.MapGroup("/pessoas");
pessoas.MapPost("/", CriarPessoa);
pessoas.MapGet("/{id:guid}", BuscarPessoaPorId);
pessoas.MapGet("/", BuscarPessoasPorTermo);

app.MapGet("/contagem-pessoas", ContarPessoas);

app.Run();

static async Task<IResult> CriarPessoa([FromBody] PessoaDto pessoa, HttpContext context)
{
    if (string.IsNullOrWhiteSpace(pessoa.Nome) || pessoa.Nome.Length > 100 ||
        string.IsNullOrWhiteSpace(pessoa.Apelido) || pessoa.Apelido.Length > 32 ||
        !DataValida(pessoa.Nascimento, out var dataNascimento) ||
        (pessoa.Stack != null && pessoa.Stack.Any(s => s.Length > 32)))
        return Results.UnprocessableEntity();

    var pessoaId = Guid.NewGuid();
    var stack = pessoa.Stack != null
        ? string.Join(";", pessoa.Stack)
        : null;

    const string insert = @"
        INSERT INTO pessoas(id, nome, apelido, nascimento, stack) 
        VALUES(@id, @nome, @apelido, @dataNascimento, @stack);
        ";

    await using var connection = await ConnectionFactory.GetPostgresConnection();
    await using var command = new NpgsqlCommand(insert, connection);
    command.Parameters.AddWithValue("id", pessoaId);
    command.Parameters.AddWithValue("nome", pessoa.Nome);
    command.Parameters.AddWithValue("apelido", pessoa.Apelido);
    command.Parameters.AddWithValue("dataNascimento", dataNascimento);

    if (stack is null) command.Parameters.AddWithValue("stack", DBNull.Value);
    else command.Parameters.AddWithValue("stack", stack);

    try
    {
        var result = await command.ExecuteNonQueryAsync();
        if (result != 1) return Results.BadRequest();
    }
    catch (Exception)
    {
        return Results.BadRequest();
    }

    context.Response.Headers["Location"] = $"/pessoas/{pessoaId}";
    return Results.Created();
}

static async Task<IResult> BuscarPessoaPorId(Guid id)
{
    const string query = @"
        SELECT nome, apelido, nascimento, stack
        FROM pessoas
        WHERE id = @id;
        ";

    await using var connection = await ConnectionFactory.GetPostgresConnection();
    await using var command = new NpgsqlCommand(query, connection);
    command.Parameters.AddWithValue("id", id);

    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        var nome = reader.GetString(0);
        var apelido = reader.GetString(1);
        var nascimento = reader.GetDateTime(2);
        var stack = await reader.IsDBNullAsync(3) ? null : reader.GetString(3);

        var pessoa = new PessoaDto
        {
            Id = id,
            Nome = nome,
            Apelido = apelido,
            Nascimento = nascimento.ToString("yyyy-MM-dd"),
            Stack = stack?.Split(";")
        };
        return Results.Ok(pessoa);
    }

    return Results.NotFound();
}

static async Task<IResult> BuscarPessoasPorTermo(string t)
{
    var query = $"%{t}%";
    const string search = @"
        SELECT id, nome, apelido, nascimento, stack
        FROM pessoas
        WHERE nome like @termo or apelido like @termo or stack like @termo;
    ";

    await using var connection = await ConnectionFactory.GetPostgresConnection();
    await using var command = new NpgsqlCommand(search, connection);
    command.Parameters.AddWithValue("termo", query);

    await using var reader = await command.ExecuteReaderAsync();

    var pessoas = new List<PessoaDto>();
    while (await reader.ReadAsync())
    {
        var id = reader.GetGuid(0);
        var nome = reader.GetString(1);
        var apelido = reader.GetString(2);
        var nascimento = reader.GetDateTime(3);
        var stack = await reader.IsDBNullAsync(4) ? null : reader.GetString(4);

        pessoas.Add(new PessoaDto
        {
            Id = id,
            Nome = nome,
            Apelido = apelido,
            Nascimento = nascimento.ToString("yyyy-MM-dd"),
            Stack = stack?.Split(";")
        });
    }

    return Results.Ok(pessoas);
}

static async Task<IResult> ContarPessoas()
{
    const string count = @"SELECT count(*) FROM pessoas;";

    await using var connection = await ConnectionFactory.GetPostgresConnection();
    await using var command = new NpgsqlCommand(count, connection);

    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        var total = reader.GetInt32(0);
        return Results.Ok(total);
    }

    return Results.BadRequest();
}

static bool DataValida(string? nascimento, out DateTime dataNascimento) =>
    DateTime.TryParseExact(nascimento, "yyyy-MM-dd",
        CultureInfo.InvariantCulture, DateTimeStyles.None,
        out dataNascimento);

public partial class Program
{
}