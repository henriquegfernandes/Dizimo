# ?? Guia de Publicação Manual - Dizimo Beta Release

## ? Versão Rápida (5 minutos)

### Passo 1: Compilar e Empacotar

```powershell
cd C:\Projects\Dizimo
.\publish-release.ps1
```

Isso vai:
- ? Compilar o projeto
- ? Criar pasta `publish/` com a app
- ? Gerar arquivo ZIP
- ? Criar tag Git
- ? Enviar tag para GitHub

### Passo 2: Criar Release no GitHub (Manual)

1. **Acesse o link:**
   https://github.com/henriquegfernandes/Dizimo/releases/new

2. **Preencha os campos:**
   - **Release title:** `Dizimo v1.0.0-beta.1`
   - **Tag:** Selecione `v1.0.0-beta.1` (já criada pelo script)
   - **Description:** Cole suas notas de release
   - **Check:** Marque "This is a pre-release" ??

3. **Faça upload do arquivo:**
   - Clique "Choose a file"
   - Selecione: `Dizimo-v1.0.0-beta.1-windows.zip`

4. **Publique:**
   - Clique "Publish release"

? **Pronto! Release está ao vivo!**

---

## ?? Processo Completo Passo a Passo

### Preparação Inicial (primeira vez)

Antes de começar, verifique:

```powershell
# 1. Estar na pasta do projeto
cd C:\Projects\Dizimo

# 2. Verificar status Git
git status

# 3. Fazer commit de qualquer mudança pendente
git add .
git commit -m "Preparação para release beta 1"
git push origin main
```

### Step 1: Atualizar Versão (opcional)

Edite `Dizimo\Dizimo.csproj`:

```xml
<PropertyGroup>
    <!-- Atualize para a nova versão -->
    <ApplicationDisplayVersion>1.0.0-beta.1</ApplicationDisplayVersion>
    <InformationalVersion>1.0.0-beta.1</InformationalVersion>
</PropertyGroup>
```

Se fez mudanças:
```powershell
git add Dizimo/Dizimo.csproj
git commit -m "Bump version to 1.0.0-beta.1"
git push origin main
```

### Step 2: Compilar e Empacotar

```powershell
cd C:\Projects\Dizimo
.\publish-release.ps1 -VersionTag "v1.0.0-beta.1" -ReleaseNotes "Primeira versão beta"
```

**Opções:**

```powershell
# Apenas versão (usa notas padrão)
.\publish-release.ps1 -VersionTag "v1.0.0-beta.2"

# Com notas customizadas
.\publish-release.ps1 `
    -VersionTag "v1.0.0-beta.1" `
    -ReleaseNotes "Versão beta 1 com suporte a backup"

# Versão final (não beta)
.\publish-release.ps1 -VersionTag "v1.0.0" -ReleaseNotes "Versão 1.0.0 estável"
```

O script vai:
- Compilar em Release
- Criar pasta `publish/`
- Gerar ZIP: `Dizimo-v1.0.0-beta.1-windows.zip`
- Criar tag Git e enviar para GitHub

### Step 3: Criar Release no GitHub (Manual)

**Opção A: Via Navegador (Recomendado)**

1. Acesse: https://github.com/henriquegfernandes/Dizimo/releases/new

2. Preencha:
   ```
   Release title: Dizimo v1.0.0-beta.1
   
   Tag: v1.0.0-beta.1 (selecione na dropdown)
   
   Description:
   ## O que há de novo ??
   
   - Suporte a backup automático
   - Interface melhorada
   - Correção de bugs
   
   ## Como testar
   - Baixe o arquivo ZIP
   - Extraia em uma pasta
   - Execute Dizimo.exe
   
   ## Feedback
   Por favor, reporte bugs em: [Issues](https://github.com/henriquegfernandes/Dizimo/issues)
   ```

3. Marque: ?? **This is a pre-release**

4. Clique: "Attach binaries" ou arraste o arquivo ZIP

5. Clique: **"Publish release"**

**Opção B: Via GitHub CLI (Avançado)**

