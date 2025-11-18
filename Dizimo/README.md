````markdown
Dizimo
======

Projeto .NET MAUI para gerenciar dizimistas e ofertas, com uma biblioteca `Dizimo.Core` contendo regras de negócio e repositórios SQLite.

Como rodar os testes (Linux/macOS):

```bash
dotnet test Dizimo/tests/Dizimo.Tests/Dizimo.Tests.csproj
```

Notas:
- A biblioteca `Dizimo.Core` contém os serviços reutilizáveis (DizimoService, BackupService, AuthService) e pode ser testada sem dependências MAUI.
- Para compilar o aplicativo MAUI completo você precisa de um runner com workloads MAUI instalados (por exemplo, Windows com Visual Studio ou um runner CI que tenha workloads). O build local em Linux não compila as plataformas MAUI.
- Backup/Restore: há export/import JSON no `BackupService`. O import tenta remapear IDs de dizimistas por `Codigo` quando possível.

Comportamento do import (importante):
- O import NÃO reusa IDs numéricos originais, a menos que haja um mapeamento criado ao importar dizimistas do arquivo (isso evita ofertas órfãs).
- Ofertas cujo `DizimistaID` não puder ser mapeado são ignoradas (puladas).
- Datas inválidas nas ofertas são ignoradas; dizimistas válidos ainda serão importados.

Testes adicionados:
- `BackupServiceTests` e `BackupServiceEdgeTests` cobrem import/export, remapeamento, ofertas órfãs e datas malformadas.
- `AuthServiceTests` cobre criação/autenticação de usuário.

Próximos passos sugeridos:
- Escrever mais testes para casos de erro no import/export.
- Implementar UI rest (se desejar que eu prossiga com as páginas MAUI).

## Self-hosted Windows runner for MAUI builds

Se você quiser que o CI construa o aplicativo MAUI de forma confiável, utilize um runner Windows self-hosted com Visual Studio e os workloads MAUI instalados. Os runners hospedados do GitHub muitas vezes não incluem todos os componentes do Visual Studio necessários para builds MAUI. Abaixo estão instruções recomendadas e um exemplo de tarefa agendada para manter o runner em execução após reinicializações.

Passos principais
- Provisione uma máquina Windows (VM ou física) com Windows 10/11 ou Windows Server.
- Instale o Visual Studio 2022 (ou superior) com o workload "Mobile development with .NET" e componentes de plataforma que você precisa (Android SDK, conexão com host macOS para iOS, etc.).
- Instale o SDK .NET 9 e Git.
- Baixe e configure o runner do GitHub Actions para este repositório (você precisará de um token de registro em Settings -> Actions -> Runners -> New self-hosted runner).

