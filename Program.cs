using Microsoft.AspNetCore.Mvc;

using DataAccess.Models.Moto;
using DataAccess.Models.Entregador;
using DataAccess.Models.Locacao;

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

List<Moto> motoclicletas = [];
List<Entregador> entregadores = [];
List<Locacao> locacoes = [];

//! MOTOS

app.MapPost("/motos", (Moto moto) =>
{
    if (string.IsNullOrEmpty(moto.identificador) || moto.ano <= 0 ||
        string.IsNullOrEmpty(moto.modelo) || string.IsNullOrEmpty(moto.placa))
        return Results.BadRequest("Dados inválidos");

    // TODO: ADD MOTO TO DB

    motoclicletas.Add(moto);

    return Results.Created();
})
.WithName("PostMotos");

app.MapGet("/motos", (string placa) =>
{
    if (string.IsNullOrEmpty(placa))
        return Results.BadRequest("Dados inválidos");

    // TODO: GET DB'S MOTORCYCLE

    List<Moto> listaMotos = new List<Moto>();

    foreach (var moto in motoclicletas)
        if (moto.placa == placa)
            listaMotos.Add(moto);

    return Results.Ok(listaMotos);
})
.WithName("GetMotos");

app.MapPut("/motos/{id}/placa", (string id, MotoUpdate moto) =>
{
    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(moto.placa))
        return Results.BadRequest("Dados inválidos");

    var motoExiste = motoclicletas.FirstOrDefault(m => m.identificador == id);

    if (motoExiste == null)
        return Results.BadRequest("Dados inválidos");

    motoclicletas[motoclicletas.IndexOf(motoExiste)] = motoExiste with { placa = moto.placa };

    return Results.Ok("Placa modificada com sucesso");
})
.WithName("PutMotos");

app.MapGet("/motos/{id}", (string id) =>
{
    if (string.IsNullOrEmpty(id))
        return Results.BadRequest("Request mal formado");

    var motoExiste = motoclicletas.FirstOrDefault(m => m.identificador == id);

    if (motoExiste == null)
        return Results.NotFound("Moto não encontrada");

    return Results.Ok(motoExiste);
})
.WithName("GetMotosID");

app.MapDelete("/motos/{id}", (string id) =>
{
    if (string.IsNullOrEmpty(id))
        return Results.BadRequest("Dados inválidos");

    int quantidade_removida = motoclicletas.RemoveAll(m => m.identificador == id);

    return Results.Ok();
})
.WithName("DeleteMoto");

//! ENTREGADORES

app.MapPost("/entregadores", (Entregador entregador) =>
{
    if (string.IsNullOrEmpty(entregador.identificador) || string.IsNullOrEmpty(entregador.nome) ||
        string.IsNullOrEmpty(entregador.cnpj) || string.IsNullOrEmpty(entregador.data_nascimento) ||
        string.IsNullOrEmpty(entregador.numero_cnh) || string.IsNullOrEmpty(entregador.tipo_cnh) ||
        string.IsNullOrEmpty(entregador.img_cnh))
        return Results.BadRequest("Dados inválidos");

    entregadores.Add(entregador);

    return Results.Created();
})
.WithName("PostEntregador");

app.MapPost("/entregadores/{id}/cnh", (string id, EntregadorUpdate entregador) =>
{
    if (string.IsNullOrEmpty(entregador.img_cnh))
        return Results.BadRequest("Dados inválidos");

    var entregadorExiste = entregadores.FirstOrDefault(e => e.identificador == id);

    if (entregadorExiste == null)
        return Results.BadRequest("Dados inválidos");

    entregadores[entregadores.IndexOf(entregadorExiste)] = entregadorExiste with { img_cnh = entregador.img_cnh };

    return Results.Created();
})
.WithName("PostEntregadorEnviaFotoCNH");

//! LOCAÇÃO

app.MapPost("/locacao", (Locacao locacao) =>
{
    if (string.IsNullOrEmpty(locacao.entregador_id) || string.IsNullOrEmpty(locacao.id_moto) ||
        string.IsNullOrEmpty(locacao.data_inicio) || string.IsNullOrEmpty(locacao.data_termino) ||
        string.IsNullOrEmpty(locacao.data_previsao_termino) || locacao.plano > 0)
        return Results.BadRequest("Dados inválidos");

    locacoes.Add(locacao);

    return Results.Created();
})
.WithName("PostLocacao");

app.MapGet("/locacao/{id}", (string id) =>
{
    if (string.IsNullOrEmpty(id))
        return Results.BadRequest("Dados inválidos");

    var locacaoExiste = locacoes.FirstOrDefault(l => l.id_moto == id);

    if (locacaoExiste == null)
        return Results.NotFound("Locação não encontrada");

    return Results.Ok(locacaoExiste);
})
.WithName("PostLocacao");

app.MapPut("/locacao/{id}/devolucao", (string id, LocacaoDevolucao locacaoDevolucao) =>
{
    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(locacaoDevolucao.data_devolucao))
        return Results.BadRequest("Dados inválidos");

    var locacaoExiste = locacoes.FirstOrDefault(l => l.id_moto == id);

    if (locacaoExiste == null)
        return Results.BadRequest("Dados inválidos");

    locacoes[locacoes.IndexOf(locacaoExiste)] = locacaoExiste with { data_devolucao = locacaoDevolucao.data_devolucao };

    return Results.Ok("Data de devolução informada com sucesso");
})
.WithName("PostLocacao");

app.Run();
