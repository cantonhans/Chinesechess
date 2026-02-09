using System;
using System.Collections.Generic;
using System.Linq;

namespace Chinesechess
{
    public class Board
    {
        public Piece[,] Grid = new Piece[9, 10];
        public Side CurrentTurn { get; private set; } = Side.Red;
        private Stack<MoveRecord> moveHistory = new Stack<MoveRecord>();

        public Board() { InitBoard(); }

        public struct MoveRecord
        {
            public int FromX, FromY, ToX, ToY;
            public Piece CapturedPiece;
            public Side PlayerSide;
        }

        public MoveRecord? GetLastMove() => moveHistory.Count == 0 ? null : moveHistory.Peek();

        private void InitBoard()
        {
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

        public bool IsValidMove(Piece p, int targetX, int targetY)
        {
            if (targetX < 0 || targetX > 8 || targetY < 0 || targetY > 9) return false;
            if (p.X == targetX && p.Y == targetY) return false;

            var targetPiece = Grid[targetX, targetY];
            if (targetPiece != null && targetPiece.Color == p.Color) return false;

            int dx = Math.Abs(targetX - p.X);
            int dy = Math.Abs(targetY - p.Y);

            switch (p.Type)
            {
                case PieceType.Chariot:
                    if (p.X != targetX && p.Y != targetY) return false;
                    return CountPiecesBetween(p.X, p.Y, targetX, targetY) == 0;

                case PieceType.Horse:
                    if (!((dx == 1 && dy == 2) || (dx == 2 && dy == 1))) return false;
                    int eyeX = p.X + (dx == 2 ? (targetX - p.X) / 2 : 0);
                    int eyeY = p.Y + (dy == 2 ? (targetY - p.Y) / 2 : 0);
                    return Grid[eyeX, eyeY] == null;

                case PieceType.Cannon:
                    if (p.X != targetX && p.Y != targetY) return false;
                    int count = CountPiecesBetween(p.X, p.Y, targetX, targetY);
                    return targetPiece == null ? count == 0 : count == 1;

                case PieceType.Elephant:
                    if (dx != 2 || dy != 2) return false;
                    if (p.Color == Side.Red && targetY < 5) return false;
                    if (p.Color == Side.Black && targetY > 4) return false;
                    return Grid[(p.X + targetX) / 2, (p.Y + targetY) / 2] == null;

                case PieceType.Advisor:
                    if (dx != 1 || dy != 1) return false;
                    if (targetX < 3 || targetX > 5) return false;
                    return p.Color == Side.Red ? targetY >= 7 : targetY <= 2;

                case PieceType.General:
                    if (targetPiece != null && targetPiece.Type == PieceType.General)
                    {
                        if (p.X == targetX && CountPiecesBetween(p.X, p.Y, targetX, targetY) == 0) return true;
                    }
                    if (dx + dy != 1) return false;
                    if (targetX < 3 || targetX > 5) return false;
                    return p.Color == Side.Red ? targetY >= 7 : targetY <= 2;

                case PieceType.Soldier:
                    if (dx + dy != 1) return false;
                    if (p.Color == Side.Red)
                    {
                        if (targetY > p.Y) return false;
                        if (p.Y > 4 && dx != 0) return false;
                    }
                    else
                    {
                        if (targetY < p.Y) return false;
                        if (p.Y < 5 && dx != 0) return false;
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
            else if (y1 == y2)
            {
                for (int x = Math.Min(x1, x2) + 1; x < Math.Max(x1, x2); x++)
                    if (Grid[x, y1] != null) count++;
            }
            return count;
        }
        public List<(int x, int y)> GetLegalMoves(Piece p)
        {
            var legalMoves = new List<(int x, int y)>();
            if (p == null) return legalMoves;

            for (int ty = 0; ty < 10; ty++)
            {
                for (int tx = 0; tx < 9; tx++)
                {
                    if (IsValidMove(p, tx, ty))
                    {
                        int oldX = p.X, oldY = p.Y;
                        var targetPiece = Grid[tx, ty];

                        Grid[tx, ty] = p;
                        Grid[oldX, oldY] = null!;
                        p.X = tx; p.Y = ty;

                        bool stillInCheck = IsInCheck(p.Color);

                        p.X = oldX; p.Y = oldY;
                        Grid[oldX, oldY] = p;
                        Grid[tx, ty] = targetPiece;

                        if (!stillInCheck)
                        {
                            legalMoves.Add((tx, ty));
                        }
                    }
                }
            }
            return legalMoves;
        }

        public bool MovePiece(int x1, int y1, int x2, int y2)
        {
            Piece p = Grid[x1, y1];
            if (p == null || p.Color != CurrentTurn) return false;

            var legalMoves = GetLegalMoves(p);
            if (!legalMoves.Any(m => m.x == x2 && m.y == y2)) return false;

            var record = new MoveRecord
            {
                FromX = x1,
                FromY = y1,
                ToX = x2,
                ToY = y2,
                CapturedPiece = Grid[x2, y2],
                PlayerSide = CurrentTurn
            };

            Grid[x2, y2] = p;
            Grid[x1, y1] = null!;
            p.X = x2; p.Y = y2;

            moveHistory.Push(record);
            CurrentTurn = (CurrentTurn == Side.Red) ? Side.Black : Side.Red;
            return true;
        }
        public bool IsCheckmate(Side side)
        {
            if (!IsInCheck(side)) return false;

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    var p = Grid[x, y];
                    if (p != null && p.Color == side)
                    {
                        if (GetLegalMoves(p).Count > 0) return false;
                    }
                }
            }
            return true;
        }

        public void UndoMove()
        {
            if (moveHistory.Count == 0) return;
            var lastMove = moveHistory.Pop();
            Piece p = Grid[lastMove.ToX, lastMove.ToY];
            Grid[lastMove.FromX, lastMove.FromY] = p;
            p.X = lastMove.FromX; p.Y = lastMove.FromY;
            Grid[lastMove.ToX, lastMove.ToY] = lastMove.CapturedPiece;
            CurrentTurn = lastMove.PlayerSide;
        }

        public void ResetGame()
        {
            Array.Clear(Grid, 0, Grid.Length);
            moveHistory.Clear();
            CurrentTurn = Side.Red;
            InitBoard();
        }

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

        public bool IsInCheck(Side victimSide)
        {
            var (genX, genY) = FindGeneral(victimSide);
            if (genX == -1) return false;

            Side attackerSide = (victimSide == Side.Red) ? Side.Black : Side.Red;

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    var p = Grid[x, y];
                    if (p != null && p.Color == attackerSide)
                    {
                        if (IsValidMove(p, genX, genY)) return true;
                    }
                }
            }
            return false;
        }
    }
}