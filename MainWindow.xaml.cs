using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VectorEditor.Enums;
using VectorEditor.SaveItems;
using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace VectorEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Point StartMove { get; set; }
        Point StartPos { get; set; }
        Point LastPos { get; set; }
        Shape ActiveShape;
        ControlType ActiveControlType { get; set; }
        Polyline CurrentLine { get; set; }
        Dictionary<Ellipse, int> PointsControllers { get; set; } = new();
        public MainWindow()
        {
            InitializeComponent();
            HideControl();

        }
        void CreateRectangle()
        {
            Rectangle myRect = new Rectangle
            {
                Stroke = System.Windows.Media.Brushes.Black,
                Fill = System.Windows.Media.Brushes.SkyBlue,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 50,
                Width = 50,
                RenderTransform = new TransformGroup()
                {
                    Children = new()
                    {
                        new RotateTransform(),
                    }
                },
                RenderTransformOrigin = new(0.5, 0.5)
            };
            drawGrid.Children.Add(myRect);
        }
        void CreateLine()
        {
            Polyline Line = new Polyline()
            {
                FillRule = FillRule.EvenOdd,
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 4,
                Points = new PointCollection { new Point(0, 0), new Point(10, 10) },
                RenderTransform = new TransformGroup()
                {
                    Children = new()
                    {
                        new RotateTransform(),
                    }
                },
            };
            drawGrid.Children.Add(Line);
        }
        Ellipse GetEllipse(double left, double top)
        {
            return new Ellipse()
            {
                Stroke = System.Windows.Media.Brushes.Black,
                Fill = System.Windows.Media.Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 10,
                Height = 10,
                Margin = new Thickness(left, top, 0, 0)
            };
        }

        private void AddNewLinePoint()
        {
            Point LastPoint = CurrentLine.Points.Last();
            CurrentLine.Points.Add(new(LastPoint.X + 20, LastPoint.Y));
            UpdateLinePoints();
        }
        void RemoveCurrentShape()
        {
            drawGrid.Children.Remove(ActiveShape);
            ActiveShape = null;
            HideControl();
        }
        private void Window_Down(object sender, MouseButtonEventArgs e)
        {
            StartPos = e.GetPosition(MasterWindow);
            if (e.Source == MasterWindow)
            {
                DragMove();
                HideControl();
            }
            else if (e.Source is Shape shape)
            {
                ActiveShape = shape;
                TakeControl();
            }
            else if (e.Source is System.Windows.Controls.Image img)
            {
                if (ImageRotate == img)
                {
                    TakeControl(ControlType.Rotate);
                }
                else if (img == ImageSize)
                {
                    TakeControl(ControlType.Size);
                }
                else if (img == AddLinePoint)
                {
                    AddNewLinePoint();
                }
                else
                {
                    RemoveCurrentShape();
                }
            }
        }


        void TakeControl(ControlType type = ControlType.Move)
        {
            StartMove = StartPos - new Vector(ActiveShape.Margin.Left, ActiveShape.Margin.Top);
            ActiveControlType = type;
            SetColorText(GetColor());
            MasterWindow.MouseMove += MasterWindow_MouseMove;
            MasterWindow.MouseUp += MasterWindow_MouseUp;
            Controlling.Visibility = Visibility.Visible;
            ColorGroup.Visibility = Visibility.Visible;

            UpdateControlTransform();
        }

        private Color GetColor()
        {
            if (ActiveShape is Rectangle rect)
            {
                return (rect.Fill as SolidColorBrush).Color;
            }
            else if (ActiveShape is Polyline line)
            {
                return (line.Stroke as SolidColorBrush).Color;
            }
            return new();
        }
        void SetColor(Color color)
        {
            var newbrush = new SolidColorBrush(color);
            if (ActiveShape is Rectangle rect)
            {
                rect.Fill = newbrush;
            }
            else if (ActiveShape is Polyline line)
            {
                line.Stroke = newbrush;
            }
        }

        private void SetColorText(Color color)
        {
            RedColor.Text = color.R.ToString();
            GreenColor.Text = color.G.ToString();
            BlueColor.Text = color.B.ToString();
        }

        void UpdateColor(byte? R = null, byte? G = null, byte? B = null)
        {
            var color = GetColor();
            if (R.HasValue)
            {
                color.R = R.Value;
            }

            if (G.HasValue)
            {
                color.G = G.Value;
            }

            if (B.HasValue)
            {
                color.B = B.Value;
            }
            SetColor(color);
        }

        private void UpdateControlTransform()
        {
            Thickness ShapeMargin = ActiveShape.Margin;
            if (ActiveShape is Polyline line)
            {
                Controlling.Margin = line.Margin;
            }
            else
            {
                Controlling.Width = Math.Max(ActiveShape.Height, ActiveShape.Width);
            }
            Controlling.Height = Controlling.Width;
            Controlling.Margin = ShapeMargin;
        }


        private void UpdateLinePoints()
        {
            ClearLinePoints();
            for (int i = 0; i < CurrentLine.Points.Count; i++)
            {
                Point point = CurrentLine.Points[i];
                Ellipse NewEllipse = GetEllipse(point.X + CurrentLine.Margin.Left, point.Y + CurrentLine.Margin.Top);
                drawGrid.Children.Add(NewEllipse);
                PointsControllers.Add(NewEllipse, i);
            }
        }

        private void ClearLinePoints()
        {
            foreach (KeyValuePair<Ellipse, int> point in PointsControllers)
            {
                drawGrid.Children.Remove(point.Key);
            }
            PointsControllers.Clear();
        }

        private void HideControl()
        {
            Controlling.Visibility = Visibility.Collapsed;
            AddLinePoint.Visibility = Visibility.Collapsed;
            CurrentLine = null;
            ColorGroup.Visibility = Visibility.Collapsed;
            ThicknessGroup.Visibility = Visibility.Collapsed;
            ClearLinePoints();
        }

        private void MasterWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MasterWindow.MouseMove -= MasterWindow_MouseMove;
            MasterWindow.MouseUp -= MasterWindow_MouseUp;
            Vector Diff = StartPos - e.GetPosition(MasterWindow);
            if (Diff.Length > 2)
            {
                DeleteLinePoint();
                HideControl();
            }
            else if (ActiveShape is Polyline line)
            {
                CurrentLine = line;
                LineThickness.Text = CurrentLine.StrokeThickness.ToString();
                UpdateLinePoints();
                AddLinePoint.Visibility = Visibility.Visible;
                ThicknessGroup.Visibility = Visibility.Visible;

            }
        }

        private void DeleteLinePoint()
        {
            if (ActiveShape is Ellipse ellipse && CurrentLine.Points.Count > 2)
            {
                int NumPoint = PointsControllers[ellipse] - 1;
                if (NumPoint > 0 && Point.Subtract(CurrentLine.Points[NumPoint], CurrentLine.Points[PointsControllers[ellipse]]).Length <= 5)
                {
                    CurrentLine.Points.RemoveAt(NumPoint);
                    UpdateLinePoints();
                }
                else
                {
                    NumPoint = PointsControllers[ellipse] + 1;
                    if (CurrentLine.Points.Count - 1 > NumPoint && Point.Subtract(CurrentLine.Points[NumPoint], CurrentLine.Points[PointsControllers[ellipse]]).Length <= 5)
                    {
                        CurrentLine.Points.RemoveAt(NumPoint);
                        UpdateLinePoints();
                    }
                }
            }
        }

        private void MasterWindow_MouseMove(object sender, MouseEventArgs e)
        {
            Point NewPoint = e.GetPosition(MasterWindow);
            Vector Vector = NewPoint - StartMove;
            TransformGroup Group = ActiveShape.RenderTransform as TransformGroup;
            switch (ActiveControlType)
            {
                case ControlType.Move:
                    Thickness Margin = ActiveShape.Margin;
                    Margin.Left = Vector.X;
                    Margin.Top = Vector.Y;
                    if (ActiveShape is Ellipse ellipse)
                    {
                        ellipse.Margin = Margin;
                        CurrentLine.Points[PointsControllers[ellipse]] = new Point(Vector.X - CurrentLine.Margin.Left, Vector.Y - CurrentLine.Margin.Top);
                    }
                    else
                    {
                        ActiveShape.Margin = Margin;
                    }
                    break;
                case ControlType.Rotate:
                    RotateTransform Rotate = Group.Children[0] as RotateTransform;
                    Rotate.Angle += NewPoint.Y - LastPos.Y;
                    break;
                case ControlType.Size:
                    ActiveShape.Width = Math.Max(0, ActiveShape.Width + NewPoint.X - LastPos.X);
                    ActiveShape.Height = Math.Max(0, ActiveShape.Height + NewPoint.Y - LastPos.Y);
                    break;
            }
            UpdateControlTransform();
            LastPos = NewPoint;
        }

        private void Rectangle_Click(object sender, RoutedEventArgs e)
        {
            CreateRectangle();
        }

        private void Line_Click(object sender, RoutedEventArgs e)
        {
            CreateLine();
        }

        private void MasterWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CurrentLine != null)
            {
                AddNewLinePoint();
            }
        }
        void Save(string path)
        {
            List<SaveObject> saves = new();
            foreach (object Child in drawGrid.Children)
            {
                saves.Add(new(Child as Shape));
            }
            File.WriteAllText(path, JsonSerializer.Serialize(saves));
        }
        void Load(string path)
        {
            drawGrid.Children.Clear();
            var LoadList = JsonSerializer.Deserialize<List<SaveObject>>(File.ReadAllText(path));
            foreach (var el in LoadList)
            {
                if (el.Type == nameof(Rectangle))
                {
                    drawGrid.Children.Add(JsonSerializer.Deserialize<SaveRectangle>(el.obj).GetRect());
                }
                else
                {
                    drawGrid.Children.Add(JsonSerializer.Deserialize<SavePolyLine>(el.obj).GetLine());
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new();

            if (saveFileDialog.ShowDialog() == true)
            {
                Save(saveFileDialog.FileName);
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();

            if (openFileDialog.ShowDialog() == true)
            {
                Load(openFileDialog.FileName);
            }
        }

        private void RedColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (byte.TryParse(RedColor.Text, out byte result))
            {
                UpdateColor(R: result);
            }
            else RedColor.Text = GetColor().R.ToString();
        }

        private void GreenColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (byte.TryParse(GreenColor.Text, out byte result))
            {
                UpdateColor(G: result);
            }
            else GreenColor.Text = GetColor().G.ToString();
        }

        private void BlueColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (byte.TryParse(BlueColor.Text, out byte result))
            {
                UpdateColor(B: result);
            }
            else BlueColor.Text = GetColor().B.ToString();
        }

        private void LineThickness_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(LineThickness.Text, out float result))
            {
                CurrentLine.StrokeThickness = result;
            }
        }
    }
}