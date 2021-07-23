using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VectorEditor.SaveItems
{
    public class SaveObject
    {
        public SaveObject()
        {

        }
        public SaveObject(Shape shape)
        {
            Type = shape.GetType().Name;
            if (shape is Rectangle rect)
            {
                obj = JsonSerializer.Serialize(new SaveRectangle(rect));
            }
            else if (shape is Polyline line)
            {
                obj = JsonSerializer.Serialize(new SavePolyLine(line));
            }
        }
        public string Type { get; set; }
        public string obj { get; set; }

    }
    public class SaveItem
    {
        public Color Color { get; set; }

        public double Left { get; set; }
        public double Top { get; set; }
        public double Angle { get; set; }
        protected void SetMargin(Shape shape)
        {
            Left = shape.Margin.Left;
            Top = shape.Margin.Top;
            Angle = ((shape.RenderTransform as TransformGroup).Children[0] as RotateTransform).Angle;
        }
    }
    public class SaveRectangle : SaveItem
    {
        public SaveRectangle() { }
        public SaveRectangle(System.Windows.Shapes.Rectangle rectangle)
        {
            SetMargin(rectangle);
            Color = (rectangle.Fill as SolidColorBrush).Color;
            Height = rectangle.Height;
            Width = rectangle.Width;
        }
        public double Height { get; set; }
        public double Width { get; set; }
        public Rectangle GetRect()
        {
            return new System.Windows.Shapes.Rectangle
            {
                Stroke = System.Windows.Media.Brushes.Black,
                Fill = new SolidColorBrush(Color),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = Height,
                Width = Width,
                RenderTransform = new TransformGroup()
                {
                    Children = new()
                    {
                        new RotateTransform(Angle),
                    }
                },
                RenderTransformOrigin = new(0.5, 0.5),
                Margin = new(Left, Top, 0, 0)
            };
        }
    }
    public class SavePolyLine : SaveItem
    {
        public SavePolyLine() { }
        public SavePolyLine(Polyline line)
        {
            SetMargin(line);
            Color = (line.Stroke as SolidColorBrush).Color;
            points = line.Points;
            StrokeThickness = line.StrokeThickness;
        }
        public PointCollection points { get; set; }
        public double StrokeThickness { get; set; }
        public Polyline GetLine()
        {
            return new()
            {
                FillRule = FillRule.EvenOdd,
                Stroke = new SolidColorBrush(Color),
                StrokeThickness = StrokeThickness,
                Points = points,
                RenderTransform = new TransformGroup()
                {
                    Children = new()
                    {
                        new RotateTransform(Angle),
                    }
                },
                Margin = new(Left, Top, 0, 0)
            };
        }
    }
}
