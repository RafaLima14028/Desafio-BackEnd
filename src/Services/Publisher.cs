using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;

using Models.Moto;
using Models.Entregador;
using Models.Locacao;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client.Events;

namespace Services.Publisher
{
    public class Publisher
    {
        private readonly ConnectionFactory _connectionFactory;
        private static readonly string _nome_fila = "MotosEntregadoresLocacoes";
        private static readonly string _nome_fila_respostas = "MotosEntregadoresLocacoesRespostas";
        private IChannel? _channel;
        private IChannel? _channel_response;
        private TaskCompletionSource<string>? _tcs;

        public Publisher()
        {
            try
            {
                this._connectionFactory = new ConnectionFactory()
                {
                    HostName = "localhost"
                };
            }
            catch (Exception e)
            {
                throw new Exception("Erro na inicialização do Publisher do barramento:", e);
            }
        }

        private async Task AbreConexao()
        {
            if (this._channel != null && !this._channel.IsOpen)
                return;

            var connection = await _connectionFactory.CreateConnectionAsync();
            this._channel = await connection.CreateChannelAsync();

            await this._channel.QueueDeclareAsync(
                queue: _nome_fila,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            Console.WriteLine($"[PUBLISHER] FILA DECLARADA ({_nome_fila}).");
        }

        private async Task AbreConexaoResposta()
        {
            if (this._channel_response != null && !this._channel_response.IsOpen)
                return;

            var connection = await _connectionFactory.CreateConnectionAsync();
            this._channel_response = await connection.CreateChannelAsync();

            await this._channel_response.QueueDeclareAsync(
                queue: _nome_fila_respostas,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            Console.WriteLine($"[PUBLISHER] FILA DECLARADA ({_nome_fila_respostas}).");
        }

        private async Task EnviaMensagem(IChannel channel, byte[] mensagem_bytes)
        {
            await channel.BasicPublishAsync
                (
                    exchange: string.Empty,
                    routingKey: _nome_fila,
                    body: mensagem_bytes
                );
        }

        private async Task<string> EsperaResposta()
        {
            await this.AbreConexaoResposta();

            if (this._channel_response == null)
                return "";

            if (this._tcs != null)
                this._tcs.TrySetCanceled();

            this._tcs = new TaskCompletionSource<string>();

            var consumer = new AsyncEventingBasicConsumer(this._channel_response);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    Console.WriteLine("Mensagem recebida no pub.");

                    var resposta = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine("Resposta do sub no pub");
                    Console.WriteLine(resposta);
                    Console.WriteLine();

                    this._tcs.TrySetResult(resposta);
                }
                catch (Exception e)
                {
                    this._tcs.TrySetException(e);
                }

                await Task.CompletedTask;
            };

            await this._channel_response.BasicConsumeAsync(
                _nome_fila_respostas,
                true,
                consumer
            );

            return await this._tcs.Task;
        }

        public async Task PublisherPostMotos(Moto moto)
        {
            try
            {
                await this.AbreConexao();

                if (this._channel == null)
                    return;

                var mensagem = JsonSerializer.Serialize(new
                {
                    identificador = moto.identificador,
                    ano = moto.ano,
                    modelo = moto.modelo,
                    placa = moto.placa,
                    funcao = "postMotos"
                });

                var body = Encoding.UTF8.GetBytes(mensagem);

                await this.EnviaMensagem(this._channel, body);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problema no pub ao envia uma nova moto: {e.Message}");
            }
        }

        public async Task<List<Moto>> PublisherGetMotos(string placa)
        {
            try
            {
                await this.AbreConexao();
                await this.AbreConexaoResposta();

                if (this._channel == null || this._channel_response == null)
                {
                    Console.WriteLine("algum canal == null");
                    return new List<Moto>();
                }

                var mensagem = JsonSerializer.Serialize(new
                {
                    placa = placa,
                    funcao = "getMotos"
                });

                Console.WriteLine(mensagem);

                var body = Encoding.UTF8.GetBytes(mensagem);

                Console.WriteLine("PUB ENVIANDO");

                await this.EnviaMensagem(this._channel, body);

                Console.WriteLine("PUB ENVIOU MENSAGEM");

                string resposta = await EsperaResposta();

                Console.WriteLine("PUB SERIALIZANDO JSON");

                List<Moto>? motos = JsonSerializer.Deserialize<List<Moto>>(resposta);

                Console.WriteLine("PUB SERIALIZOU JSON");
                Console.WriteLine("SAINDO DO PUB");

                if (motos == null)
                {
                    Console.WriteLine("Motos == null no pub");
                    motos = new List<Moto>();
                }

                Console.WriteLine(motos);

                return motos;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problema no pub ao consultar uma moto com a placa: {e.Message}");
            }

            Console.WriteLine("SAINDO DO PUB");

            return new List<Moto>();
        }

