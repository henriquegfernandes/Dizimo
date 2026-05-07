using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using Dizimo.Application.Dizimistas.Commands;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Dizimistas.Queries;
using Dizimo.Services;
using Dizimo.Application.Reporting.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.IO;

namespace Dizimo.ViewModels;

public partial class DizimistaCadastroViewModel(CreateDizimistaHandler createHandler, UpdateDizimistaHandler updateHandler, GetDizimistaHandlers getHandler, INavigationService navigationService, IDialogService dialogService) : ObservableObject, INavigationAware
{
    private readonly CreateDizimistaHandler _createHandler = createHandler;
    private readonly UpdateDizimistaHandler _updateHandler = updateHandler;
    private readonly GetDizimistaHandlers _getHandler = getHandler;
    private readonly INavigationService _navigationService = navigationService;
    private readonly IDialogService _dialogService = dialogService;

    private int _numeroCadastro;
    public int NumeroCadastro
    {
        get => _numeroCadastro;
        set => SetProperty(ref _numeroCadastro, value);
    }

    private string _nome = string.Empty;
    public string Nome
    {
        get => _nome;
        set => SetProperty(ref _nome, value);
    }

    private DateTime _dataNascimento = DateTime.Today;
    public DateTime DataNascimento
    {
        get => _dataNascimento;
        set => SetProperty(ref _dataNascimento, value);
    }

    private bool _ativo = true;
    public bool Ativo
    {
        get => _ativo;
        set => SetProperty(ref _ativo, value);
    }

    private Guid _id;
    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private string _telefone = string.Empty;
    public string Telefone
    {
        get => _telefone;
        set => SetProperty(ref _telefone, value);
    }

    private string _whatsapp = string.Empty;
    public string Whatsapp
    {
        get => _whatsapp;
        set => SetProperty(ref _whatsapp, value);
    }

    private DateTime _dataCadastro = DateTime.Today;
    public DateTime DataCadastro
    {
        get => _dataCadastro;
        set => SetProperty(ref _dataCadastro, value);
    }

    private string _rua = string.Empty;
    public string Rua
    {
        get => _rua;
        set => SetProperty(ref _rua, value);
    }

    private string _numero = string.Empty;
    public string Numero
    {
        get => _numero;
        set => SetProperty(ref _numero, value);
    }

    private string _bairro = string.Empty;
    public string Bairro
    {
        get => _bairro;
        set => SetProperty(ref _bairro, value);
    }

    private string _cidade = "Osasco";
    public string Cidade
    {
        get => _cidade;
        set => SetProperty(ref _cidade, value);
    }

    private string _uf = "SP";
    public string Uf
    {
        get => _uf;
        set => SetProperty(ref _uf, value);
    }

    private string _cep = string.Empty;
    public string Cep
    {
        get => _cep;
        set => SetProperty(ref _cep, value);
    }

    private string _complemento = string.Empty;
    public string Complemento
    {
        get => _complemento;
        set => SetProperty(ref _complemento, value);
    }

    public List<string> EstadosBrasileiros { get; } =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    public Endereco Endereco => new()
    {
        Rua = Rua,
        Numero = Numero,
        Complemento = _complemento,
        Bairro = Bairro,
        Cidade = Cidade,
        UF = Uf,
        CEP = Cep
    };

    /// <summary>
    /// Remove caracteres especiais de campos numericos
    /// </summary>
    private void LimparCamposNumericos()
    {
        // Remove caracteres nao numericos do telefone
        Telefone = new string([.. Telefone.Where(char.IsDigit)]);
        
        // Remove caracteres nao numericos do whatsapp
        Whatsapp = new string([.. Whatsapp.Where(char.IsDigit)]);
        
        // Remove caracteres nao numericos do CEP
        Cep = new string([.. Cep.Where(char.IsDigit)]);
        
        // Remove caracteres nao numericos do numero
        Numero = new string([.. Numero.Where(char.IsDigit)]);
    }

    /// <summary>
    /// Valida os campos de telefone, whatsapp e CEP
    /// </summary>
    /// <returns>Mensagem de erro, ou null se valido</returns>
    private string? ValidarCampos()
    {
        // Contar apenas digitos do telefone
        var telefoneLimpo = new string([.. Telefone.Where(char.IsDigit)]);
        
        // Contar apenas digitos do whatsapp
        var whatsappLimpo = new string([.. Whatsapp.Where(char.IsDigit)]);
        
        // Contar apenas digitos do CEP
        var cepLimpo = new string([.. Cep.Where(char.IsDigit)]);

        // Validar telefone
        if (!string.IsNullOrWhiteSpace(telefoneLimpo))
        {
            if (telefoneLimpo.Length < 10 || telefoneLimpo.Length > 11)
            {
                return "Telefone deve conter entre 10 e 11 digitos.";
            }
        }

        // Validar whatsapp
        if (!string.IsNullOrWhiteSpace(whatsappLimpo))
        {
            if (whatsappLimpo.Length < 10 || whatsappLimpo.Length > 11)
            {
                return "WhatsApp deve conter entre 10 e 11 digitos.";
            }
        }

        // Validar CEP
        if (!string.IsNullOrWhiteSpace(cepLimpo))
        {
            if (cepLimpo.Length != 8)
            {
                return "CEP deve conter exatamente 8 digitos.";
            }
        }

        return null;
    }

