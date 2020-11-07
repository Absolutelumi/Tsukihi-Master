using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using Discord.Commands;
using System.Reflection;

namespace Tsukihi.Objects
{
    public class HelpMenu
    {
        private IMessageChannel Channel { get; set; }

        private IUserMessage HelpMessage { get; set; }

        private Embed HelpMessageEmbed { get; set; }

        private ReactionListener Listener { get; set; }

        private Dictionary<IEmote, Embed> Tabs { get; set; }

        private Dictionary<int, IEmote> TabOrder { get; set; }

        public HelpMenu(IMessageChannel channel)
        {
            Channel = channel;

            GetTabs();

            HelpMessageEmbed = Tabs.Where(tab => tab.Value.Title == "General").First().Value;

            bool success = SendMessage().Result;

            SetupListener();

            var timer = new Timer(1000 * 60 * 10);
            timer.Elapsed += async (_, __) =>
            {
                await HelpMessage.RemoveAllReactionsAsync();
                Listener?.Dispose();
                timer.Dispose();
            };
        }

        private async Task<bool> SendMessage()
        {
            if (HelpMessage != null) await HelpMessage.ModifyAsync(msg =>
            {
                msg.Embed = HelpMessageEmbed;
            });
            else HelpMessage = await Channel.SendMessageAsync(string.Empty, embed: HelpMessageEmbed);
            return true;
        }

        private async Task UpdateMessageAsync(IEmote emote, ReactionListener.Action action)
        {
            HelpMessageEmbed = Tabs.Where(tab => tab.Key.Equals(emote)).First().Value;
            await SendMessage();
        }

        private void SetupListener()
        {
            Listener = new ReactionListener(HelpMessage, TabOrder);
            Listener.Initialize();
            Listener.OnReactionChanged += UpdateMessageAsync;
        }

        private void GetTabs()
        {
            Tabs = new Dictionary<IEmote, Embed>();
            TabOrder = new Dictionary<int, IEmote>(); 
            var theCrap = 
                Assembly.GetEntryAssembly().GetTypes().Where(type => type != typeof(ModuleBase) && typeof(ModuleBase).IsAssignableFrom(type));

            foreach (var module in theCrap)
            {
                IEmote emote = (IEmote)module.GetField("Emote")?.GetValue(null);
                int? order = (int?)module.GetField("Order")?.GetValue(null);
                if (order == null || emote == null) continue; 
                Embed embed = new EmbedBuilder()
                    .WithTitle(module.Name)
                    .WithDescription(BuildModuleDescription(module))
                    .WithColor(Discord.Color.Green)
                    .Build();

                Tabs.Add(emote, embed);
                TabOrder.Add((int)order, emote); 
            }
        }

        private string BuildModuleDescription(System.Type module)
        {
            string moduleDescription = string.Empty;
            List<CommandInfo> Commands = new List<CommandInfo>();

            foreach (var method in module.GetMethods())
            {
                var commandAttribute = (CommandAttribute)method.GetCustomAttribute(typeof(CommandAttribute), true);
                if (commandAttribute == null) continue;
                var summaryAttribute = (SummaryAttribute)method.GetCustomAttribute(typeof(SummaryAttribute), true) ?? new SummaryAttribute("No summary given.");
                var parameters = method.GetParameters();

                Commands.Add(new CommandInfo()
                {
                    Name = commandAttribute.Text,
                    Parameters = parameters,
                    Summary = summaryAttribute.Text
                });
            }

            foreach (var command in Commands)
            {
                string paramString = string.Empty;
                foreach (var param in command.Parameters)
                {
                    string paramStuff = param.IsOptional && param.DefaultValue != null ? $"{param.Name} = {param.DefaultValue}" : param.Name;
                    paramString += $" <{paramStuff}> ";
                }
                moduleDescription += $"\n !{command.Name} {paramString} : {command.Summary} \n";
            }

            return moduleDescription;
        }

        private struct CommandInfo
        {
            public string Name;
            public System.Reflection.ParameterInfo[] Parameters;
            public string Summary;
        }
    }

    public class Help : ModuleBase
    {
        [Command("help")]
        public Task GetHelp()
        {
            new HelpMenu(Context.Channel);
            return Task.CompletedTask;
        }
    }
}
