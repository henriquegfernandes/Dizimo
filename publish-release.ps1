# Script de publicação manual do Dizimo para GitHub Release Beta
# Execute este script para compilar, empacotar e criar um release no GitHub

param(
    [string]$VersionTag = "v1.0.0-beta.1",
    [string]$ReleaseNotes = "Initial beta release"
)

$ErrorActionPreference = "Stop"

Write-Host "?? Iniciando publicação manual do Dizimo..." -ForegroundColor Cyan
Write-Host ""

# 1. Verificar Git
Write-Host "1??  Verificando repositório Git..." -ForegroundColor Yellow
if (-not (Test-Path ".git")) {
    Write-Host "? Não estamos em um repositório Git!" -ForegroundColor Red
    exit 1
}
Write-Host "? Repositório Git encontrado" -ForegroundColor Green

# 2. Compilar
Write-Host "`n2??  Compilando projeto (Release)..." -ForegroundColor Yellow
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Erro na compilação!" -ForegroundColor Red
    exit 1
}
Write-Host "? Compilação concluída com sucesso" -ForegroundColor Green

# 3. Publicar
Write-Host "`n3??  Publicando aplicação..." -ForegroundColor Yellow
$publishDir = "publish"
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

dotnet publish Dizimo/Dizimo.csproj -c Release -o $publishDir
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Erro na publicação!" -ForegroundColor Red
    exit 1
}
Write-Host "? Aplicação publicada em: $publishDir" -ForegroundColor Green

# 4. Criar ZIP
Write-Host "`n4??  Criando arquivo comprimido..." -ForegroundColor Yellow
$zipName = "Dizimo-$VersionTag-windows.zip"
if (Test-Path $zipName) {
    Remove-Item $zipName
}

Compress-Archive -Path $publishDir -DestinationPath $zipName
Write-Host "? Arquivo criado: $zipName" -ForegroundColor Green

# 5. Criar Tag Git
Write-Host "`n5??  Criando Git tag: $VersionTag..." -ForegroundColor Yellow
$tagExists = git tag -l $VersionTag
if ($tagExists) {
    Write-Host "??  Tag $VersionTag já existe. Deseja deletá-la e recriar? (s/n)" -ForegroundColor Yellow
    $response = Read-Host
    if ($response -eq "s") {
        git tag -d $VersionTag
        git push origin --delete $VersionTag
        Write-Host "? Tag antiga deletada" -ForegroundColor Green
    } else {
        Write-Host "Operação cancelada" -ForegroundColor Yellow
        exit 0
    }
}

git tag -a $VersionTag -m "Release: $VersionTag`n`n$ReleaseNotes"
git push origin $VersionTag
Write-Host "? Tag criada e enviada para GitHub" -ForegroundColor Green

# 6. Instruções finais
Write-Host "`n" -ForegroundColor Green
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "? PREPARAÇÃO CONCLUÍDA!" -ForegroundColor Green
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "?? Arquivo pronto: $zipName" -ForegroundColor Cyan
Write-Host "???  Tag Git: $VersionTag" -ForegroundColor Cyan
Write-Host ""
Write-Host "??  PRÓXIMA ETAPA - Criar Release no GitHub (manual):" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Acesse: https://github.com/henriquegfernandes/Dizimo/releases/new" -ForegroundColor White
Write-Host ""
Write-Host "2. Preencha os campos:" -ForegroundColor White
Write-Host "   • Release title: Dizimo $VersionTag" -ForegroundColor Cyan
Write-Host "   • Tag: Selecione '$VersionTag'" -ForegroundColor Cyan
Write-Host "   • Description:" -ForegroundColor Cyan
Write-Host "     $ReleaseNotes" -ForegroundColor Cyan
Write-Host "   • Marque: ? This is a pre-release" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. Clique em 'Choose a file' e selecione:" -ForegroundColor White
Write-Host "   $zipName" -ForegroundColor Cyan
Write-Host ""
Write-Host "4. Clique em 'Publish release'" -ForegroundColor White
Write-Host ""
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "Link do repositório:" -ForegroundColor Cyan
Write-Host "https://github.com/henriquegfernandes/Dizimo" -ForegroundColor White
Write-Host "????????????????????????????????????????????????????????????" -ForegroundColor Green
