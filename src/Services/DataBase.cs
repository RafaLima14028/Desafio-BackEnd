using Npgsql;

using Models.Moto;
using Models.Entregador;
using Models.Locacao;

namespace Services.DataBase
{
    public class DataBase
    {
        private static string formato = "yyyy-MM-ddTHH:mm:ssZ";

        private static DataBase? database_instance; // Singleton instance

        private string conexaoString;

        private DataBase()
        {
            this.conexaoString = "Host=localhost;Port=5432;Database=motoDB;Username=postgres;Password=1234";

            var conexao = new NpgsqlConnection(this.conexaoString);
            conexao.Open();

            string criaTabelaMotosQuery = @"
                CREATE TABLE IF NOT EXISTS Motos (
                    IDENTIFICADOR VARCHAR(100) PRIMARY KEY,
                    ANO INT NOT NULL,
                    MODELO VARCHAR(100) NOT NULL,
                    PLACA VARCHAR(20) UNIQUE NOT NULL
                );
            ";

            var comando = new NpgsqlCommand(criaTabelaMotosQuery, conexao);
            comando.ExecuteNonQuery();

            string criaTabelaEntregadoresQuery = @"
                CREATE TABLE IF NOT EXISTS Entregadores (
                    IDENTIFICADOR VARCHAR(100) PRIMARY KEY,
                    NOME VARCHAR(100) NOT NULL,
                    CNPJ VARCHAR(50) NOT NULL,
                    DATA_NASCIMENTO TIMESTAMP NOT NULL,
                    NUMERO_CNH VARCHAR(50) NOT NULL,
                    TIPO_CNH VARCHAR(20) NOT NULL,
                    IMAGEM_CNH VARCHAR(100)
                );
            ";

            comando = new NpgsqlCommand(criaTabelaEntregadoresQuery, conexao);
            comando.ExecuteNonQuery();

            string criaTabelaLotacaoQuery = @"
                CREATE TABLE IF NOT EXISTS Locacoes (
                    ENTREGADOR_ID VARCHAR(100) NOT NULL,
                    MOTO_ID VARCHAR(100) NOT NULL,
                    DATA_INICIO TIMESTAMP NOT NULL,
                    DATA_TERMINO TIMESTAMP NOT NULL,
                    DATA_PREVISAO_TERMINO TIMESTAMP NOT NULL,
                    PLANO INTEGER NOT NULL,
                    DATA_DEVOLUCAO TIMESTAMP,

                    CONSTRAINT FK_ENTREGADOR_ID FOREIGN KEY (ENTREGADOR_ID) REFERENCES Entregadores (IDENTIFICADOR),
                    CONSTRAINT FK_MOTO FOREIGN KEY (MOTO_ID) REFERENCES Motos (IDENTIFICADOR)
                );
            ";

            comando = new NpgsqlCommand(criaTabelaLotacaoQuery, conexao);
            comando.ExecuteNonQuery();

            conexao.Close();
        }

        public static DataBase GetInstanceDataBase()
        {
            if (database_instance == null)
                database_instance = new DataBase();

            return database_instance;
        }

        private static DateTime stringParaDetetime(string data)
        {
            return DateTime.Parse(data);
        }

        public bool PostMotos(Moto moto)
        {
            try
            {
                var conexao = new NpgsqlConnection(this.conexaoString);
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
                var conexao = new NpgsqlConnection(this.conexaoString);
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
                Console.WriteLine($"Erro no banco de dados ao pegar moto usando a placa: {e.Message}");
            }

            return motos_lista;
        }

        public bool PutMotos(string id, string placa)
        {
            try
            {
                var conexao = new NpgsqlConnection(this.conexaoString);
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
                Console.WriteLine($"Erro no banco de dados ao atualizar a placa da moto: {e.Message}");

                return false;
            }

            return true;
        }

        public Moto? GetMotoID(string id)
        {
            try
            {
                var conexao = new NpgsqlConnection(this.conexaoString);
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
                Console.WriteLine($"Erro no banco de dados ao pegar moto usando o ID: {e.Message}");

                return null;
            }
        }

        public bool DeleteMoto(string id)
        {
            try
            {
                var conexao = new NpgsqlConnection(this.conexaoString);
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
                Console.WriteLine($"Erro no banco de dados ao deletar moto usando o ID: {e.Message}");

                return false;
            }

            return true;
        }

