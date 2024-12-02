<h1 align="Center">Desafio Backend</h1>

<h2>Description:</h2>

This project is a backend solution that utilizes .NET and PostgreSQL to manage motorcycle rentals for delivery people. It includes integration with RabbitMQ. The architecture above allows for clear organization, separating models, services, and environment configurations.

<h2>Prerequisites:</h2>

- **Git**
- **.NET SDK**
- **PostgreSQL**
- **Erlang** (Required in Windows)
- **RabbitMQ**
- **Depenencies**:
  - **Npgsql**
  - **RabbitMQ.Client**

<h2>Implemented Requirements:</h2>

- Motos:

  - [x] POST /motos
  - [x] GET /motos
  - [x] PUT /motos/{id}/placa
  - [x] GET /motos/{id}
  - [x] DELETE /motos/{id}

- Entregadores:

  - [x] POST /entregadores
  - [x] POST /entregadores/{id}/cnh

- Locação:

  - [x] POST /locacao
  - [x] GET /locacao/{id}
  - [x] PUT /locacao/{id}/devolucao

- [x] Database
- [x] Publisher/Subscriber

<h2>How to use:</h2>

<h3>Step 1: Clone the repository:</h3>

Run in terminal:

```bash
git clone https://github.com/RafaLima14028/Desafio-BackEnd.git
cd Desafio-Backend
```

<h3>Step 2: Install dependencies</h3>

**Step 2.1: Install Npgsql (Library for PostgreSQL)**

```bash
dotnet add package Npgsql
```

**Step 2.2: Install RabbitMQ (Client for RabbitMQ)**

```bash
dotnet add package RabbitMQ.Client
```

<h3>Step 3: Configure RabbitMQ</h3>

**Windows**:

1. Download and install RabbitMQ and Erlang, if you don't have them.

2. Initialize the RabbitMQ service:

```bash
rabbitmq-plugins enable rabbitmq_managments
```

