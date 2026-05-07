# 📐 Análise Arquitetural - Clean Architecture Violations

## 🔴 Problemas Identificados

A camada de Apresentação (**Dizimo**) contém **serviços de negócio** que deveriam estar na camada de Aplicação (**Dizimo.Application**).

### Violações Críticas:

#### 1. **Excel Services** (❌ Negócio, não UI)
- `DizimistaExcelService.cs`
- `AniversariantesExcelService.cs`  
- `OfertaExcelService.cs`

**Problema:** Lógica de formatação e exportação de dados (negócio)
**Localização ERRADA:** `/Dizimo/Services/`
**Localização CORRETA:** `/Dizimo.Application/Excel/Services/` ou `/Dizimo.Application/Reporting/`

---

#### 2. **PDF Services** (❌ Negócio, não UI)
- `DizimistaPdfService.cs`
- `AniversariantesPdfService.cs`
- `OfertaPdfService.cs`

**Problema:** Lógica de geração de relatórios em PDF (negócio)
**Localização ERRADA:** `/Dizimo/Services/`
**Localização CORRETA:** `/Dizimo.Application/Reporting/Services/`

---

#### 3. **Backup Services** (⚠️ Infraestrutura, não UI)
- `BackupService.cs`
- `LocalBackupService.cs`

**Problema:** Lógica de persistência de dados (infraestrutura)
**Localização ERRADA:** `/Dizimo/Services/`
**Localização CORRETA:** `/Dizimo.Infrastructure/Backup/Services/`

---

#### 4. **Authentication Service** (❌ Negócio, não UI)
- `AuthenticationService.cs`
- `IAuthenticationService.cs`

**Problema:** Lógica de autenticação é negócio crítico
**Localização ERRADA:** `/Dizimo/Services/`
**Localização CORRETA:** `/Dizimo.Application/Authentication/Services/`

---

## 🟢 Serviços Corretos (Infrastructure UI)

✅ **Dialogs & Notifications:**
- `DialogService.cs`
- `AvaloniaDialogService.cs`
- `IDialogService.cs`
- `ModalErrorHandler.cs`
- `IErrorHandler.cs`

✅ **Navigation:**
- `NavigationService.cs`
- `INavigationService.cs`

✅ **UI Preferences:**
- `ThemeService.cs`
- `FilterCacheService.cs`
- `LocalPreferencesService.cs`
- `IPreferencesService.cs`

✅ **UI Helpers:**
- `DataPathProvider.cs`
- `IDataPathProvider.cs`
- `BackupOnCloseService.cs` (orquestrador de UI)
- `SessaoService.cs` (tolerável, mas poderia ir para Application)

---

## 📊 Impacto da Arquitetura Atual

### Problema 1: Testes Unitários
- ❌ Não é possível testar ExcelService sem a UI Avalonia
- ❌ PdfService acoplado com Dialog/Navigation
- ❌ Não há separação clara de responsabilidades

### Problema 2: Reutilização de Código
- ❌ ExcelService, PdfService, BackupService não podem ser usados em outras UI (WPF, WebAPI)
- ❌ AuthenticationService acoplado com Avalonia Services

### Problema 3: Manutenção
- ❌ Difícil localizar lógica de negócio (está misturada com UI)
- ❌ Difícil fazer refatoração isolada

### Problema 4: Escalabilidade
- ❌ Nova feature precisa adicionar serviço em camada errada
- ❌ Não há padrão claro donde novos services devem ser colocados

---

## 🔧 Plano de Refatoração

### Fase 1: Mover Reporting Services
```
Dizimo.Application/
├── Reporting/
│   ├── Services/
│   │   ├── ExcelExportService.cs
│   │   ├── PdfReportService.cs
│   │   └── IReportingService.cs
│   ├── Commands/
│   └── Queries/
```

### Fase 2: Mover Authentication
```
Dizimo.Application/
├── Authentication/
│   ├── Services/
│   │   └── AuthenticationService.cs
│   ├── Commands/
│   └── IAuthenticationService.cs
```

### Fase 3: Mover Backup (Infraestrutura)
```
Dizimo.Infrastructure/
├── Backup/
│   ├── Services/
│   │   ├── BackupService.cs
│   │   └── LocalBackupService.cs
│   └── Repositories/
```

### Fase 4: Refatorar Dependency Injection
```csharp
// Application Layer Services
builder.Services.AddScoped<IReportingService, ExcelReportingService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Infrastructure Layer Services
builder.Services.AddScoped<IBackupService, LocalBackupService>();

// Presentation Layer Services (apenas UI)
builder.Services.AddScoped<IDialogService, AvaloniaDialogService>();
builder.Services.AddScoped<INavigationService, NavigationService>();
```

---

## 📋 Checklist de Refatoração

- [ ] Criar pastas em `Dizimo.Application/Reporting/`
- [ ] Crear interfaces `IReportingService`, `IPdfReportService`
- [ ] Mover ExcelServices para Application
- [ ] Mover PdfServices para Application
- [ ] Criar `IAuthenticationService` em Application
- [ ] Mover AuthenticationService para Application
- [ ] Criar `IBackupService` em Infrastructure
- [ ] Mover BackupServices para Infrastructure
- [ ] Atualizar injeção de dependência
- [ ] Atualizar imports em ViewModels
- [ ] Executar testes
- [ ] Validar build

---

## ✅ Benefícios da Refatoração

1. **Testabilidade** ✅ - Serviços de negócio isolados
2. **Reutilização** ✅ - Mesmos serviços em múltiplas UIs
3. **Manutenção** ✅ - Código organizado por responsabilidade
4. **Escalabilidade** ✅ - Novo padrão clara para novos features
5. **SOLID Principles** ✅ - Melhor separation of concerns
6. **Clean Architecture** ✅ - Dependências apontam só para camadas internas

