using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using System.Linq;
using Dizimo.ViewModels;

namespace Dizimo.Pages.Controls;

/// <summary>
/// Controle de gráfico de pizza (Donut Chart) que exibe dados com porcentagem no gráfico
/// e quantidade detalhada na tooltip.
/// Segue SOLID Principles com responsabilidades bem definidas.
/// </summary>
public class PieChartControl : UserControl
{
    // Configurações do gráfico
    private const double CanvasWidth = 500;
    private const double CanvasHeight = 500;
    private const double ChartRadius = 120;
    private const double DonutInnerRadius = 70;
    private const double CenterX = 250;  // Ajustado para o novo tamanho do canvas
    private const double CenterY = 250;  // Ajustado para o novo tamanho do canvas
    private const double ExternalLabelRadius = 60;  // Aumentado para os labels ficarem bem fora
    private const int PercentageDecimalPlaces = 0;

    public static readonly StyledProperty<System.Collections.ObjectModel.ObservableCollection<GraficoData>?> DatasProperty =
        AvaloniaProperty.Register<PieChartControl, System.Collections.ObjectModel.ObservableCollection<GraficoData>?>(
            nameof(Datas),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public System.Collections.ObjectModel.ObservableCollection<GraficoData>? Datas
    {
        get => GetValue(DatasProperty);
        set
        {
            // Desinscrever coleção anterior
            if (GetValue(DatasProperty) is System.Collections.ObjectModel.ObservableCollection<GraficoData> oldDatas)
            {
                oldDatas.CollectionChanged -= OnDatasCollectionChanged;
            }
            
            // Definir novo valor
            SetValue(DatasProperty, value);
            
            // Inscrever na nova coleção
            if (value is not null)
            {
                value.CollectionChanged += OnDatasCollectionChanged;
            }
            
            DrawPieChart();
        }
    }

    private void OnDatasCollectionChanged(object? _, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        DrawPieChart();
    }

    private Canvas? _canvas;

    public PieChartControl()
    {
        // Criar Canvas diretamente em C# com tamanho maior para acomodar labels externos
        _canvas = new Canvas
        {
            Background = new SolidColorBrush(Colors.Transparent),
            Width = CanvasWidth,
            Height = CanvasHeight
        };
        
        Content = _canvas;
        
        // Monitorar mudanças de propriedade
        this.PropertyChanged += (_, e) =>
        {
            if (e.Property == DatasProperty)
            {
                DrawPieChart();
            }
        };

        DrawPieChart();
    }

    /// <summary>
    /// Desenha o gráfico de pizza com base nos dados fornecidos.
    /// </summary>
    private void DrawPieChart()
    {
        if (_canvas is null || Datas is null || !Datas.Any())
        {
            _canvas?.Children.Clear();
            return;
        }

        _canvas.Children.Clear();

        var validData = Datas.Where(d => d.Quantidade > 0).ToList();
        if (!validData.Any()) return;

        var total = validData.Sum(d => d.Quantidade);
        
        double startAngle = 0;
        foreach (var data in validData)
        {
            double sliceAngle = (data.Quantidade / (double)total) * 360;
            double percentual = (data.Quantidade / (double)total) * 100;
            
            // Desenhar fatia
            DrawSlice(startAngle, sliceAngle, data.CorHex, data.Periodo, data.Quantidade);
            
            // Desenhar label com porcentagem e linha conectora (todos fora do gráfico)
            DrawExternalPercentageLabel(startAngle, sliceAngle, percentual, data.CorHex);
            
            startAngle += sliceAngle;
        }
    }

    /// <summary>
    /// Desenha uma fatia do gráfico com tooltip.
    /// </summary>
    private void DrawSlice(double startAngle, double sliceAngle, string colorHex, string periodo, int quantidade)
    {
        try
        {
            if (_canvas is null) return;
            
            var color = Color.Parse(colorHex);
            var brush = new SolidColorBrush(color);
            var tooltip = CreateTooltip(periodo, quantidade);

            var pathShape = CreateSlicePath(startAngle, sliceAngle, brush, tooltip);
            _canvas.Children.Add(pathShape);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PIECHART] Erro ao desenhar slice: {ex.Message}");
        }
    }