    public async void OnNavigatedTo(NavigationParameters parameters)
    {
        // Tenta extrair o ID do parâmetro de navegação
        Guid? dizimistaId = null;
        
        if (parameters != null && parameters.TryGetValue("id", out var idObj))
        {
            if (idObj is Guid guidId)
                dizimistaId = guidId;
            else if (Guid.TryParse(idObj?.ToString(), out var parsedId))
                dizimistaId = parsedId;
        }

        if (dizimistaId.HasValue && dizimistaId.Value != Guid.Empty)
        {
            var dizimista = await _getHandler.Handle(new GetDizimistaByIdQuery(dizimistaId.Value));
            if (dizimista != null)
            {
                Id = dizimista.Id;
                NumeroCadastro = dizimista.NumeroCadastro;
                Nome = dizimista.Nome;
                DataNascimento = dizimista.DataNascimento;
                Ativo = dizimista.Ativo;
                Telefone = dizimista.Telefone;
                Whatsapp = dizimista.Whatsapp;
                DataCadastro = dizimista.DataCadastro;
                Rua = dizimista.Endereco?.Rua ?? string.Empty;
                Numero = dizimista.Endereco?.Numero ?? string.Empty;
                Complemento = dizimista.Endereco?.Complemento ?? string.Empty;
                Bairro = dizimista.Endereco?.Bairro ?? string.Empty;
                Cidade = dizimista.Endereco?.Cidade ?? "Osasco";
                Uf = dizimista.Endereco?.UF ?? "SP";
                Cep = dizimista.Endereco?.CEP ?? string.Empty;
                IsEditMode = true;
            }
        }
        else
        {
            LimparCampos();
        }
    }

    public void OnNavigatedFrom()
    {
        // Lógica ao sair da página se necessário
    }

    private void LimparCampos()
    {
        Id = Guid.Empty;
        NumeroCadastro = 0;
        Nome = string.Empty;
        DataNascimento = DateTime.Today;
        Ativo = true;
        Telefone = string.Empty;
        Whatsapp = string.Empty;
        DataCadastro = DateTime.Today;
        Rua = string.Empty;
        Numero = string.Empty;
        Complemento = string.Empty;
        Bairro = string.Empty;
        Cidade = "Osasco";
        Uf = "SP";
        Cep = string.Empty;
        IsEditMode = false;
    }

