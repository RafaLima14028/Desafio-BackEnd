namespace Models.Entregador
{
    public record Entregador(string identificador, string nome, string cnpj, string data_nascimento, string numero_cnh, string tipo_cnh, string? imagem_cnh = null);

    public record EntregadorUpdate(string imagem_cnh);
}
