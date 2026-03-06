# MinimalAPI - Sistema de Gestão de Veículos e Usuários

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![MySQL](https://img.shields.io/badge/MySQL-Database-4479A1?logo=mysql)](https://www.mysql.com/)
[![JWT](https://img.shields.io/badge/JWT-Auth-000000?logo=jsonwebtokens)](https://jwt.io/)

API RESTful desenvolvida com **ASP.NET Core Minimal APIs** para gerenciamento de veículos e usuários, com autenticação JWT e controle de acesso baseado em roles.

## 🚀 Tecnologias

- **.NET 10** - Framework principal
- **ASP.NET Core Minimal APIs** - Arquitetura da API
- **Entity Framework Core 9** - ORM
- **MySQL** - Banco de dados (Pomelo)
- **JWT Bearer Authentication** - Autenticação e autorização
- **BCrypt.Net** - Hash de senhas
- **Scalar/OpenAPI** - Documentação interativa da API
- **MSTest** - Testes de integração

## 📋 Funcionalidades

### 🔐 Autenticação e Autorização
- Login com JWT (suporte a cookies e headers)
- Controle de acesso baseado em roles (Admin/User)
- Tokens com tempo de expiração configurável
- Políticas de autorização personalizadas

### 👥 Gerenciamento de Usuários
- Cadastro e listagem de usuários
- Listagem paginada com filtros (email, cargo, ordenação)
- Busca por ID
- Controle de usuários ativos/inativos
- Hash seguro de senhas com BCrypt
- Validação de dados com Data Annotations

### 🚗 Gerenciamento de Veículos
- CRUD completo de veículos
- Campos: Nome, Marca, Ano, Cor
- Validação de ano com Data Annotations customizada
- Timestamps automáticos (CreatedAt, UpdatedAt)

## 🏗️ Estrutura do Projeto

```
MinimalAPI/
├── Api/
│   ├── Domain/
│   │   ├── Models/              # Entidades do domínio (Usuario, Veiculo)
│   │   ├── DTOs/                # Data Transfer Objects
│   │   ├── Services/            # Lógica de negócio
│   │   ├── Interfaces/          # Contratos de serviços
│   │   └── DataAnnotations/     # Validações customizadas
│   ├── Infrastructure/          # DbContext e inicialização do banco
│   ├── Extensions Methods/      # Extensões de Claims e HttpContext
│   ├── Migrations/              # Migrações do EF Core
│   ├── Enums/                   # Enumerações (Cargo)
│   ├── Program.cs               # Ponto de entrada
│   └── Startup.cs               # Configuração de serviços e middlewares
└── Test/
    ├── Helpers/                 # Setup para testes de integração
    └── Requests/                # Testes de endpoints
```

## 🔧 Configuração

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)
- Um editor de código (Visual Studio 2022, VS Code, Rider)

### Instalação

1. **Clone o repositório**
```bash
git clone https://github.com/vinicius-sribeiro/MinimalAPI.git
cd MinimalAPI
```

2. **Restaure as dependências**
```bash
dotnet restore
```

3. **Configure a string de conexão**

Edite o arquivo `Api/appsettings.json` ou use User Secrets:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=minimalapi;User=root;Password=sua_senha;"
  },
  "Jwt": {
    "Key": "sua_chave_secreta_aqui_com_no_minimo_32_caracteres",
    "Issuer": "MinimalAPI",
    "Audience": "MinimalAPI-Users"
  }
}
```

**Usando User Secrets (recomendado para desenvolvimento):**
```bash
cd Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=minimalapi;User=root;Password=sua_senha;"
dotnet user-secrets set "Jwt:Key" "sua_chave_secreta_aqui"
```

4. **Execute as migrations**
```bash
cd Api
dotnet ef database update
```

O projeto possui seed automático que criará um usuário admin padrão no primeiro run em modo Development.

5. **Execute a aplicação**
```bash
dotnet run --project Api
```

A API estará disponível em:
- **HTTPS**: `https://localhost:7082`
- **HTTP**: `http://localhost:5000`
- **Documentação**: `https://localhost:7082/scalar/v1` (em Development)

## 🧪 Testes

Execute os testes de integração:

```bash
dotnet test
```

Os testes utilizam um banco de dados em memória (InMemory) para garantir isolamento.

## 🔐 API Endpoints

### Autenticação

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/auth/login` | Autentica um usuário e retorna token JWT | Não |

**Exemplo de request:**
```json
{
  "email": "admin@teste.com",
  "senha": "123456"
}
```

### Usuários

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| GET | `/api/usuarios/{id}` | Busca usuário por ID | Admin |
| GET | `/api/usuarios` | Lista todos os usuários (paginado) | Admin |

**Parâmetros de listagem:**
- `email` - Filtro por email (LIKE)
- `cargos` - Filtro por cargo (Admin/User)
- `pagina` - Número da página (padrão: 1)
- `pageSize` - Itens por página (5-50, padrão: 10)
- `ordenarCrescente` - Ordenação (true/false)

### Veículos

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| GET | `/api/veiculos` | Lista todos os veículos | Sim |
| GET | `/api/veiculos/{id}` | Busca veículo por ID | Sim |
| POST | `/api/veiculos` | Cadastra novo veículo | Admin |
| PUT | `/api/veiculos/{id}` | Atualiza veículo | Admin |
| DELETE | `/api/veiculos/{id}` | Remove veículo | Admin |

## 🔑 Autenticação JWT

A API suporta dois métodos de envio do token:

1. **Header Authorization (recomendado)**
```
Authorization: Bearer seu_token_jwt_aqui
```

2. **Cookie**
```
Cookie: access_token=seu_token_jwt_aqui
```

## 🛡️ CORS

Por padrão, a API permite requisições das seguintes origens:
- `http://localhost:3000` (React, Next.js)
- `https://localhost:7082` (API local)

Para adicionar novas origens, edite a política CORS em `Startup.cs`.

## 📖 Documentação Interativa

Em ambiente de desenvolvimento, acesse a documentação interativa:

- **Scalar UI**: `https://localhost:7082/scalar/v1`

A documentação inclui suporte completo a JWT - você pode autenticar diretamente pela interface.

## 🏛️ Arquitetura

O projeto segue princípios de **Clean Architecture** com separação de responsabilidades:

- **Domain**: Modelos, DTOs, interfaces e serviços de domínio
- **Infrastructure**: Acesso a dados (DbContext) e inicialização
- **Extensions**: Métodos auxiliares para Claims e HttpContext
- **Startup Pattern**: Separação da configuração da aplicação para facilitar testes

### Padrões Utilizados

- **Dependency Injection**
- **DTO Pattern** para transferência de dados
- **Service Layer** para lógica de negócio
- **Extension Methods** para código mais limpo

## 🔄 Migrations

Para criar uma nova migration:
```bash
cd Api
dotnet ef migrations add NomeDaMigracao
```

Para aplicar migrations pendentes:
```bash
dotnet ef database update
```

Para reverter para uma migration específica:
```bash
dotnet ef database update NomeDaMigracao
```
