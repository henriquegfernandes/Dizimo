using Dizimo.Domain.Repositories;
using Dizimo.Application.Dizimistas.Commands;
using System.Threading.Tasks;
using Dizimo.Domain.Entities;

namespace Dizimo.Application.Dizimistas.Handlers;

public class UpdateDizimistaHandler
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateDizimistaHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UpdateDizimistaCommand command)
    {
        var dizimista = new Dizimista
        {
            Id = command.Id,
            NumeroCadastro = command.NumeroCadastro,
            Nome = command.Nome,
            DataNascimento = command.DataNascimento,
            Ativo = command.Ativo,
            Endereco = command.Endereco,
            Telefone = command.Telefone,
            Whatsapp = command.Whatsapp,
            DataCadastro = command.DataCadastro
        };
        await _unitOfWork.Dizimistas.UpdateAsync(dizimista);
        await _unitOfWork.SaveChangesAsync();
    }
}