        public bool PostEntregador(Entregador entregador)
        {
            try
            {
                var conexao = new NpgsqlConnection(this.conexaoString);
                conexao.Open();

                var data_nascimento_entregador = stringParaDetetime(entregador.data_nascimento);

                if (!string.IsNullOrEmpty(entregador.imagem_cnh))
                {
                    string postNovoEntregadorQuery = @"
                        INSERT INTO Entregadores (IDENTIFICADOR, NOME, CNPJ, DATA_NASCIMENTO,NUMERO_CNH, TIPO_CNH, IMAGEM_CNH) 
                        VALUES (@IDENTIFICADOR, @NOME, @CNPJ, @DATA_NASCIMENTO, @NUMERO_CNH,@TIPO_CNH, @IMAGEM_CNH);
                    ";

                    var comando = new NpgsqlCommand(postNovoEntregadorQuery, conexao);

                    comando.Parameters.AddWithValue("IDENTIFICADOR", entregador.identificador);
                    comando.Parameters.AddWithValue("NOME", entregador.nome);
                    comando.Parameters.AddWithValue("CNPJ", entregador.cnpj);
                    comando.Parameters.AddWithValue("DATA_NASCIMENTO", data_nascimento_entregador);
                    comando.Parameters.AddWithValue("NUMERO_CNH", entregador.numero_cnh);
                    comando.Parameters.AddWithValue("TIPO_CNH", entregador.tipo_cnh);
                    comando.Parameters.AddWithValue("IMAGEM_CNH", entregador.imagem_cnh);

                    comando.ExecuteNonQuery();
                }
                else
                {
                    string postNovoEntregadorQuery = @"
                        INSERT INTO Entregadores (IDENTIFICADOR, NOME, CNPJ, DATA_NASCIMENTO,NUMERO_CNH, TIPO_CNH) 
                        VALUES (@IDENTIFICADOR, @NOME, @CNPJ, @DATA_NASCIMENTO, @NUMERO_CNH,@TIPO_CNH);
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
                Console.WriteLine($"Erro no banco de dados ao adicionar novo entregador: {e.Message}");

                return false;
            }

            return true;
        }

        public bool PostEntregadorEnviaFotoCNH(string id, string imagem_cnh)
        {
            try
            {
                var conexao = new NpgsqlConnection(this.conexaoString);
                conexao.Open();

                string postEntregadorEnviaFotoCNHQuery = @"
                    UPDATE Entregadores
                    SET IMAGEM_CNH=@IMAGEM_CNH
                    WHERE IDENTIFICADOR=@ID;
                ";

                var comando = new NpgsqlCommand(postEntregadorEnviaFotoCNHQuery, conexao);

                comando.Parameters.AddWithValue("IMAGEM_CNH", imagem_cnh);
                comando.Parameters.AddWithValue("ID", id);

                comando.ExecuteNonQuery();

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro no banco de dados ao cadastrar foto do entregador: {e.Message}");

                return false;
            }

            return true;
        }

        public bool PostLocacao(Locacao locacao)
        {
            try
            {
                var conexao = new NpgsqlConnection(this.conexaoString);
                conexao.Open();

                string postLotacaoQuery = @"
                    INSERT INTO Locacoes (ENTREGADOR_ID, MOTO_ID, DATA_INICIO, DATA_TERMINO, DATA_PREVISAO_TERMINO, PLANO)
                    VALUES (@ENTREGADOR_ID, @MOTO_ID, @DATA_INICIO, @DATA_TERMINO, @DATA_PREVISAO_TERMINO, @PLANO);
                ";

                var comando = new NpgsqlCommand(postLotacaoQuery, conexao);

                comando.Parameters.AddWithValue("ENTREGADOR_ID", locacao.entregador_id);
                comando.Parameters.AddWithValue("MOTO_ID", locacao.moto_id);
                comando.Parameters.AddWithValue("DATA_INICIO", stringParaDetetime(locacao.data_inicio));
                comando.Parameters.AddWithValue("DATA_TERMINO", stringParaDetetime(locacao.data_termino));
                comando.Parameters.AddWithValue("DATA_PREVISAO_TERMINO", stringParaDetetime(locacao.data_previsao_termino));
                comando.Parameters.AddWithValue("PLANO", locacao.plano);

                comando.ExecuteNonQuery();

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro no banco de dados ao cadastrar nova locacao: {e.Message}");

                return false;
            }

            return true;
        }

        public Locacao? GetLocacao(string id)
        {
            Locacao locacaoExiste;

            try
            {
                var conexao = new NpgsqlConnection(this.conexaoString);
                conexao.Open();

                string getLotacaoQuery = @"
                    SELECT * FROM Locacoes
                    WHERE MOTO_ID=@ID;
                ";

                var comando = new NpgsqlCommand(getLotacaoQuery, conexao);

                comando.Parameters.AddWithValue("ID", id);

                var leitor = comando.ExecuteReader();

                if (leitor.Read())
                {
                    var entregador_id = leitor.GetString(leitor.GetOrdinal("ENTREGADOR_ID"));
                    var moto_id = leitor.GetString(leitor.GetOrdinal("MOTO_ID"));
                    var data_inicio = leitor.GetDateTime(leitor.GetOrdinal("DATA_INICIO")).ToString(formato);
                    var data_termino = leitor.GetDateTime(leitor.GetOrdinal("DATA_TERMINO")).ToString(formato);
                    var data_previsao_termino = leitor.GetDateTime(leitor.GetOrdinal("DATA_PREVISAO_TERMINO")).ToString(formato);
                    var plano = leitor.GetInt32(leitor.GetOrdinal("PLANO"));
                    string? data_devolucao = leitor.IsDBNull(leitor.GetOrdinal("DATA_DEVOLUCAO"))
                                                ? null
                                                : leitor.GetDateTime(leitor.GetOrdinal("DATA_DEVOLUCAO")).ToString(formato);

                    locacaoExiste = new Locacao
                    (
                        entregador_id,
                        moto_id,
                        data_inicio,
                        data_termino,
                        data_previsao_termino,
                        plano,
                        data_devolucao
                    );

                    conexao.Close();

                    return locacaoExiste;
                }

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro no banco de dados ao cadastrar nova locacao: {e.Message}");
            }

            return null;
        }

        public bool PutLocacao(string id, string data_devolucao)
        {
            try
            {
                var conexao = new NpgsqlConnection(this.conexaoString);
                conexao.Open();

                string postLotacaoQuery = @"
                    UPDATE Locacoes
                    SET DATA_DEVOLUCAO=@DATA_DEVOLUCAO
                    WHERE MOTO_ID=@ID;
                ";

                var comando = new NpgsqlCommand(postLotacaoQuery, conexao);

                comando.Parameters.AddWithValue("DATA_DEVOLUCAO", stringParaDetetime(data_devolucao));
                comando.Parameters.AddWithValue("ID", id);

                comando.ExecuteNonQuery();

                conexao.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro no banco de dados ao cadastrar nova locacao: {e.Message}");

                return false;
            }

            return true;
        }
    }
}
