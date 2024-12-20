namespace Models.Locacao
{
    public record Locacao(string entregador_id, string moto_id, string data_inicio, string data_termino, string data_previsao_termino, int plano, string? data_devolucao);

    public record LocacaoDevolucao(string data_devolucao);

    public record LocacaoRetorno(string identificador, float valor_diaria, string entregador_id, string moto_id, string data_inicio, string data_termino, string data_previsao_termino, string? data_devolucao = null);

    public record LocacaoCompleta(string identificador, int plano, float valor_diaria, string data_inicio, string data_termino, string data_previsao_termino, string? data_devolucao = null);
}
