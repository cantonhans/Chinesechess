using System.Collections.Generic;

namespace ChineseChessGUI
{
    public class Board
    {
        public Piece[,] Grid = new Piece[9, 10];

        public Board() { InitBoard(); }

        public struct MoveRecord
        {
            public int FromX, FromY, ToX, ToY;
            public Piece CapturedPiece; // 记录被吃掉的棋子，用于回退
            public Side PlayerSide;     // 记录是谁走的
        }

        private void InitBoard()
        {
            // 红方 (下半部分)
            AddPiece(new Piece(PieceType.Chariot, Side.Red, 0, 9));
            AddPiece(new Piece(PieceType.Horse, Side.Red, 1, 9));
            AddPiece(new Piece(PieceType.Elephant, Side.Red, 2, 9));
            AddPiece(new Piece(PieceType.Advisor, Side.Red, 3, 9));
            AddPiece(new Piece(PieceType.General, Side.Red, 4, 9));
            AddPiece(new Piece(PieceType.Advisor, Side.Red, 5, 9));
            AddPiece(new Piece(PieceType.Elephant, Side.Red, 6, 9));
            AddPiece(new Piece(PieceType.Horse, Side.Red, 7, 9));
            AddPiece(new Piece(PieceType.Chariot, Side.Red, 8, 9));
            AddPiece(new Piece(PieceType.Cannon, Side.Red, 1, 7));
            AddPiece(new Piece(PieceType.Cannon, Side.Red, 7, 7));
            for (int i = 0; i < 9; i += 2) AddPiece(new Piece(PieceType.Soldier, Side.Red, i, 6));

            // 黑方 (上半部分)
            AddPiece(new Piece(PieceType.Chariot, Side.Black, 0, 0));
            AddPiece(new Piece(PieceType.Horse, Side.Black, 1, 0));
            AddPiece(new Piece(PieceType.Elephant, Side.Black, 2, 0));
            AddPiece(new Piece(PieceType.Advisor, Side.Black, 3, 0));
            AddPiece(new Piece(PieceType.General, Side.Black, 4, 0));
            AddPiece(new Piece(PieceType.Advisor, Side.Black, 5, 0));
            AddPiece(new Piece(PieceType.Elephant, Side.Black, 6, 0));
            AddPiece(new Piece(PieceType.Horse, Side.Black, 7, 0));
            AddPiece(new Piece(PieceType.Chariot, Side.Black, 8, 0));
            AddPiece(new Piece(PieceType.Cannon, Side.Black, 1, 2));
            AddPiece(new Piece(PieceType.Cannon, Side.Black, 7, 2));
            for (int i = 0; i < 9; i += 2) AddPiece(new Piece(PieceType.Soldier, Side.Black, i, 3));
        }

        private void AddPiece(Piece p) => Grid[p.X, p.Y] = p;

        // 核心：判断是否可以移动
        public bool IsValidMove(Piece p, int targetX, int targetY)
        {
            // 1. 基础检查
            if (targetX < 0 || targetX > 8 || targetY < 0 || targetY > 9) return false;
            if (p.X == targetX && p.Y == targetY) return false;

            var targetPiece = Grid[targetX, targetY];
            if (targetPiece != null && targetPiece.Color == p.Color) return false; // 不能吃己方

            int dx = Math.Abs(targetX - p.X);
            int dy = Math.Abs(targetY - p.Y);

            switch (p.Type)
            {
                case PieceType.Chariot: // 车
                    if (p.X != targetX && p.Y != targetY) return false;
                    return CountPiecesBetween(p.X, p.Y, targetX, targetY) == 0;

                case PieceType.Horse: // 马
                    if (!((dx == 1 && dy == 2) || (dx == 2 && dy == 1))) return false;
                    int eyeX = p.X + (dx == 2 ? (targetX - p.X) / 2 : 0);
                    int eyeY = p.Y + (dy == 2 ? (targetY - p.Y) / 2 : 0);
                    return Grid[eyeX, eyeY] == null; // 蹩马腿

                case PieceType.Cannon: // 炮
                    if (p.X != targetX && p.Y != targetY) return false;
                    int count = CountPiecesBetween(p.X, p.Y, targetX, targetY);
                    if (targetPiece == null) return count == 0; // 不吃子中间0个
                    return count == 1; // 吃子中间1个

                case PieceType.Elephant: // 象/相
                    if (dx != 2 || dy != 2) return false; // 走田字
                    if (p.Color == Side.Red && targetY < 5) return false; // 红不越河
                    if (p.Color == Side.Black && targetY > 4) return false; // 黑不越河
                    return Grid[(p.X + targetX) / 2, (p.Y + targetY) / 2] == null; // 塞象眼

                case PieceType.Advisor: // 士/仕
                    if (dx != 1 || dy != 1) return false; // 走斜线
                    if (targetX < 3 || targetX > 5) return false; // 限制在九宫格横向
                    if (p.Color == Side.Red && targetY < 7) return false; // 红九宫纵向
                    if (p.Color == Side.Black && targetY > 2) return false; // 黑九宫纵向
                    return true;

                case PieceType.General: // 将/帅
                    if (dx + dy != 1) return false; // 走直线一步
                    if (targetX < 3 || targetX > 5) return false; // 九宫格横向
                    if (p.Color == Side.Red && targetY < 7) return false;
                    if (p.Color == Side.Black && targetY > 2) return false;
                    return true;

                case PieceType.Soldier: // 兵/卒
                    if (dx + dy != 1) return false; // 只能走一步
                    if (p.Color == Side.Red)
                    {
                        if (targetY > p.Y) return false; // 兵不能后退
                        if (p.Y > 4 && dx != 0) return false; // 未过河不能左右
                    }
                    else
                    {
                        if (targetY < p.Y) return false; // 卒不能后退
                        if (p.Y < 5 && dx != 0) return false; // 未过河不能左右
                    }
                    return true;
            }
            return false;
        }

