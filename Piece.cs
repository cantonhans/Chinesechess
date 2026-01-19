using System;

namespace Chinesechess
{
    public enum Side { Red, Black }
    public enum PieceType { Chariot, Horse, Elephant, Advisor, General, Cannon, Soldier }

    public class Piece
    {
        public PieceType Type { get; set; }
        public Side Color { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Piece(PieceType type, Side color, int x, int y)
        {
            Type = type; Color = color; X = x; Y = y;
        }

        public string GetName() => Color == Side.Red ? GetRedName() : GetBlackName();

        private string GetRedName() => Type switch
        {
            PieceType.General => "帅",
            PieceType.Chariot => "车",
            PieceType.Horse => "马",
            PieceType.Cannon => "炮",
            PieceType.Advisor => "仕",
            PieceType.Elephant => "相",
            PieceType.Soldier => "兵",
            _ => ""
        };

        private string GetBlackName() => Type switch
        {
            PieceType.General => "将",
            PieceType.Chariot => "車",
            PieceType.Horse => "馬",
            PieceType.Cannon => "砲",
            PieceType.Advisor => "士",
            PieceType.Elephant => "象",
            PieceType.Soldier => "卒",
            _ => ""
        };
    }
}