using Dizimo.ViewModels;
using Dizimo.Application.Dizimistas.Handlers;
using Dizimo.Application.Dizimistas.Queries;
using System;
using Microsoft.Maui.Controls;

namespace Dizimo.Pages;

[QueryProperty(nameof(DizimistaId), "id")]
public partial class DizimistaDetalhesPage : ContentPage
{
    private readonly DizimistaDetalhesViewModel _viewModel;
    private string? _dizimistaId;

    public string? DizimistaId
    {
        get => _dizimistaId;
        set
        {
            _dizimistaId = value;
            System.Diagnostics.Debug.WriteLine($"[INFO] DizimistaId property set: {value}");
            if (!string.IsNullOrEmpty(value) && Guid.TryParse(value, out var id))
            {
                System.Diagnostics.Debug.WriteLine($"[INFO] ID parseado com sucesso: {id}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await _viewModel.LoadDizimistaAsync(id);
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Falha ao parsear ID: {value}");
            }
        }
    }

    public DizimistaDetalhesPage(DizimistaDetalhesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        System.Diagnostics.Debug.WriteLine("[INFO] DizimistaDetalhesPage construtor executado");
    }
}
