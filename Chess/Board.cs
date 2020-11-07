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
                Pieces[i, 1] = new Pawn(PlayerType.White, this);
                Pieces[i, 6] = new Pawn(PlayerType.Black, this);

                switch (i)
                {
                    case 0:
                    case 7:
                        Pieces[i, 0] = new Rook(PlayerType.White);
                        Pieces[i, 7] = new Rook(PlayerType.Black);
                        break;

                    case 1:
                    case 6:
                        Pieces[i, 0] = new Knight(PlayerType.White);
                        Pieces[i, 7] = new Knight(PlayerType.Black);
                        break;

                    case 2:
                    case 5:
                        Pieces[i, 0] = new Bishop(PlayerType.White);
                        Pieces[i, 7] = new Bishop(PlayerType.Black);
                        break;

                    case 3:
                        Pieces[i, 0] = new Queen(PlayerType.White);
                        Pieces[i, 7] = new Queen(PlayerType.Black);
                        break;

                    case 4:
                        Pieces[i, 0] = new King(PlayerType.White);
                        Pieces[i, 7] = new King(PlayerType.Black);
                        break; 
                }
            }
        }

        public bool Move(PlayerType player, int x1, int y1, int x2, int y2)
        {
            IPiece piece = Pieces[x1, y1];

            if (piece == null || piece.Type != player) return false;

            if (InCheck(player).Key)
            {
                // repeated logic - move into one func ?
                IPiece[,] piecesCopy = new IPiece[8, 8];
                CopyPieces(Pieces, piecesCopy);

                Pieces[x2, y2] = Pieces[x1, y1];
                Pieces[x1, y1] = null; 
                // doesn't account for weird moves like en passant etc

                if (InCheck(player).Key)
                {
                    Pieces = piecesCopy;
                    return false; 
                }

                CopyPieces(piecesCopy, Pieces);  
            }

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
                        (Pieces[x2 + (x1 - x2 > 0 ? 2 : -2), y2] as King).FirstMove = false; // Used to be -2 : 2, probably was issue, needs to be tested

                        return true; 
                    }
                }

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

            Pieces[x2, y2] = Pieces[x1, y1];
            Pieces[x1, y1] = null;

            return true;
        }

        private void CopyPieces(IPiece[,] source, IPiece[,] target)
        {
            for (int i = 0; i < 8; i++) for (int j = 0; j < 8; j++) target[i, j] = source[i, j];
        }

        private bool Check(PlayerType turn, int kingx, int kingy)
        {
            for (int i = 0; i < 8; i++) for (int j = 0; j < 8; j++)
                {
                    if (Pieces[i, j] == null || Pieces[i, j].Type == turn || !Pieces[i, j].CanMove(i, j, kingx, kingy, Pieces))
                        continue;
                    return true; 
                }

            return false; 
        }

        private bool CheckMate(PlayerType turn, int kingx, int kingy)
        {
            for (int i = 0; i < 8; i++) for (int j = 0; j < 8; j++)
                {
                    if (Pieces[i, j]?.Type != turn) continue; 

                    for (int k = 0; k < 8; k++) for (int l = 0; l < 8; l++)
                        {
                            if (!Pieces[i, j].CanMove(i, j, k, l, Pieces)) continue;

                            IPiece[,] piecesCopy = new IPiece[8, 8];
                            CopyPieces(Pieces, piecesCopy);

                            // Doesn't check for weird moves such as en passant etc
                            Pieces[k, l] = Pieces[i, j];
                            Pieces[i, j] = null;

                            if (!Check(turn, kingx, kingy))
                            {
                                Pieces = piecesCopy;
                                return false;
                            }

                            CopyPieces(piecesCopy, Pieces);
                        }
                }

            return true;
        }

        public KeyValuePair<bool, bool> InCheck(PlayerType turn) // Finds out if given player is in check
        {
            // Key is for whether player is in check, value is for whether player is in checkmate
            // Ally king pos
            int kingx = 0, kingy = 0;

            for (int i = 0; i < 8; i++) for (int j = 0; j < 8; j++)
                {
                    if (Pieces[i, j] != null && Pieces[i, j] is King && Pieces[i, j].Type == turn)
                    {
                        kingx = i;
                        kingy = j; 
                    }
                }

            bool check = Check(turn, kingx, kingy);
            bool checkMate = check ? CheckMate(turn, kingx, kingy) : false; 
            return new KeyValuePair<bool, bool>(check, checkMate);
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
            for (int i = 0; i < 8; i++) if (Pieces[i, 0] is Pawn || Pieces[i, 7] is Pawn) return true;
            return false; 
        }

        public void Promote(IPiece piece, int x, int y) => Pieces[x, y] = piece;
    }
}
