using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using Tsukihi; 
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Tsukihi.Services;

namespace Tsukihi.Objects
{
    public class Commands
    {
        private DiscordSocketClient Client = Tsukihi.Client;

        public  static Dictionary<ulong, string> GuildPrefixes { get; set; }

        private CommandService commands { get; set; }
        private IServiceProvider services { get; set; }

        public Commands()
        {
            GuildPrefixes = new Dictionary<ulong, string>();

            foreach (var data in File.ReadAllLines(Tsukihi.PrefixPath))
            {
                var splitData = data.Split(',');
                GuildPrefixes.Add(Convert.ToUInt64(splitData[0]), splitData[1]);
            }

            // Services not used by modules
            

            // Instantialize services that have functionality outside of being used in modules
            services = new ServiceCollection()
            .AddSingleton(Client)
            .AddSingleton(new AdminService())
            .BuildServiceProvider();

            commands = new CommandService();
        }

        private string GetGuildPrefix(ulong id)
        {
            string prefix;
            return GuildPrefixes.TryGetValue(id, out prefix)
                ? prefix
                : "!"; 
        }

        public async Task Handle(SocketMessage messageParam)
        {
            string guildPrefix = GetGuildPrefix((messageParam.Author as SocketGuildUser).Guild.Id); 

            SocketUserMessage message = messageParam as SocketUserMessage;

            if (message == null) return;

            int argPos = 0;

            if (!(message.HasStringPrefix(guildPrefix, ref argPos) 
                || message.HasMentionPrefix(Client.CurrentUser, ref argPos)) 
                || message.Author.IsBot 
                || message.Content == guildPrefix.ToString()) return;

            var context = new CommandContext(Client, message);
            var result = await commands.ExecuteAsync(context, argPos, services);

            if (!result.IsSuccess) await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        public async Task Install()
        {
            Client.MessageReceived += async (SocketMessage msg) => await Handle(msg);

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }
    }
}
