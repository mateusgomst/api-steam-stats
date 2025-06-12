# 🎮 Steam Stats API

Projeto para consultar a API da Steam e exibir informações sobre jogos, como **preços**, **histórico de preços** e **detalhes gerais**.  
A aplicação permitirá aos usuários:

- Interagir com jogos favoritos
- Acompanhar promoções
- Receber notificações sobre variações de preço

---

🛠️ Desenvolvido com .NET 8 e PostgreSQL (via Docker)

📡 API REST para consumo de dados e integração com aplicações externas.

📦 Mais informações abaixo sobre como rodar o projeto.


## 🚀 Como rodar o projeto

### ✅ Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) instalado
- [Docker](https://www.docker.com/products/docker-desktop/) instalado e rodando
- Git instalado (opcional, para clonar o projeto)

---

### 🧬 Clone o repositório

```bash
git clone https://github.com/mateusgomst/api-steam-stats
cd api-steam-stats
````

---

### 🐳 Suba o banco de dados com Docker Compose

```bash
docker-compose up -d
```

> 🔥 **Importante:** Certifique-se de que o Docker está rodando antes de executar o comando acima.

---

### ⚙️ Execute a aplicação

```bash
dotnet run
```

> O `dotnet run` irá restaurar automaticamente todas as dependências do projeto e iniciar a API.

---

## 🛢️ Acessar o banco de dados PostgreSQL

O banco de dados é executado dentro de um container Docker, então você **não precisa ter o PostgreSQL instalado localmente**.

Use o comando abaixo para acessar o banco diretamente no terminal:

```bash
docker exec -it db_api_steam_stats bash
psql -U postgres -d postgres
```

---