3. Access the managment panel available: [http://localhost:15672](http://localhost:15672).

4. The default username and password are:

```bash
login: guest
password: guest
```

**Linux**:

1. Install RabbitMQ with the following command:

```bash
sudo apt update
sudo apt install rabbitmq-server
```

2. Enable and start the service:

```bash
sudo systemctl enable rabbitmq-server
sudo systemctl start rabbitmq-server
```

3. Enable the management plugin:

```bash
sudo rabbitmq-plugins enable rabbitmq_management
```

4. Access the managment panel available: [http://localhost:15672](http://localhost:15672).

5. The default username and password are:

```bash
login: guest
password: guest
```

**Mac OS**:

1. Use Homebrew to install RabbitMQ:

```bash
brew install rabbitmq
```

2. Start the service:

```bash
brew services start rabbitmq
```

3. Activate the management plugin:

```bash
rabbitmq-plugins enable rabbitmq_management
```

4. Access the managment panel available: [http://localhost:15672](http://localhost:15672).

5. The default username and password are:

```bash
login: guest
password: guest
```

<h3>Step 4: Run the project</h3>

Open the directory and run:

```bash
dotnet run
```

<h2>API Reference:</h2>

<h3>Endpoints:</h3>

<h4>Add Motorcycle:</h4>

- **URL:** `http://localhost:5101/motos`
- **HTTP Method:** `POST`
- **Parameters:**

  - **Header:**
    - `Content-Type`: `application/json`
  - **Body:** (JSON)

  ```json
  {
    "identificador": "moto123",
    "ano": 2024,
    "modelo": "Mottu Sport",
    "placa": "CDX-0101"
  }
  ```

- **Reponses:**

  - **201 Created:** It has no body.

  - **400 Bad Resquest:**

  ```json
  {
    "mensagem": "Dados inválidos"
  }
  ```

<h4>Get Motorcycle:</h4>

- **URL:** `http://localhost:5101/motos`
- **HTTP Method:** `GET`
- **Parameters:**

  - **Header:**
    - `Content-Type`: `application/json`
  - **Body:** (JSON)

  ```json
  {
    "placa": "CDX-0101"
  }
  ```

- **Reponses:**

  - **200 Ok:**

  ```json
  [
    {
      "identificador": "moto123",
      "ano": 2020,
      "modelo": "Mottu Sport",
      "placa": "CDX-0101"
    }
  ]
  ```

  - **400 Bad Request:**

  ```json
  {
    "mensagem": "Dados inválidos"
  }
  ```

<h4>Modify Motorcycle Plate:</h4>

- **URL:** `http://localhost:5101/motos/{id}/placa`
- **HTTP Method:** `PUT`
- **Parameters:**

  - **Header:**
    - `Content-Type`: `application/json`
    - `id`: `string` (ID of the motorcycle that will be modified)
  - **Body:** (JSON)

  ```json
  {
    "placa": "ABC-1234"
  }
  ```

- **Reponses:**

  - **200 Ok:**

  ```json
  {
    "mensagem": "Placa modificada com sucesso"
  }
  ```

  - **400 Bad Resquest:**

  ```json
  {
    "mensagem": "Dados inválidos"
  }
  ```

<h4>Get Motorcycle ID:</h4>

- **URL:** `http://localhost:5101/motos/{id}`
- **HTTP Method:** `GET`
- **Parameters:**

  - **Header:**
    - `Content-Type`: `application/json`
    - `id`: `string` (ID of the motorcycle that will be consulted)
  - **Body:** It has no body

- **Reponses:**

  - **200 Ok:**

  ```json
  {
    "identificador": "moto123",
    "ano": 2020,
    "modelo": "Mottu Sport",
    "placa": "CDX-0101"
  }
  ```

  - **400 Bad Request:**

  ```json
  {
    "mensagem": "Request mal formado"
  }
  ```

  - **404 Not Found:**

  ```json
  {
    "mensagem": "Moto não encontrada"
  }
  ```

<h4>Delete Motorcycle:</h4>

- **URL:** `http://localhost:5101/motos/{id}`
- **HTTP Method:** `DELETE`
- **Parameters:**

  - **Header:**
    - `Content-Type`: `application/json`
    - `id`: `string` (ID of the motorcycle that will be consulted)
  - **Body:** It has no body

- **Reponses:**

  - **200 Ok:** It has no body

  - **400 Bad Request:**

  ```json
  {
    "mensagem": "Dados inválidos"
  }
  ```

<h4>Add Deliveryman:</h4>

- **URL:** `http://localhost:5101/entregadores`
- **HTTP Method:** `POST`
- **Parameters:**

  - **Header:**
    - `Content-Type`: `application/json`
  - **Body:** (JSON)

  ```json
  {
    "identificador": "entregador123",
    "nome": "João da Silva",
    "cnpj": "12345678901234",
    "data_nascimento": "1990-01-01T00:00:00Z",
    "numero_cnh": "12345678900",
    "tipo_cnh": "A",
    "imagem_cnh": "base64string"
  }
  ```

- **Reponses:**

  - **201 Created:** It has no body

  - **400 Bad Request:**

  ```json
  {
    "mensagem": "Dados inválidos"
  }
  ```

<h4>Add Photo to Deliveryman:</h4>

- **URL:** `http://localhost:5101/entregadores/{id}/cnh`
- **HTTP Method:** `POST`
- **Parameters:**

  - **Header:**
    - `Content-Type`: `application/json`
    - `id`: `string` (ID of the deliveryman that will be changed)
  - **Body:** (JSON)

  ```json
  {
    "imagem_cnh": "base64string"
  }
  ```

- **Reponses:**

  - **201 Created:** It has no body

  - **400 Bad Request:**

  ```json
  {
    "mensagem": "Dados inválidos"
  }
  ```

<h4>Add Rental:</h4>

- **URL:** `http://localhost:5101/locacao`
- **HTTP Method:** `POST`
- **Parameters:**

  - **Header:**
    - `Content-Type`: `application/json`
  - **Body:** (JSON)

  ```json
  {
    "entregador_id": "entregador123",
    "moto_id": "moto123",
    "data_inicio": "2024-01-01T00:00:00Z",
    "data_termino": "2024-01-07T23:59:59Z",
    "data_previsao_termino": "2024-01-07T23:59:59Z",
    "plano": 7
  }
  ```

- **Reponses:**

  - **201 Created:** It has no body

  - **400 Bad Request:**

  ```json
  {
    "mensagem": "Dados inválidos"
  }
  ```

<h4>Get Rental:</h4>

- **URL:** `http://localhost:5101/locacao/{id}`
- **HTTP Method:** `GET`
- **Parameters:**

  - **Header:**
    - `Content-Type`: `application/json`
    - `id`: `string` (ID of the rental that will be consulted)
  - **Body:** It has no body

- **Reponses:**

  - **200 Ok:**

  ```json
  {
    "identificador": "locacao123",
    "valor_diaria": 10,
    "entregador_id": "entregador123",
    "moto_id": "moto123",
    "data_inicio": "2024-01-01T00:00:00Z",
    "data_termino": "2024-01-07T23:59:59Z",
    "data_previsao_termino": "2024-01-07T23:59:59Z",
    "data_devolucao": "2024-01-07T18:00:00Z"
  }
  ```

  - **400 Bad Request:**

  ```json
  {
    "mensagem": "Dados inválidos"
  }
  ```

  - **404 Not Found:**

  ```json
  {
    "mensagem": "Locação não encontrada"
  }
  ```

<h4>Add Rental Return Date:</h4>

- **URL:** `http://localhost:5101/locacao/{id}/devolucao`
- **HTTP Method:** `GET`
- **Parameters:**

  - **Header:**
    - `Content-Type`: `application/json`
    - `id`: `string` (ID of the rental that will be consulted)
  - **Body:** (JSON)

  ```json
  {
    "data_devolucao": "2024-01-07T18:00:00Z"
  }
  ```

- **Reponses:**

  - **200 Ok:**

  ```json
  {
    "mensagem": "Data de devolução informada com sucesso"
  }
  ```

  - **400 Bad Request:**

  ```json
  {
    "mensagem": "Dados inválidos"
  }
  ```

<h2>Messaging with RabbitMQ:</h2>

<h3>Publisher:</h3>

- <h4>Mensage Flow:</h4>

  - The `Publisher` class utilizes RabbitMQ for message exchange. RabbitMQ is a message broker that ensures reliable communication between distributed services. In this implementation:

  1. **Queue Declaration:**

     - Two queues are created: -`_nome_file` (`MotosEntregadoresLocacoes`): Used for sending messages to the system. -`_nome_file_respostas` (`MotosEntregadoresLocacoesRespostas`): Used for reciving responses from consumers.

     - These queues are declared with properties: `durable: false`, `exclusive: false`, and `autoDelete: false`, meaning they are not durable, not restricted to a single connection, and will not auto-delete when unused.

  2. **Message Sending:**

     - The `EnviaMensagem` method sends serialized JSON messages to the `_nome_fila` queue.

     - Each message contains operation details, including the action type (e.g., `postMotos`, `getMotos`), as well as specific data depending on the operation.

  3. **Response Handling:**

     - Responses are received asynchronously via the `_nome_fila_respostas` queue using an `AsyncEventingBasicConsumer`.

     - The response is processed, deserialized, and returned to the requesting method, enabling synchronous-like behavior despite asynchronous operations.

- <h4>Message Structure:</h4>

  - Messages exchanged between the `Publisher` and the RabbitMQ broker follow a structured JSON format. Below are examples of message formats for different operations:

    1.  **Motorcycle Registration** (`postMotos`):

        ```json
        {
          "identificador": "moto123",
          "ano": 2024,
          "modelo": "Mottu Sport",
          "placa": "ABC-1234",
          "funcao": "postMotos"
        }
        ```

    2.  **Query Motos** (`getMotos`):

        ```json
        {
          "placa": "ABC-1234",
          "funcao": "getMotos"
        }
        ```

    3.  **Update Moto License Plate** (`putMotos`):

        ```json
        {
          "identificador_moto": "moto123",
          "placa": "ABC-1234",
          "funcao": "putMotos"
        }
        ```

    4.  **Update Moto License Plate** (`postEntregadores`):

        ```json
        {
          "identificador": "entregador123",
          "nome": "João da Silva",
          "cnpj": "123",
          "data_nascimento": "1990-01-01T00:00:00Z",
          "numero_cnh": "123",
          "tipo_cnh": "A",
          "imagem_cnh": "base64string",
          "funcao": "postEntregadores"
        }
        ```

  Each message includes a funcao field specifying the intended operation for easy identification by the consumer.

- <h4>RabbitMQ Configuration:</h4>

  1. **Connection Initialization:**

     - The `Publisher` uses a `ConnectionFactory` with the following default configuration:

       ```csharp
         new ConnectionFactory()
         {
           HostName = "localhost"
         };
       ```

     - This assumes RabbitMQ is running locally. For remote servers, update the `HostName` and add credentials as needed.

  2. **Queue Declaration:**

     - Queues are declared using `QueueDeclareAsync`. Example:

       ```csharp
       await this._channel.QueueDeclareAsync(
         queue: "MotosEntregadoresLocacoes",
         durable: false,
         exclusive: false,
         autoDelete: false,
         arguments: null
       );
       ```

     - Ensure the queues exist in RabbitMQ before sending or consuming messages.

  3. **Message Publishing:**

     - Messages are published with `BasicPublishAsync`. Example:
       ```csharp
       await channel.BasicPublishAsync(
         exchange: string.Empty,
         routingKey: "MotosEntregadoresLocacoes",
         body: mensagem_bytes
       );
       ```

  4. **Response Consumption:**

     - The `_nome_fila_respostas` queue is consumed using an `AsyncEventingBasicConsumer`:

       ```csharp
       var consumer = new AsyncEventingBasicConsumer(this._channel_response);

       consumer.ReceivedAsync += async (model, ea) =>
       {
         var resposta = Encoding.UTF8.GetString(ea.Body.ToArray());
         this._tcs.TrySetResult(resposta);
         await Task.CompletedTask;
       };

       await this._channel_response.BasicConsumeAsync(
         "MotosEntregadoresLocacoesRespostas",
         true,
         consumer
       );
       ```

<h3>Subscriber:</h3>

- <h4>Message Flow:</h4>

  - The `Subscriber` class leverages RabbitMQ as a message broker for inter-service communication. Here’s how the flow works:

    1. **Queue Declaration:**

       - The `Subscriber` declares two queues:
         - `_nome_fila` (`MotosEntregadoresLocacoes`) for receiving requests.
         - `_nome_fila_respostas` (`MotosEntregadoresLocacoesRespostas`) for sending resposes.

    2. **Listening to Messages:**

       - The `Subscriber` sets up an asynchronous consumer (`AsyncEventingBasicConsumer`) to listen for messages in the `_nome_fila`.

       - When a message is received, the content is deserialized into JSON and processed based on the `funcao` (function) field, which determines the type of operation to be executed.

    3. **Message Processing:**

       - For each `funcao` value, the `Subscriber` invokes a corresponding method, such as `PostMotos`, `GetMotos`, or `PutLocacao`. These methods interact with the database and may prepare a response for the `_nome_fila_respostas` queue.

    4. **Response Publishing:**

       - For operations requiring responses (e.g., `GetMotos`), the processed data is serialized into JSON and sent to the `_nome_fila_respostas` queue. Other services can then consume this response.

- <h4>Message Structure:</h4>

  - The messages exchanged between the `Subscriber` and RabbitMQ follow a JSON format. Below is the general structure:

    1. **Request Message:**

       ```json
       {
         "funcao": "string", // The operation to execute (e.g., "postmotos", "getmotos").
         "field1": "value1", // Operation-specific fields.
         "field2": "value2"
       }
       ```

       - Example for creating a new motorcycle:

       ```json
       {
         "funcao": "postmotos",
         "identificador": "moto123",
         "ano": 2024,
         "modelo": "Honda CB500",
         "placa": "XYZ-1234"
       }
       ```

    2. **Response Message:**

       ```json
       [
         {
           "field1": "value1",
           "field2": "value2"
         }
       ]
       ```

       - Example for fetching motorcycles:

       ```json
       [
         {
           "identificador": "moto123",
           "ano": 2024,
           "modelo": "Honda CB500",
           "placa": "XYZ-1234"
         }
       ]
       ```

<h4>RabbitMQ Configuration:</h4>

- To ensure proper functioning of the `Subscriber`, RabbitMQ must be configured as follows:

  1. **Connection Setup:**

     - The ConnectionFactory is initialized with the RabbitMQ host:
       ```csharp
       this._connectionFactory = new ConnectionFactory
       {
           HostName = "localhost"
       };
       ```

  2. **Queue Declaration:**

     - The queues for receiving and sending messages are declared:

       ```csharp
       await this._channel.QueueDeclareAsync(
          queue: "MotosEntregadoresLocacoes",
          durable: false,
          exclusive: false,
          autoDelete: false,
          arguments: null
       );

       await this._channel_response.QueueDeclareAsync(
          queue: "MotosEntregadoresLocacoesRespostas",
          durable: false,
          exclusive: false,
          autoDelete: false,
          arguments: null
       );
       ```

  3. **Consumer Configuration:**

     - An asynchronous consumer listens for messages from the queue:

       ```csharp
       var consumer = new AsyncEventingBasicConsumer(this._channel);
       consumer.ReceivedAsync += async (model, ea) =>
       {
         var body = ea.Body.ToArray();
         var message = Encoding.UTF8.GetString(body);
         // Message processing logic here
       };

       await this._channel.BasicConsumeAsync(
         queue: "MotosEntregadoresLocacoes",
         autoAck: true,
         consumer: consumer
       );
       ```

  4. **Message Publishing:**

     - Responses are sent back to the designated response queue:

       ```csharp
       await this._channel_response.BasicPublishAsync(
         exchange: string.Empty,
         routingKey: "MotosEntregadoresLocacoesRespostas",
         body: Encoding.UTF8.GetBytes(responseMessage)
       );
       ```

<h2>Database:</h2>

The database has the following structure:

1. **Motos Table:**

   - **IDENTIFICADOR** (VARCHAR, PRIMARY KEY): Unique identifier for each motorcycle.
   - **ANO**(INT): Year of the motorcycle.
   - **MODELO** (VARCHAR): Model of the motorcycle.
   - **PLACA** (VARCHAR, UNIQUE): License plate number.

2. **Entregador Table:**

   - **IDENTIFICADOR** (VARCHAR, PRIMARY KEY): Unique identifier for each delivery person.
   - **NOME**(VARCHAR): Name og the delivery person.
   - **CNPJ** (VARCHAR, UNIQUE): Unique identification number.
   - **DATA_NASCIMENTO** (TIMESTAMP): Date of birthday.
   - **NUMERO_CNH** (VARCHAR, UNIQUE): Driver's license number.
   - **TIPO_CNH** (VARCHAR): Type/category of driver's licence.
   - **IMAGEM_CNH** (VARCHAR): Path to the driver's licence image.

3. **Locacoes Table:**

   - **IDENTIFICADOR** (VARCHAR, PRIMARY KEY): Unique identifier for each rental. Default is `locacao` + sequence.
   - **ENTREGADOR_ID** (VARCHAR, FOREING KEY): References `IDENTIFICADOR` in the `Entregadores` table.
   - **MOTO_ID** (VARCHAR, FOREING KEY): References `IDENTIFICADOR` in the `Motos` table.
   - **DATA_INICIO** (TIMESTAMP): Start date and time of the rental.
   - **DATA_TERMINO** (TIMESTAMP): End data and time of the rental.
   - **DATA_PREVISAO_TERMINO** (TIMESTAMP): Expected end data and time of the rental.
   - **PLANO** (INTEGER): Plan code for the rental.
   - **VALOR_PLANO** (REAL): Daily cost of the plan.
   - **DATA_DEVOLUCAO** (TIMESTAMP, NULLABLE): Actual return date and time.
   - **Relationships:**
     - `Motos` and `Entregadores` tables are referenced by the `Locacoes` table through foreign key.
     - A sequence, `locacoes_identificador_seq`, generates identifiers for `Locacoes`.

**Datagram:**

A simplified representation:

```table
Entregadores                      Motos
+----------------------+   +-------------------+
| IDENTIFICADOR (PK)   |   | IDENTIFICADOR (PK)|
| NOME                 |   | ANO               |
| CNPJ (Unique)        |   | MODELO            |
| DATA_NASCIMENTO      |   | PLACA (Unique)    |
| NUMERO_CNH (Unique)  |   +-------------------+
| TIPO_CNH             |
| IMAGEM_CNH           |
+----------------------+
        |
        | References
        v
    Locacoes
+-------------------------------+
| IDENTIFICADOR (PK, Sequence)  |
| ENTREGADOR_ID (FK)            |
| MOTO_ID (FK)                  |
| DATA_INICIO                   |
| DATA_TERMINO                  |
| DATA_PREVISAO_TERMINO         |
| PLANO                         |
| VALOR_PLANO                   |
| DATA_DEVOLUCAO                |
+-------------------------------+
```

**Connection Configuration:**

To connect to PostgreSQL using Npgsql:

1.  **Connection String:**

    - Use a connection string to define how the application connection connects to PostgreSQL:

      ```csharp
      ststring connectionString = "Host=localhost;Port=5432;Database=motoDB;Username=postgres;Password=1234";
      ```

2.  **Creating a Connection:**

    - Initialize and open a connection:

      ```csharp
      using Npgsql;

      _conexaoString = "Host=localhost;Port=5432;Database=motoDB;Username=postgres;Password=1234";

      var conexao = new NpgsqlConnection(connectionString);
      conexao.Open();
      ```

3.  **Executing SQL Commands:**

    - Use `NpgsqlCommand` to execute queries:

      ```csharp
      string query = "SELECT * FROM Motos";

      var comando = new NpgsqlCommand(query, conexao);
      var leitor = comando.ExecuteReader();

      while (leitor.Read())
      {
        Console.WriteLine($"Model: {leitor["MODELO"]}, Year: {leitor["ANO"]}");
      }
      ```

4.  **Handling Exceptions:**

    - Wrap connections in `try-catch` blocks to handle erros:

      ```csharp
      try
      {
        using var conexao = new NpgsqlConnection(_conexaoString);
        conexao.Open();

        Console.WriteLine("Connection successful.");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
      }
      ```

<h2>Project Organization:</h2>

```bash
Desafio-Backend:.
│   .gitignore
│   appsettings.Development.json
│   appsettings.json
│   Desafio-Backend.csproj
│   Desafio-Backend.http
│   Desafio-BackEnd.sln
│   DOCUMENTATION.md
│   Program.cs
│   README.md
│
├───imagens_entregadores
│
├───Properties
│       launchSettings.json
│
└───src
    ├───Models
    │       EntregadoresData.cs
    │       LocacaoData.cs
    │       MotoData.cs
    │
    └───Services
            DataBase.cs
            Publisher.cs
            Subscriber.cs
```

- **imagens_entregadores:** Includes image files linked to registered delivery personnel.

- **src:** Includes the core project components, structured in subdirectories:

  - **Models:** Define the data classes and the structure of the types of data.

    - **MotoData.cs:** Template for motorcycle data, including year, model, and license plate.

    - **EntregadoresData.cs:** Model for data associated with delivery personnel, including name, CNPJ, and driver's license.

    - **LocacaoData.cs:** Template for rental data, including delivery person, motorcycle, and dates.

  - **Services:** Includes service classes that offer business logic and integration with external systems.

    - **DataBase.cs:** Class for managing database connections and creating tables, sequences, and relations.

    - **Publisher.cs:** Implementing message publishing on RabbitMQ

    - **Subscriber.cs:** RabbitMQ message subscription and consumption implementation