        private int CountPiecesBetween(int x1, int y1, int x2, int y2)
        {
            int count = 0;
            if (x1 == x2)
            {
                for (int y = Math.Min(y1, y2) + 1; y < Math.Max(y1, y2); y++)
                    if (Grid[x1, y] != null) count++;
            }
            else
            {
                for (int x = Math.Min(x1, x2) + 1; x < Math.Max(x1, x2); x++)
                    if (Grid[x, y1] != null) count++;
            }
            return count;
        }

        public Side CurrentTurn { get; private set; } = Side.Red; // 红方先手

        public bool MovePiece(int x1, int y1, int x2, int y2)
        {
            Piece p = Grid[x1, y1];
            if (p == null || p.Color != CurrentTurn || !IsValidMove(p, x2, y2)) return false;

            // 记录这一步，用于悔棋或模拟回退
            var record = new MoveRecord
            {
                FromX = x1,
                FromY = y1,
                ToX = x2,
                ToY = y2,
                CapturedPiece = Grid[x2, y2],
                PlayerSide = CurrentTurn
            };

            // 模拟移动
            Grid[x2, y2] = p;
            Grid[x1, y1] = null!;
            p.X = x2; p.Y = y2;

            // 检查：移动后自己是否被将军（自杀步判断）
            if (IsInCheck(CurrentTurn))
            {
                // 如果移动后导致自己被将军，则必须回退（非法）
                p.X = x1; p.Y = y1;
                Grid[x1, y1] = p;
                Grid[x2, y2] = record.CapturedPiece;
                return false;
            }

            // 成功移动，存入历史
            moveHistory.Push(record);
            CurrentTurn = (CurrentTurn == Side.Red) ? Side.Black : Side.Red;
            return true;
        }
        private Stack<MoveRecord> moveHistory = new Stack<MoveRecord>();

        public void UndoMove()
        {
            if (moveHistory.Count == 0) return;

            var lastMove = moveHistory.Pop();
            Piece p = Grid[lastMove.ToX, lastMove.ToY];

            // 1. 把棋子移回原位
            Grid[lastMove.FromX, lastMove.FromY] = p;
            p.X = lastMove.FromX;
            p.Y = lastMove.FromY;

            // 2. 恢复被吃掉的棋子
            Grid[lastMove.ToX, lastMove.ToY] = lastMove.CapturedPiece;

            // 3. 切换回上一个玩家的回合
            CurrentTurn = lastMove.PlayerSide;
        }
        // 找到某个颜色的将/帅的位置
        public (int x, int y) FindGeneral(Side side)
        {
            for (int y = 0; y < 10; y++)
                for (int x = 0; x < 9; x++)
                {
                    var p = Grid[x, y];
                    if (p != null && p.Type == PieceType.General && p.Color == side)
                        return (x, y);
                }
            return (-1, -1);
        }

        // 判断某一方是否正在被“将军”
        public bool IsInCheck(Side victimSide)
        {
            var (genX, genY) = FindGeneral(victimSide);
            Side attackerSide = (victimSide == Side.Red) ? Side.Black : Side.Red;

            // 遍历棋盘寻找攻击方的棋子
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    var p = Grid[x, y];
                    if (p != null && p.Color == attackerSide)
                    {
                        // 如果攻击方的棋子能合法移动到将的位置，就是将军
                        if (IsValidMove(p, genX, genY)) return true;
                    }
                }
            }
            return false;
        }
    }
}