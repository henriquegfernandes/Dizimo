using Dizimo.Application.Dizimistas.Commands;
using Dizimo.Domain.Repositories;

namespace Dizimo.Application.Dizimistas.Handlers;

public class DeleteDizimistaHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDizimistaHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteDizimistaCommand command)
    {
        await _unitOfWork.Dizimistas.DeleteAsync(command.Id);
        await _unitOfWork.SaveChangesAsync();
    }
}