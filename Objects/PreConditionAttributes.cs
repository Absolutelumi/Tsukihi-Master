using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Tsukihi.Objects
{
    public class RequireAdmin : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var guildUser = context.User as IGuildUser;

            if (guildUser == null) return PreconditionResult.FromError("This cannot be done outside of a server.");

            return guildUser.GuildPermissions.Administrator
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("You need admin to use this command.");
        }
    }

    public class RequireDevServer : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return context.Guild.Id == 666427799124312106
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("This can only be run in the dev server, and shouldn't be visible in the help menu, so how did you find this? :thinking:");
        }
    }
}
