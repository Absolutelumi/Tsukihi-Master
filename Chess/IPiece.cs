namespace Tsukihi.Chess
{
    public interface IPiece
    {
        string ImagePath { get; set; }

        string Emoji { get; set; }

        Player Type { get; set; }

        bool CanMove(int x1, int y1, int x2, int y2, IPiece[,] pieces);

        void AfterMove();
    }
}
