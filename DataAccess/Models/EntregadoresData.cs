namespace DataAccess.Models.Entregador
{
    public record Entregador(string identificador, string nome, string cnpj, string data_nascimento, string numero_cnh, string tipo_cnh, string img_cnh);

    public record EntregadorUpdate(string img_cnh);
}
