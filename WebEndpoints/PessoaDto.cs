namespace WebEndpoints;

public record PessoaDto
{
    public Guid Id { get; set; }
    public string? Nome { get; set; }
    public string? Apelido { get; set; }
    public string? Nascimento { get; set; }
    public IEnumerable<string>? Stack { get; set; }
}