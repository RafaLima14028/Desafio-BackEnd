namespace DataAccess.Models.Locacao
{
    public record Locacao(string entregador_id, string id_moto, string data_inicio, string data_termino, string data_previsao_termino, int plano, string data_devolucao);

    public record LocacaoDevolucao(string data_devolucao);
}