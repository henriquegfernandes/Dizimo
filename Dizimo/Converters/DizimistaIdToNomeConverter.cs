using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Dizimo.Domain.Repositories;

namespace Dizimo.Converters;

public class DizimistaIdToNomeConverter : IValueConverter
{
    private IUnitOfWork? _unitOfWork;

    public void SetUnitOfWork(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Guid dizimistaId && _unitOfWork != null)
        {
            try
            {
                var dizimista = _unitOfWork.Dizimistas.GetByIdAsync(dizimistaId).GetAwaiter().GetResult();
                return $"{dizimista?.NumeroCadastro} - {dizimista?.Nome}" ?? "Desconhecido";
            }
            catch
            {
                return "Erro ao carregar";
            }
        }
        return "ID desconhecido";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
