using System;

namespace Tsukihi.Chess
{
    public class King : IPiece
    {
        public string ImagePath { get; set; }

        public PlayerType Type { get; set; }
        
        public string Emoji { get; set; }

        public bool FirstMove { get; set; }

        public King(PlayerType type)
        {
            Type = type;

            ImagePath = Tsukihi.ConfigPath + "ChessResources\\" + $"{(Type == PlayerType.White ? "white" : "black")}King.png"; 

            FirstMove = true; 

            if (Type == PlayerType.White) Emoji = "♕";
            else Emoji = "♛"; 
        }

        public bool CanMove(int x1, int y1, int x2, int y2, IPiece[,] pieces)
        {
            if (Math.Abs(x2 - x1) > 1 || Math.Abs(y2 - y1) > 1) return false;

            if (x1 == x2) return true; 

            if (y1 == y2) return true;

            if (Math.Abs(y2 - y1) == Math.Abs(x2 - x1)) return true;

            return false;
        }
    }
}
