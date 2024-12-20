using Microsoft.AspNetCore.Mvc;

using Models.Moto;
using Models.Entregador;
using Models.Locacao;

using Services.Publisher;
using Services.Subscriber;

class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        app.UseHttpsRedirection();

        _ = Task.Run(() => ExecutaSubscriber());

        ConfiguraRotasAPI(app);

        app.Run();
    }

    private static void ConfiguraRotasAPI(WebApplication app)
    {
        var publisher = new Publisher();

        //! MOTOS

        app.MapPost("/motos", (Moto moto) =>
        {
            if (string.IsNullOrEmpty(moto.identificador) || moto.ano <= 0 ||
                string.IsNullOrEmpty(moto.modelo) || string.IsNullOrEmpty(moto.placa))
                return Results.BadRequest(new { mensagem = "Dados inválidos" });

            _ = publisher.PublisherPostMotos(moto);

            return Results.Created();
        })
        .WithName("PostMotos");

        app.MapGet("/motos", async (string placa) =>
       {
           if (string.IsNullOrEmpty(placa))
               return Results.BadRequest(new { mensagem = "Dados inválidos" });

           var lista_motos = await publisher.PublisherGetMotos(placa);

           return Results.Ok(lista_motos);
       })
       .WithName("GetMotos");

        app.MapPut("/motos/{id}/placa", (string id, MotoUpdate moto) =>
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(moto.placa))
                return Results.BadRequest(new { mensagem = "Dados inválidos" });

            _ = publisher.PublisherPutMotos(id, moto.placa);

            return Results.Ok(new { mensagem = "Placa modificada com sucesso" });
        })
        .WithName("PutMotos");

        app.MapGet("/motos/{id}", async (string id) =>
        {
            if (string.IsNullOrEmpty(id))
                return Results.BadRequest(new { mensagem = "Request mal formado" });

            var motoExiste = await publisher.PublisherGetMotosID(id);

            if (motoExiste == null)
                return Results.NotFound(new { mensagem = "Moto não encontrada" });

            return Results.Ok(motoExiste);
        })
        .WithName("GetMotosID");

        app.MapDelete("/motos/{id}", (string id) =>
        {
            if (string.IsNullOrEmpty(id))
                return Results.BadRequest(new { mensagem = "Dados inválidos" });

            _ = publisher.PublisherDeleteMotos(id);

            return Results.Ok();
        })
        .WithName("DeleteMotos");

        //! ENTREGADORES

        app.MapPost("/entregadores", (Entregador entregador) =>
        {
            if (string.IsNullOrEmpty(entregador.identificador) || string.IsNullOrEmpty(entregador.nome) ||
                string.IsNullOrEmpty(entregador.cnpj) || string.IsNullOrEmpty(entregador.data_nascimento) ||
                string.IsNullOrEmpty(entregador.numero_cnh) || string.IsNullOrEmpty(entregador.tipo_cnh))
                return Results.BadRequest(new { mensagem = "Dados inválidos" });

            _ = publisher.PublisherPostEntregadores(entregador);

            return Results.Created();
        })
        .WithName("PostEntregador");

        app.MapPost("/entregadores/{id}/cnh", (string id, EntregadorUpdate entregador) =>
        {
            if (string.IsNullOrEmpty(entregador.imagem_cnh))
                return Results.BadRequest(new { mensagem = "Dados inválidos" });

            _ = publisher.PublisherPostEntregadoresEnviaFotoCNH(id, entregador.imagem_cnh);

            return Results.Created();
        })
        .WithName("PostEntregadorEnviaFotoCNH");

        //! LOCAÇÃO

        app.MapPost("/locacao", (Locacao locacao) =>
        {
            if (string.IsNullOrEmpty(locacao.entregador_id) || string.IsNullOrEmpty(locacao.moto_id) ||
                string.IsNullOrEmpty(locacao.data_inicio) || string.IsNullOrEmpty(locacao.data_termino) ||
                string.IsNullOrEmpty(locacao.data_previsao_termino) || locacao.plano <= 0)
                return Results.BadRequest(new { mensagem = "Dados inválidos" });

            _ = publisher.PublisherPostLocacao(locacao);

            return Results.Created();
        })
        .WithName("PostLocacao");

        app.MapGet("/locacao/{id}", async (string id) =>
        {
            if (string.IsNullOrEmpty(id))
                return Results.BadRequest(new { mensagem = "Dados inválidos" });

            var locacaoExiste = await publisher.PublisherGetLocacao(id);

            if (locacaoExiste == null)
                return Results.NotFound(new { mensagem = "Locação não encontrada" });

            return Results.Ok(locacaoExiste);
        })
        .WithName("GetLocacao");

        app.MapPut("/locacao/{id}/devolucao", (string id, LocacaoDevolucao locacaoDevolucao) =>
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(locacaoDevolucao.data_devolucao))
                return Results.BadRequest(new { mensagem = "Dados inválidos" });

            _ = publisher.PublisherPutLocacao(id, locacaoDevolucao.data_devolucao);

            return Results.Ok(new { mensagem = "Data de devolução informada com sucesso" });
        })
        .WithName("PutLocacao");
    }

    private static void ExecutaSubscriber()
    {
        var subscriber = new Subscriber();

        _ = subscriber.EscutandoFila();
    }
}
