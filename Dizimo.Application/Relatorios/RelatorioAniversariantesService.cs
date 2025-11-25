using Dizimo.Domain.Entities;
using Dizimo.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Dizimo.Application.Relatorios;

public class RelatorioAniversariantesService
{
    private readonly IUnitOfWork _unitOfWork;
    public RelatorioAniversariantesService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IEnumerable<Dizimista>> GetAniversariantesAsync(int mes)
    {
        var dizimistas = await _unitOfWork.Dizimistas.GetAniversariantesAsync(mes);
        return dizimistas;
    }
}
