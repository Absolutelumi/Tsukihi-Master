using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Tsukihi.Chess;

namespace Tsukihi.Services
{
    public class ChessService
    {
        Board ChessBoard { get; set; }

        IUserMessage ChessMessage { get; set; }

        private KeyValuePair<IUser, PlayerType> Player1 { get; set; } // Always white player

        private KeyValuePair<IUser, PlayerType> Player2 { get; set; } // Always black player

        private PlayerType Turn { get; set; }

        private KeyValuePair<bool, PlayerType> Check { get; set; }

        private Regex PositionRegex = new Regex("([A-H])([1-8]) (to )?([A-H])([1-8])", RegexOptions.IgnoreCase);

        public ChessService(IMessageChannel channel, IUser user)
        {
            ChessBoard = new Board();

            Turn = PlayerType.White;

            Player1 = new KeyValuePair<IUser, PlayerType>(user, PlayerType.White);

            Check = new KeyValuePair<bool, PlayerType>(false, PlayerType.White); 

            ChessMessage = channel.SendFileAsync(ChessBoard.UpdateBoardImage(channel.Id.ToString()), GetStatus()).Result;

            Tsukihi.Client.MessageReceived += HandleChessCommand;
        }

        private async Task HandleChessCommand(SocketMessage message)
        {
            if (Player2.Key == null && Turn == PlayerType.Black && message.Author != Player1.Key 
                && PositionRegex.IsMatch(message.Content))
                Player2 = new KeyValuePair<IUser, PlayerType>(message.Author, PlayerType.Black); 

            if (message.Author != Player1.Key && message.Author != Player2.Key) return;
            if ((message.Author == Player1.Key ? Player1 : Player2).Value != Turn) return; 
            if (!PositionRegex.IsMatch(message.Content)) return;

            PlayerType player = message.Author == Player1.Key ? Player1.Value : Player2.Value;

            Match positionMatch = PositionRegex.Match(message.Content);

            int x1 = ConvertLetterToCoord(positionMatch.Groups[1].Value) - 1;
            int x2 = ConvertLetterToCoord(positionMatch.Groups[4].Value) - 1;

            int y1 = Convert.ToInt32(positionMatch.Groups[2].Value) - 1;
            int y2 = Convert.ToInt32(positionMatch.Groups[5].Value) - 1;

            if (!ChessBoard.Move(player, x1, y1, x2, y2)) 
            {
                await ChessMessage.DeleteAsync();
                await message.DeleteAsync(); 
                ChessMessage = await message.Channel.SendFileAsync(ChessBoard.UpdateBoardImage(message.Channel.Id.ToString()), GetStatus("You cannot make that move!"));
                return; 
            }

            if (Check.Key == true) Check = new KeyValuePair<bool, PlayerType>(false, PlayerType.White);

            // Checks for check on opposite player
            var checkValues = ChessBoard.InCheck(player == PlayerType.White ? PlayerType.Black : PlayerType.White);

            // Check logic
            if (checkValues.Key)
                Check = new KeyValuePair<bool, PlayerType>(true, player == PlayerType.White ? PlayerType.Black : PlayerType.White);
            // Check Mate Logic - Key for check, value for mate - finds out if next turn's player is in checkmate
            if (checkValues.Value) 
            {
                await ChessMessage.DeleteAsync();
                await message.Channel.SendFileAsync(ChessBoard.UpdateBoardImage(message.Channel.Id.ToString()), 
                    $"**Congratulations {(Turn == PlayerType.White ? Player1.Key.Mention : Player2.Key.Mention)}! You have won!**");
                Tsukihi.Client.MessageReceived -= HandleChessCommand;
                return;
            }

            // Promotion logic
            if (ChessBoard.InPromotionState())
            {
                await ChessMessage.DeleteAsync();
                ChessMessage = await message.Channel.SendFileAsync(ChessBoard.UpdateBoardImage(message.Channel.Id.ToString()),
                    $"{(Turn == PlayerType.White ? Player1.Key.Mention : Player2.Key.Mention)} " +
                    $"can promote their pawn! Type which piece you'd like! (Just type piece name, nothing else)");
            }
            while (ChessBoard.InPromotionState())
            {
                await Task.Delay(10000);

                foreach (var msg in message.Channel.GetMessagesAsync(10).FlattenAsync().Result)
                {
                    if (msg.Author == (Turn == PlayerType.White ? Player1.Key : Player2.Key)
                        && msg.Timestamp.CompareTo(ChessMessage.Timestamp) > 0)
                    {
                        if (msg.Content.Split(' ').Length != 1) return;

                        string[] pieceNames = { "queen", "bishop", "rook", "knight" };

                        string piece = string.Empty;

                        foreach (string name in pieceNames) // Will make so any msg by player (of one word) will promote - bad?
                        {
                            if (piece == string.Empty) piece = name;
                            else if (Extensions.ComputeLevenshteinDistance(msg.Content.ToLower(), name) <
                            Extensions.ComputeLevenshteinDistance(msg.Content.ToLower(), piece)) piece = name;
                        }

                        switch (piece)
                        {
                            case "queen":
                                ChessBoard.Promote(new Queen(Turn), x2, y2);
                                break;

                            case "bishop":
                                ChessBoard.Promote(new Bishop(Turn), x2, y2);
                                break;

                            case "rook":
                                ChessBoard.Promote(new Rook(Turn), x2, y2);
                                break;

                            case "knight":
                                ChessBoard.Promote(new Knight(Turn), x2, y2);
                                break;
                        }

                        await msg.DeleteAsync();
                    }
                }
            }

            Turn = Turn == PlayerType.White ? PlayerType.Black : PlayerType.White;

            await ChessMessage.DeleteAsync();
            ChessMessage = await message.Channel.SendFileAsync(ChessBoard.UpdateBoardImage(message.Channel.Id.ToString()), GetStatus());

            await message.DeleteAsync(); 
        }

        private string GetStatus(string errorMsg = "")
        {
            string checkMsg = 
                Check.Key ? $"{(Check.Value == PlayerType.White ? Player1.Key.Mention : Player2.Key.Mention)} " +
                $"is in check! They can only make a move to get out of check! \n" 
                : "";
            errorMsg += Check.Key ? "" : "\n"; 
            return $"{checkMsg}{errorMsg} **It is {(Turn == PlayerType.White ? Player1.Key.Mention : (Player2.Key == null ? "someone" : Player2.Key.Mention))}'s  ({(Turn == PlayerType.White ? "White" : "Black")}) turn!**"; // B/c player1 always white and vise versa
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
