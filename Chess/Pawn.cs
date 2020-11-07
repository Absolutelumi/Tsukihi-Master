using System;

namespace Tsukihi.Chess
{
    public class Pawn : IPiece
    {
        public string ImagePath { get; set; }

        public PlayerType Type { get; set; }

        public string Emoji { get; set; }

        public bool EnPassant { get; set; }

        public Pawn(PlayerType type, Board chessBoard)
        {
            Type = type;

            ImagePath = Tsukihi.ConfigPath + "ChessResources\\" + $"{(Type == PlayerType.White ? "white" : "black")}Pawn.png";

            EnPassant = false; 

            if (Type == PlayerType.White) Emoji = "♙";
            else Emoji = "♟"; 
        }

        public bool CanMove(int x1, int y1, int x2, int y2, IPiece[,] pieces)
        {
            bool canMove = false;

            // Basic movement
            if (pieces[x2, y2] == null && Type == PlayerType.White && y2 - y1 == 1 && Math.Abs(x2 - x1) == 0) canMove = true;
            else if (pieces[x2, y2] == null && Type == PlayerType.Black && y2 - y1 == -1 && Math.Abs(x2 - x1) == 0) canMove = true;

            // Moving 2 forward if first turn 
            else if (pieces[x2, y2] == null && Type == PlayerType.White && y1 == 1 && y2 - y1 == 2 && Math.Abs(x2 - x1) == 0) { EnPassant = true; return true; }
            else if (pieces[x2, y2] == null && Type == PlayerType.Black && y1 == 6 && y2 - y1 == -2 && Math.Abs(x2 - x1) == 0) { EnPassant = true; return true; }

            // Taking piece
            else if (Type == PlayerType.White && pieces[x2, y2] != null && y2 - y1 == 1 && Math.Abs(x2 - x1) == 1) canMove = true;
            else if (Type == PlayerType.Black && pieces[x2, y2] != null && y2 - y1 == -1 && Math.Abs(x2 - x1) == 1) canMove = true;

            // EnPassnt
            else if (Type == PlayerType.White && pieces[x2, y2] == null && (pieces[x2, y1] as Pawn)?.EnPassant == true && y2 - y1 == 1 && Math.Abs(x2 - x1) == 1) { pieces[x2, y1] = null;  canMove = true; }
            else if (Type == PlayerType.Black && pieces[x2, y2] == null && (pieces[x2, y1] as Pawn)?.EnPassant == true && y2 - y1 == -1 && Math.Abs(x2 - x1) == 1) { pieces[x2, y1] = null; canMove = true; }

            if (canMove && EnPassant) EnPassant = false;
            return canMove; 
        }
    }
}
