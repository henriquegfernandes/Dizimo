using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Dizimo.Domain.Entities;

namespace Dizimo.Converters;

public class PerfilToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PerfilUsuario perfil)
        {
            return perfil == PerfilUsuario.Admin ? "Administrador" : "Padrão";
        }

        if (value is string perfilStr && Enum.TryParse<PerfilUsuario>(perfilStr, out var perfilEnum))
        {
            return perfilEnum == PerfilUsuario.Admin ? "Administrador" : "Padrão";
        }

        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return str == "Administrador" ? PerfilUsuario.Admin : PerfilUsuario.Padrao;
        }
        return PerfilUsuario.Padrao;
    }
}
