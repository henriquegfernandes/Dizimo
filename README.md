# Dizimo - Sistema de Controle Financeiro para Pastoral do Dízimo

Este projeto é um sistema de controle financeiro para a pastoral do dízimo da comunidade católica Jesus Operário, desenvolvido em .NET MAUI para rodar nativamente no Windows. O sistema segue Clean Architecture, MVVM, CQRS e Repository/UoW, com banco de dados local SQLite e backup automático/manual para uma pasta configurável (sincronizável com nuvem).

## Funcionalidades

- CRUD de dizimistas (com número de cadastro)
- Relatórios gerais, aniversariantes e exportação em CSV
- Exclusão/inativação de dizimistas
- CRUD de ofertas, busca por data/dizimista, relatório por data
- Importação/exportação de dados via planilha modelo
- Autenticação e controle de usuários (admin/padrão)
- Backup automático/manual para pasta local (sincronizável com OneDrive, Google Drive, etc.)
- Interface moderna com navegação rápida e menu global

## Arquitetura

- **.NET MAUI** (UI Windows)
- **SQLite** (banco local)
- **Clean Architecture** (Domain, Application, Infrastructure, UI)
- **MVVM** (CommunityToolkit.Mvvm)
- **CQRS** (Commands/Queries/Handlers)
- **Repository/UoW**
- **Testes unitários** (xUnit, Moq)
- **CI/CD** (GitHub Actions, build, testes e release automático)

## Dependências

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Microsoft.EntityFrameworkCore.Sqlite](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite)
- [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm)
- [Syncfusion.Maui.Toolkit](https://www.nuget.org/packages/Syncfusion.Maui.Toolkit)
- [xUnit](https://www.nuget.org/packages/xunit)
- [Moq](https://www.nuget.org/packages/Moq)

## Como Buildar e Executar

1. **Clone o repositório:**
```sh
git clone https://github.com/henriquegfernandes/Dizimo.git
cd Dizimo
```

2. **Restaure as dependências:**
```sh
dotnet restore
```

3. **Build do projeto:**
```sh
dotnet build --configuration Release
```

4. **Executar o app MAUI (Windows):**
```sh
dotnet run --project Dizimo/Dizimo.csproj -c Release
```
> O app será iniciado em modo desktop Windows.

5. **Rodar os testes unitários:**
```sh
dotnet test Dizimo.Tests/Dizimo.Tests.csproj
```

## Backup e Sincronização

- Configure a pasta de backup pelo menu do app.
- Sincronize essa pasta com OneDrive, Google Drive ou outro serviço de nuvem para manter o backup atualizado.

## CI/CD

- O projeto possui integração contínua via GitHub Actions.
- A cada push/pull request na branch `main`, o workflow:
  - Restaura dependências
  - Builda e testa o projeto
  - Publica o artefato do app Windows como release automático (disponível na aba Releases do GitHub)

## Estrutura de Pastas

```
Dizimo.Domain/         # Entidades e regras de negócio
Dizimo.Application/    # CQRS, serviços de aplicação
Dizimo.Infrastructure/ # Persistência, backup, repositórios
Dizimo/                # Projeto MAUI (UI)
Dizimo.Tests/          # Testes unitários
.github/workflows/     # CI/CD (GitHub Actions)
```

## Observações

- Para rodar o app, é necessário o .NET 10 SDK e Windows 10 ou superior.
- Para backup automático, configure a pasta de backup e sincronize com o serviço de nuvem desejado.
- Para dúvidas ou sugestões, abra uma issue no repositório.
