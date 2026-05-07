using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using System.Linq;
using Dizimo.ViewModels;

namespace Dizimo.Controls;

public class PieChartControl : UserControl
{
    public static readonly StyledProperty<System.Collections.ObjectModel.ObservableCollection<GraficoData>?> DatasProperty =
        AvaloniaProperty.Register<PieChartControl, System.Collections.ObjectModel.ObservableCollection<GraficoData>?>(
            nameof(Datas),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public System.Collections.ObjectModel.ObservableCollection<GraficoData>? Datas
    {
        get => GetValue(DatasProperty);
        set
        {
            System.Diagnostics.Debug.WriteLine($"[PIECHART] Datas setter chamado com {value?.Count ?? 0} items");
            
            // Unsubscribe from old collection if exists
            if (GetValue(DatasProperty) is System.Collections.ObjectModel.ObservableCollection<GraficoData> oldDatas)
            {
                oldDatas.CollectionChanged -= OnDatasCollectionChanged;
            }
            
            // Set new value
            SetValue(DatasProperty, value);
            
            // Subscribe to new collection
            if (value is not null)
            {
                value.CollectionChanged += OnDatasCollectionChanged;
                System.Diagnostics.Debug.WriteLine("[PIECHART] Subscribed to collection changes");
            }
            
            DrawPieChart();
        }
    }

    private void OnDatasCollectionChanged(object? _, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[PIECHART] Collection changed - redrawing chart");
        DrawPieChart();
    }

    private Canvas? _canvas;

    public PieChartControl()
    {
        // Create Canvas directly in C#
        _canvas = new Canvas
        {
            Background = new SolidColorBrush(Colors.Transparent),
            Width = 400,
            Height = 400
        };
        
        Content = _canvas;
        
        System.Diagnostics.Debug.WriteLine("[PIECHART] PieChartControl construtor chamado");
        
        // Monitor property changes
        this.PropertyChanged += (_, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"[PIECHART] Property changed: {e.Property?.Name}");
            if (e.Property == DatasProperty)
            {
                System.Diagnostics.Debug.WriteLine("[PIECHART] Datas property mudou, redrawing...");
                DrawPieChart();
            }
        };

        DrawPieChart();
    }

    private void DrawPieChart()
    {
        System.Diagnostics.Debug.WriteLine($"[PIECHART] DrawPieChart chamado - Canvas: {_canvas != null}, Datas: {Datas?.Count ?? 0} items");
        
        if (_canvas is null)
        {
            System.Diagnostics.Debug.WriteLine("[PIECHART] ERRO: Canvas é null");
            return;
        }
        
        if (Datas is null || !Datas.Any())
        {
            System.Diagnostics.Debug.WriteLine("[PIECHART] ERRO: Datas é null ou vazio");
            _canvas.Children.Clear();
            return;
        }

        _canvas.Children.Clear();

        var total = Datas.Sum(d => d.Quantidade);
        System.Diagnostics.Debug.WriteLine($"[PIECHART] Total: {total}, Items com quantidade > 0: {Datas.Count(d => d.Quantidade > 0)}");
        
        if (total == 0) 
        {
            System.Diagnostics.Debug.WriteLine("[PIECHART] Total é 0, nenhuma fatia será desenhada");
            return;
        }

        double startAngle = 0;
        double radius = 120;
        double donutInnerRadius = 70;
        double centerX = 200;
        double centerY = 200;

        int sliceCount = 0;
        foreach (var data in Datas.Where(d => d.Quantidade > 0))
        {
            double sliceAngle = (data.Quantidade / (double)total) * 360;
            double percentual = (data.Quantidade / (double)total) * 100;
            System.Diagnostics.Debug.WriteLine($"[PIECHART] Slice {sliceCount}: {data.Periodo} = {data.Quantidade} ({sliceAngle}°, {percentual:F1}%)");
            
            // Draw slice with tooltip
            DrawSlice(centerX, centerY, radius, donutInnerRadius, startAngle, sliceAngle, data.CorHex, data.Periodo, data.Quantidade);
            
            // Draw label with percentage
            DrawNumberLabel(centerX, centerY, radius, donutInnerRadius, startAngle, sliceAngle, $"{percentual:F0}%", data.CorHex);
            
            startAngle += sliceAngle;
            sliceCount++;
        }
        
        System.Diagnostics.Debug.WriteLine($"[PIECHART] Total de {sliceCount} fatias desenhadas");
    }

