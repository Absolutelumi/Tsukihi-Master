using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tsukihi.Chess;

namespace Tsukihi.Services
{
    public class ChessService
    {
        Board ChessBoard { get; set; }

        IUserMessage ChessMessage { get; set; }

        private KeyValuePair<IUser, Player> Player1 { get; set; } // Always white player

        private KeyValuePair<IUser, Player> Player2 { get; set; } // Always black player

        private Player Turn { get; set; }

        private Regex PositionRegex = new Regex("([A-H])([1-8]) to ([A-H])([1-8])", RegexOptions.IgnoreCase);

        public ChessService(IMessageChannel channel, IUser user)
        {
            ChessBoard = new Board();

            Turn = Player.White;

            Player1 = new KeyValuePair<IUser, Player>(user, Player.White);

            ChessMessage = channel.SendFileAsync(ChessBoard.UpdateBoardImage(channel.Id.ToString()), GetStatus()).Result;

            Tsukihi.Client.MessageReceived += HandleChessCommand; 
        }

        private async Task HandleChessCommand(SocketMessage message)
        {
            if (Player2.Key == null && Turn == Player.Black && message.Author != Player1.Key && PositionRegex.IsMatch(message.Content)) Player2 = new KeyValuePair<IUser, Player>(message.Author, Player.Black); 
            if (message.Author != Player1.Key && message.Author != Player2.Key) return;
            if ((message.Author == Player1.Key ? Player1 : Player2).Value != Turn) return; 
            if (!PositionRegex.IsMatch(message.Content)) return;

            Player player = message.Author == Player1.Key ? Player1.Value : Player2.Value;

            Match positionMatch = PositionRegex.Match(message.Content);

            int x1 = ConvertLetterToCoord(PositionRegex.Match(message.Content).Groups[1].Value);
            int x2 = ConvertLetterToCoord(PositionRegex.Match(message.Content).Groups[3].Value); 

            int y1 = Convert.ToInt32(positionMatch.Groups[2].Value);
            int y2 = Convert.ToInt32(positionMatch.Groups[4].Value);

            if (!ChessBoard.Move(player, --x1, --y1, --x2, --y2))
            {
                await ChessMessage.DeleteAsync();
                await message.DeleteAsync(); 
                ChessMessage = await message.Channel.SendFileAsync(ChessBoard.UpdateBoardImage(message.Channel.Id.ToString()), GetStatus("You cannot make that move!"));
                return; 
            }

            // Promotion logic..........
            if (ChessBoard.InPromotionState())
            {
                await ChessMessage.DeleteAsync();
                ChessMessage = await message.Channel.SendFileAsync(ChessBoard.UpdateBoardImage(message.Channel.Id.ToString()),
                    $"{(Turn == Player.White ? Player1.Key.Mention : Player2.Key.Mention)} can promote their pawn! Type which piece you'd like! (Just type piece name, nothing else)");

                // Gets here, then doesnt pick up response msg? Maybe need to unsubscribe event from lambda ? 
                while (ChessBoard.InPromotionState())
                {
                    Tsukihi.Client.MessageReceived += async (msg) =>
                    {
                        if (msg.Author == (Turn == Player.White ? Player1.Key : Player2.Key))
                        {
                            if (msg.Content.Split(' ').Length != 1) return;  

                            string[] pieceNames = { "queen", "bishop", "rook", "knight" };

                            string piece = string.Empty; 
                            
                            foreach (string name in pieceNames)
                            {
                                if (piece == string.Empty) piece = name;
                                else if (Extensions.ComputeLevenshteinDistance(msg.Content.ToLower(), name) < Extensions.ComputeLevenshteinDistance(msg.Content.ToLower(), piece)) piece = name; 
                            }

                            switch (piece)
                            {
                                case "queen":
                                    ChessBoard.Promote(new Queen(Turn), --x2, --y2); 
                                    break;

                                case "bishop":
                                    ChessBoard.Promote(new Bishop(Turn), --x2, --y2);
                                    break;

                                case "rook":
                                    ChessBoard.Promote(new Rook(Turn), --x2, --y2);
                                    break;

                                case "knight":
                                    ChessBoard.Promote(new Knight(Turn), --x2, --y2);
                                    break;
                            }

                            await msg.DeleteAsync(); 
                        }
                    };
                }
            }

            Turn = Turn == Player.White ? Player.Black : Player.White;

            await ChessMessage.DeleteAsync();
            ChessMessage = await message.Channel.SendFileAsync(ChessBoard.UpdateBoardImage(message.Channel.Id.ToString()), GetStatus());

            await message.DeleteAsync(); 
        }

        private string GetStatus(string errorMsg = "")
        {
            // ToDo: Add {optional} string in beginning that is blank if no check, and states which player is in check/checkmate when applicable 
            return $"{errorMsg} **It is {(Turn == Player.White ? Player1.Key.Mention : (Player2.Key == null ? "someone" : Player2.Key.Mention))}'s  ({(Turn == Player.White ? "White" : "Black")}) turn!**"; // B/c player1 always white and vise versa
        }

        private int ConvertLetterToCoord(string letter)
        {
            switch (letter.ToUpper())
            {
                case "A":
                    return 1;

                case "B":
                    return 2;

                case "C":
                    return 3;

                case "D":
                    return 4;

                case "E":
                    return 5;

                case "F":
                    return 6;

                case "G":
                    return 7;

                case "H":
                    return 8;

                default:
                    return 0; 
            }
        }
    }
}
