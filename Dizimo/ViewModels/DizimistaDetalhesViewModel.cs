using Dizimo.Domain.Entities;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Dizimistas.Commands;
using Dizimo.Application.Dizimistas.Queries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Repositories;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dizimo.ViewModels
{
    public partial class DizimistaDetalhesViewModel : ObservableObject
    {
        private readonly InativarDizimistaHandler _inativarHandler;
        private readonly GetDizimistaHandlers _getDizimistaHandlers;
        private readonly IUnitOfWork _unitOfWork;
        private Dizimista? _dizimista;
        private Guid _currentDizimistaId;
        private string _ultimaOferta = string.Empty;

        public Dizimista? Dizimista
        {
            get => _dizimista;
            set
            {
                System.Diagnostics.Debug.WriteLine($"[INFO] DizimistaDetalhesViewModel.Dizimista sendo definido: {value?.Nome ?? "NULL"}");
                if (SetProperty(ref _dizimista, value))
                {
                    // Notificar mudanças nas propriedades computadas quando Dizimista muda
                    OnPropertyChanged(nameof(EnderecoCompleto));
                    OnPropertyChanged(nameof(TelefoneFormatado));
                    OnPropertyChanged(nameof(WhatsappFormatado));
                    OnPropertyChanged(nameof(CepFormatado));
                }
            }
        }

        public string UltimaOferta
        {
            get => _ultimaOferta;
            set => SetProperty(ref _ultimaOferta, value);
        }

        public string TelefoneFormatado
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_dizimista?.Telefone))
                    return string.Empty;

                var cleaned = new string(_dizimista.Telefone.Where(char.IsDigit).ToArray());

                if (cleaned.Length == 10)
                    return $"({cleaned.Substring(0, 2)}) {cleaned.Substring(2, 4)}-{cleaned.Substring(6)}";
                
                if (cleaned.Length == 11)
                    return $"({cleaned.Substring(0, 2)}) {cleaned.Substring(2, 5)}-{cleaned.Substring(7)}";

                return _dizimista.Telefone;
            }
        }

        public string WhatsappFormatado
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_dizimista?.Whatsapp))
                    return string.Empty;

                var cleaned = new string(_dizimista.Whatsapp.Where(char.IsDigit).ToArray());

                if (cleaned.Length == 11)
                    return $"({cleaned.Substring(0, 2)}) {cleaned.Substring(2, 5)}-{cleaned.Substring(7)}";

                return _dizimista.Whatsapp;
            }
        }

        public string CepFormatado
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_dizimista?.Endereco?.CEP))
                    return string.Empty;

                var cleaned = new string(_dizimista.Endereco.CEP.Where(char.IsDigit).ToArray());

                if (cleaned.Length == 8)
                    return $"{cleaned.Substring(0, 5)}-{cleaned.Substring(5)}";

                return _dizimista.Endereco.CEP;
            }
        }

        public string EnderecoCompleto
        {
            get
            {
                // Verificar se o dizimista está nulo
                if (_dizimista == null)
                {
                    System.Diagnostics.Debug.WriteLine("[WARN] EnderecoCompleto: Dizimista é null");
                    return string.Empty;
                }

                // Verificar se o endereço está nulo
                if (_dizimista.Endereco == null)
                {
                    System.Diagnostics.Debug.WriteLine("[WARN] EnderecoCompleto: Endereco é null");
                    return string.Empty;
                }

                // Verificar se os campos obrigatórios do endereço estão preenchidos
                if (string.IsNullOrWhiteSpace(_dizimista.Endereco.Rua) || 
                    string.IsNullOrWhiteSpace(_dizimista.Endereco.Numero))
                {
                    System.Diagnostics.Debug.WriteLine("[WARN] EnderecoCompleto: Rua ou Número vazios");
                    return string.Empty;
                }

                try
                {
                    var endereco = _dizimista.Endereco;
                    var complemento = string.IsNullOrWhiteSpace(endereco.Complemento) 
                        ? "" 
                        : $", {endereco.Complemento}";
                    
                    // Formatar CEP dentro do endereço
                    var cepFormatado = CepFormatado;
                    
                    var enderecoStr = $"{endereco.Rua}, {endereco.Numero}{complemento} - {endereco.Bairro}, {endereco.Cidade} - {endereco.UF}, {cepFormatado}";
                    
                    System.Diagnostics.Debug.WriteLine($"[INFO] EnderecoCompleto: {enderecoStr}");
                    return enderecoStr;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao montar EnderecoCompleto: {ex.Message}");
                    return string.Empty;
                }
            }
        }

        public DizimistaDetalhesViewModel(InativarDizimistaHandler inativarHandler, GetDizimistaHandlers getDizimistaHandlers, IUnitOfWork unitOfWork)
        {
            _inativarHandler = inativarHandler ?? throw new ArgumentNullException(nameof(inativarHandler));
            _getDizimistaHandlers = getDizimistaHandlers ?? throw new ArgumentNullException(nameof(getDizimistaHandlers));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            System.Diagnostics.Debug.WriteLine("[INFO] DizimistaDetalhesViewModel construtor executado");
        }

        public async Task LoadDizimistaAsync(Guid id)
        {
            System.Diagnostics.Debug.WriteLine($"[INFO] LoadDizimistaAsync chamado com ID: {id}");
            _currentDizimistaId = id;
            try
            {
                var dizimista = await _getDizimistaHandlers.Handle(new GetDizimistaByIdQuery(id));
                System.Diagnostics.Debug.WriteLine($"[INFO] Dizimista retornado do handler: {dizimista?.Nome ?? "NULL"}");
                
                if (dizimista != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[INFO] Dizimista detalhes - Nome: {dizimista.Nome}, Telefone: {dizimista.Telefone}, Whatsapp: {dizimista.Whatsapp}");
                    System.Diagnostics.Debug.WriteLine($"[INFO] Endereco objeto é null? {dizimista.Endereco == null}");
                    
                    if (dizimista.Endereco != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[INFO] Endereco - Rua: {dizimista.Endereco.Rua}");
                        System.Diagnostics.Debug.WriteLine($"[INFO] Endereco - Número: {dizimista.Endereco.Numero}");
                        System.Diagnostics.Debug.WriteLine($"[INFO] Endereco - Complemento: {dizimista.Endereco.Complemento}");
                        System.Diagnostics.Debug.WriteLine($"[INFO] Endereco - Bairro: {dizimista.Endereco.Bairro}");
                        System.Diagnostics.Debug.WriteLine($"[INFO] Endereco - Cidade: {dizimista.Endereco.Cidade}");
                        System.Diagnostics.Debug.WriteLine($"[INFO] Endereco - UF: {dizimista.Endereco.UF}");
                        System.Diagnostics.Debug.WriteLine($"[INFO] Endereco - CEP: {dizimista.Endereco.CEP}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[ERRO] Endereco é NULL após carregar o dizimista!");
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[INFO] Status Ativo: {dizimista.Ativo}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[ERRO] Dizimista carregado é NULL!");
                }
                
                // Definir o dizimista (isso irá disparar as notificações de mudança)
                Dizimista = dizimista;
                
                // Carregar última oferta
                await CarregarUltimaOfertaAsync(id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao carregar dizimista: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
            }
        }

        private async Task CarregarUltimaOfertaAsync(Guid dizimistaId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[INFO] CarregarUltimaOfertaAsync chamado para ID: {dizimistaId}");
                var ofertas = await _unitOfWork.Ofertas.GetByDizimistaAsync(dizimistaId);
                
                if (ofertas != null)
                {
                    var ultimaOferta = ofertas.OrderByDescending(o => o.Data).FirstOrDefault();
                    if (ultimaOferta != null)
                    {
                        UltimaOferta = $"R$ {ultimaOferta.Valor:F2} em {ultimaOferta.Data:dd/MM/yyyy}";
                        System.Diagnostics.Debug.WriteLine($"[INFO] Última oferta encontrada: {UltimaOferta}");
                    }
                    else
                    {
                        UltimaOferta = "Nenhuma oferta registrada";
                        System.Diagnostics.Debug.WriteLine("[INFO] Nenhuma oferta encontrada");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao carregar última oferta: {ex.Message}");
                UltimaOferta = "Erro ao carregar oferta";
            }
        }

        [RelayCommand]
        public async Task EditarAsync()
        {
            if (_dizimista != null)
            {
                await Shell.Current.GoToAsync($"dizimista-cadastro?id={_dizimista.Id}");
            }
        }

        [RelayCommand]
        public async Task ExcluirAsync()
        {
            if (_dizimista != null)
            {
                try
                {
                    var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (mainPage != null)
                    {
                        bool confirm = await mainPage.DisplayAlertAsync(
                            "Confirmação de Exclusão",
                            $"Deseja realmente excluir o dizimista {_dizimista.Nome}? Esta ação não pode ser desfeita.",
                            "Sim",
                            "Não");
                        
                        if (!confirm) return;
                    }

                    System.Diagnostics.Debug.WriteLine($"[INFO] ExcluirAsync - Excluindo dizimista ID: {_dizimista.Id}");
                    
                    // Usar o UnitOfWork para excluir
                    await _unitOfWork.Dizimistas.DeleteAsync(_dizimista.Id);
                    await _unitOfWork.SaveChangesAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"[INFO] ExcluirAsync - Dizimista excluído com sucesso");
                    
                    // Voltar para a lista
                    await Shell.Current.GoToAsync("..", true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERRO] ExcluirAsync - Erro ao excluir: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
                    
                    var mainPage = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (mainPage != null)
                    {
                        await mainPage.DisplayAlertAsync("Erro", $"Erro ao excluir dizimista: {ex.Message}", "OK");
                    }
                }
            }
        }
    }
}