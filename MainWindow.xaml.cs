using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Input;

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
            mouseOverTrigger.Setters.Add(new Setter(Button.OpacityProperty, 0.7));

            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty,
                new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))));
            mouseOverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, Brushes.OrangeRed));

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
                    if (game.IsCheckmate(game.CurrentTurn))
                    {
                        string winner = (game.CurrentTurn == Side.Red) ? "Black" : "Red";
                        MessageLabel.Text = $"{winner} WIN!";
                        MessageLabel.Foreground = Brushes.Gold;
                        MessageBox.Show($"Checkmate! {winner} Wins!", "Game Over");
                    }
                }
                else
                {
                    if (game.IsInCheck(game.CurrentTurn))
                    {
                        MessageLabel.Text = "Invalid Move: Your General is still under attack!";
                        MessageLabel.Foreground = Brushes.Red;
                    }

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

        private void ChessGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedPoint != null)
            {
                selectedPoint = null;
                RefreshUI();
            }
        }
        private void ShowValidMoves(Piece p)
        {
            var legalMoves = game.GetLegalMoves(p);

            foreach (var move in legalMoves)
            {
                buttons[move.x, move.y].Background = new SolidColorBrush(Color.FromArgb(100, 144, 238, 144));
                if (game.Grid[move.x, move.y] == null)
                {
                    buttons[move.x, move.y].BorderBrush = Brushes.LightGray;
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

            if (game.CurrentTurn == Side.Red)
            {
                TopBar.Background = new SolidColorBrush(Color.FromRgb(150, 0, 0));
            }
            else
            {
                TopBar.Background = new SolidColorBrush(Color.FromRgb(40, 40, 40));
            }
            StatusLabel.Text = $"Turn: {(game.CurrentTurn == Side.Red ? "Red" : "Black")}";

            var last = game.GetLastMove();
            if (last != null)
            {
                var p = game.Grid[last.Value.ToX, last.Value.ToY];
                string pieceName = p?.GetName() ?? "";
                MessageLabel.Text = $"Last move: {last.Value.PlayerSide} {pieceName} ({last.Value.FromX},{last.Value.FromY}) -> ({last.Value.ToX},{last.Value.ToY})";
                MessageLabel.Foreground = Brushes.LightGray;
            }

            if (game.IsInCheck(game.CurrentTurn))
            {
                if (!game.IsCheckmate(game.CurrentTurn))
                {
                    MessageLabel.Text = "⚠️ CHECK! You must protect your General!";
                    MessageLabel.Foreground = Brushes.OrangeRed;
                }
                var (gx, gy) = game.FindGeneral(game.CurrentTurn);
                if (gx != -1) buttons[gx, gy].Background = Brushes.Pink;
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            game.UndoMove();
            RefreshUI();
        }
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Sure to RESTART?", "Hint", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                game.ResetGame();
                selectedPoint = null;
                RefreshUI();
                StatusLabel.Text = "GAME has Restarted, now: Red";
            }
        }
        private void DrawBoardLines()
        {
            BoardCanvas.Children.Clear();

            double step = 50;
            double startX = 10;
            double startY = 10;

            for (int i = 0; i < 10; i++)
            {
                AddLine(startX, startY + i * step, startX + 400, startY + i * step);
            }

            for (int i = 0; i < 9; i++)
            {
                AddLine(startX + i * step, startY, startX + i * step, startY + 200);
                AddLine(startX + i * step, startY + 250, startX + i * step, startY + 450);
            }

            AddLine(startX, startY, startX, startY + 450);
            AddLine(startX + 400, startY, startX + 400, startY + 450);

            AddLine(startX + 150, startY, startX + 250, startY + 100);
            AddLine(startX + 250, startY, startX + 150, startY + 100);

            AddLine(startX + 150, startY + 350, startX + 250, startY + 450);
            AddLine(startX + 250, startY + 350, startX + 150, startY + 450);

            AddText("楚 河", startX + 60, startY + 210);
            AddText("汉 界", startX + 260, startY + 210);
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