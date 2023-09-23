using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WebEndpoints;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// PostgreSQL config
var pgConnString = configuration.GetConnectionString("PostgreSQL");
ArgumentException.ThrowIfNullOrWhiteSpace(pgConnString);
builder.Services.AddNpgsqlDataSource(pgConnString);

var app = builder.Build();

var pessoas = app.MapGroup("/pessoas");
pessoas.MapPost("/", CriarPessoa);
pessoas.MapGet("/{id:guid}", BuscarPessoaPorId);
pessoas.MapGet("/", BuscarPessoasPorTermo);

app.MapGet("/contagem-pessoas", ContarPessoas);

app.Run();

async Task<IResult> CriarPessoa([FromBody] PessoaDto? pessoa, HttpContext context, NpgsqlConnection connection)
{
    if (pessoa is null) return Results.BadRequest();
    if (string.IsNullOrWhiteSpace(pessoa.Nome) || pessoa.Nome.Length > 100 ||
        string.IsNullOrWhiteSpace(pessoa.Apelido) || pessoa.Apelido.Length > 32 ||
        !DataValida(pessoa.Nascimento, out var dataNascimento) ||
        (pessoa.Stack != null && pessoa.Stack.Any(s => s.Length > 32)))
        return Results.UnprocessableEntity();

    var pessoaId = Guid.NewGuid();
    var stack = pessoa.Stack != null
        ? string.Join(";", pessoa.Stack)
        : string.Empty;

    await connection.OpenAsync();

    await using var command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO pessoas(id, nome, apelido, nascimento, stack) 
        VALUES(@id, @nome, @apelido, @dataNascimento, @stack);";
    command.Parameters.AddWithValue("id", pessoaId);
    command.Parameters.AddWithValue("nome", pessoa.Nome);
    command.Parameters.AddWithValue("apelido", pessoa.Apelido);
    command.Parameters.AddWithValue("dataNascimento", dataNascimento);
    command.Parameters.AddWithValue("stack", stack);

    try
    {
        var result = await command.ExecuteNonQueryAsync();
        if (result != 1) return Results.BadRequest();
    }
    catch (Exception)
    {
        return Results.BadRequest();
    }

    await connection.DisposeAsync();

    context.Response.Headers["Location"] = $"/pessoas/{pessoaId}";
    return Results.Created();
}

async Task<IResult?> BuscarPessoaPorId(Guid id, NpgsqlConnection connection)
{
    await connection.OpenAsync();

    await using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT nome, apelido, nascimento, stack
        FROM pessoas
        WHERE id = @id;";
    command.Parameters.AddWithValue("id", id);

    await using var reader = await command.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return Results.NotFound();

    var stack = reader.GetString(3);
    var pessoa = new PessoaDto
    {
        Id = id,
        Nome = reader.GetString(0),
        Apelido = reader.GetString(1),
        Nascimento = reader.GetDateTime(2).ToString("yyyy-MM-dd"),
        Stack = stack == string.Empty ? null : stack.Split(";")
    };

    await connection.DisposeAsync();
    
    return Results.Ok(pessoa);
}

async Task<IResult> BuscarPessoasPorTermo([FromQuery] string? t, NpgsqlConnection connection)
{
    if (string.IsNullOrWhiteSpace(t)) return Results.BadRequest();

    await connection.OpenAsync();

    var query = $"%{t}%";
    await using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT id, nome, apelido, nascimento, stack
        FROM pessoas
        WHERE busca_trgm ILIKE @termo
        LIMIT 50;";
    command.Parameters.AddWithValue("termo", query);

    await using var reader = await command.ExecuteReaderAsync();

    var pessoasEncontradas = new List<PessoaDto>();
    while (await reader.ReadAsync())
    {
        var stack = reader.GetString(4);
        pessoasEncontradas.Add(new PessoaDto
        {
            Id = reader.GetGuid(0),
            Nome = reader.GetString(1),
            Apelido = reader.GetString(2),
            Nascimento = reader.GetDateTime(3).ToString("yyyy-MM-dd"),
            Stack = stack == string.Empty ? null : stack.Split(";")
        });
    }

    await connection.DisposeAsync();

    return Results.Ok(pessoasEncontradas);
}

async Task<IResult> ContarPessoas(NpgsqlConnection connection)
{
    await connection.OpenAsync();

    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT count(*) FROM pessoas;";

    var total = await command.ExecuteScalarAsync();

    await connection.DisposeAsync();

    return Results.Ok(total);
}

static bool DataValida(string? nascimento, out DateTime dataNascimento) =>
    DateTime.TryParseExact(nascimento, "yyyy-MM-dd",
        CultureInfo.InvariantCulture, DateTimeStyles.None,
        out dataNascimento);

// ReSharper disable once ClassNeverInstantiated.Global
public partial class Program
{
}