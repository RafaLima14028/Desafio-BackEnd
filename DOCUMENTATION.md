<h1 align="Center">Desafio Backend</h1>

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

- Entregadores (Review later):

  - [x] POST /entregadores
  - [x] POST /entregadores/{id}/cnh

- Locação (Review later):

  - [x] POST /locacao
  - [x] GET /locacao/{id}
  - [x] PUT /locacao/{id}/devolucao

- [ ] Database
- [ ] Pub/Sub

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
