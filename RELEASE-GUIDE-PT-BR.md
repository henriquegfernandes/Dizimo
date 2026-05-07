## 📋 Guia de Release Multi-Plataforma - Dizimo

### ⚡ Quick Start (5 minutos)

#### No Linux/macOS:
```bash
./publish-release.sh v1.0.0 "Descrição do release"
```

#### No Windows PowerShell:
```powershell
.\publish-release-multiplatform.ps1 -VersionTag "v1.0.0" -ReleaseNotes "Descrição do release"
```

---

## ✅ Checklist Pré-Release

Antes de fazer o release, verifique:

### Testes e Código
- [ ] Código-fonte testado localmente
- [ ] Não há erros de compilação (`dotnet build -c Release` sucede)
- [ ] Versão atualizada no `Dizimo/Dizimo.csproj`:
  - `<ApplicationDisplayVersion>`
  - `<ApplicationVersion>`
  - `<AssemblyVersion>`
  - `<FileVersion>`
  - `<InformationalVersion>`

### Git
- [ ] Todas as mudanças foram commitadas
- [ ] `git status` mostra "nothing to commit, working tree clean"
- [ ] Deseja-se criar um novo commit? Se sim: `git add . && git commit -m "Release v1.0.0"`
- [ ] `git push origin main` completou com sucesso

### Preparação do Release
- [ ] Notas de release foram preparadas
- [ ] Nenhuma tag com o mesmo nome existe (`git tag | grep v1.0.0`)

---

## 🚀 Processode Release Passo-a-Passo

### Passo 1: Preparação Final

```bash
# Navegue até a pasta do projeto
cd ~/Projects/Dizimo

# Verifique status
git status

# Se necessário, commit e push
git add .
git commit -m "Preparation for v1.0.0 release"
git push origin main
```

### Passo 2: Atualizar Versão (Opcional)

Edit `Dizimo/Dizimo.csproj`:

```xml
<PropertyGroup>
    <ApplicationDisplayVersion>1.0.0</ApplicationDisplayVersion>
    <ApplicationVersion>1</ApplicationVersion>
    <AssemblyVersion>1.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
    <InformationalVersion>1.0.0</InformationalVersion>
</PropertyGroup>
```

Commit e push:
```bash
git add Dizimo/Dizimo.csproj
git commit -m "Bump version to 1.0.0"
git push origin main
```

### Passo 3: Executar Script de Publicação

#### Opção A: Linux/macOS
```bash
./publish-release.sh v1.0.0 "Versão 1.0.0 - Lançamento oficial"
```

#### Opção B: Windows PowerShell
```powershell
.\publish-release-multiplatform.ps1 -VersionTag "v1.0.0" -ReleaseNotes "Versão 1.0.0 - Lançamento oficial"
```

O script irá:
- ✅ Compilar em modo Release
- ✅ Publicar para 4 plataformas:
  - Windows (x64)
  - Linux (x64)
  - macOS (Intel x64)
  - macOS (Apple Silicon ARM64)
- ✅ Criar arquivos ZIP para cada plataforma
- ✅ Criar tag Git
- ✅ Fazer push da tag para GitHub

### Passo 4: Criar Release no GitHub (Manual)

1. Acesse: https://github.com/henriquegfernandes/Dizimo/releases/new

2. Preencha os campos:
   - **Release title**: `Dizimo v1.0.0`
   - **Tag**: Selecione `v1.0.0` (já criada pelo script)
   - **Description**: Cole suas notas de release:
   
   ```markdown
   ## Dizimo v1.0.0 - Lançamento Oficial
   
   ### 🎁 Principais Recursos
   - ✅ Gerenciamento de dízimos
   - ✅ Relatórios e gráficos
   - ✅ Backup automático offline
   - ✅ Suporte multi-usuário
   - ✅ Interface em português
   
   ### 🖥️ Plataformas Suportadas
   - Windows 10+ (x64)
   - Linux (x64)
   - macOS 10.13+ (Intel e Apple Silicon)
   
   ### 🐛 Problemas Conhecidos
   - [Se houver, liste aqui]
   
   ### 📥 Instalação
   
   1. Download o arquivo adequado para seu SO
   2. Extraia o ZIP em uma pasta
   3. Execute o aplicativo
   
   ### 🙏 Agradecimentos
   Obrigado aos testadores beta!
   ```

3. **Upload dos arquivos**: Arraste e solte os 4 arquivos ZIP:
   - `Dizimo-v1.0.0-win-x64.zip`
   - `Dizimo-v1.0.0-linux-x64.zip`
   - `Dizimo-v1.0.0-osx-x64.zip`
   - `Dizimo-v1.0.0-osx-arm64.zip`

4. **(Opcional) Marque**: "This is a pre-release" se for versão beta/teste

5. **Clique**: "Publish release"

---

## 📊 Saídas Esperadas

### Estrutura de Diretórios Após o Build

```
Dizimo/
├── publish/
│   ├── Dizimo-v1.0.0-win-x64/      (≈300-400 MB)
│   ├── Dizimo-v1.0.0-linux-x64/    (≈300-400 MB)
│   ├── Dizimo-v1.0.0-osx-x64/      (≈300-400 MB)
│   └── Dizimo-v1.0.0-osx-arm64/    (≈300-400 MB)
├── Dizimo-v1.0.0-win-x64.zip       (≈80-120 MB)
├── Dizimo-v1.0.0-linux-x64.zip     (≈80-120 MB)
├── Dizimo-v1.0.0-osx-x64.zip       (≈80-120 MB)
└── Dizimo-v1.0.0-osx-arm64.zip     (≈80-120 MB)
```

