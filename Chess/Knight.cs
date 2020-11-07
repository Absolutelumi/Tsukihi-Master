using System;

namespace Tsukihi.Chess
{
    public class Knight : IPiece
    {
        public string ImagePath { get; set; }

        public PlayerType Type { get; set; }

        public string Emoji { get; set; }

        public Knight(PlayerType type)
        {
            Type = type;

            ImagePath = Tsukihi.ConfigPath + "ChessResources\\" + $"{(Type == PlayerType.White ? "white" : "black")}Knight.png";

            if (Type == PlayerType.White) Emoji = "♘";
            else Emoji = "♞"; 
        }

        public bool CanMove(int x1, int y1, int x2, int y2, IPiece[,] pieces)
        {
            if (Math.Abs(x2 - x1) == 2 && Math.Abs(y2 - y1) == 1) return true;
            if (Math.Abs(y2 - y1) == 2 && Math.Abs(x2 - x1) == 1) return true;

            return false; 
        }
    }
}
