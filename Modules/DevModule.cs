using Discord.Commands;
using System.Threading.Tasks;
using Tsukihi.Objects;

namespace Tsukihi.Modules
{
    public class DevModule : ModuleBase
    {
        [Command("test"), RequireDevServer]
        public async Task GetChannelId() => await Context.Channel.SendMessageAsync(Context.Channel.Id.ToString());
    }
}