### Tamanhos Típicos

| Plataforma | Tamanho do publish | Tamanho do ZIP |
|------------|-------------------|----------------|
| Windows (x64) | 350-400 MB | 90-120 MB |
| Linux (x64) | 350-400 MB | 90-120 MB |
| macOS (Intel) | 350-400 MB | 90-120 MB |
| macOS (ARM) | 350-400 MB | 90-120 MB |
| **Total** | **≈1.4-1.6 GB** | **≈360-480 MB** |

---

## 🔧 Troubleshooting

### ❌ Erro: "dotnet: command not found"

**Solução**: Verifique se .NET SDK 10.0 está instalado:
```bash
dotnet --version
# Deve mostrar: 10.0.x ou superior

# Se não tiver, instale de: https://dotnet.microsoft.com/download
```

### ❌ Erro: "Tag already exists"

```bash
# Delete a tag local
git tag -d v1.0.0

# Delete no GitHub
git push origin --delete v1.0.0

# Recriar executando o script novamente
./publish-release.sh v1.0.0 "descrição"
```

### ❌ Erro: "ZIP file not found"

Significa que o build ou publish falhou. Verifique:
1. Saída do comando `dotnet publish`
2. Espaço em disco disponível (≈2GB mínimo)
3. Permissões de escrita na pasta

### ❌ Script não tem permissão de execução (Linux/macOS)

```bash
chmod +x publish-release.sh
./publish-release.sh v1.0.0
```

### ❌ Build falha para plataforma específica

Alguns motivos possíveis:
1. Código específico do Windows (use `[SupportedOSPlatform]` se necessário)
2. Dependência não compatível
3. Sistema operacional sem ferramentas necessárias

**Solução**: Verifique a saída completa:
```bash
./publish-release.sh v1.0.0 2>&1 | tail -100
```

---

## 📚 Notas de Release Profissionais

### Exemplo Beta 1:
```markdown
## Dizimo v1.0.0-beta.1

### 🎯 Objetivos Beta
Esta é a primeira versão beta do Dizimo. Procuramos feedback sobre:
- Estabilidade geral
- Performance
- Interface de usuário

### 🎁 Principais Recursos
- Gerenciamento de dízimos
- Relatórios básicos
- Backup automático offline
- Suporte multi-usuário

### ⚠️ Problemas Conhecidos
- Gráficos podem ser lentos com muitos registros
- Backup em nuvem ainda não implementado

### 🙋 Procuramos Testadores
Para reportar bugs: https://github.com/henriquegfernandes/Dizimo/issues
```

### Exemplo Release Candidato:
```markdown
## Dizimo v1.0.0-rc.1

### ✅ Correções
- Crash ao abrir gráficos com muitos dados
- Sincronização ao adicionar usuário
- Lentidão ao filtrar dízimos

### ✨ Novos Recursos
- Exportação para CSV
- Tema escuro
- Atalhos de teclado

### 📈 Melhorias
- 40% de melhoria em performance de carregamento
- Interface de relatórios melhorada
- Validação de campos aprimorada

### 📊 Estatísticas
- 15 issues resolvidas
- 3 novos recursos
- 200+ linhas de código refatoradas
```

---

## 🔄 Versões Subsequentes

Para betas posteriores ou versions finais:

### Via Bash/macOS:
```bash
./publish-release.sh v1.0.0-beta.2 "Beta 2 com melhorias"
./publish-release.sh v1.0.0-rc.1 "Release Candidate 1"
./publish-release.sh v1.0.0 "Versão 1.0 estável"
```

### Via Windows PowerShell:
```powershell
.\publish-release-multiplatform.ps1 -VersionTag "v1.0.0-beta.2"
.\publish-release-multiplatform.ps1 -VersionTag "v1.0.0-rc.1"
.\publish-release-multiplatform.ps1 -VersionTag "v1.0.0"
```

---

## 💡 Dicas

1. **Sem downloads por enquanto?** Use `--dry-run` mentalmente:
   - Execute o script
   - Verifique que os ZIPs foram criados
   - Não faça push da tag (`git push --delete origin v1.0.0` se precisar reverter)

2. **Automatizar releases futuras?** Configure um GitHub Actions workflow na pasta `.github/workflows/`

3. **Verificar downloads?** No GitHub Releases, você verá contadores de download por arquivo

4. **Distribuir entre plataformas?** Oriente usuários a:
   - Windows: Baixar `win-x64`
   - Linux: Baixar `linux-x64`
   - macOS Intel: Baixar `osx-x64`
   - macOS Apple Silicon (M1/M2/M3): Baixar `osx-arm64`

---

## 🚀 Próximos Passos

- [ ] Executar o script de release
- [ ] Verificar se os 4 arquivos ZIP foram criados
- [ ] Fazer upload dos ZIPs no GitHub
- [ ] Compartilhar link do release com usuários
- [ ] Coletar feedback dos usuários
- [ ] Planejar versão seguinte

---

## 📞 Suporte

- Issues: https://github.com/henriquegfernandes/Dizimo/issues
- Discussões: https://github.com/henriquegfernandes/Dizimo/discussions

