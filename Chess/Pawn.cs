using System;

namespace Tsukihi.Chess
{
    public class Pawn : IPiece
    {
        public string ImagePath { get; set; }

        public Player Type { get; set; }

        public string Emoji { get; set; }

        public bool EnPassant { get; set; }

        public Pawn(Player type, Board chessBoard)
        {
            Type = type;

            ImagePath = Tsukihi.ConfigPath + "ChessResources\\" + $"{(Type == Player.White ? "white" : "black")}Pawn.png";

            EnPassant = false;

            if (Type == Player.White) Emoji = "♙";
            else Emoji = "♟";
        }

        public void AfterMove()
        {
            EnPassant = false;
        }

        public bool CanMove(int x1, int y1, int x2, int y2, IPiece[,] pieces)
        {
            // Basic movement
            if (Type == Player.White && y2 - y1 == 1 && x2 == x1 && pieces[x2, y2] == null) return true;
            if (Type == Player.Black && y2 - y1 == -1 && x2 == x1 && pieces[x2, y2] == null) return true;

            // Moving 2 forward if first turn
            if (Type == Player.White && y1 == 1 && y2 - y1 == 2)
            {
                EnPassant = true;
                return true;
            }
            if (Type == Player.Black && y1 == 6 && y2 - y1 == -2)
            {
                EnPassant = true;
                return true;
            }

            // Taking piece
            if (Type == Player.White && pieces[x2, y2] != null && pieces[x2, y2].Type == Player.Black && y2 - y1 == 1 && Math.Abs(x2 - x1) == 1) return true;
            if (Type == Player.Black && pieces[x2, y2] != null && pieces[x2, y2].Type == Player.White && y2 - y1 == -1 && Math.Abs(x2 - x1) == 1) return true;

            // EnPassnt
            if (Type == Player.White && pieces[x2, y2] == null && pieces[x2, y1] is Pawn && (pieces[x2, y1] as Pawn).EnPassant && y2 - y1 == 1 && Math.Abs(x2 - x1) == 1) return true;
            if (Type == Player.Black && pieces[x2, y2] == null && pieces[x2, y1] is Pawn && (pieces[x2, y1] as Pawn).EnPassant && y2 - y1 == -1 && Math.Abs(x2 - x1) == 1) return true;

            // Promotion - Handled externally?

            return false;
        }
    }
}