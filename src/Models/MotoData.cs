namespace Models.Moto
{
    public record Moto(string identificador, int ano, string modelo, string placa);
    
    public record MotoUpdate(string placa);

    public record MotoCadastradaEvent(string identificador, int ano, string modelo, string placa);
}