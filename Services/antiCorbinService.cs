using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;

namespace Tsukihi.Services
{
    public class AntiCorbinService
    {
        public AntiCorbinService() { Tsukihi.Client.MessageReceived += handleCorbinMessage; }

        private async Task handleCorbinMessage(SocketMessage msg)
        {
            //if (msg.Author.Id != 224728208715677696) return;
            if (msg.Attachments.Count == 0) return;

            var color = Extensions.GetBestColor(msg.Attachments.First().Url);
            if (false) await msg.DeleteAsync();
        }
    }
}
