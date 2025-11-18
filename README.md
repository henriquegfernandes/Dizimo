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
- Implementar UI rest.
