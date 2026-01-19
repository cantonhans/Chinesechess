using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;

namespace Chinesechess
{
    public partial class MainWindow : Window
    {
        private Board game = new Board();
        private Button[,] buttons = new Button[9, 10];
        private Point? selectedPoint = null;
        public MainWindow()
        {
            InitializeComponent();
            DrawBoardLines();
            InitializeGraphicBoard();
            RefreshUI();
        }

        private void InitializeGraphicBoard()
        {
            ChessGrid.Children.Clear();
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    Button btn = new Button
                    {
                        Width = 44,
                        Height = 44,
                        FontSize = 22,
                        FontWeight = FontWeights.Bold,
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Tag = new Point(x, y),
                        Template = CreateChessTemplate()
                    };
                    btn.Click += ChessTile_Click;
                    buttons[x, y] = btn;
                    ChessGrid.Children.Add(btn);
                }
            }
        }

        private ControlTemplate CreateChessTemplate()
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));

            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.Name = "PieceBorder";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(22));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(2));
            border.SetBinding(Border.BackgroundProperty, new Binding("Background")
            {
                RelativeSource = RelativeSource.TemplatedParent
            });
            border.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush")
            {
                RelativeSource = RelativeSource.TemplatedParent
            });

            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            Trigger mouseOverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Button.OpacityProperty, 0.8));
            template.Triggers.Add(mouseOverTrigger);

            return template;
        }

        private void ChessTile_Click(object sender, RoutedEventArgs e)
        {
            Button clickedBtn = (Button)sender;
            Point p = (Point)clickedBtn.Tag;
            int x = (int)p.X;
            int y = (int)p.Y;

            if (selectedPoint == null)
            {
                Piece piece = game.Grid[x, y];
                if (piece != null && piece.Color == game.CurrentTurn)
                {
                    selectedPoint = p;
                    RefreshUI();
                    clickedBtn.Background = Brushes.Yellow;
                    ShowValidMoves(piece);
                }
            }
            else
            {
                int x1 = (int)selectedPoint.Value.X;
                int y1 = (int)selectedPoint.Value.Y;

                if (game.MovePiece(x1, y1, x, y))
                {
                    selectedPoint = null;
                    RefreshUI();
                }
                else
                {
                    Piece targetPiece = game.Grid[x, y];
                    if (targetPiece != null && targetPiece.Color == game.CurrentTurn)
                    {
                        selectedPoint = p;
                        RefreshUI();
                        clickedBtn.Background = Brushes.Yellow;
                        ShowValidMoves(targetPiece);
                    }
                    else
                    {
                        selectedPoint = null;
                        RefreshUI();
                    }
                }
            }
        }
        private void ShowValidMoves(Piece p)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (game.IsValidMove(p, x, y))
                    {
                        buttons[x, y].Background = new SolidColorBrush(Color.FromArgb(100, 144, 238, 144));
                        if (game.Grid[x, y] == null)
                        {
                            buttons[x, y].BorderBrush = Brushes.LightGray;
                        }
                    }
                }
            }
        }

        private void RefreshUI()
        {
            if (game == null) return;
            foreach (var btn in buttons)
            {
                btn.Background = Brushes.Transparent;
                btn.BorderBrush = Brushes.Transparent;
                btn.Content = "";
            }

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    var piece = game.Grid[x, y];
                    if (piece != null)
                    {
                        var btn = buttons[x, y];
                        btn.Content = piece.GetName();
                        btn.Foreground = (piece.Color == Side.Red) ? Brushes.Red : Brushes.Black;
                        btn.Background = new SolidColorBrush(Color.FromRgb(240, 217, 181));
                        btn.BorderBrush = Brushes.SaddleBrown;
                    }
                }
            }
            StatusLabel.Text = $"Current: {(game.CurrentTurn == Side.Red ? "Red" : "Black")}";
            if (game.IsInCheck(game.CurrentTurn))
            {
                StatusLabel.Text += " [Checkmate!]";
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            game.UndoMove();
            RefreshUI();
        }
        private void DrawBoardLines()
        {
            BoardCanvas.Children.Clear();
            double step = 50;
            for (int i = 0; i < 10; i++)
            {
                AddLine(0, i * step, 400, i * step);
            }
            for (int i = 0; i < 9; i++)
            {
                AddLine(i * step, 0, i * step, 200);
                AddLine(i * step, 250, i * step, 450);
            }

            AddLine(0, 0, 0, 450);
            AddLine(400, 0, 400, 450);

            AddLine(150, 0, 250, 100); AddLine(250, 0, 150, 100);
            AddLine(150, 350, 250, 450); AddLine(250, 350, 150, 450);

            AddText("楚 河", 60, 210);
            AddText("汉 界", 260, 210);
        }

        private void AddLine(double x1, double y1, double x2, double y2)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.SaddleBrown,
                StrokeThickness = 2
            };
            BoardCanvas.Children.Add(line);
        }

        private void AddText(string content, double x, double y)
        {
            TextBlock txt = new TextBlock
            {
                Text = content,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.SaddleBrown,
            };
            Canvas.SetLeft(txt, x);
            Canvas.SetTop(txt, y);
            BoardCanvas.Children.Add(txt);
        }
    }
}