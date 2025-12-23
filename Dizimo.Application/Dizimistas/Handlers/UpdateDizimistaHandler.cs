using Dizimo.Domain.Repositories;
using Dizimo.Application.Dizimistas.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Dizimo.Application.Dizimistas.Handlers;

public class UpdateDizimistaHandler
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateDizimistaHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UpdateDizimistaCommand command)
    {
        var dizimista = await _unitOfWork.Dizimistas.GetByIdAsync(command.Id) ?? 
            throw new KeyNotFoundException($"Dizimista with Id {command.Id} not found.");

        dizimista.NumeroCadastro = command.NumeroCadastro;
        dizimista.Nome = command.Nome;
        dizimista.DataNascimento = command.DataNascimento;
        dizimista.Ativo = command.Ativo;
        dizimista.Endereco = command.Endereco;
        dizimista.Telefone = command.Telefone;
        dizimista.Whatsapp = command.Whatsapp;
        dizimista.DataCadastro = command.DataCadastro;

        // Garantir que campos obrigatórios do Endereco tenham valores válidos
        if (dizimista.Endereco != null)
        {
            if (string.IsNullOrWhiteSpace(dizimista.Endereco.UF))
            {
                dizimista.Endereco.UF = "SP";
            }
            if (string.IsNullOrWhiteSpace(dizimista.Endereco.Cidade))
            {
                dizimista.Endereco.Cidade = "Osasco";
            }
        }
        
        await _unitOfWork.Dizimistas.UpdateAsync(dizimista);
        await _unitOfWork.SaveChangesAsync();
    }
}