    /// <summary>
    /// Cria o caminho (Path) para uma fatia do gráfico.
    /// </summary>
    private Avalonia.Controls.Shapes.Path CreateSlicePath(
        double startAngle, 
        double sliceAngle, 
        SolidColorBrush brush, 
        string tooltip)
    {
        PathFigure pathFigure;

        if (sliceAngle >= 359.9)
        {
            // Caso especial: círculo completo (duas semicircunferências)
            pathFigure = CreateFullCirclePathFigure();
        }
        else
        {
            // Fatia normal
            pathFigure = CreateSlicePathFigure(startAngle, sliceAngle);
        }

        var pathGeometry = new PathGeometry { Figures = new PathFigures { pathFigure } };
        
        var pathShape = new Avalonia.Controls.Shapes.Path
        {
            Data = pathGeometry,
            Fill = brush,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
        };
        
        ToolTip.SetTip(pathShape, tooltip);
        return pathShape;
    }

    /// <summary>
    /// Cria PathFigure para uma fatia normal.
    /// </summary>
    private PathFigure CreateSlicePathFigure(double startAngle, double sliceAngle)
    {
        var pathFigure = new PathFigure 
        { 
            StartPoint = GetPointOnCircle(CenterX, CenterY, ChartRadius, startAngle), 
            IsFilled = true,
            IsClosed = true
        };
        
        var arcSegment = new ArcSegment
        {
            Point = GetPointOnCircle(CenterX, CenterY, ChartRadius, startAngle + sliceAngle),
            Size = new Size(ChartRadius, ChartRadius),
            RotationAngle = 0,
            IsLargeArc = sliceAngle > 180,
            SweepDirection = SweepDirection.Clockwise
        };
        
#pragma warning disable CS8602
        pathFigure.Segments.Add(arcSegment);
        pathFigure.Segments.Add(new LineSegment 
        { 
            Point = GetPointOnCircle(CenterX, CenterY, DonutInnerRadius, startAngle + sliceAngle) 
        });

        var innerArcSegment = new ArcSegment
        {
            Point = GetPointOnCircle(CenterX, CenterY, DonutInnerRadius, startAngle),
            Size = new Size(DonutInnerRadius, DonutInnerRadius),
            RotationAngle = 0,
            IsLargeArc = sliceAngle > 180,
            SweepDirection = SweepDirection.CounterClockwise
        };
        
        pathFigure.Segments.Add(innerArcSegment);
        pathFigure.Segments.Add(new LineSegment { Point = GetPointOnCircle(CenterX, CenterY, ChartRadius, startAngle) });
#pragma warning restore CS8602

        return pathFigure;
    }

    /// <summary>
    /// Cria PathFigure para um círculo completo (caso extremo de 360 graus).
    /// </summary>
    private PathFigure CreateFullCirclePathFigure()
    {
        var pathFigure = new PathFigure 
        { 
            StartPoint = GetPointOnCircle(CenterX, CenterY, ChartRadius, 0), 
            IsFilled = true,
            IsClosed = true
        };

#pragma warning disable CS8602
        // Primeira semicircunferência (externa)
        pathFigure.Segments.Add(new ArcSegment
        {
            Point = GetPointOnCircle(CenterX, CenterY, ChartRadius, 180),
            Size = new Size(ChartRadius, ChartRadius),
            RotationAngle = 0,
            IsLargeArc = false,
            SweepDirection = SweepDirection.Clockwise
        });

        // Segunda semicircunferência (externa)
        pathFigure.Segments.Add(new ArcSegment
        {
            Point = GetPointOnCircle(CenterX, CenterY, ChartRadius, 0),
            Size = new Size(ChartRadius, ChartRadius),
            RotationAngle = 0,
            IsLargeArc = false,
            SweepDirection = SweepDirection.Clockwise
        });

        // Linha até raio interno
        pathFigure.Segments.Add(new LineSegment 
        { 
            Point = GetPointOnCircle(CenterX, CenterY, DonutInnerRadius, 0),
            IsStroked = true
        });

        // Primeira semicircunferência (interna - reversa)
        pathFigure.Segments.Add(new ArcSegment
        {
            Point = GetPointOnCircle(CenterX, CenterY, DonutInnerRadius, 180),
            Size = new Size(DonutInnerRadius, DonutInnerRadius),
            RotationAngle = 0,
            IsLargeArc = false,
            SweepDirection = SweepDirection.CounterClockwise
        });

        // Segunda semicircunferência (interna - reversa)
        pathFigure.Segments.Add(new ArcSegment
        {
            Point = GetPointOnCircle(CenterX, CenterY, DonutInnerRadius, 0),
            Size = new Size(DonutInnerRadius, DonutInnerRadius),
            RotationAngle = 0,
            IsLargeArc = false,
            SweepDirection = SweepDirection.CounterClockwise
        });
#pragma warning restore CS8602

        return pathFigure;
    }

