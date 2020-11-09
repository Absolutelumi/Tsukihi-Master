using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Tsukihi.Objects;

namespace Tsukihi
{
    public class Tsukihi
    {
        public static readonly string ConfigPath = Properties.Settings.Default.ConfigDirectory;
        public static readonly string TempPath = Properties.Settings.Default.TempDirectory;

        public static readonly string PrefixPath = Tsukihi.ConfigPath + "\\prefix.txt";

        public Dictionary<ulong, string> GuildPrefixes = new Dictionary<ulong, string>();

        public static DiscordSocketClient Client { get; set; }


        public Commands Commands;
        private DiscordSocketConfig clientConfig;

        public Tsukihi() => StartAsync().GetAwaiter().GetResult();

        private Task Logger(LogMessage e)
        {
            if (e.Message != null && (e.Message.Contains("Rate limit") || e.Message.Contains("blocking the gateway task"))) return Task.CompletedTask;
            switch (e.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{e.Severity,8}] {e.Source}: {e.Message}");
            Console.ResetColor();

            return Task.CompletedTask;
        }

        private async Task StartAsync()
        {
            clientConfig = new DiscordSocketConfig()
            {
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LogLevel = LogSeverity.Info
            };
            Client = new DiscordSocketClient(clientConfig);

            Client.Log += Logger;

            await Client.LoginAsync(TokenType.Bot, Properties.Settings.Default.DiscordToken);
            await Client.StartAsync();

            Client.Ready += async () =>
            {
                Console.WriteLine("Tsukihi has now connected to:");
                Console.WriteLine(string.Join(", ", Client.Guilds));

                Commands = new Commands();
                await Commands.Install();

                // Bot status  ? ?
            };

            Client.Disconnected += async (_) => // Currently just restarts entire client on disconnect - problematic 
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = Directory.GetCurrentDirectory() + "\\Tsukihi.exe"
                });
                Environment.Exit(0);
            };

            await Task.Delay(-1);
        }
    }
}