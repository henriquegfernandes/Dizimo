using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dizimo.Domain.Entities;
using Dizimo.Application.Usuarios.Handlers;
using Dizimo.Application.Usuarios.Commands;
using Dizimo.Application.Usuarios.Queries;
using Dizimo.Domain.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System;
using Dizimo.Infrastructure.Services;

namespace Dizimo.ViewModels;

public partial class UsuarioListViewModel : ObservableObject
{
    private readonly GetUsuarioHandlers _getHandlers;
    private readonly CreateUsuarioHandler _createHandler;
    private readonly UpdateUsuarioHandler _updateHandler;
    private readonly DeleteUsuarioHandler _deleteHandler;
    private readonly InativarUsuarioHandler _inativarHandler;
    private readonly SessaoService _sessaoService;

    [ObservableProperty] private ObservableCollection<Usuario> usuarios = new();
    [ObservableProperty] private Usuario? selectedUsuario;

    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private string login = string.Empty;
    [ObservableProperty] private string senha = string.Empty;
    [ObservableProperty] private bool ativo = true;
    [ObservableProperty] private Guid id;
    [ObservableProperty] private bool isEditMode;

    public UsuarioListViewModel(
        GetUsuarioHandlers getHandlers,
        CreateUsuarioHandler createHandler,
        UpdateUsuarioHandler updateHandler,
        DeleteUsuarioHandler deleteHandler,
        InativarUsuarioHandler inativarHandler,
        SessaoService sessaoService)
    {
        _getHandlers = getHandlers;
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _inativarHandler = inativarHandler;
        _sessaoService = sessaoService;
    }

    [RelayCommand]
    public async Task CarregarUsuariosAsync()
    {
        var lista = await _getHandlers.Handle(new GetAllUsuariosQuery());
        Usuarios = new ObservableCollection<Usuario>(lista);
    }

    [RelayCommand]
    public async Task SalvarAsync()
    {
        if (IsEditMode)
        {
            await _updateHandler.Handle(new UpdateUsuarioCommand(Id, Nome, Login, SessaoService.HashSenha(Senha), Ativo));
        }
        else
        {
            await _createHandler.Handle(new CreateUsuarioCommand(Nome, Login, SessaoService.HashSenha(Senha)));
        }
        await CarregarUsuariosAsync();
        LimparCampos();
    }

    [RelayCommand]
    public void EditarUsuario()
    {
        if (SelectedUsuario != null)
        {
            Id = SelectedUsuario.Id;
            Nome = SelectedUsuario.Nome;
            Login = SelectedUsuario.Login;
            Senha = SelectedUsuario.SenhaHash;
            Ativo = SelectedUsuario.Ativo;
            IsEditMode = true;
        }
    }

    [RelayCommand]
    public async Task ExcluirUsuarioAsync()
    {
        if (SelectedUsuario != null)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmação", $"Deseja excluir o usuário '{SelectedUsuario.Nome}'?", "Sim", "Não");
            if (confirm)
            {
                await _deleteHandler.Handle(new DeleteUsuarioCommand(SelectedUsuario.Id));
                await CarregarUsuariosAsync();
                LimparCampos();
            }
        }
    }

    [RelayCommand]
    public async Task InativarUsuarioAsync()
    {
        if (SelectedUsuario != null)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmação", $"Deseja inativar o usuário '{SelectedUsuario.Nome}'?", "Sim", "Não");
            if (confirm)
            {
                await _inativarHandler.Handle(new InativarUsuarioCommand(SelectedUsuario.Id));
                await CarregarUsuariosAsync();
                LimparCampos();
            }
        }
    }

    private void LimparCampos()
    {
        Id = Guid.Empty;
        Nome = string.Empty;
        Login = string.Empty;
        Senha = string.Empty;
        Ativo = true;
        IsEditMode = false;
        SelectedUsuario = null;
    }
}
