using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tsukihi.Chess
{
    public class Board
    {
        private IPiece[,] Pieces { get; set; }

        private string ImagePath { get; set; }

        public Board()
        {
            Pieces = new IPiece[8, 8];

            ImagePath = Tsukihi.ConfigPath + "ChessResources\\board.png"; 

            for (int i = 0; i < 8; i++)
            {
                Pieces[i, 1] = new Pawn(Player.White, this);
                Pieces[i, 6] = new Pawn(Player.Black, this);

                switch (i)
                {
                    case 0:
                    case 7:
                        Pieces[i, 0] = new Rook(Player.White);
                        Pieces[i, 7] = new Rook(Player.Black);
                        break;

                    case 1:
                    case 6:
                        Pieces[i, 0] = new Knight(Player.White);
                        Pieces[i, 7] = new Knight(Player.Black);
                        break;

                    case 2:
                    case 5:
                        Pieces[i, 0] = new Bishop(Player.White);
                        Pieces[i, 7] = new Bishop(Player.Black);
                        break;

                    case 3:
                        Pieces[i, 0] = new Queen(Player.White);
                        Pieces[i, 7] = new Queen(Player.Black);
                        break;

                    case 4:
                        Pieces[i, 0] = new King(Player.White);
                        Pieces[i, 7] = new King(Player.Black);
                        break; 
                }
            }
        }

        public bool Move(Player player, int x1, int y1, int x2, int y2)
        {
            IPiece piece = Pieces[x1, y1];

            if (piece == null || piece.Type != player) return false;
            // First half self explanatory, second returns false if player is trying to move piece to another ally's position, IF they are not rook or king (b/c of castling) 
            if (!Pieces[x1, y1].CanMove(x1, y1, x2, y2, Pieces)) return false;
            if (Pieces[x2, y2] != null) if ((!(Pieces[x1, y1] is Rook) && !(Pieces[x1, y1] is King) && Pieces[x2, y2].Type == Pieces[x1, y1].Type)) return false;

            // Castling Logic - Optimize ???????
            if (Pieces[x2, y2] != null) if (Pieces[x1, y1] is Rook || Pieces[x1, y1] is King && Pieces[x2, y2] is Rook || Pieces[x2, y2] is King)
            {
                if (Pieces[x1, y1] is Rook)
                {
                    if ((Pieces[x1, y1] as Rook).FirstMove)
                    {
                        Pieces[x2 + (x1 - x2 > 0 ? 2 : -2), y2] = Pieces[x2, y2];
                        Pieces[x2, y2] = null;

                        Pieces[x1 + (x1 - x2 > 0 ? -2 : 3), y1] = Pieces[x1, y1];
                        Pieces[x1, y1] = null;

                        (Pieces[x1 + (x1 - x2 > 0 ? -2 : 3), y1] as Rook).FirstMove = false;
                        (Pieces[x2 + (x1 - x2 > 0 ? 2 : -2), y2] as King).FirstMove = false; 

                        return true; 
                    }
                }

                // This part doesnt work, ? 
                else
                {
                    if ((Pieces[x1, y1] as King).FirstMove)
                    {
                        Pieces[x1 + (x2 - x1 > 0 ? 2 : -2), y1] = Pieces[x1, y1];
                        Pieces[x1, y1] = null;

                        Pieces[x2 + (x2 - x1 > 0 ? -2 : 3), y2] = Pieces[x2, y2];
                        Pieces[x2, y2] = null;

                        (Pieces[x2 + (x2 - x1 > 0 ? -2 : 3), y2] as Rook).FirstMove = false;
                        (Pieces[x1 + (x2 - x1 > 0 ? 2 : -2), y1] as King).FirstMove = false;

                        return true;
                    }
                }

                return false; 
            }

            if (Pieces[x1, y1] is Pawn && x2 - x1 != 0) Pieces[x2, y1] = null; 

            Pieces[x2, y2] = Pieces[x1, y1];

            Pieces[x1, y1] = null;

            return true;
        }

        public KeyValuePair<Player, bool> InCheck()
        {
            // Key is for player in check, value is for whether player is checkmate'd or not 
            // Returns null on key if no one is check'd 

            throw new NotImplementedException(); 
        }

        public string UpdateBoardImage(string channelId)
        {
            Bitmap boardImage = new Bitmap(this.ImagePath); 

            using (var graphics = Graphics.FromImage(boardImage))
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        // If needed for performance, can make bitmap variable for each piece type and load here manually, but pref. not considering ammount of code neccessary to do
                        if (Pieces[j, i] == null) continue; 
                        graphics.DrawImage(new Bitmap(Pieces[j, i].ImagePath), new Rectangle(65 + 138 * j, 65 + 138 * (7 - i), 120, 120));
                    }
                }
            }

            string path = Tsukihi.TempPath + $"chess{channelId}.png";
            boardImage.Save(path);
            return path; 
        }

        public bool InPromotionState()
        {
            for (int i = 0; i < 8; i++)
            {
                if (Pieces[i, 0] is Pawn) return true;
                if (Pieces[i, 7] is Pawn) return true; 
            }

            return false; 
        }

        public void Promote(IPiece piece, int x, int y) => Pieces[x, y] = piece;
    }
}