    [RelayCommand]
    public async Task SalvarAsync()
    {
        // Limpar caracteres especiais antes de salvar
        LimparCamposNumericos();
        
        // Validar campos
        var erroValidacao = ValidarCampos();
        if (!string.IsNullOrEmpty(erroValidacao))
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] {erroValidacao}");
            return;
        }

        // Validacao de codigo duplicado ao cadastrar novo dizimista
        if (!IsEditMode && NumeroCadastro > 0)
        {
            var dizimistaExistente = await _getHandler.Handle(new GetDizimistaByNumeroCadastroQuery(NumeroCadastro));
            if (dizimistaExistente != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Código duplicado: {NumeroCadastro}");
                return;
            }
        }

        // Ao editar, verificar se o codigo foi alterado e ja existe outro dizimista com o novo codigo
        if (IsEditMode && NumeroCadastro > 0)
        {
            var dizimistaExistente = await _getHandler.Handle(new GetDizimistaByNumeroCadastroQuery(NumeroCadastro));
            if (dizimistaExistente != null && dizimistaExistente.Id != Id)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Código duplicado ao editar: {NumeroCadastro}");
                return;
            }
        }
        
        if (IsEditMode)
        {
            await _updateHandler.Handle(new UpdateDizimistaCommand(Id, NumeroCadastro, Nome, DataNascimento, Ativo, Endereco, Telefone, Whatsapp, DataCadastro));
        }
        else
        {
            await _createHandler.Handle(new CreateDizimistaCommand(NumeroCadastro, Nome, DataNascimento, Endereco, Telefone, Whatsapp, DataCadastro));
        }
        
        System.Diagnostics.Debug.WriteLine($"[INFO] Dizimista salvo com sucesso");
        _navigationService.Navigate("dizimistas");
    }

    [RelayCommand]
    public async Task BaixarModeloAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[INFO] BaixarModeloAsync iniciado");
            
            var fileName = $"dizimista_modelo_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                try
                {
                    var storageProvider = desktop.MainWindow.StorageProvider;
                    
                    if (storageProvider != null)
                    {
                        var file = await storageProvider.SaveFilePickerAsync(new()
                        {
                            Title = "Salvar Planilha Modelo",
                            DefaultExtension = "xlsx",
                            FileTypeChoices = new[] { new FilePickerFileType("Arquivo Excel") { Patterns = new[] { "*.xlsx" } } },
                            SuggestedFileName = fileName
                        });

                        if (file != null)
                        {
                            var excelStream = DizimistaExcelService.GerarModelo();
                            await using var fileStream = await file.OpenWriteAsync();
                            await fileStream.WriteAsync(excelStream.ToArray());
                            System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo modelo salvo com sucesso em: {file.Path}");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao abrir file picker: {ex.Message}");
                }
            }

            // Fallback: salvar em Downloads
            var excelStreamFallback = DizimistaExcelService.GerarModelo();
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
                Directory.CreateDirectory(downloadsPath);

            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, excelStreamFallback.ToArray());

            System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo modelo salvo em: {filePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao baixar modelo: {ex.Message}");
        }
    }

    [RelayCommand]
    public void Voltar()
    {
        try
        {
            _navigationService.GoBack();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao voltar: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ImportarAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[INFO] ImportarAsync iniciado");
            
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                try
                {
                    var storageProvider = desktop.MainWindow.StorageProvider;
                    if (storageProvider != null)
                    {
                        var files = await storageProvider.OpenFilePickerAsync(new()
                        {
                            Title = "Selecionar Planilha para Importar",
                            AllowMultiple = false,
                            FileTypeFilter = new[] { new FilePickerFileType("Arquivos Excel") { Patterns = new[] { "*.xlsx", "*.xls" } } }
                        });

                        if (files.Count > 0)
                        {
                            var file = files[0];
                            System.Diagnostics.Debug.WriteLine($"[INFO] Arquivo selecionado: {file.Name}");

                            // Ler arquivo
                            await using var stream = await file.OpenReadAsync();
                            using var memoryStream = new MemoryStream();
                            await stream.CopyToAsync(memoryStream);
                            var excelBytes = memoryStream.ToArray();

                            // Importar dizimistas do arquivo
                            var dizimistasImportados = await DizimistaExcelService.ImportarAsync(excelBytes);
                            
                            if (dizimistasImportados.Count == 0)
                            {
                                await _dialogService.ShowAlertAsync("Importação", "Nenhum dizimista foi encontrado no arquivo.");
                                return;
                            }

                            System.Diagnostics.Debug.WriteLine($"[INFO] {dizimistasImportados.Count} dizimistas lidos da planilha");

                            // Salvar cada dizimista importado
                            int sucessos = 0;
                            int erros = 0;

                            foreach (var dizimista in dizimistasImportados)
                            {
                                try
                                {
                                    // Verificar se já existe um dizimista com o mesmo número de cadastro
                                    var existente = await _getHandler.Handle(new GetDizimistaByNumeroCadastroQuery(dizimista.NumeroCadastro));
                                    if (existente != null)
                                    {
                                        erros++;
                                        continue;
                                    }

                                    // Criar novo dizimista
                                    var cmd = new CreateDizimistaCommand(
                                        dizimista.NumeroCadastro,
                                        dizimista.Nome,
                                        dizimista.DataNascimento,
                                        dizimista.Endereco,
                                        dizimista.Telefone,
                                        dizimista.Whatsapp,
                                        dizimista.DataCadastro
                                    );
                                    
                                    await _createHandler.Handle(cmd);
                                    sucessos++;
                                }
                                catch (Exception ex)
                                {
                                    erros++;
                                    System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao importar dizimista: {ex.Message}");
                                }
                            }

                            System.Diagnostics.Debug.WriteLine($"[INFO] Importação concluída: {sucessos} sucesso(s), {erros} erro(s)");

                            // Mostrar resultado
                            var mensagem = $"Importação concluída!\n\n✓ {sucessos} dizimista(s) importado(s) com sucesso";
                            if (erros > 0)
                                mensagem += $"\n✗ {erros} erro(s) durante a importação";

                            mensagem += "\n\nDeseja cadastrar outro dizimista ou voltar para a lista?";

                            var result = await _dialogService.ShowConfirmAsync(
                                "Importação Concluída",
                                mensagem,
                                "Cadastrar Outro",
                                "Voltar para Lista");

                            if (!result)
                            {
                                _navigationService.Navigate("dizimistas");
                            }
                            else
                            {
                                LimparCampos();
                            }
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao abrir file picker: {ex.Message}");
                    await _dialogService.ShowErrorAsync($"Erro ao abrir file picker: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine("[ERRO] StorageProvider não disponível");
            await _dialogService.ShowErrorAsync("StorageProvider não disponível");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao importar: {ex.Message}");
            await _dialogService.ShowErrorAsync($"Erro ao importar: {ex.Message}");
        }
    }
}

