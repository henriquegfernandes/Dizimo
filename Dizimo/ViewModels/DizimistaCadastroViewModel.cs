using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Application.Dizimistas.Commands;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Dizimistas.Queries;
using Dizimo.Services;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls;

namespace Dizimo.ViewModels;

public partial class DizimistaCadastroViewModel : ObservableObject, IQueryAttributable
{
    private readonly CreateDizimistaHandler _createHandler;
    private readonly UpdateDizimistaHandler _updateHandler;
    private readonly GetDizimistaHandlers _getHandler;

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

    public List<string> EstadosBrasileiros { get; } = new()
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    public DizimistaCadastroViewModel(CreateDizimistaHandler createHandler, UpdateDizimistaHandler updateHandler, GetDizimistaHandlers getHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _getHandler = getHandler;
    }

    public Endereco Endereco => new Endereco
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
    /// Remove caracteres especiais de campos numéricos
    /// </summary>
    private void LimparCamposNumericos()
    {
        // Remove caracteres não numéricos do telefone
        Telefone = new string(Telefone.Where(char.IsDigit).ToArray());
        
        // Remove caracteres não numéricos do whatsapp
        Whatsapp = new string(Whatsapp.Where(char.IsDigit).ToArray());
        
        // Remove caracteres não numéricos do CEP
        Cep = new string(Cep.Where(char.IsDigit).ToArray());
        
        // Remove caracteres não numéricos do número
        Numero = new string(Numero.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Valida os campos de telefone, whatsapp e CEP
    /// </summary>
    /// <returns>Mensagem de erro, ou null se válido</returns>
    private string? ValidarCampos()
    {
        // Contar apenas dígitos do telefone
        var telefoneLimpo = new string(Telefone.Where(char.IsDigit).ToArray());
        
        // Contar apenas dígitos do whatsapp
        var whatsappLimpo = new string(Whatsapp.Where(char.IsDigit).ToArray());
        
        // Contar apenas dígitos do CEP
        var cepLimpo = new string(Cep.Where(char.IsDigit).ToArray());

        // Validar telefone
        if (!string.IsNullOrWhiteSpace(telefoneLimpo))
        {
            if (telefoneLimpo.Length < 10 || telefoneLimpo.Length > 11)
            {
                return "Telefone deve conter entre 10 e 11 dígitos.";
            }
        }

        // Validar whatsapp
        if (!string.IsNullOrWhiteSpace(whatsappLimpo))
        {
            if (whatsappLimpo.Length < 10 || whatsappLimpo.Length > 11)
            {
                return "WhatsApp deve conter entre 10 e 11 dígitos.";
            }
        }

        // Validar CEP
        if (!string.IsNullOrWhiteSpace(cepLimpo))
        {
            if (cepLimpo.Length != 8)
            {
                return "CEP deve conter exatamente 8 dígitos.";
            }
        }

        return null;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idObj) && Guid.TryParse(idObj?.ToString(), out var dizimistaId))
        {
            var dizimista = await _getHandler.Handle(new GetDizimistaByIdQuery(dizimistaId));
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
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlertAsync("Erro de Validação", erroValidacao, "OK");
            }
            return;
        }

