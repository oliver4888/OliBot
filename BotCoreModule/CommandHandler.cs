using Common;
using System;
using DSharpPlus;
using System.Linq;
using System.Reflection;
using Common.Attributes;
using Common.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace BotCoreModule
{
    public class CommandHandler : ICommandHandler
    {
        // This will be moved to a db and configurable per server 
        const string _commandPrefix = "??";

        readonly ILogger<CommandHandler> _logger;
        readonly IBotCoreModule _botCoreModuleInstance;

        readonly IList<ICommand> _commands = new List<ICommand>();
        public IReadOnlyCollection<ICommand> Commands => _commands as IReadOnlyCollection<ICommand>;

        public CommandHandler(ILogger<CommandHandler> logger, IBotCoreModule botCoreModuleInstance)
        {
            _logger = logger;
            _botCoreModuleInstance = botCoreModuleInstance;
            _botCoreModuleInstance.DiscordClient.MessageCreated += OnMessageCreated;
        }

        public void RegisterCommands<T>() => RegisterCommands(typeof(T));

        public void RegisterCommands(Type commandClass)
        {
            if (commandClass == null)
                throw new ArgumentNullException(nameof(commandClass));

            IEnumerable<MethodInfo> commands = commandClass.GetMethods().Where(methodInfo => methodInfo.IsDefined(typeof(CommandAttribute), false));

            if (!commands.Any())
            {
                _logger.LogWarning($"No commands where found in the given type: {commandClass.Name}");
                return;
            }

            object commandClassInstance = Activator.CreateInstance(commandClass);

            foreach (MethodInfo command in commands)
                _commands.Add(new Command(commandClass, ref commandClassInstance, command));

            _logger.LogInformation($"Registered {_commands.Count()} command(s) for Type {commandClass.FullName}");
        }

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || !e.Message.Content.StartsWith(_commandPrefix)) return;

            string[] messageParts = e.Message.Content.Remove(0, _commandPrefix.Length).Split(" ");

            string commandName = messageParts[0].ToLowerInvariant();

            if (_commands.Any(command => command.Name == commandName))
                await HandleCommand(e, commandName, messageParts);
        }

        private async Task HandleCommand(MessageCreateEventArgs e, string commandName, string[] messageParts)
        {
            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
            CommandContext ctx = new CommandContext(e, _botCoreModuleInstance, member, e.Channel.PermissionsFor(member));

            ICommand command = _commands.FirstOrDefault(command => command.Name == commandName);
            switch (command.PermissionLevel)
            {
                case BotPermissionLevel.HostOwner:
                    if (e.Author.Id != _botCoreModuleInstance.HostOwnerID)
                    {
                        await e.Channel.SendMessageAsync($"{e.Author.Mention} You are not authorised to use this command!");
                        return;
                    }
                    break;
                case BotPermissionLevel.Admin:
                    if (!ctx.ChannelPermissions.HasFlag(Permissions.Administrator))
                    {
                        await e.Channel.SendMessageAsync($"{e.Author.Mention} This command can only be used by an administrator!");
                        return;
                    }
                    break;
            }

            if (command.Permissions != Permissions.None && !(
                e.Guild.Owner.Id == e.Author.Id ||
                ctx.ChannelPermissions.HasFlag(Permissions.Administrator) ||
                ctx.ChannelPermissions.HasFlag(command.Permissions)))
            {
                await e.Channel.SendMessageAsync($"{e.Author.Mention} You do not have the required permissions to use this command!");
                return;
            }

            _logger.LogDebug($"Running command '{commandName}' for {e.Author.Username}({e.Author.Id}) in channel: {e.Channel.Name}/{e.Channel.Id}, guild: {e.Guild.Name}/{e.Guild.Id}");

            try
            {
                await (command.MethodDelegate.DynamicInvoke(new object[] { ctx }) as Task);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running command {commandName}", ex);
            }
        }
    }
}