        public async Task PublisherPutMotos(string id, string placa)
        {
            try
            {
                await this.AbreConexao();

                if (this._channel == null)
                    return;

                var mensagem = JsonSerializer.Serialize(new
                {
                    identificador_moto = id,
                    placa = placa,
                    funcao = "putMotos"
                });

                var body = Encoding.UTF8.GetBytes(mensagem);

                await this.EnviaMensagem(this._channel, body);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problema no pub ao modificar a placa de uma moto: {e.Message}");
            }
        }

        public async Task<Moto?> PublisherGetMotosID(string id)
        {
            try
            {
                await this.AbreConexao();
                await this.AbreConexaoResposta();

                if (this._channel == null || this._channel_response == null)
                    return null;

                var mensagem = JsonSerializer.Serialize(new
                {
                    identificador = id,
                    funcao = "getMotosID"
                });

                var body = Encoding.UTF8.GetBytes(mensagem);

                await this.EnviaMensagem(this._channel, body);

                string resposta = await EsperaResposta();

                Moto? moto = JsonSerializer.Deserialize<Moto?>(resposta);

                return moto;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problema no pub ao consultar uma moto pelo ID: {e.Message}");
            }

            return null;
        }

        public async Task PublisherDeleteMotos(string id)
        {
            try
            {
                await this.AbreConexao();

                if (this._channel == null)
                    return;

                var mensagem = JsonSerializer.Serialize(new
                {
                    identificador = id,
                    funcao = "deleteMotos"
                });

                var body = Encoding.UTF8.GetBytes(mensagem);

                await this.EnviaMensagem(this._channel, body);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problema no pub ao remover uma moto: {e.Message}");
            }
        }

        public async Task PublisherPostEntregadores(Entregador entregador)
        {
            try
            {
                await this.AbreConexao();

                if (this._channel == null)
                    return;

                var mensagem = JsonSerializer.Serialize(new
                {
                    identificador = entregador.identificador,
                    nome = entregador.nome,
                    cnpj = entregador.cnpj,
                    data_nascimento = entregador.data_nascimento,
                    numero_cnh = entregador.numero_cnh,
                    tipo_cnh = entregador.tipo_cnh,
                    imagem_cnh = entregador.imagem_cnh,
                    funcao = "postEntregadores"
                });

                var body = Encoding.UTF8.GetBytes(mensagem);

                await this.EnviaMensagem(this._channel, body);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problema no pub ao cadastrar um entregador: {e.Message}");
            }
        }

        public async Task PublisherPostEntregadoresEnviaFotoCNH(string id, string img_cnh_entregador)
        {
            try
            {
                await this.AbreConexao();

                if (this._channel == null)
                    return;

                var mensagem = JsonSerializer.Serialize(new
                {
                    identificador = id,
                    img_cnh_entregador = img_cnh_entregador,
                    funcao = "postEntregadoresEnviaFotoCNH"
                });

                var body = Encoding.UTF8.GetBytes(mensagem);

                await this.EnviaMensagem(this._channel, body);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problema no pub ao enviar foto da CNH do entregador: {e.Message}");
            }
        }

        public async Task PublisherPostLocacao(Locacao locacao)
        {
            try
            {
                await this.AbreConexao();

                if (this._channel == null)
                    return;

                var mensagem = JsonSerializer.Serialize(new
                {
                    entregador_id = locacao.entregador_id,
                    moto_id = locacao.moto_id,
                    data_inicio = locacao.data_inicio,
                    data_termino = locacao.data_termino,
                    data_previsao_termino = locacao.data_previsao_termino,
                    plano = locacao.plano,
                    funcao = "postLocacao"
                });

                var body = Encoding.UTF8.GetBytes(mensagem);

                await this.EnviaMensagem(this._channel, body);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problema no pub ao criar uma locação: {e.Message}");
            }
        }

        public async Task<LocacaoRetorno?> PublisherGetLocacao(string id)
        {
            try
            {
                await this.AbreConexao();
                await this.AbreConexaoResposta();

                if (this._channel == null || this._channel_response == null)
                    return null;

                var mensagem = JsonSerializer.Serialize(new
                {
                    identificador = id,
                    funcao = "getLocacao"
                });

                var body = Encoding.UTF8.GetBytes(mensagem);

                await this.EnviaMensagem(this._channel, body);

                string resposta = await EsperaResposta();

                LocacaoRetorno? locacao = JsonSerializer.Deserialize<LocacaoRetorno?>(resposta);

                return locacao;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problema no pub ao consultar uma locação pelo ID: {e.Message}");
            }

            return null;
        }

        public async Task PublisherPutLocacao(string id, string data_devolucao)
        {
            try
            {
                await this.AbreConexao();

                if (this._channel == null)
                    return;

                var mensagem = JsonSerializer.Serialize(new
                {
                    identificador = id,
                    data_devolucao = data_devolucao,
                    funcao = "putLocacao"
                });

                var body = Encoding.UTF8.GetBytes(mensagem);

                await this.EnviaMensagem(this._channel, body);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problema no pub ao informar data de devolução e cálculo do valor: {e.Message}");
            }
        }
    }
}
