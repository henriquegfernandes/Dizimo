using System.Globalization;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.DependencyInjection;
using Dizimo.Domain.Repositories;

namespace Dizimo.Converters;

public class DizimistaIdToNomeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Guid dizimistaId && dizimistaId != Guid.Empty)
            try
            {
                var unitOfWork = Ioc.Default.GetService<IUnitOfWork>();
                if (unitOfWork != null)
                {
                    var dizimista = unitOfWork.Dizimistas.GetByIdAsync(dizimistaId).GetAwaiter().GetResult();
                    if (dizimista != null) return $"{dizimista.NumeroCadastro} - {dizimista.Nome}";
                }
            }
            catch
            {
                return "Carregando...";
            }

        return "ID desconhecido";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}