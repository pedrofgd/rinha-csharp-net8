using System.Globalization;

namespace WebEndpoints;

public record PessoaDto
{
    public Guid Id { get; init; }
    public string? Nome { get; init; }
    public string? Apelido { get; init; }
    public string? Nascimento { get; init; }
    public IEnumerable<string>? Stack { get; init; }
}