using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Tsukihi.Objects;
using Tsukihi.Services;

namespace Tsukihi.Modules
{
    public class General : ModuleBase
    {
        public static IEmote Emote = new Emoji("1️⃣");

        public static int Order = 1; 

        [Command("setprefix"), Summary("Sets command prefix for the server (admin only)"), RequireAdmin]
        public async Task SetPrefix(string prefix)
        {
            if (Commands.GuildPrefixes.ContainsKey(Context.Guild.Id)) Commands.GuildPrefixes.Remove(Context.Guild.Id);
            if (prefix != "!") Commands.GuildPrefixes.Add(Context.Guild.Id, prefix);

            File.WriteAllLines(Tsukihi.PrefixPath, Commands.GuildPrefixes.Select(guildPrefix => $"{guildPrefix.Key},{guildPrefix.Value}"));

            await Context.Channel.SendMessageAsync($"This guild's command prefix is now {prefix}"); 
        }

        [Command("getav"), Summary("Gets avatar of user")]
        public async Task GetAv(string name = null)
        {
            string username = name ?? Context.User.Username;

            var desiredUser = Context.User;

            if (name != null)
            {
                // Possibly too much resource devotion ? 
                foreach (var user in Context.Channel.GetUsersAsync().FlattenAsync().Result)
                {
                    if (Extensions.ComputeLevenshteinDistance(username, user.Username) < Extensions.ComputeLevenshteinDistance(username, desiredUser.Username)) desiredUser = user;
                }
            }

            await Context.Channel.SendMessageAsync(desiredUser.GetAvatarUrl()); 
        }

        [Command("roll"), Summary("Rolls an x sided die y times")]
        public async Task Roll([Summary("Ammount of sides on die")] int sides = 6, [Summary("The ammount of rolls")] int rollCount = 1)
        {
            var rolls = Enumerable.Range(0, rollCount).Select(_ => Extensions.rng.Next(1, sides + 1));
            await ReplyAsync(":game_die: " + string.Join(" , ", rolls));
        }

        [Command("whowins"), Summary("Who?")]
        public async Task WhoWins([Remainder] string value)
        {
            string[] values = value.Split(' ');

            string amount1 = values[0];
            string type1 = "";
            string amount2 = ""; 
            string type2 = "";

            int type2Start = 0; 

            for (int i = 1; i < values.Length; i++)
            {
                if (values[i].All(char.IsDigit))
                {
                    type2Start = i + 1;
                    amount2 = values[i];
                    break; 
                }
                else type1 += values[i] + ' '; 
            }

            for (int i = type2Start; i < values.Length; i++) type2 += values[i] + ' '; 

            Random random = new Random();

            if (random.Next(0, 2) == 0) await ReplyAsync($"{amount1} {type1}");
            else await ReplyAsync($"{amount2} {type2}");
        }

        [Command("show"), Summary("Gets image from danbooru with given keywords")]
        public async Task ShowImage([Remainder] string keywords)
        {
            var image = GelbooruService.GetRandomImage(keywords.Split(' '));
           bool isImage = image.Contains("png") || image.Contains("jpg") || image.Contains("jpeg");
            await ReplyAsync(embed: new EmbedBuilder()
                .WithTitle(isImage ? string.Empty : "Image not found!")
                .WithImageUrl(isImage ? image : string.Empty)
                //.WithColor(isImage ? Extensions.GetBestColor(image).GetDiscordColor() : new Color(0, 0, 0))
                .Build());
        }
    }
}
