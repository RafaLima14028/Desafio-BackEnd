using Npgsql;

using Models.Moto;
using Models.Entregador;
using Models.Locacao;

namespace Services.DataBase
{
    public class DataBase
    {
        private static string _formato = "yyyy-MM-ddTHH:mm:ssZ";
        private readonly static string _diretorio_imagens = "imagens_entregadores";
        private readonly static string _diretorio_completo = Path.Combine(Directory.GetCurrentDirectory(), _diretorio_imagens);
        private static DataBase? _database_instance; // Instancia Singleton
        private string _conexaoString;

        private DataBase()
        {
            this._conexaoString = "Host=localhost;Port=5432;Database=motoDB;Username=postgres;Password=1234";

            var conexao = new NpgsqlConnection(this._conexaoString);
            conexao.Open();

            string criaSequenciaIdentificadorLocacoesQuery = @"
                CREATE SEQUENCE IF NOT EXISTS locacoes_identificador_seq START 1;
            ";

            var comando = new NpgsqlCommand(criaSequenciaIdentificadorLocacoesQuery, conexao);
            comando.ExecuteNonQuery();

            string criaTabelaMotosQuery = @"
                CREATE TABLE IF NOT EXISTS Motos (
                    IDENTIFICADOR VARCHAR(100) PRIMARY KEY,
                    ANO INT NOT NULL,
                    MODELO VARCHAR(100) NOT NULL,
                    PLACA VARCHAR(20) UNIQUE NOT NULL
                );
            ";

            comando = new NpgsqlCommand(criaTabelaMotosQuery, conexao);
            comando.ExecuteNonQuery();

            string criaTabelaEntregadoresQuery = @"
                CREATE TABLE IF NOT EXISTS Entregadores (
                    IDENTIFICADOR VARCHAR(100) PRIMARY KEY,
                    NOME VARCHAR(100) NOT NULL,
                    CNPJ VARCHAR(50) UNIQUE NOT NULL,
                    DATA_NASCIMENTO TIMESTAMP NOT NULL,
                    NUMERO_CNH VARCHAR(50) UNIQUE NOT NULL,
                    TIPO_CNH VARCHAR(3) NOT NULL,
                    IMAGEM_CNH VARCHAR(500)
                );
            ";

            comando = new NpgsqlCommand(criaTabelaEntregadoresQuery, conexao);
            comando.ExecuteNonQuery();

            string criaTabelaLotacaoQuery = @"
                CREATE TABLE IF NOT EXISTS Locacoes (
                    IDENTIFICADOR VARCHAR(50) PRIMARY KEY DEFAULT 'locacao' || nextval('locacoes_identificador_seq'),
                    ENTREGADOR_ID VARCHAR(100) NOT NULL,
                    MOTO_ID VARCHAR(100) NOT NULL,
                    DATA_INICIO TIMESTAMP NOT NULL,
                    DATA_TERMINO TIMESTAMP NOT NULL,
                    DATA_PREVISAO_TERMINO TIMESTAMP NOT NULL,
                    PLANO INTEGER NOT NULL,
                    VALOR_PLANO REAL NOT NULL,
                    DATA_DEVOLUCAO TIMESTAMP,

                    CONSTRAINT FK_ENTREGADOR_ID FOREIGN KEY (ENTREGADOR_ID) REFERENCES Entregadores (IDENTIFICADOR),
                    CONSTRAINT FK_MOTO_ID FOREIGN KEY (MOTO_ID) REFERENCES Motos (IDENTIFICADOR)
                );
            ";

            comando = new NpgsqlCommand(criaTabelaLotacaoQuery, conexao);
            comando.ExecuteNonQuery();

            conexao.Close();

            Console.WriteLine("[DATABASE] PRONTO...");
        }

        public static DataBase GetInstanceDataBase()
        {
            if (_database_instance == null)
                _database_instance = new DataBase();

            return _database_instance;
        }

        private static DateTime stringParaDetetime(string data)
        {
            return DateTime.Parse(data);
        }