    private void DrawSlice(double centerX, double centerY, double radius, double innerRadius, double startAngle, double sliceAngle, string colorHex, string periodo, int quantidade)
    {
        try
        {
            if (_canvas is null) return;
            
            var color = Color.Parse(colorHex);
            var brush = new SolidColorBrush(color);
            var tooltip = $"{periodo}\n{quantidade} dizimista(s)";

            // Handle special case when slice is 360 degrees (full circle)
            if (sliceAngle >= 359.9)
            {
                // Draw as two semicircles to create a complete donut
                var outerPathFigure = new PathFigure 
                { 
                    StartPoint = GetPointOnCircle(centerX, centerY, radius, 0), 
                    IsFilled = true,
                    IsClosed = true
                };

#pragma warning disable CS8602
                // First semicircle (outer)
                outerPathFigure.Segments.Add(new ArcSegment
                {
                    Point = GetPointOnCircle(centerX, centerY, radius, 180),
                    Size = new Size(radius, radius),
                    RotationAngle = 0,
                    IsLargeArc = false,
                    SweepDirection = SweepDirection.Clockwise
                });

                // Second semicircle (outer)
                outerPathFigure.Segments.Add(new ArcSegment
                {
                    Point = GetPointOnCircle(centerX, centerY, radius, 0),
                    Size = new Size(radius, radius),
                    RotationAngle = 0,
                    IsLargeArc = false,
                    SweepDirection = SweepDirection.Clockwise
                });

                // Line to inner radius
                outerPathFigure.Segments.Add(new LineSegment 
                { 
                    Point = GetPointOnCircle(centerX, centerY, innerRadius, 0),
                    IsStroked = true
                });

                // First semicircle (inner - backwards)
                outerPathFigure.Segments.Add(new ArcSegment
                {
                    Point = GetPointOnCircle(centerX, centerY, innerRadius, 180),
                    Size = new Size(innerRadius, innerRadius),
                    RotationAngle = 0,
                    IsLargeArc = false,
                    SweepDirection = SweepDirection.CounterClockwise
                });

                // Second semicircle (inner - backwards)
                outerPathFigure.Segments.Add(new ArcSegment
                {
                    Point = GetPointOnCircle(centerX, centerY, innerRadius, 0),
                    Size = new Size(innerRadius, innerRadius),
                    RotationAngle = 0,
                    IsLargeArc = false,
                    SweepDirection = SweepDirection.CounterClockwise
                });
#pragma warning restore CS8602

                var pathGeometry = new PathGeometry { Figures = new PathFigures { outerPathFigure } };
                
                var pathShape = new Avalonia.Controls.Shapes.Path
                {
                    Data = pathGeometry,
                    Fill = brush,
                    Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
                };
                
                ToolTip.SetTip(pathShape, tooltip);

                _canvas.Children.Add(pathShape);
            }
            else
            {
                // Normal slice drawing for angles < 360
                var outerPathFigure = new PathFigure 
                { 
                    StartPoint = GetPointOnCircle(centerX, centerY, radius, startAngle), 
                    IsFilled = true,
                    IsClosed = true
                };
                
                var arcSegment = new ArcSegment
                {
                    Point = GetPointOnCircle(centerX, centerY, radius, startAngle + sliceAngle),
                    Size = new Size(radius, radius),
                    RotationAngle = 0,
                    IsLargeArc = sliceAngle > 180,
                    SweepDirection = SweepDirection.Clockwise
                };
                
#pragma warning disable CS8602
                outerPathFigure.Segments.Add(arcSegment);

                // Line to inner radius
                outerPathFigure.Segments.Add(new LineSegment 
                { 
                    Point = GetPointOnCircle(centerX, centerY, innerRadius, startAngle + sliceAngle) 
                });

                // Draw inner arc (backwards)
                var innerArcSegment = new ArcSegment
                {
                    Point = GetPointOnCircle(centerX, centerY, innerRadius, startAngle),
                    Size = new Size(innerRadius, innerRadius),
                    RotationAngle = 0,
                    IsLargeArc = sliceAngle > 180,
                    SweepDirection = SweepDirection.CounterClockwise
                };
                
                outerPathFigure.Segments.Add(innerArcSegment);
                outerPathFigure.Segments.Add(new LineSegment { Point = GetPointOnCircle(centerX, centerY, radius, startAngle) });
#pragma warning restore CS8602

                var pathGeometry = new PathGeometry { Figures = new PathFigures { outerPathFigure } };
                
                var pathShape = new Avalonia.Controls.Shapes.Path
                {
                    Data = pathGeometry,
                    Fill = brush,
                    Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
                };
                
                ToolTip.SetTip(pathShape, tooltip);

                _canvas.Children.Add(pathShape);
            }
            
            System.Diagnostics.Debug.WriteLine($"[PIECHART] Slice desenhada com cor {colorHex}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao desenhar slice: {ex.Message}");
        }
    }

