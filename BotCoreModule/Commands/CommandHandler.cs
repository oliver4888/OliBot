using Common;
using System;
using DSharpPlus;
using System.Linq;
using Common.Attributes;
using Common.Interfaces;
using System.Reflection;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using BotCoreModule.Commands.Models;
using BotCoreModule.Commands.Converters;
using BotCoreModule.Commands.Extensions;

namespace BotCoreModule.Commands
{
    public class CommandHandler : ICommandHandler
    {
        // This will be moved to a db and configurable per server 
        public string CommandPrefix { get; private set; }

        readonly ILogger<CommandHandler> _logger;
        readonly IBotCoreModule _botCoreModuleInstance;

        readonly IList<ICommand> _commands = new List<ICommand>();
        public IReadOnlyCollection<ICommand> Commands => _commands as IReadOnlyCollection<ICommand>;

        readonly IDictionary<Type, IGenericConverter> _converters = new Dictionary<Type, IGenericConverter>();

        readonly MethodInfo ConvertGeneric;

        public CommandHandler(ILogger<CommandHandler> logger, IBotCoreModule botCoreModuleInstance, string commandPrefix)
        {
            _logger = logger;
            _botCoreModuleInstance = botCoreModuleInstance;
            _botCoreModuleInstance.DiscordClient.MessageCreated += OnMessageCreated;
            CommandPrefix = commandPrefix;

            // Auto register default converters
            GetType().Assembly.GetTypes().Where(t =>
                !t.IsInterface
                && string.Equals(t.Namespace, typeof(IGenericConverter).Namespace, StringComparison.Ordinal)
                && typeof(IGenericConverter).IsAssignableFrom(t))
                    .ToList().ForEach(t =>
                        _converters.Add(
                            t.GetInterface($"{nameof(IConverter<int>)}`1").GenericTypeArguments[0], // nameof(IConverter<int>) will return IConverter
                            Activator.CreateInstance(t) as IGenericConverter));

            _logger.LogDebug($"{nameof(CommandHandler)}: Registered {_converters.Count} type converters.");

            ConvertGeneric = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(mi => mi.Name == nameof(TryConvertParameter) && mi.ContainsGenericParameters);
        }

        public void RegisterCommands<T>() => RegisterCommands(typeof(T));

        public void RegisterCommands(Type commandClass)
        {
            if (commandClass == null)
                throw new ArgumentNullException(nameof(commandClass));

            IEnumerable<MethodInfo> commands = commandClass.GetMethods().Where(methodInfo => methodInfo.IsDefined(typeof(CommandAttribute), false));

            if (!commands.Any())
            {
                _logger.LogWarning($"No commands where found in the given type: {commandClass.FullName}");
                return;
            }

            object commandClassInstance = Activator.CreateInstance(commandClass);

            foreach (MethodInfo command in commands)
                _commands.Add(new Command(commandClass, ref commandClassInstance, command));

            _logger.LogInformation($"Registered {commands.Count()} command{(commands.Count() > 1 ? "s" : "")} for type {commandClass.FullName}");
        }

        private bool TryConvertParameter<T>(string value, out object converted)
        {
            converted = null;
            Type type = typeof(T);

            if (type.IsEnum)
            {
                if (EnumConverter.TryParse(type, value, out object parsedValue))
                {
                    converted = parsedValue;
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Unable to convert parameter to enum {type.FullName}: {value}");
                    return false;
                }
            }
            else if (_converters.ContainsKey(type))
            {
                if ((_converters[type] as IConverter<T>).TryParse(value, out T parsedValue))
                {
                    converted = parsedValue;
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Unable to convert parameter to type {type.FullName}: {value}");
                    return false;
                }
            }

            _logger.LogWarning($"No converter present for type {type.FullName}");
            return false;
        }

        private bool TryConvertParameter(string value, out object converted, Type type)
        {
            converted = null;
            MethodInfo method = ConvertGeneric.MakeGenericMethod(type);
            try
            {
                object[] parameters = new object[] { value, null };
                if ((bool)method.Invoke(this, parameters))
                {
                    converted = parameters[1];
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to convert parameter via {nameof(ConvertGeneric)}");
                return false;
            }
        }

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || !e.Message.Content.StartsWith(CommandPrefix)) return;

            string[] messageParts = e.Message.Content.Remove(0, CommandPrefix.Length).Split(" ");
            messageParts[0] = messageParts[0].ToLowerInvariant();

            if (_commands.TryGetCommand(messageParts[0], out ICommand command))
            {
                IList<object> parameters = new List<object>();
                bool failedConversion = false;
                for (int i = 1; i < command.Parameters.Count; i++)
                {
                    ICommandParameter param = command.Parameters[i];
                    if (param.Type == typeof(string))
                        parameters.Add(messageParts[i]);
                    else
                    {
                        if (TryConvertParameter(messageParts[i], out object converted, param.Type))
                            parameters.Add(converted);
                        else
                        {
                            failedConversion = true;
                            break;
                        }
                    }
                }

                if (failedConversion)
                {
                    await e.Channel.SendMessageAsync($"{e.Author.Mention}, unable to convert one or more command arguments.");
                    return;
                }

                if (e.Channel.IsPrivate)
                {
                    if (!command.DisableDMs)
                        await HandleCommandDMs(e, command, messageParts[0], parameters);
                }
                else
                    await HandleCommand(e, command, messageParts[0], parameters);
            }
        }

        private async Task HandleCommandDMs(MessageCreateEventArgs e, ICommand command, string aliasUsed, IList<object> parameters)
        {
            if (command.PermissionLevel == BotPermissionLevel.HostOwner && e.Author.Id != _botCoreModuleInstance.HostOwnerID)
            {
                await e.Channel.SendMessageAsync($"{e.Author.Mention} You are not authorised to use this command!");
                return;
            }

            _logger.LogDebug($"Running command " +
                $"{(aliasUsed == command.Name ? $"`{command.Name}`" : $"`{command.Name}` (alias `{aliasUsed}`)")} for {e.Author.Username}({e.Author.Id}) in DMs");

            await InvokeCommand(command, new CommandContext(e, _botCoreModuleInstance, null, Permissions.None), parameters);
        }

        private async Task HandleCommand(MessageCreateEventArgs e, ICommand command, string aliasUsed, IList<object> parameters)
        {
            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
            CommandContext ctx = new CommandContext(e, _botCoreModuleInstance, member, e.Channel.PermissionsFor(member));

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

            _logger.LogDebug($"Running command {(aliasUsed == command.Name ? $"`{command.Name}`" : $"`{command.Name}` (alias `{aliasUsed}`)")}" +
                $" for {e.Author.Username}({e.Author.Id}) in channel: {e.Channel.Name}/{e.Channel.Id}, guild: {e.Guild.Name}/{e.Guild.Id}");

            await InvokeCommand(command, ctx, parameters);
        }

        private async Task InvokeCommand(ICommand command, CommandContext ctx, IList<object> parameters)
        {
            object[] args = new object[] { ctx };
            args = args.Concat(parameters).ToArray();

            try
            {
                if (command.CommandMethod.ReturnType == typeof(Task))
                    await (command.MethodDelegate.DynamicInvoke(args) as Task);
                else
                    command.MethodDelegate.DynamicInvoke(args);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error running command `{command.Name}`");
            }
        }
    }
}
