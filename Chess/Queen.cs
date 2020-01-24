using System;

namespace Tsukihi.Chess
{
    public class Queen : IPiece
    {
        public string ImagePath { get; set; }

        public Player Type { get; set; }

        public string Emoji { get; set; }

        public Queen(Player type)
        {
            Type = type;

            ImagePath = Tsukihi.ConfigPath + "ChessResources\\" + $"{(Type == Player.White ? "white" : "black")}Queen.png";

            if (Type == Player.White) Emoji = "♔";
            else Emoji = "♚";
        }

        public void AfterMove()
        {
        }

        public bool CanMove(int x1, int y1, int x2, int y2, IPiece[,] pieces)
        {
            if (x1 == x2)
            {
                bool yIncreasing = y2 - y1 > 0;

                if (yIncreasing)
                {
                    for (int i = y1 + 1; i < y2; i++)
                    {
                        if (pieces[x1, i] != null) return false;
                    }
                }
                else
                {
                    for (int i = y1 - 1; i > y2; i--)
                    {
                        if (pieces[x1, i] != null) return false;
                    }
                }

                return true;
            }

            if (y1 == y2)
            {
                bool xIncreasing = x2 - x1 > 0;

                if (xIncreasing)
                {
                    for (int i = x1 + 1; i < x2; i++)
                    {
                        if (pieces[i, y1] != null) return false;
                    }
                }
                else
                {
                    for (int i = x1 - 1; i > x2; i--)
                    {
                        if (pieces[i, y1] != null) return false;
                    }
                }

                return true;
            }

            if (Math.Abs(y2 - y1) == Math.Abs(x2 - x1))
            {
                // Xi is x-indexer, starts at one above current pos cause current pos' piece is already known same for yi
                bool yIncreasing = (y2 - y1) > 0;
                int yi = y1 + (yIncreasing ? 1 : -1);
                bool xIncreasing = (x2 - x1) > 0;

                if (xIncreasing)
                {
                    for (int xi = x1 + 1; xi < x2; xi++)
                    {
                        if (pieces[xi, yi] != null) return false;
                        if (yIncreasing) yi++;
                        else yi--;
                    }
                }
                else
                {
                    for (int xi = x1 + -1; x2 < xi; xi--)
                    {
                        if (pieces[xi, yi] != null) return false;
                        if (yIncreasing) yi++;
                        else yi--;
                    }
                }

                return true;
            }

            return false;
        }
    }
}