Se tiver [GitHub CLI](https://cli.github.com/) instalado:

```powershell
# Login (primeira vez)
gh auth login

# Criar release
gh release create v1.0.0-beta.1 \
    --title "Dizimo v1.0.0-beta.1" \
    --notes "Primeira versão beta" \
    --prerelease \
    Dizimo-v1.0.0-beta.1-windows.zip
```

---

## ?? Fluxo Completo de Exemplo

Exemplo real de publicação:

```powershell
# 1. Navegar para a pasta
cd C:\Projects\Dizimo

# 2. Verificar que tudo está commitado
git status
# (deve mostrar: working tree clean)

# 3. Executar script de publicação
.\publish-release.ps1 -VersionTag "v1.0.0-beta.1" -ReleaseNotes "Beta 1 com melhorias de UI"

# Resultado:
# ? Compilação concluída
# ? Aplicação publicada
# ? Arquivo ZIP criado: Dizimo-v1.0.0-beta.1-windows.zip
# ? Tag criada: v1.0.0-beta.1
# ? Tag enviada para GitHub

# 4. Abrir GitHub no navegador
Start-Process "https://github.com/henriquegfernandes/Dizimo/releases/new"

# 5. Preencher formulário (conforme instruções acima)
# 6. Fazer upload de: Dizimo-v1.0.0-beta.1-windows.zip
# 7. Clicar "Publish release"

# 8. (Opcional) Verificar no navegador
Start-Process "https://github.com/henriquegfernandes/Dizimo/releases"
```

---

## ?? Notas de Release Profissionais

### Exemplo Beta 1:

```markdown
## Dizimo v1.0.0-beta.1

### ?? Objetivos da Beta
Esta é a primeira versão beta do Dizimo. Estamos buscando feedback sobre:
- Estabilidade geral
- Performance
- Interface de usuário

### ? Principais Recursos
- Gerenciamento de dízimos
- Relatórios básicos
- Backup local
- Suporte a múltiplos usuários

### ?? Problemas Conhecidos
- [Issue #1] Gráficos podem ser lentos com muitos registros
- [Issue #2] Backup de nuvem ainda não implementado

### ?? Testadores Necessários
Procuramos por:
- Testadores interessados em dar feedback
- Relatórios de bugs
- Sugestões de melhorias

### ?? Como Relatar Bugs
1. Acesse: https://github.com/henriquegfernandes/Dizimo/issues
2. Clique "New issue"
3. Descreva o problema detalhadamente

### ?? Download
Disponível em: [Assets abaixo](#assets)
```

### Exemplo Beta 2:

```markdown
## Dizimo v1.0.0-beta.2

### ?? O que mudou

#### ? Corrigido
- Crash ao abrir gráficos com muitos dados
- Problema de sincronização ao adicionar usuário
- Lentidão ao filtrar ofertas

#### ? Novo
- Exportação para CSV
- Tema escuro
- Atalhos de teclado

#### ?? Melhorado
- Performance de carregamento em 40%
- Interface de relatórios
- Validação de campos

### ?? Estatísticas
- 15 issues resolvidas
- 3 features novas
- 200+ linhas de código refatoradas

### ?? Agradecimentos
Obrigado aos beta testers que reportaram bugs!
```

---

## ? Troubleshooting

### ? Erro: "Tag já existe"

```powershell
# Deletar tag local
git tag -d v1.0.0-beta.1

# Deletar no GitHub
git push origin --delete v1.0.0-beta.1

# Recriar
.\publish-release.ps1 -VersionTag "v1.0.0-beta.1"
```

### ? Erro: "Arquivo ZIP não encontrado"

```powershell
# Verificar se foi criado
ls Dizimo-v*.zip

# Se não existir, o script teve erro de compilação
# Verifique a saída acima
```

### ? Não consigo fazer upload na interface do GitHub

**Solução A: Drag and drop**
- Arraste o ZIP diretamente no campo de descrição

**Solução B: Botão de upload**
- Procure por "Attach binaries" ou "Add files"

**Solução C: GitHub CLI**
```powershell
gh release upload v1.0.0-beta.1 Dizimo-v1.0.0-beta.1-windows.zip
```

### ? Release não aparece

- Verifique se clicou "Publish release" (não "Save as draft")
- Aguarde alguns segundos e recarregue a página
- Confirme que é um "pre-release" (não draft)

---

## ? Checklist de Publicação

Antes de cada release:

- [ ] Testes executados localmente
- [ ] Código commitado e enviado (git push)
- [ ] Versão atualizada no `.csproj`
- [ ] Notas de release preparadas
- [ ] Script executado com sucesso
- [ ] Arquivo ZIP verificado
- [ ] Release criado no GitHub
- [ ] Arquivo ZIP enviado
- [ ] Release marcado como "pre-release"
- [ ] Link compartilhado com testadores

---

## ?? Próximas Versões

Para beta 2, 3, etc:

```powershell
.\publish-release.ps1 -VersionTag "v1.0.0-beta.2" -ReleaseNotes "Beta 2 com correções"
.\publish-release.ps1 -VersionTag "v1.0.0-beta.3" -ReleaseNotes "Beta 3 final"
```

Para versão final:

```powershell
.\publish-release.ps1 -VersionTag "v1.0.0" -ReleaseNotes "Versão 1.0 estável"
```

---

## ?? Suporte

Dúvidas?
- GitHub Issues: https://github.com/henriquegfernandes/Dizimo/issues
- GitHub Discussions: https://github.com/henriquegfernandes/Dizimo/discussions
