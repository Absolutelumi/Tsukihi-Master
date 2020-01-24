using Discord.Commands;
using System.Threading.Tasks;
using Tsukihi.Services;

namespace Tsukihi.Modules
{
    public class Chess : ModuleBase
    {
        [Command("Chess"), Summary("Play a game of chess")]
        public async Task PlayChess() => new ChessService(Context.Channel, Context.User);
    }
}