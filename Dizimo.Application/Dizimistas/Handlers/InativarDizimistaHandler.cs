using Dizimo.Domain.Repositories;
using Dizimo.Application.Dizimistas.Commands;
using System.Threading.Tasks;

namespace Dizimo.Application.Dizimistas.Handlers;

public class InativarDizimistaHandler
{
    private readonly IUnitOfWork _unitOfWork;
    public InativarDizimistaHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(InativarDizimistaCommand command)
    {
        await _unitOfWork.Dizimistas.InativarAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
    }
}
