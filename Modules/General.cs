using Discord;
using Discord.Commands;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tsukihi.Objects;

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
    }
}