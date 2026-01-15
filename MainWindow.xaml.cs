using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;

namespace ChineseChessGUI
{
    public partial class MainWindow : Window
    {
        // 核心逻辑对象（原 Program.cs 里的 gameBoard）
        private Board game = new Board();

        // 用于在内存中引用界面上的按钮，方便刷新
        private Button[,] buttons = new Button[9, 10];

        // 模拟原本的“第一次输入 (x1,y1)”
        private Point? selectedPoint = null;

        public MainWindow()
        {
            InitializeComponent();
            DrawBoardLines();        // 1. 画背景线
            InitializeGraphicBoard(); // 2. 生成透明按钮
            RefreshUI();             // 3. 摆放棋子
        }

        // 代替原 DrawBoard 的初始化部分
        private void InitializeGraphicBoard()
        {
            ChessGrid.Children.Clear();
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    Button btn = new Button
                    {
                        Width = 44,   // 略小于50，防止棋子互相挤占
                        Height = 44,
                        FontSize = 22,
                        FontWeight = FontWeights.Bold,
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Tag = new Point(x, y),
                        // 使用模板将按钮变为圆形
                        Template = CreateChessTemplate()
                    };
                    btn.Click += ChessTile_Click;
                    buttons[x, y] = btn;
                    ChessGrid.Children.Add(btn);
                }
            }
        }

        // 动态创建一个圆形的按钮模板
        private ControlTemplate CreateChessTemplate()
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));

            // 1. 创建 Border 作为棋子的圆盘形状
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.Name = "PieceBorder";

            // 设置圆角（22是44像素宽的一半），使其成为正圆
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(22));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(2));

            // --- 核心点：将 Border 的颜色绑定到 Button 自身的属性 ---
            // 这样你在 RefreshUI 或 ShowValidMoves 里设置的 Background 才会生效
            border.SetBinding(Border.BackgroundProperty, new Binding("Background")
            {
                RelativeSource = RelativeSource.TemplatedParent
            });
            border.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush")
            {
                RelativeSource = RelativeSource.TemplatedParent
            });

            // 2. 创建 ContentPresenter 用于显示棋子文字（如“馬”、“炮”）
            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            // 将文字放入圆盘中
            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            // 3. 添加触发器：鼠标悬停时稍微加深一点背景，提升手感
            Trigger mouseOverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            // 当鼠标悬停且该位置有颜色时，稍微改变透明度或亮度
            mouseOverTrigger.Setters.Add(new Setter(Button.OpacityProperty, 0.8));
            template.Triggers.Add(mouseOverTrigger);

            return template;
        }

        // 处理点击：对应原 Program.cs 里的输入解析逻辑
        private void ChessTile_Click(object sender, RoutedEventArgs e)
        {
            Button clickedBtn = (Button)sender;
            Point p = (Point)clickedBtn.Tag;
            int x = (int)p.X;
            int y = (int)p.Y;

            if (selectedPoint == null)
            {
                // --- 第一次点击：选子 ---
                Piece piece = game.Grid[x, y];
                if (piece != null && piece.Color == game.CurrentTurn)
                {
                    selectedPoint = p;
                    RefreshUI(); // 先清理旧的高亮
                    clickedBtn.Background = Brushes.Yellow; // 选中状态

                    // 【关键代码】：显示可移动位置
                    ShowValidMoves(piece);
                }
            }
            else
            {
                // --- 第二次点击：落子 ---
                int x1 = (int)selectedPoint.Value.X;
                int y1 = (int)selectedPoint.Value.Y;

                if (game.MovePiece(x1, y1, x, y))
                {
                    selectedPoint = null;
                    RefreshUI(); // 移动成功，重绘棋盘
                }
                else
                {
                    // 如果点的是自己的其他棋子，切换选中目标
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
                        // 点击了非法位置，取消选择
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
                        // 设置半透明绿色背景
                        buttons[x, y].Background = new SolidColorBrush(Color.FromArgb(100, 144, 238, 144));

                        // 如果是空位，给它一个很淡的边框提示位置
                        if (game.Grid[x, y] == null)
                        {
                            buttons[x, y].BorderBrush = Brushes.LightGray;
                        }
                    }
                }
            }
        }

        // 代替原 DrawBoard 的打印部分
        private void RefreshUI()
        {
            if (game == null) return;

            // for (int y = 0; y < 10; y++)
            // {
            //     for (int x = 0; x < 9; x++)
            //     {
            //         var piece = game.Grid[x, y];
            //         // --- 关键修复：定义 btn 变量 ---
            //         var btn = buttons[x, y];

            //         if (piece != null)
            //         {
            //             btn.Content = piece.GetName();
            //             // 设置棋子可见性（有子时显示圆盘）
            //             btn.Opacity = 1.0;
            //             btn.IsHitTestVisible = true;
            //             btn.Foreground = (piece.Color == Side.Red) ? Brushes.Red : Brushes.Black;
            //         }
            //         else
            //         {
            //             btn.Content = "";
            //             // 关键：没子时让按钮变透明，但依然可以点击（方便落子）
            //             btn.Opacity = 0.0;
            //             btn.IsHitTestVisible = true;
            //         }
            //     }
            // }
            foreach (var btn in buttons)
            {
                // 彻底透明化
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

                        // 只有有子的地方，才显示木质圆盘和边框
                        btn.Background = new SolidColorBrush(Color.FromRgb(240, 217, 181));
                        btn.BorderBrush = Brushes.SaddleBrown;
                    }
                }
            }
            // 更新状态文本
            StatusLabel.Text = $"当前回合: {(game.CurrentTurn == Side.Red ? "红方" : "黑方")}";
            if (game.IsInCheck(game.CurrentTurn))
            {
                StatusLabel.Text += " [将军！]";
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
            double step = 50; // 每个格子的绝对大小

            // 画横线 (0 到 450)
            for (int i = 0; i < 10; i++)
            {
                AddLine(0, i * step, 400, i * step);
            }

            // 画竖线 (0 到 400)
            for (int i = 0; i < 9; i++)
            {
                // 楚河汉界：竖线在第4格和第5格之间断开（y坐标200到250）
                AddLine(i * step, 0, i * step, 200);
                AddLine(i * step, 250, i * step, 450);
            }

            // 补齐左右两条长竖线
            AddLine(0, 0, 0, 450);
            AddLine(400, 0, 400, 450);

            // 九宫格 (X形)
            AddLine(150, 0, 250, 100); AddLine(250, 0, 150, 100); // 黑方
            AddLine(150, 350, 250, 450); AddLine(250, 350, 150, 450); // 红方

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