using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tsukihi.Services
{
    public class AdminService
    {
        private Dictionary<ulong, ulong> DefaultRoles { get; set; }

        private static string DefaultRolePath = Tsukihi.ConfigPath + "defaultroles.txt";

        public AdminService()
        {
            DefaultRoles = new Dictionary<ulong, ulong>();

            Tsukihi.Client.UserJoined += OnUserJoined;

            foreach (var data in File.ReadAllLines(DefaultRolePath))
            {
                var splitData = data.Split(',');
                DefaultRoles.Add(Convert.ToUInt64(splitData[0]), Convert.ToUInt64(splitData[1]));
            }
        }

        public void UpdateDefaultRoles(ulong serverId, ulong roleId)
        {
            if (DefaultRoles.ContainsKey(serverId)) DefaultRoles.Remove(serverId);
            DefaultRoles.Add(serverId, roleId);
            File.WriteAllLines(DefaultRolePath, DefaultRoles.Select(defaultRole => $"{defaultRole.Key},{defaultRole.Value}"));
        }

        private async Task OnUserJoined(SocketGuildUser user)
        {
            if (!DefaultRoles.ContainsKey(user.Guild.Id)) return;

            DefaultRoles.TryGetValue(user.Guild.Id, out ulong roleId);

            await user.AddRoleAsync(user.Guild.GetRole(roleId));
        }
    }
}