        private static bool TipoCnhValido(string tipo_cnh)
        {
            switch (tipo_cnh.ToLower())
            {
                case "a":
                    return true;
                case "b":
                    return true;
                case "ab":
                    return true;
                case "a+b":
                    return true;
                default:
                    return false;
            }
        }

        private static float ValorDoPlano(int plano)
        {
            switch (plano)
            {
                case 7:
                    return 30.00F;
                case 15:
                    return 28.00F;
                case 30:
                    return 22.00F;
                case 45:
                    return 20.00F;
                case 50:
                    return 18.00F;
                default:
                    return -1.00F;
            }
        }

        private static void CriaPastaDeImagensEntregadores()
        {
            if (!Directory.Exists(_diretorio_completo))
                Directory.CreateDirectory(_diretorio_completo);
        }

        private static string SalvarImagemEntregadorCnh(string entregador_id, byte[] imagem_cnh)
        {
            CriaPastaDeImagensEntregadores();

            string nome_arquivo = $"{entregador_id}.png";
            string nome_pasta = Path.Combine(_diretorio_completo, nome_arquivo);

            System.IO.File.WriteAllBytes(nome_pasta, imagem_cnh);

            string nome_pasta_relativa = Path.Combine(_diretorio_imagens, nome_arquivo);

            return nome_pasta_relativa;
        }

        public bool PostMotos(Moto moto)
        {
            try
            {
                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                string insereMotoQuery = @"
                INSERT INTO Motos (IDENTIFICADOR, ANO, MODELO, PLACA)
                VALUES (@IDENTIFICADOR, @ANO, @MODELO, @PLACA);
            ";

                var comando = new NpgsqlCommand(insereMotoQuery, conexao);

                comando.Parameters.AddWithValue("IDENTIFICADOR", moto.identificador);
                comando.Parameters.AddWithValue("ANO", moto.ano);
                comando.Parameters.AddWithValue("MODELO", moto.modelo);
                comando.Parameters.AddWithValue("PLACA", moto.placa);

                comando.ExecuteNonQuery();

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro no banco de dados ao cadastrar moto: {e.Message}");
                return false;
            }

            return true;
        }

        public List<Moto> GetMotos(string placa_moto)
        {
            var motos_lista = new List<Moto>();

            try
            {
                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                string getMotosPelaPlaca = @"
                    SELECT * FROM Motos
                    WHERE PLACA=@PLACA;
                ";

                var comando = new NpgsqlCommand(getMotosPelaPlaca, conexao);

                comando.Parameters.AddWithValue("PLACA", placa_moto);

                var leitor = comando.ExecuteReader();

                while (leitor.Read())
                {
                    var moto = new Moto
                    (
                        leitor.GetString(leitor.GetOrdinal("IDENTIFICADOR")),
                        leitor.GetInt32(leitor.GetOrdinal("ANO")),
                        leitor.GetString(leitor.GetOrdinal("MODELO")),
                        leitor.GetString(leitor.GetOrdinal("PLACA"))
                    );

                    motos_lista.Add(moto);
                }

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DATABASE] Erro: Ao pegar moto usando a placa: {e.Message}");
            }

            return motos_lista;
        }

        public bool PutMotos(string id, string placa)
        {
            try
            {
                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                string putAtualizacaoPlacaMotoQuery = @"
                    UPDATE Motos
                    SET PLACA=@PLACA
                    WHERE IDENTIFICADOR=@IDENTIFICADOR;
                ";

                var comando = new NpgsqlCommand(putAtualizacaoPlacaMotoQuery, conexao);

                comando.Parameters.AddWithValue("PLACA", placa);
                comando.Parameters.AddWithValue("IDENTIFICADOR", id);

                comando.ExecuteNonQuery();

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DATABASE] Erro: Ao atualizar a placa da moto: {e.Message}");

                return false;
            }

