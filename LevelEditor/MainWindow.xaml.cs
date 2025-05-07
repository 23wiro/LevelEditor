using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TileEditor
{


    public enum LT
    {
        None,
        Ground,
        Spike,
        Platform,
        Flag
    }


    public partial class MainWindow : Window
    {
        private LT[,] mapData = null;
        private bool isDrawing = false;
        private LT currentTileType = LT.None;
        private int rows = 0, cols = 0;

        private Brush GetTileBrush(LT tile)
        {
            return tile switch
            {
                LT.Ground => Brushes.Green,
                LT.Spike => Brushes.Red,
                LT.Platform => Brushes.Blue,
                LT.Flag => Brushes.Yellow,
                _ => Brushes.White, // LT.None
            };
        }

        public MainWindow()
        {
            InitializeComponent();
            TileTypeComboBox.SelectionChanged += TileTypeComboBox_SelectionChanged;
        }

        private void TileTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TileTypeComboBox.SelectedItem is ComboBoxItem item)
            {
                currentTileType = (LT)Enum.Parse(typeof(LT), item.Content.ToString());
            }
        }

        private void GenerateGrid(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(WidthBox.Text, out cols) || !int.TryParse(HeightBox.Text, out rows))
            {
                MessageBox.Show("Invalid grid size");
                return;
            }

            mapData = new LT[rows, cols];
            GridPanel.Children.Clear();
            GridPanel.Columns = cols;
            GridPanel.Rows = rows;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Border border = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5),
                        Background = Brushes.White
                    };
                    border.Tag = Tuple.Create(i, j);

                    TextBlock text = new TextBlock
                    {
                        Text = LT.None.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    border.Child = text;

                    border.MouseLeftButtonDown += Cell_MouseDown;
                    border.MouseLeftButtonUp += Cell_MouseUp;
                    border.MouseEnter += Cell_MouseEnter;

                    GridPanel.Children.Add(border);
                }
            }
        }

        private void Cell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDrawing = true;
                UpdateCell((Border)sender);
            }
        }

        private void Cell_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isDrawing && e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateCell((Border)sender);
            }
        }

        private void Cell_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDrawing = false;
            }
        }

        private void UpdateCell(Border border)
        {
            var coord = (Tuple<int, int>)border.Tag;
            int r = coord.Item1, c = coord.Item2;
            mapData[r, c] = currentTileType;

            if (border.Child is TextBlock text)
            {
                text.Text = currentTileType.ToString();
            }

            border.Background = GetTileBrush(currentTileType);
        }


        private void ExportGrid(object sender, RoutedEventArgs e)
        {
            if (mapData == null) return;
            var sb = new StringBuilder();
            sb.AppendLine($"public Enum[,] level = new Enum[{rows}, {cols}]");
            sb.AppendLine("{");
            for (int i = 0; i < rows; i++)
            {
                sb.Append("  {");
                for (int j = 0; j < cols; j++)
                {
                    sb.Append($"LT.{mapData[i, j]}");
                    if (j < cols - 1) sb.Append(", ");
                }
                sb.Append("}");
                if (i < rows - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("};");
            OutputTextBox.Text = sb.ToString();
        }
    }
}
