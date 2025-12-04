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
                SetProperty(ref _dizimista, value);
            }
        }

        public string UltimaOferta
        {
            get => _ultimaOferta;
            set => SetProperty(ref _ultimaOferta, value);
        }

        public string AtivarInativarText => _dizimista?.Ativo == true ? "Inativar" : "Ativar";

        public string EnderecoCompleto
        {
            get
            {
                if (_dizimista?.Endereco == null)
                {
                    System.Diagnostics.Debug.WriteLine("[WARN] EnderecoCompleto: Dizimista ou Endereco é null");
                    return string.Empty;
                }
                var endereco = _dizimista.Endereco;
                var complemento = string.IsNullOrWhiteSpace(endereco.Complemento) ? "" : $", {endereco.Complemento}";
                var enderecoStr = $"{endereco.Rua}, {endereco.Numero}{complemento} - {endereco.Bairro}, {endereco.Cidade} - {endereco.UF}, {endereco.CEP}";
                System.Diagnostics.Debug.WriteLine($"[INFO] EnderecoCompleto: {enderecoStr}");
                return enderecoStr;
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
                    System.Diagnostics.Debug.WriteLine($"[INFO] Endereco presente: {dizimista.Endereco != null}");
                    System.Diagnostics.Debug.WriteLine($"[INFO] Status Ativo: {dizimista.Ativo}");
                }
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
        public async Task AtivarInativarAsync()
        {
            if (_dizimista != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[INFO] AtivarInativarAsync - Chamando handler para ID: {_dizimista.Id}");
                    await _inativarHandler.Handle(new InativarDizimistaCommand(_dizimista.Id));
                    System.Diagnostics.Debug.WriteLine($"[INFO] AtivarInativarAsync - Handler executado com sucesso");
                    
                    await _unitOfWork.ClearDbContextAsync();
                    System.Diagnostics.Debug.WriteLine($"[INFO] AtivarInativarAsync - Recarregando dados do dizimista");
                    await LoadDizimistaAsync(_currentDizimistaId);
                    
                    System.Diagnostics.Debug.WriteLine($"[INFO] AtivarInativarAsync - Dados recarregados. Novo status: {(_dizimista?.Ativo == true ? "Ativo" : "Inativo")}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERRO] AtivarInativarAsync - Erro ao processar: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[ERRO] Stack trace: {ex.StackTrace}");
                }
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