Exemplo rápido (PowerShell) — substitua os placeholders antes de executar
[![CI](https://github.com/henriquegfernandes/Dizimo/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/henriquegfernandes/Dizimo/actions/workflows/ci.yml)

# Dizimo

Projeto .NET MAUI para gerenciar dizimistas e ofertas, com uma biblioteca `Dizimo.Core` contendo regras de negócio e repositórios SQLite.

## Como rodar os testes (Linux/macOS)

```bash
dotnet test tests/Dizimo.Tests/Dizimo.Tests.csproj
```

Notas:
- A biblioteca `Dizimo.Core` contém os serviços reutilizáveis (DizimoService, BackupService, AuthService) e pode ser testada sem dependências MAUI.
- Para compilar o aplicativo MAUI completo você precisa de um runner com workloads MAUI instalados (por exemplo, Windows com Visual Studio ou um runner CI que tenha workloads). O build local em Linux não compila as plataformas MAUI.
- Backup/Restore: há export/import JSON no `BackupService`. O import tenta remapear IDs de dizimistas por `Codigo` quando possível.

### Comportamento do import (importante):
- O import NÃO reusa IDs numéricos originais, a menos que haja um mapeamento criado ao importar dizimistas do arquivo (isso evita ofertas órfãs).
- Ofertas cujo `DizimistaID` não puder ser mapeado são ignoradas (puladas).
- Datas inválidas nas ofertas são ignoradas; dizimistas válidos ainda serão importados.

### Testes adicionados:
- `BackupServiceTests` e `BackupServiceEdgeTests` cobrem import/export, remapeamento, ofertas órfãs e datas malformadas.
- `AuthServiceTests` cobre criação/autenticação de usuário.

### Self-hosted Windows runner for MAUI builds

Se você quiser que o CI construa o aplicativo MAUI de forma confiável, utilize um runner Windows self-hosted com Visual Studio e os workloads MAUI instalados. Os runners hospedados do GitHub muitas vezes não incluem todos os componentes do Visual Studio necessários para builds MAUI. Abaixo estão instruções recomendadas e um exemplo de tarefa agendada para manter o runner em execução após reinicializações.

Passos principais
- Provisione uma máquina Windows (VM ou física) com Windows 10/11 ou Windows Server.
- Instale o Visual Studio 2022 (ou superior) com o workload "Mobile development with .NET" e componentes de plataforma que você precisa (Android SDK, conexão com host macOS para iOS, etc.).
- Instale o SDK .NET 9 e Git.
- Baixe e configure o runner do GitHub Actions para este repositório (você precisará de um token de registro em Settings -> Actions -> Runners -> New self-hosted runner).

Exemplo rápido (PowerShell) — substitua os placeholders antes de executar

```powershell
# Criar pasta do runner
New-Item -ItemType Directory -Path C:\actions-runner -Force
Set-Location C:\actions-runner

# Baixe o pacote do runner de https://github.com/actions/runner/releases (escolha o win-x64 zip)
# Exemplo (substitua X.Y.Z pela versão):
# $url = "https://github.com/actions/runner/releases/download/vX.Y.Z/actions-runner-win-x64-X.Y.Z.zip"
# Invoke-WebRequest -Uri $url -OutFile actions-runner.zip
# Expand-Archive actions-runner.zip -Force

# Configure o runner (obtenha o token nas configurações do repositório -> Actions -> Runners -> Add runner)
.\config.cmd --url https://github.com/<OWNER>/<REPO> --token YOUR_TOKEN_HERE

# Teste executando interativamente
.\run.cmd
```

Executar como tarefa agendada (recomendado) — inicia o runner no boot

```powershell
$action = New-ScheduledTaskAction -Execute "C:\Windows\System32\cmd.exe" -Argument "/c \"C:\actions-runner\run.cmd\""
$trigger = New-ScheduledTaskTrigger -AtStartup
Register-ScheduledTask -Action $action -Trigger $trigger -TaskName "GitHubActionsRunner" -Description "Starts GitHub Actions Runner for Dizimo CI" -User "SYSTEM" -RunLevel Highest

# Para remover a tarefa:
# Unregister-ScheduledTask -TaskName "GitHubActionsRunner" -Confirm:$false
```

Observações e troubleshooting
- Assegure que a conta configurada tenha permissões para acessar o Visual Studio e os SDKs. Executar como `SYSTEM` inicia o runner, mas pode ser problemático para ferramentas interativas; usar uma conta de serviço com permissões adequadas geralmente funciona melhor.
- Verifique workloads SDKs/Visual Studio:

```powershell
dotnet --list-sdks
dotnet workload list
```

- Para executar o runner como serviço, considere usar NSSM (Non-Sucking Service Manager) para envolver `run.cmd` como um serviço do Windows. Consulte a documentação oficial do runner para opções adicionais.

Opcional: unit systemd (hosts Linux)

Se optar por um host Linux, segue um unit systemd mínimo para iniciar o runner no boot (APENAS para referencia; MAUI targets específicos normalmente requerem Windows):

```ini
[Unit]
Description=GitHub Actions Runner
After=network.target

[Service]
Type=simple
User=runner
WorkingDirectory=/home/runner/actions-runner
ExecStart=/home/runner/actions-runner/run.sh
Restart=always

[Install]
WantedBy=multi-user.target
```

Referências
- https://docs.github.com/actions/hosting-your-own-runners
- https://docs.microsoft.com/visualstudio/install/required-components-for-dotnet-maui

## Inspectando resultados de teste (TRX)

O workflow de CI agora gera arquivos TRX para os testes (`Dizimo.Core` e `Dizimo.Tests`) e os publica como artefatos. Para inspecionar os resultados:

1. Abra a página da execução do workflow na aba "Actions" do repositório.
2. Selecione a execução relevante (o último run do PR) e, na seção "Artifacts", faça o download dos artefatos `Dizimo.Core.trx` e `Dizimo.Tests.trx`.
3. Para abrir/analisar os TRX localmente:

	 - Usando o Visual Studio: abra o Visual Studio, vá em Test -> Windows -> Test Explorer e importe o arquivo TRX (ou execute os testes via Test Explorer e os resultados aparecerão automaticamente).

	 - Usando `vstest.console.exe` (a partir do Developer Command Prompt):

		```powershell
		vstest.console.exe path\to\Dizimo.Core.trx
		```

	 - Converter TRX para JUnit/XML (opcional) com ferramentas como `trx2junit` ou `ReportUnit`, para integrar com outras ferramentas de relatório.

4. Se preferir, rode os testes localmente com `dotnet test` e compare o resultado com o TRX disponível.

Observação: os TRX carregados no workflow são úteis para depurar testes que falharam no ambiente CI e contêm stack traces e detalhes de falha.
ci: trigger run 20251118T191711Z
