using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using Models.Moto;
using Models.Entregador;
using Models.Locacao;

using Services.DataBase;

namespace Services.Subscriber
{
    public class Subscriber
    {
        private readonly ConnectionFactory _connectionFactory;
        private static readonly string _nome_fila = "MotosEntregadoresLocacoes";
        private static readonly string _nome_fila_respostas = "MotosEntregadoresLocacoesRespostas";
        private DataBase.DataBase _dataBase;
        private IChannel? _channel;
        private IChannel? _channel_response;

        public Subscriber()
        {
            try
            {
                this._connectionFactory = new ConnectionFactory
                {
                    HostName = "localhost"
                };

                this._dataBase = DataBase.DataBase.GetInstanceDataBase();
            }
            catch (Exception e)
            {
                throw new Exception($"[SUBSCRIBER] Erro: Na inicialização: {e.Message}");
            }
        }

        private async Task AbreConexao()
        {
            if (this._channel != null && !this._channel.IsOpen)
                return;

            var connection = await this._connectionFactory.CreateConnectionAsync();
            this._channel = await connection.CreateChannelAsync();

            await this._channel.QueueDeclareAsync(
                        queue: _nome_fila,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

            Console.WriteLine($"[SUBSCRIBER] FILA DECLARADA ({_nome_fila}).");
        }

        private async Task AbreConexaoResposta()
        {
            if (this._channel_response != null && !this._channel_response.IsOpen)
                return;

            var connection = await this._connectionFactory.CreateConnectionAsync();
            this._channel_response = await connection.CreateChannelAsync();

            await this._channel_response.QueueDeclareAsync(
                queue: _nome_fila_respostas,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            Console.WriteLine($"[SUBSCRIBER] FILA DECLARADA ({_nome_fila_respostas}).");
        }

        public async Task EscutandoFila()
        {
            await this.AbreConexao();

            if (this._channel == null)
                return;

            try
            {
                var consumer = new AsyncEventingBasicConsumer(this._channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    Console.WriteLine("[SUBSCRIBER] MENSAGEM RECEBIDA.");

                    var body = ea.Body.ToArray();
                    var mensagem = Encoding.UTF8.GetString(body);

                    var jsonDoc = JsonDocument.Parse(mensagem);

                    await ProcessaMensagens(jsonDoc);
                };

                await this._channel.BasicConsumeAsync(
                    queue: _nome_fila,
                    autoAck: true,
                    consumer: consumer
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SUBSCRIBER] Erro: No escutando a fila: {e.Message}");
            }
        }

        private JsonObject? TransformaRemoveCampoETransformaEmJson(JsonElement root)
        {
            var jsonObject = JsonNode.Parse(root.GetRawText())?.AsObject();

            if (jsonObject == null)
                return null;

            jsonObject.Remove("funcao");

            return jsonObject;
        }

        private async Task ProcessaMensagens(JsonDocument jsonDoc)
        {
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("funcao", out var funcaoElement))
            {
                var funcao = funcaoElement.GetString();

                if (string.IsNullOrEmpty(funcao))
                    return;

                funcao = funcao.ToLower();

                var jsonObject = this.TransformaRemoveCampoETransformaEmJson(root);

                if (jsonObject == null)
                    return;

                switch (funcao)
                {
                    case "postmotos":
                        this.PostMotos(jsonObject);
                        break;

                    case "getmotos":
                        await this.GetMotos(jsonObject);
                        break;

                    case "putmotos":
                        this.PutMotos(jsonObject);
                        break;

                    case "getmotosid":
                        await this.GetMotosID(jsonObject);
                        break;

                    case "deletemotos":
                        this.DeleteMotos(jsonObject);
                        break;

                    case "postentregadores":
                        this.PostEntregadores(jsonObject);
                        break;

                    case "postentregadoresenviafotocnh":
                        this.PostEntregadoresEnviaFotoCNH(jsonObject);
                        break;

                    case "postlocacao":
                        this.PostLocacao(jsonObject);
                        break;

                    case "getlocacao":
                        await this.GetLocacao(jsonObject);
                        break;

                    case "putlocacao":
                        this.PutLocacao(jsonObject);
                        break;
                }
            }

            await Task.CompletedTask;
        }

        private void PostMotos(JsonObject jsonObject)
        {
            var identificador = jsonObject["identificador"]?.ToString();
            var ano = jsonObject["ano"]?.GetValue<int>() ?? -1;
            var modelo = jsonObject["modelo"]?.ToString();
            var placa = jsonObject["placa"]?.ToString();

            if (string.IsNullOrEmpty(identificador) || ano <= 0 ||
            string.IsNullOrEmpty(modelo) || string.IsNullOrEmpty(placa))
                return;

            Moto moto = new Moto(identificador, ano, modelo, placa);

            this._dataBase.PostMotos(moto);

            if (moto != null && moto.ano == 2024)
                Console.WriteLine($"[SUBSCRIBER] Moto de 2024 recebida: {moto}");
        }

        private async Task GetMotos(JsonObject jsonObject)
        {
            var placa = jsonObject["placa"]?.ToString();

            if (this._channel == null)
                return;

            if (string.IsNullOrEmpty(placa))
                return;

            await this.AbreConexaoResposta();

            if (this._channel_response == null)
                return;

            string mensagem_resposta = "";
            List<Moto> motos = this._dataBase.GetMotos(placa);

            if (motos == null)
            {
                List<Moto> motos_vazia = new List<Moto>();
                mensagem_resposta = JsonSerializer.Serialize(motos_vazia);
            }
            else
            {
                mensagem_resposta = JsonSerializer.Serialize(motos);
            }

            await this._channel_response.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: _nome_fila_respostas,
                    body: Encoding.UTF8.GetBytes(mensagem_resposta)
                );
        }