        // Validação de código duplicado ao cadastrar novo dizimista
        if (!IsEditMode && NumeroCadastro > 0)
        {
            var dizimistaExistente = await _getHandler.Handle(new GetDizimistaByNumeroCadastroQuery(NumeroCadastro));
            if (dizimistaExistente != null)
            {
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Código Duplicado", $"Já existe um dizimista cadastrado com o código {NumeroCadastro}. Por favor, insira um código diferente.", "OK");
                }
                return;
            }
        }

        // Ao editar, verificar se o código foi alterado e já existe outro dizimista com o novo código
        if (IsEditMode && NumeroCadastro > 0)
        {
            var dizimistaExistente = await _getHandler.Handle(new GetDizimistaByNumeroCadastroQuery(NumeroCadastro));
            if (dizimistaExistente != null && dizimistaExistente.Id != Id)
            {
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlertAsync("Código Duplicado", $"Já existe outro dizimista cadastrado com o código {NumeroCadastro}. Por favor, insira um código diferente.", "OK");
                }
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
        
        if (IsEditMode)
        {
            // Se está editando, apenas voltar à lista
            await Shell.Current.GoToAsync("..", true);
        }
        else
        {
            // Se é novo cadastro, perguntar se deseja cadastrar outro
            var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                var resultado = await mainPage.DisplayAlertAsync(
                    "Sucesso",
                    "Dizimista cadastrado com sucesso! Deseja cadastrar outro dizimista?",
                    "Sim",
                    "Não");

                if (resultado)
                {
                    // Limpar o formulário para novo cadastro
                    LimparCampos();
                }
                else
                {
                    // Ir para a lista de dizimistas
                    await Shell.Current.GoToAsync("..", true);
                }
            }
            else
            {
                await Shell.Current.GoToAsync("..", true);
            }
        }
    }

    [RelayCommand]
    public async Task BaixarModeloAsync()
    {
        try
        {
            var excelService = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services.GetService<DizimistaExcelService>();

            if (excelService == null)
            {
                var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlertAsync("Erro", "Serviço de Excel não está disponível.", "OK");
                return;
            }

            var templateStream = excelService.GerarModelo();
            var fileName = $"dizimista_modelo_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

#if WINDOWS
            var result = await CommunityToolkit.Maui.Storage.FileSaver.Default.SaveAsync(fileName, templateStream, CancellationToken.None);

            var mainPageResult = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPageResult != null)
            {
                if (result.IsSuccessful)
                    await mainPageResult.DisplayAlertAsync("Sucesso", "Planilha modelo baixada com sucesso!", "OK");
                else
                    await mainPageResult.DisplayAlertAsync("Erro", "Erro ao salvar o arquivo.", "OK");
            }
#else
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            if (!Directory.Exists(downloadsPath))
            {
                Directory.CreateDirectory(downloadsPath);
            }

            var filePath = Path.Combine(downloadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, templateStream.ToArray());

            var mainPageSuccess = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPageSuccess != null)
                await mainPageSuccess.DisplayAlertAsync("Sucesso", 
                    $"Planilha modelo baixada com sucesso!\n\nLocalização: {filePath}", "OK");
#endif
        }
        catch (Exception ex)
        {
            var mainPageError = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPageError != null)
                await mainPageError.DisplayAlertAsync("Erro", $"Erro ao baixar modelo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task ImportarAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione a planilha de dizimistas",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".xlsx" } },
                    { DevicePlatform.macOS, new[] { ".xlsx" } },
                    { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } },
                    { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                }),
            });

            if (result == null)
                return;

            var excelService = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services.GetService<DizimistaExcelService>();

            if (excelService == null)
            {
                var mainPageNull = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPageNull != null)
                    await mainPageNull.DisplayAlertAsync("Erro", "Serviço de Excel não está disponível.", "OK");
                return;
            }

            using (var stream = await result.OpenReadAsync())
            {
                var fileBytes = new byte[stream.Length];
                await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
                var dizimistas = await excelService.ImportarAsync(fileBytes);

                if (dizimistas.Count > 0)
                {
                    var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (mainPage != null)
                    {
                        bool confirmar = await mainPage.DisplayAlertAsync(
                            "Confirmar Importação",
                            $"{dizimistas.Count} dizimista(s) encontrado(s). Deseja importar?",
                            "Sim", "Não");

                        if (confirmar)
                        {
                            LimparCampos();
                            // Mensagem de sucesso
                            await mainPage.DisplayAlertAsync("Sucesso", 
                                $"{dizimistas.Count} dizimista(s) importado(s) com sucesso!", "OK");
                        }
                    }
                }
                else
                {
                    var mainPageEmpty = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (mainPageEmpty != null)
                        await mainPageEmpty.DisplayAlertAsync("Aviso", "Nenhum dizimista encontrado na planilha.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            var mainPageError = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPageError != null)
                await mainPageError.DisplayAlertAsync("Erro", $"Erro ao importar: {ex.Message}", "OK");
        }
    }
}