    /// <summary>
    /// Desenha o label com porcentagem SEMPRE fora do gráfico com linha conectora colorida.
    /// </summary>
    private void DrawExternalPercentageLabel(double startAngle, double sliceAngle, double percentual, string colorHex)
    {
        try
        {
            if (_canvas is null) return;

            var color = Color.Parse(colorHex);
            var colorBrush = new SolidColorBrush(color);
            
            var midAngle = startAngle + sliceAngle / 2;
            var percentageText = FormatPercentage(percentual);

            // Ponto na borda externa do donut
            var borderPoint = GetPointOnCircle(CenterX, CenterY, ChartRadius + 5, midAngle);
            
            // Ponto externo onde o label será colocado
            var externalRadius = ChartRadius + ExternalLabelRadius + 20;
            var labelPosition = GetPointOnCircle(CenterX, CenterY, externalRadius, midAngle);

            // Desenhar linha colorida do gráfico até o label
            var line = new Avalonia.Controls.Shapes.Line
            {
                StartPoint = borderPoint,
                EndPoint = labelPosition,
                Stroke = colorBrush,
                StrokeThickness = 2,
                Opacity = 0.8
            };
            _canvas.Children.Add(line);

            // Desenhar background (pequeno retângulo/badge) para o número
            var badgeSize = 30;
            var background = new Rectangle
            {
                Width = badgeSize,
                Height = badgeSize,
                Fill = colorBrush,
                RadiusX = 4,
                RadiusY = 4,
                Opacity = 0.9
            };
            Canvas.SetLeft(background, labelPosition.X - badgeSize / 2);
            Canvas.SetTop(background, labelPosition.Y - badgeSize / 2);
            _canvas.Children.Add(background);

            // Desenhar texto da porcentagem
            var textBlock = new TextBlock
            {
                Text = percentageText,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeight.Bold,
                TextAlignment = TextAlignment.Center,
                FontSize = 12
            };

            // Medir o texto para centralizá-lo corretamente
            var textSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            textBlock.Measure(textSize);
            
            Canvas.SetLeft(textBlock, labelPosition.X - textBlock.DesiredSize.Width / 2);
            Canvas.SetTop(textBlock, labelPosition.Y - textBlock.DesiredSize.Height / 2 - 1);

            _canvas.Children.Add(textBlock);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PIECHART] Erro ao desenhar label externo: {ex.Message}");
        }
    }

    /// <summary>
    /// Cria texto de tooltip com período e quantidade.
    /// </summary>
    private static string CreateTooltip(string periodo, int quantidade)
    {
        return $"{periodo}\n{quantidade} dizimista{(quantidade != 1 ? "s" : "")}";
    }

    /// <summary>
    /// Obtém um ponto na circunferência dado o centro, raio e ângulo.
    /// </summary>
    private static Point GetPointOnCircle(double centerX, double centerY, double radius, double angleInDegrees)
    {
        var angleInRadians = (angleInDegrees - 90) * System.Math.PI / 180.0;
        var x = centerX + radius * System.Math.Cos(angleInRadians);
        var y = centerY + radius * System.Math.Sin(angleInRadians);
        return new Point(x, y);
    }

    /// <summary>
    /// Formata a porcentagem com o número de casas decimais configurado.
    /// </summary>
    private static string FormatPercentage(double percentual)
    {
        var format = $"F{PercentageDecimalPlaces}";
        return percentual.ToString(format) + "%";
    }
}