        private void PutMotos(JsonObject jsonObject)
        {
            var id = jsonObject["identificador_moto"]?.ToString();
            var placa = jsonObject["placa"]?.ToString();

            if (this._channel == null)
                return;
            if (string.IsNullOrEmpty(placa) || string.IsNullOrEmpty(id))
                return;

            this._dataBase.PutMotos(id, placa);
        }

        private async Task GetMotosID(JsonObject jsonObject)
        {
            var identificador = jsonObject["identificador"]?.ToString();

            if (string.IsNullOrEmpty(identificador))
                return;

            if (this._channel == null)
                return;

            await this.AbreConexaoResposta();

            if (this._channel_response == null)
                return;

            Moto? moto = this._dataBase.GetMotosID(identificador);

            var mensagem_resposta = JsonSerializer.Serialize(moto);

            await this._channel_response.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _nome_fila_respostas,
                body: Encoding.UTF8.GetBytes(mensagem_resposta)
            );
        }

        private void DeleteMotos(JsonObject jsonObject)
        {
            var identificador = jsonObject["identificador"]?.ToString();

            if (string.IsNullOrEmpty(identificador))
                return;

            this._dataBase.DeleteMoto(identificador);
        }

        private void PostEntregadores(JsonObject jsonObject)
        {
            var identificador = jsonObject["identificador"]?.ToString();
            var nome = jsonObject["nome"]?.ToString();
            var cnpj = jsonObject["cnpj"]?.ToString();
            var data_nascimento = jsonObject["data_nascimento"]?.ToString();
            var numero_cnh = jsonObject["numero_cnh"]?.ToString();
            var tipo_cnh = jsonObject["tipo_cnh"]?.ToString();
            var imagem_cnh = jsonObject["imagem_cnh"]?.ToString();

            if (string.IsNullOrEmpty(identificador) || string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(cnpj) || string.IsNullOrEmpty(data_nascimento) || string.IsNullOrEmpty(numero_cnh) || string.IsNullOrEmpty(tipo_cnh))
                return;

            Entregador entregador = new Entregador(
                identificador,
                nome,
                cnpj,
                data_nascimento,
                numero_cnh,
                tipo_cnh,
                imagem_cnh
            );

            this._dataBase.PostEntregador(entregador);
        }

        private void PostEntregadoresEnviaFotoCNH(JsonObject jsonObject)
        {
            var identificador = jsonObject["identificador"]?.ToString();
            var img_cnh_entregador = jsonObject["img_cnh_entregador"]?.ToString();

            if (string.IsNullOrEmpty(identificador) || string.IsNullOrEmpty(img_cnh_entregador))
                return;

            this._dataBase.PostEntregadorEnviaFotoCNH(identificador, img_cnh_entregador);
        }

        private void PostLocacao(JsonObject jsonObject)
        {
            var entregador_id = jsonObject["entregador_id"]?.ToString();
            var moto_id = jsonObject["moto_id"]?.ToString();
            var data_inicio = jsonObject["data_inicio"]?.ToString();
            var data_termino = jsonObject["data_termino"]?.ToString();
            var data_previsao_termino = jsonObject["data_previsao_termino"]?.ToString();
            int plano = jsonObject["plano"]?.GetValue<int>() ?? 0;

            if (string.IsNullOrEmpty(entregador_id) || string.IsNullOrEmpty(moto_id) || string.IsNullOrWhiteSpace(data_inicio) || string.IsNullOrEmpty(data_termino) || string.IsNullOrEmpty(data_previsao_termino) || plano <= 0)
                return;

            Locacao locacao = new Locacao(
                entregador_id,
                moto_id,
                data_inicio,
                data_termino,
                data_previsao_termino,
                plano,
                null
            );

            this._dataBase.PostLocacao(locacao);
        }

        private async Task GetLocacao(JsonObject jsonObject)
        {
            await this.AbreConexaoResposta();

            if (this._channel == null || this._channel_response == null)
                return;

            var identificador = jsonObject["identificador"]?.ToString();

            if (string.IsNullOrEmpty(identificador))
                return;

            LocacaoRetorno? locacao = this._dataBase.GetLocacao(identificador);

            var mensagem_resposta = JsonSerializer.Serialize(locacao);

            await this._channel_response.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _nome_fila_respostas,
                body: Encoding.UTF8.GetBytes(mensagem_resposta)
            );
        }

        private void PutLocacao(JsonObject jsonObject)
        {
            var identificador = jsonObject["identificador"]?.ToString();
            var data_devolucao = jsonObject["data_devolucao"]?.ToString();

            if (string.IsNullOrEmpty(identificador) || string.IsNullOrEmpty(data_devolucao))
                return;

            this._dataBase.PutLocacao(identificador, data_devolucao);
        }
    }
}
