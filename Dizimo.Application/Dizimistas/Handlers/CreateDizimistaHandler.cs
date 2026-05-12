using Dizimo.Application.Dizimistas.Commands;
using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;

namespace Dizimo.Application.Dizimistas.Handlers;

public class CreateDizimistaHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateDizimistaHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateDizimistaCommand command)
    {
        var dizimista = new Dizimista
        {
            Id = Guid.NewGuid(),
            NumeroCadastro = command.NumeroCadastro,
            Nome = command.Nome,
            DataNascimento = command.DataNascimento,
            Ativo = true,
            Endereco = command.Endereco,
            Telefone = command.Telefone,
            Whatsapp = command.Whatsapp,
            DataCadastro = command.DataCadastro
        };
        await _unitOfWork.Dizimistas.AddAsync(dizimista);
        await _unitOfWork.SaveChangesAsync();
        return dizimista.Id;
    }
}