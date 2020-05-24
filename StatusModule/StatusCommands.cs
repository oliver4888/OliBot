using Common;
using Common.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace StatusModule
{
    public class StatusCommands
    {
        [Command(permissionLevel: BotPermissionLevel.HostOwner, groupName: "Bot Status")]
        [Description("Changes the bots status.")]
        public async Task SetStatus(CommandContext ctx, ActivityType type, string activity)
        {
            if (!StatusModule.IsValidStatus(type, activity))
            {
                await ctx.Channel.SendMessageAsync($"{ctx.Author.Mention}, Invalid status!");
                return;
            }

            // Temp until command system supports params or a [RemainingText] attribute
            string content = ctx.Message.Content;

            StatusModule.StatusConfig.Mode = StatusMode.Manual;
            StatusModule.StatusTimer.Stop();
            await StatusModule.SetStatus(type, content[content.IndexOf(activity)..]);
        }

        [Command(permissionLevel: BotPermissionLevel.HostOwner, groupName: "Bot Status")]
        [Description("Adds the new custom status.")]
        public async Task AddStatus(CommandContext ctx, ActivityType type, string activity)
        {
            if (!StatusModule.IsValidStatus(type, activity))
            {
                await ctx.Channel.SendMessageAsync($"{ctx.Author.Mention}, Invalid status!");
                return;
            }

            // Temp until command system supports params or a [RemainingText] attribute
            string content = ctx.Message.Content;

            await StatusModule.AddStatus(type, content[content.IndexOf(activity)..]);
        }

        [Command(permissionLevel: BotPermissionLevel.HostOwner, groupName: "Bot Status")]
        [Description("Changes the StatusMode.")]
        public void SetStatusMode(CommandContext ctx, StatusMode mode)
        {
            StatusModule.StatusConfig.Mode = mode;

            if (StatusModule.StatusConfig.Mode == StatusMode.Automatic) {
                StatusModule.StatusTimer.Start();
            } else {
                StatusModule.StatusTimer.Stop();
            }
        }
    }
}
