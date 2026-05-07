# Dizimo - Sistema de Controle Financeiro para Pastoral do Dízimo

Dizimo é uma aplicação desktop cross-platform criada com **Avalonia + .NET 10** em Clean Architecture para gerenciar dízimos, ofertas, relatórios e backups com interface moderna e responsiva. Suporta **Windows, Linux e macOS** (tanto x64 quanto ARM64).

## 🎯 Destaques principais
- **Multi-plataforma**: Windows, Linux e macOS (Intel e Apple Silicon)
- **Multi-arquitetura**: Suporte completo para x64 e ARM64 em todas as plataformas
- **Instaladores nativos**: Setup.exe (Windows), AppImage (Linux), DMG (macOS)
- **Backup automático e manual**: SQLite offline com sincronização em nuvem
- **Autenticação e permissões**: Admin, Secretário, Tesoureiro com controle granular
- **Dashboards e relatórios**: Gráficos com percentuais, exportação para Excel/CSV
- **CI/CD automatizado**: Build multi-plataforma com release automático no GitHub

## ✨ Funcionalidades principais
- Cadastro e edição de dizimistas com histórico de ofertas
- Gestão de ofertas com filtros por período, dizimista e tipo de pagamento
- Relatórios gerais, aniversariantes e análises financeiras
- Importação/exportação de dados em Excel
- Backup automático ao fechar app e restauração manual
- Autenticação de usuários com 3 níveis de permissão
- Interface responsiva com tema Fluent Design
- Suporte completo a múltiplos idiomas (português)

## 🏗️ Arquitetura e camadas
- **[Dizimo/](Dizimo/)** – UI Avalonia 12.0.2, páginas XAML, ViewModels e recursos
- **[Dizimo.Application/](Dizimo.Application/)** – Handlers, DTOs, mapeamentos e serviços
- **[Dizimo.Domain/](Dizimo.Domain/)** – Entidades, value objects, enums e contratos
- **[Dizimo.Infrastructure/](Dizimo.Infrastructure/)** – SQLite, backup, repositórios
- **[Dizimo.Tests/](Dizimo.Tests/)** – xUnit + Moq, testes de domínio e infraestrutura
- **[.github/workflows/](.github/workflows/)** – CI/CD multi-plataforma com release automático

## 📦 Estrutura do repositório
```
Dizimo/
├── Pages/              # Páginas XAML (Login, Dashboard, Cadastros, etc)
├── ViewModels/         # MVVM ViewModels
├── Services/           # Serviços da aplicação
├── Converters/         # Conversores de dados
├── Resources/          # Ícones, fontes, temas
├── Behaviors/          # Behaviors customizados
└── Dizimo.csproj       # Configuração do projeto

Dizimo.Application/
├── Dizimistas/         # Handlers de dízimos
├── Ofertas/            # Handlers de ofertas
├── Dashboard/          # Services de dashboard
└── Reporting/          # Serviços de relatório

Dizimo.Domain/
├── Entities/           # Classes de domínio
├── Models/             # DTOs e models
└── Repositories/       # Contratos de repositório

Dizimo.Infrastructure/
├── Persistence/        # DbContext e configurações
├── Repositories/       # Implementação de repositórios
├── Backup/Services/    # Serviço de backup
└── Migrations/         # Migrações do banco

Dizimo.Tests/
├── Domain/             # Testes de entidades
└── Infrastructure/     # Testes de serviços

build-scripts/
├── Dizimo.nsi          # NSIS script para Setup.exe
├── build-appimage.sh   # Builder para AppImage
└── build-dmg.sh        # Builder para DMG
```

## 🚀 Como instalador e executar

### Via Instalador (Recomendado)
1. **Windows**: Download `Dizimo-v1.1.0-windows-x64-Setup.exe` ou ARM64
   - Execute e siga o wizard
   - App será instalado em Program Files com atalho no Desktop

2. **Linux**: Download `Dizimo-v1.1.0-linux-x86_64.AppImage`
   ```bash
   chmod +x Dizimo-v1.1.0-linux-x86_64.AppImage
   ./Dizimo-v1.1.0-linux-x86_64.AppImage
   ```
   Funciona em **qualquer distribuição Linux**

3. **macOS**: Download `Dizimo-v1.1.0-macos-x86_64.dmg`
   - Duplo-clique para montar
   - Arraste a aplicação para Applications folder

### Via Código-fonte
1. Clone o repositório:
   ```bash
   git clone https://github.com/henriquegfernandes/Dizimo.git
   cd Dizimo
   ```

2. Garanta que tem .NET 10 SDK instalado

3. Restaure dependências:
   ```bash
   dotnet restore
   ```

4. Compile em Release:
   ```bash
   dotnet build --configuration Release
   ```

5. Execute a aplicação:
   ```bash
   dotnet run --project Dizimo/Dizimo.csproj -c Release
   ```

## 🧪 Testes

Execute todos os testes unitários:
```bash
dotnet test Dizimo.Tests/Dizimo.Tests.csproj --configuration Release
```

Os testes utilizam:
- **xUnit** para assertions
- **Moq** para mocking de dependências
- Cobertura de domínio e infraestrutura

## 🔄 Backup e Sincronização

Configure a pasta de backup na tela de Configurações dentro do app. Os backups podem ser:
- **Automáticos**: Executados automaticamente ao fechar o app
- **Manuais**: Sob demanda através da interface

A pasta pode ser sincronizada com OneDrive, Google Drive ou similar.

## 🚢 CI/CD e Release

### Workflow Automático
O pipeline em [.github/workflows/ci.yml](.github/workflows/ci.yml):
- Compila para 6 plataformas em paralelo (x64 e ARM64)
- Executa testes
- Gera instaladores nativos
- Cria release automático no GitHub quando push para `main`

### Plataformas Suportadas
- **Windows**: x64 e ARM64 (Setup.exe + ZIP)
- **Linux**: x64 e ARM64 (AppImage + ZIP)
- **macOS**: x64 Intel e ARM64 Apple Silicon (DMG)

### Scripts de Build (opcional)
```bash
# Windows Setup.exe
build-scripts/Dizimo.nsi

# Linux AppImage
bash build-scripts/build-appimage.sh publish-dir v1.1.0

# macOS DMG
bash build-scripts/build-dmg.sh publish-dir v1.1.0
```

## 📋 Versão

- **Atual**: v1.1.0
- **Framework**: Avalonia 12.0.2
- **.NET**: 10.0
- **Banco de dados**: SQLite
- **Arquitetura**: Clean Architecture com MVVM

## 🔗 Links úteis

- [Manual de Publicação](RELEASE-GUIDE-PT-BR.md)
- [Checklist de Release](RELEASE-READINESS.md)
- [Relatório de Análise](RELATORIO_FINAL.md)
- [GitHub Releases](https://github.com/henriquegfernandes/Dizimo/releases)

## 💬 Apoio e contato

Abra issues em https://github.com/henriquegfernandes/Dizimo/issues para relatar bugs, sugerir melhorias ou pedir ajuda.

---

**Desenvolvido com ❤️ usando Avalonia + .NET 10 em Clean Architecture**