    private void DrawNumberLabel(double centerX, double centerY, double radius, double innerRadius, double startAngle, double sliceAngle, string number, string colorHex)
    {
        try
        {
            if (_canvas is null) return;

            var midAngle = startAngle + sliceAngle / 2;
            var color = Color.Parse(colorHex);
            var midRadiusInsideDonut = (radius + innerRadius) / 2.0;
            var labelPositionInsideDonut = GetPointOnCircle(centerX, centerY, midRadiusInsideDonut, midAngle);

            const double minAngleForInsideLabel = 30; // Se a fatia for menor que 30 graus, coloca fora

            if (sliceAngle >= minAngleForInsideLabel)
            {
                // Label inside the donut
                var textBlock = new TextBlock
                {
                    Text = number,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontWeight = FontWeight.Bold,
                    TextAlignment = TextAlignment.Center,
                    FontSize = 14
                };

                Canvas.SetLeft(textBlock, labelPositionInsideDonut.X - 15);
                Canvas.SetTop(textBlock, labelPositionInsideDonut.Y - 12);

                _canvas.Children.Add(textBlock);
            }
            else
            {
                // Label outside the donut with connector line
                var externalRadius = radius + 40;
                var labelExternalPosition = GetPointOnCircle(centerX, centerY, externalRadius, midAngle);

                // Draw line from donut edge to external label
                var lineStartPoint = GetPointOnCircle(centerX, centerY, radius + 5, midAngle);
                
                var line = new Avalonia.Controls.Shapes.Line
                {
                    StartPoint = lineStartPoint,
                    EndPoint = labelExternalPosition,
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 2
                };
                _canvas.Children.Add(line);

                // Draw external label
                var textBlock = new TextBlock
                {
                    Text = number,
                    Foreground = new SolidColorBrush(color),
                    FontWeight = FontWeight.Bold,
                    TextAlignment = TextAlignment.Center,
                    FontSize = 12
                };

                Canvas.SetLeft(textBlock, labelExternalPosition.X - 12);
                Canvas.SetTop(textBlock, labelExternalPosition.Y - 12);

                _canvas.Children.Add(textBlock);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao desenhar number label: {ex.Message}");
        }
    }

    private Point GetPointOnCircle(double centerX, double centerY, double radius, double angleInDegrees)
    {
        var angleInRadians = (angleInDegrees - 90) * System.Math.PI / 180.0;
        var x = centerX + radius * System.Math.Cos(angleInRadians);
        var y = centerY + radius * System.Math.Sin(angleInRadians);
        return new Point(x, y);
    }
}