            return true;
        }

        public Moto? GetMotosID(string id)
        {
            try
            {
                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                string getMotoIDQuery = @"
                    SELECT * FROM Motos
                    WHERE IDENTIFICADOR=@ID;
                ";

                var comando = new NpgsqlCommand(getMotoIDQuery, conexao);

                comando.Parameters.AddWithValue("ID", id);

                var leitor = comando.ExecuteReader();

                if (leitor.Read())
                {
                    Moto moto = new Moto
                    (
                        leitor.GetString(leitor.GetOrdinal("IDENTIFICADOR")),
                        leitor.GetInt32(leitor.GetOrdinal("ANO")),
                        leitor.GetString(leitor.GetOrdinal("MODELO")),
                        leitor.GetString(leitor.GetOrdinal("PLACA"))
                    );

                    conexao.Close();

                    return moto;
                }

                conexao.Close();

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DATABASE] Erro: Ao pegar moto usando o ID: {e.Message}");

                return null;
            }
        }

        public bool DeleteMoto(string id)
        {
            try
            {
                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                string deleteMotoQuery = @"
                    DELETE FROM Motos
                    WHERE IDENTIFICADOR=@ID;
                ";

                var comando = new NpgsqlCommand(deleteMotoQuery, conexao);

                comando.Parameters.AddWithValue("ID", id);

                comando.ExecuteNonQuery();

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DATABASE] Erro: Ao deletar moto usando o ID: {e.Message}");

                return false;
            }

            return true;
        }

        public bool PostEntregador(Entregador entregador)
        {
            if (!TipoCnhValido(entregador.tipo_cnh))
                return false;

            try
            {
                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                var data_nascimento_entregador = stringParaDetetime(entregador.data_nascimento);

                string? pasta_relativa = null;

                if (!string.IsNullOrEmpty(entregador.imagem_cnh))
                {
                    byte[] imagemBytes = Convert.FromBase64String(entregador.imagem_cnh);

                    pasta_relativa = SalvarImagemEntregadorCnh(entregador.identificador, imagemBytes);
                }

                if (pasta_relativa != null)
                {
                    string? postNovoEntregadorQuery = @"
                        INSERT INTO Entregadores (IDENTIFICADOR, NOME, CNPJ, DATA_NASCIMENTO,NUMERO_CNH, TIPO_CNH, IMAGEM_CNH) 
                        VALUES (@IDENTIFICADOR, @NOME, @CNPJ, @DATA_NASCIMENTO, @NUMERO_CNH, @TIPO_CNH, @IMAGEM_CNH);
                    ";

                    var comando = new NpgsqlCommand(postNovoEntregadorQuery, conexao);

                    comando.Parameters.AddWithValue("IDENTIFICADOR", entregador.identificador);
                    comando.Parameters.AddWithValue("NOME", entregador.nome);
                    comando.Parameters.AddWithValue("CNPJ", entregador.cnpj);
                    comando.Parameters.AddWithValue("DATA_NASCIMENTO", data_nascimento_entregador);
                    comando.Parameters.AddWithValue("NUMERO_CNH", entregador.numero_cnh);
                    comando.Parameters.AddWithValue("TIPO_CNH", entregador.tipo_cnh);
                    comando.Parameters.AddWithValue("IMAGEM_CNH", pasta_relativa);

                    comando.ExecuteNonQuery();
                }
                else
                {
                    string? postNovoEntregadorQuery = @"
                        INSERT INTO Entregadores (IDENTIFICADOR, NOME, CNPJ, DATA_NASCIMENTO,NUMERO_CNH, TIPO_CNH) 
                        VALUES (@IDENTIFICADOR, @NOME, @CNPJ, @DATA_NASCIMENTO, @NUMERO_CNH, @TIPO_CNH);
                    ";

                    var comando = new NpgsqlCommand(postNovoEntregadorQuery, conexao);

                    comando.Parameters.AddWithValue("IDENTIFICADOR", entregador.identificador);
                    comando.Parameters.AddWithValue("NOME", entregador.nome);
                    comando.Parameters.AddWithValue("CNPJ", entregador.cnpj);
                    comando.Parameters.AddWithValue("DATA_NASCIMENTO", data_nascimento_entregador);
                    comando.Parameters.AddWithValue("NUMERO_CNH", entregador.numero_cnh);
                    comando.Parameters.AddWithValue("TIPO_CNH", entregador.tipo_cnh);

                    comando.ExecuteNonQuery();
                }

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DATABASE] Erro: Ao adicionar novo entregador: {e.Message}");

                return false;
            }

            return true;
        }

        public bool PostEntregadorEnviaFotoCNH(string id, string imagem_cnh)
        {
            Entregador? entregador = this.GetEntregador(id);

            if (entregador == null)
                return false;

            try
            {
                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                byte[] imagemBytes = Convert.FromBase64String(imagem_cnh);

                string pasta_relativa = SalvarImagemEntregadorCnh(id, imagemBytes);

                string postEntregadorEnviaFotoCNHQuery = @"
                    UPDATE Entregadores
                    SET IMAGEM_CNH=@IMAGEM_CNH
                    WHERE IDENTIFICADOR=@ID;
                ";

                var comando = new NpgsqlCommand(postEntregadorEnviaFotoCNHQuery, conexao);

                comando.Parameters.AddWithValue("IMAGEM_CNH", pasta_relativa);
                comando.Parameters.AddWithValue("ID", id);

                comando.ExecuteNonQuery();

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DATABASE] Erro: Ao cadastrar foto do entregador: {e.Message}");

                return false;
            }

            return true;
        }

        private Entregador? GetEntregador(string id)
        {
            try
            {
                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                string getEntregadorQuery = @"
                    SELECT * FROM Entregadores
                    WHERE IDENTIFICADOR=@ID;
                ";

                var comando = new NpgsqlCommand(getEntregadorQuery, conexao);

                comando.Parameters.AddWithValue("ID", id);

                var leitor = comando.ExecuteReader();

                if (leitor.Read())
                {
                    Entregador entregador = new Entregador(
                        leitor.GetString(leitor.GetOrdinal("IDENTIFICADOR")),
                        leitor.GetString(leitor.GetOrdinal("NOME")),
                        leitor.GetString(leitor.GetOrdinal("CNPJ")),
                        leitor.GetDateTime(leitor.GetOrdinal("DATA_NASCIMENTO")).ToString(_formato),
                        leitor.GetString(leitor.GetOrdinal("NUMERO_CNH")),
                        leitor.GetString(leitor.GetOrdinal("TIPO_CNH"))
                    );

                    conexao.Close();

                    return entregador;
                }

                conexao.Close();

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DATABASE] Erro: Ao pegar entregador usando o ID: {e.Message}");

                return null;
            }
        }

        public bool PostLocacao(Locacao locacao)
        {
            try
            {
                Entregador? entregador = this.GetEntregador(locacao.entregador_id);

                if (entregador == null)
                    return false;
                else if (entregador.tipo_cnh.ToLower() == "b")
                { return false; }

                DateTime data = DateTime.Parse(locacao.data_inicio, null, System.Globalization.DateTimeStyles.RoundtripKind);
                DateTime dataMaisUmDia = data.AddDays(1);

                string novaDataMaisUmDia = dataMaisUmDia.ToString(_formato);

                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                string postLotacaoQuery = @"
                    INSERT INTO Locacoes (ENTREGADOR_ID, MOTO_ID, DATA_INICIO, DATA_TERMINO, DATA_PREVISAO_TERMINO, PLANO, VALOR_PLANO)
                    VALUES (@ENTREGADOR_ID, @MOTO_ID, @DATA_INICIO, @DATA_TERMINO, @DATA_PREVISAO_TERMINO, @PLANO, @VALOR_PLANO);
                ";

                var comando = new NpgsqlCommand(postLotacaoQuery, conexao);

                comando.Parameters.AddWithValue("ENTREGADOR_ID", locacao.entregador_id);
                comando.Parameters.AddWithValue("MOTO_ID", locacao.moto_id);
                comando.Parameters.AddWithValue("DATA_INICIO", stringParaDetetime(novaDataMaisUmDia));
                comando.Parameters.AddWithValue("DATA_TERMINO", stringParaDetetime(locacao.data_termino));
                comando.Parameters.AddWithValue("DATA_PREVISAO_TERMINO", stringParaDetetime(locacao.data_previsao_termino));
                comando.Parameters.AddWithValue("PLANO", locacao.plano);
                comando.Parameters.AddWithValue("VALOR_PLANO", ValorDoPlano(locacao.plano));

                comando.ExecuteNonQuery();

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DATABASE] Erro: Ao cadastrar uma nova locacção: {e.Message}");

                return false;
            }

            return true;
        }

        public LocacaoRetorno? GetLocacao(string id)
        {
            try
            {
                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                string getLotacaoQuery = @"
                    SELECT * FROM Locacoes
                    WHERE IDENTIFICADOR=@ID;
                ";

                var comando = new NpgsqlCommand(getLotacaoQuery, conexao);

                comando.Parameters.AddWithValue("ID", id);

                var leitor = comando.ExecuteReader();

                if (leitor.Read())
                {
                    var identificador = leitor.GetString(leitor.GetOrdinal("IDENTIFICADOR"));
                    var valor_plano = leitor.GetFloat(leitor.GetOrdinal("VALOR_PLANO"));

                    var entregador_id = leitor.GetString(leitor.GetOrdinal("ENTREGADOR_ID"));
                    var moto_id = leitor.GetString(leitor.GetOrdinal("MOTO_ID"));
                    var data_inicio = leitor.GetDateTime(leitor.GetOrdinal("DATA_INICIO")).ToString(_formato);
                    var data_termino = leitor.GetDateTime(leitor.GetOrdinal("DATA_TERMINO")).ToString(_formato);
                    var data_previsao_termino = leitor.GetDateTime(leitor.GetOrdinal("DATA_PREVISAO_TERMINO")).ToString(_formato);
                    var plano = leitor.GetInt32(leitor.GetOrdinal("PLANO"));
                    string? data_devolucao = leitor.IsDBNull(leitor.GetOrdinal("DATA_DEVOLUCAO"))
                                                ? null
                                                : leitor.GetDateTime(leitor.GetOrdinal("DATA_DEVOLUCAO")).ToString(_formato);

                    LocacaoRetorno locacaoExiste = new LocacaoRetorno(
                        identificador,
                        valor_plano,
                        entregador_id,
                        moto_id,
                        data_inicio,
                        data_termino,
                        data_previsao_termino,
                        data_devolucao
                    );

                    conexao.Close();

                    return locacaoExiste;
                }

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DATABASE] Erro: Ao consultar uma locação: {e.Message}");
            }

            return null;
        }

        private LocacaoCompleta? GetTodosDadosLocacao(string id)
        {
            try
            {
                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                string getTodasInformacoesLocacaoQuery = @"
                    SELECT * FROM Locacoes
                    WHERE IDENTIFICADOR=@IDENTIFICADOR;
                ";

                var comando = new NpgsqlCommand(getTodasInformacoesLocacaoQuery, conexao);

                comando.Parameters.AddWithValue("IDENTIFICADOR", id);

                var leitor = comando.ExecuteReader();

                if (leitor.Read())
                {
                    string? data_devolucao = leitor.IsDBNull(leitor.GetOrdinal("DATA_DEVOLUCAO"))
                            ? null
                            : leitor.GetDateTime(leitor.GetOrdinal("DATA_DEVOLUCAO")).ToString(_formato);

                    if (data_devolucao == null)
                        return null;

                    var identificador = leitor.GetString(leitor.GetOrdinal("IDENTIFICADOR")).ToString();
                    var plano = leitor.GetInt32(leitor.GetOrdinal("PLANO"));
                    var valor_diaria = leitor.GetFloat(leitor.GetOrdinal("VALOR_PLANO"));
                    var data_inicio = leitor.GetDateTime(leitor.GetOrdinal("DATA_INICIO")).ToString(_formato);
                    var data_termino = leitor.GetDateTime(leitor.GetOrdinal("DATA_TERMINO")).ToString(_formato);
                    var data_previsao_termino = leitor.GetDateTime(leitor.GetOrdinal("DATA_PREVISAO_TERMINO")).ToString(_formato);

                    LocacaoCompleta locacao = new LocacaoCompleta(
                        identificador,
                        plano,
                        valor_diaria,
                        data_inicio,
                        data_termino,
                        data_previsao_termino,
                        data_devolucao
                    );

                    conexao.Close();

                    return locacao;
                }

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DATABASE] Erro: Ao pegar todas as informações de locação: {e.Message}");
            }

            return null;
        }

        private static float CalculaValorTotal(int plano, float valor_diaria, string data_inicio, string data_devolucao, string data_previsao_termino)
        {
            DateTime dataInicio = DateTime.Parse(data_inicio);
            DateTime dataDevolucao = DateTime.Parse(data_devolucao);
            DateTime dataPrevisaoTermino = DateTime.Parse(data_previsao_termino);

            int diasTotaisPrevistos = (dataPrevisaoTermino - dataInicio).Days;
            int diasTotaisRealizados = (dataDevolucao - dataInicio).Days;

            if (diasTotaisRealizados < 0)
                throw new ArgumentException("A data de devolução não pode ser anterior à data de início.");

            float valorTotal;

            if (dataDevolucao < dataPrevisaoTermino) // Devolução antecipada
            {
                int diasNaoEfetivados = (dataPrevisaoTermino - dataDevolucao).Days;
                valorTotal = diasTotaisRealizados * valor_diaria;

                float multaPorcentagem = plano == 7 ? 0.20f : plano == 15 ? 0.40f : 0;

                if (multaPorcentagem > 0)
                {
                    float valorMulta = diasNaoEfetivados * valor_diaria * multaPorcentagem;
                    valorTotal += valorMulta;
                }
            }
            else if (dataDevolucao > dataPrevisaoTermino) // Devolução atrasada
            {
                int diasAdicionais = (dataDevolucao - dataPrevisaoTermino).Days;
                valorTotal = diasTotaisPrevistos * valor_diaria + diasAdicionais * 50;
            }
            else // Devolução na data prevista
                valorTotal = diasTotaisPrevistos * valor_diaria;

            return valorTotal;
        }


        public bool PutLocacao(string id, string data_devolucao)
        {
            try
            {
                var conexao = new NpgsqlConnection(this._conexaoString);
                conexao.Open();

                string postLotacaoQuery = @"
                    UPDATE Locacoes
                    SET DATA_DEVOLUCAO=@DATA_DEVOLUCAO
                    WHERE IDENTIFICADOR=@IDENTIFICADOR;
                ";

                var comando = new NpgsqlCommand(postLotacaoQuery, conexao);

                comando.Parameters.AddWithValue("DATA_DEVOLUCAO", stringParaDetetime(data_devolucao));
                comando.Parameters.AddWithValue("IDENTIFICADOR", id);

                comando.ExecuteNonQuery();

                conexao.Close();

                LocacaoCompleta? locacao = this.GetTodosDadosLocacao(id);

                if (locacao != null)
                {
                    if (locacao.data_devolucao == null)
                        return false;

                    float valor_total = CalculaValorTotal(locacao.plano, locacao.valor_diaria, locacao.data_inicio, locacao.data_devolucao, locacao.data_previsao_termino);

                    Console.WriteLine($"[DATABASE] Total a pagar pelo entregador: {valor_total}");
                }
                else
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DATABASE] Erro: Ao alterar data de devolução de uma locação: {e.Message}");
            }

            return false;
        }
    }
}
