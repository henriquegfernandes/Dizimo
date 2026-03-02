# Dizimo - Sistema de Controle Financeiro para Pastoral do Dízimo

Dizimo é uma aplicação desktop Windows criada com .NET MAUI e Clean Architecture para organizar cadastros de dizimistas, ofertas, relatórios e backups com uma navegação moderna baseada em AppShell.

## Destaques recentes
- Login com controle de permissões (admin/padrão) e rotas principais disparadas diretamente da [AppShell.xaml](Dizimo/AppShell.xaml).
- Dashboard e listagens que combinam filtros por data, dizimista e usuário responsável para acelerar decisões financeiras.
- Página de configuração de backup ([Dizimo/Pages/BackupConfigPage.xaml](Dizimo/Pages/BackupConfigPage.xaml)) que define pastas locais, ativa backups automáticos e prepara os arquivos para sincronização em nuvem.
- Importação/exportação via planilha modelo e exportação de relatórios em CSV para continuar análises fora do app.
- Fluxo completo de CRUD para dizimistas, ofertas e usuários com telas otimizadas de cadastro, edição e confirmação de exclusão/inativação.

## Funcionalidades principais
- Cadastro e edição de dizimistas com número de inscrição, situação e histórico de ofertas.
- Gestão de ofertas com filtros por intervalo, dizimista, categoria e usuário.
- Relatórios gerais, aniversariantes e de movimentação financeira com exportação em CSV.
- Importação e exportação de dados usando o modelo oficial de planilha.
- Configuração de backups automáticos/manual para pastas monitoradas e sincronizáveis com OneDrive, Google Drive ou outra solução.
- Autenticação e gestão de usuários (admin e padrão) com telas dedicadas para listagem e cadastro.
- Interface responsiva com navegação global, page stack simplificada e componentes inspirados em Fluent UI.

## Arquitetura e camadas
- [.NET MAUI](Dizimo/) para UI, Behaviors, estilos e ViewModels compartilhados.
- [Dizimo.Application](Dizimo.Application/) com CQRS, comandos/queries, handlers e serviços de aplicação.
- [Dizimo.Domain](Dizimo.Domain/) contendo entidades, value objects, enums e contratos de repositório.
- [Dizimo.Infrastructure](Dizimo.Infrastructure/) cuidando da persistência SQLite, backup, repository e Unit of Work.
- [Dizimo.Tests](Dizimo.Tests/) rodando testes unitários com xUnit e Moq (17 testes: 15 de domínio, 2 de infraestrutura).
- Integração contínua via [.github/workflows](.github/workflows/) para restaurar, buildar, testar e publicar artefatos automaticamente.

## Estrutura do repositório
- Dizimo/ – projeto .NET MAUI com páginas, recursos, comportamentos e AppShell.
- Dizimo.Application/ – camada de aplicação com comandos, DTOs, mapeamentos e serviços.
- Dizimo.Domain/ – entidades de negócio, validações e contratos.
- Dizimo.Infrastructure/ – implementação de persistência, backup e repositórios.
- Dizimo.Tests/ – suite unificada de testes unitários (Domain e Infrastructure).
  - Domain/ – testes de entidades e regras de negócio (15 testes).
  - Infrastructure/ – testes de serviços de infraestrutura como Sessão (2 testes).
- [publish-release.ps1](publish-release.ps1) – script que compila, empacota, cria tags e gera o instalador.
- [PUBLISHING-GUIDE.md](PUBLISHING-GUIDE.md) – passo a passo para gerar releases manuais e resolver problemas comuns.

## Como buildar e executar
1. Clone o repositório e entre na pasta:
   ```powershell
   git clone https://github.com/henriquegfernandes/Dizimo.git
   cd Dizimo
   ```
2. Garanta que o .NET 10 SDK está instalado e que você está em um Windows 10 ou superior.
3. Restaure dependências:
   ```powershell
   dotnet restore
   ```
4. Compile em Release:
   ```powershell
   dotnet build --configuration Release
   ```
5. Execute o aplicativo MAUI para Windows:
   ```powershell
   dotnet run --project Dizimo/Dizimo.csproj -c Release
   ```
6. O SQLite local e a pasta de backup são configuráveis pela interface.

## Testes
- Execute todos os testes unitários:
  ```powershell
  dotnet test Dizimo.Tests/Dizimo.Tests.csproj
  ```
  Isso executará os 17 testes unificados (15 de domínio, 2 de infraestrutura).

- Organize seus testes por escopo em pastas:
  - `Dizimo.Tests/Domain/` – testes de entidades e regras de negócio.
  - `Dizimo.Tests/Infrastructure/` – testes de serviços de infraestrutura.

- Os testes usam **xUnit** para assertions e **Moq** para mocking de dependências.

## Backup e sincronização
Configure a pasta de backup pela tela de configuração dentro do aplicativo ([Dizimo/Pages/BackupConfigPage.xaml](Dizimo/Pages/BackupConfigPage.xaml)). Os backups podem ser executados automaticamente ou sob demanda e a pasta configurada pode ser sincronizada com ferramentas como OneDrive ou Google Drive.

## Publicação e release
- Use o script [publish-release.ps1](publish-release.ps1) para compilar, criar artefatos, gerar tags e produzir o ZIP pronto para publicação. Ele aceita parâmetros como `-VersionTag` e `-ReleaseNotes`.
- Consulte o [PUBLISHING-GUIDE.md](PUBLISHING-GUIDE.md) para o fluxo completo de release (atualização de versão, criação de release no GitHub, checklist e troubleshooting).
- Os artefatos finais ficam em `publish/` com nome no formato `Dizimo-vX.Y.Z-windows.zip`.

## CI/CD e distribuição
O pipeline em [.github/workflows](.github/workflows/) restaura dependências, compila em Release, executa testes e publica releases automáticos quando a branch main recebe um push ou pull request aprovado.

## Apoio e contato
Abra issues em https://github.com/henriquegfernandes/Dizimo/issues caso precise relatar bugs, sugerir melhorias ou pedir ajuda.
