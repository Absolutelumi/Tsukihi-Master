using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Tsukihi.Services;
using System.Linq;
using Tsukihi.Objects;

namespace Tsukihi.Modules
{
    public class Admin : ModuleBase
    {
        public static IEmote Emote = new Emoji("2️⃣");

        public static int Order = 2;

        public AdminService AdminService { get; set; }

        [Command("setdefaultrole"), Summary("Sets a role to be assigned to anybody that joins the server"), RequireAdmin]
        public async Task SetDefaultRole(IRole role)
        {
            if ((Context.Guild.GetUserAsync(Tsukihi.Client.CurrentUser.Id).Result as SocketGuildUser).Roles.Max(guildRole => guildRole.Position) <= role.Position)
            {
                await Context.Channel.SendMessageAsync("My role isn't high enough to set people to this role >_<");
                return;
            }
            AdminService.UpdateDefaultRoles(Context.Guild.Id, role.Id);
            await Context.Channel.SendMessageAsync($"The default role for this server is now {role.Name}."); 
        }

        [Command("deletemessages"), Summary("(Mass)Deletes messages"), RequireAdmin]
        public async Task DeleteMessages(int count)
        {
            foreach (var message in Context.Channel.GetMessagesAsync(count+1).FlattenAsync().Result)
            {
                await message.DeleteAsync(); 
            }

            await Context.Channel.SendMessageAsync($"Deleted {count} messages."); 
        }

        [Command("deletemessagesfrom"), Summary("Deletes messages from a user in a channel up to 1000 messages ago"), RequireAdmin]
        public async Task DeleteMessagesFrom(IUser user)
        {
            foreach (var message in Context.Channel.GetMessagesAsync(1000).FlattenAsync().Result)
            {
                if (message.Author == user) await message.DeleteAsync();
            }

            await Context.Channel.SendMessageAsync($"Deleted {user.Mention}'s messages."); 
        }
    }
}
