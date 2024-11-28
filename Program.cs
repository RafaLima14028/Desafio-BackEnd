using Microsoft.AspNetCore.Mvc;

using Models.Moto;
using Models.Entregador;
using Models.Locacao;

using Services.DataBase;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

List<Locacao> locacoes = [];

DataBase db = DataBase.GetInstanceDataBase();

//! MOTOS

app.MapPost("/motos", (Moto moto) =>
{
    if (string.IsNullOrEmpty(moto.identificador) || moto.ano <= 0 ||
        string.IsNullOrEmpty(moto.modelo) || string.IsNullOrEmpty(moto.placa))
        return Results.BadRequest("Dados inválidos");

    db.PostMotos(moto);

    return Results.Created();
})
.WithName("PostMotos");

app.MapGet("/motos", (string placa) =>
{
    if (string.IsNullOrEmpty(placa))
        return Results.BadRequest("Dados inválidos");

    var lista_motos = db.GetMotos(placa);

    return Results.Ok(lista_motos);
})
.WithName("GetMotos");

app.MapPut("/motos/{id}/placa", (string id, MotoUpdate moto) =>
{
    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(moto.placa))
        return Results.BadRequest("Dados inválidos");

    db.PutMotos(id, moto.placa);

    return Results.Ok("Placa modificada com sucesso");
})
.WithName("PutMotos");

app.MapGet("/motos/{id}", (string id) =>
{
    if (string.IsNullOrEmpty(id))
        return Results.BadRequest("Request mal formado");

    var motoExiste = db.GetMotoID(id);

    if (motoExiste == null)
        return Results.NotFound("Moto não encontrada");

    return Results.Ok(motoExiste);
})
.WithName("GetMotosID");

app.MapDelete("/motos/{id}", (string id) =>
{
    if (string.IsNullOrEmpty(id))
        return Results.BadRequest("Dados inválidos");

    db.DeleteMoto(id);

    return Results.Ok();
})
.WithName("DeleteMoto");

//! ENTREGADORES

app.MapPost("/entregadores", (Entregador entregador) =>
{
    if (string.IsNullOrEmpty(entregador.identificador) || string.IsNullOrEmpty(entregador.nome) ||
        string.IsNullOrEmpty(entregador.cnpj) || string.IsNullOrEmpty(entregador.data_nascimento) ||
        string.IsNullOrEmpty(entregador.numero_cnh) || string.IsNullOrEmpty(entregador.tipo_cnh) ||
        string.IsNullOrEmpty(entregador.imagem_cnh))
        return Results.BadRequest("Dados inválidos");

    db.PostEntregador(entregador);

    return Results.Created();
})
.WithName("PostEntregador");

app.MapPost("/entregadores/{id}/cnh", (string id, EntregadorUpdate entregador) =>
{
    if (string.IsNullOrEmpty(entregador.imagem_cnh))
        return Results.BadRequest("Dados inválidos");

    db.PostEntregadorEnviaFotoCNH(id, entregador.imagem_cnh);

    return Results.Created();
})
.WithName("PostEntregadorEnviaFotoCNH");

//! LOCAÇÃO

app.MapPost("/locacao", (Locacao locacao) =>
{
    if (string.IsNullOrEmpty(locacao.entregador_id) || string.IsNullOrEmpty(locacao.moto_id) ||
        string.IsNullOrEmpty(locacao.data_inicio) || string.IsNullOrEmpty(locacao.data_termino) ||
        string.IsNullOrEmpty(locacao.data_previsao_termino) || locacao.plano <= 0)
        return Results.BadRequest("Dados inválidos");

    // locacoes.Add(locacao);
    db.PostLocacao(locacao);

    return Results.Created();
})
.WithName("PostLocacao");

app.MapGet("/locacao/{id}", (string id) =>
{
    if (string.IsNullOrEmpty(id))
        return Results.BadRequest("Dados inválidos");

    var locacaoExiste = db.GetLocacao(id);

    if (locacaoExiste == null)
        return Results.NotFound("Locação não encontrada");

    return Results.Ok(locacaoExiste);
})
.WithName("GetLocacao");

app.MapPut("/locacao/{id}/devolucao", (string id, LocacaoDevolucao locacaoDevolucao) =>
{
    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(locacaoDevolucao.data_devolucao))
        return Results.BadRequest("Dados inválidos");

    db.PutLocacao(id, locacaoDevolucao.data_devolucao);

    return Results.Ok("Data de devolução informada com sucesso");
})
.WithName("PutLocacao");

app.Run();
