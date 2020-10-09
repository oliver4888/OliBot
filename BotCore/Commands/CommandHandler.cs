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
using BotCore.Commands.Models;
using System.Collections.Generic;
using BotCore.Commands.Converters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BotCore.Commands
{
    public class CommandHandler : ICommandHandler
    {
        public string CommandPrefix { get; private set; }

        readonly ILogger<CommandHandler> _logger;
        readonly IBotCoreModule _botCoreModuleInstance;
        readonly IServiceProvider _services;

        readonly IList<ICommand> _commands = new List<ICommand>();
        public IReadOnlyCollection<ICommand> Commands => _commands as IReadOnlyCollection<ICommand>;

        readonly IDictionary<Type, IGenericConverter> _converters = new Dictionary<Type, IGenericConverter>();

        readonly MethodInfo ConvertGeneric;

        public CommandHandler(ILogger<CommandHandler> logger, IBotCoreModule botCoreModuleInstance, IServiceProvider services, string commandPrefix)
        {
            _logger = logger;
            _botCoreModuleInstance = botCoreModuleInstance;
            _botCoreModuleInstance.DiscordClient.MessageCreated += (e) =>
            {
                Task.Run(async () =>
                {
                    await OnMessageCreated(e);
                });

                return Task.CompletedTask;
            };
            _services = services;

            CommandPrefix = commandPrefix;

            // Auto register default converters
            GetType().Assembly.GetTypes().Where(t =>
                !t.IsInterface && typeof(IGenericConverter).IsAssignableFrom(t))
                    .ToList().ForEach(t =>
                        _converters.Add(
                            t.GetInterface($"{nameof(IConverter<int>)}`1").GenericTypeArguments[0], // nameof(IConverter<int>) will return IConverter
                            Activator.CreateInstance(t) as IGenericConverter));

            // + 1: EnumConverter
            _logger.LogDebug($"{nameof(CommandHandler)}: Registered {_converters.Count + 1} type converters.");

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
                _logger.LogWarning($"No commands were found in type: {commandClass.FullName}");
                return;
            }

            object commandClassInstance = Activator.CreateInstance(commandClass);

            foreach (MethodInfo command in commands)
                _commands.Add(new Command(commandClass, ref commandClassInstance, command));

            _logger.LogInformation($"Registered {commands.Count()} command{(commands.Count() > 1 ? "s" : "")} for type {commandClass.FullName}");
        }

        public void RegisterConverter<T>() => RegisterConverter(typeof(T));

        public void RegisterConverter(Type converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            if (!typeof(IGenericConverter).IsAssignableFrom(converter))
                throw new ArgumentException($"{typeof(IGenericConverter).FullName} is not assignable from {converter.FullName}.", nameof(converter));

            Type iConverterType = converter.GetInterface($"{nameof(IConverter<int>)}`1");

            if (iConverterType == null)
                throw new ArgumentException($"{converter.FullName} does not implement {nameof(IConverter<int>)}.");

            Type conversionType = iConverterType.GenericTypeArguments[0]; // nameof(IConverter<int>) will return IConverter

            if (_converters.ContainsKey(conversionType))
                _logger.LogWarning($"Ignoring duplicate converter registration for type {conversionType.FullName}");
            else
                _converters.Add(
                    conversionType,
                    Activator.CreateInstance(converter) as IGenericConverter);
        }

        private bool TryConvertParameter<T>(string value, ICommandParameter parameter, CommandContext ctx, out object converted)
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
                else if (!parameter.Required)
                {
                    converted = parameter.ParameterInfo.DefaultValue;
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
                if ((_converters[type] as IConverter<T>).TryParse(value, ctx, out T parsedValue))
                {
                    converted = parsedValue;
                    return true;
                }
                else if (!parameter.Required)
                {
                    converted = parameter.ParameterInfo.DefaultValue;
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

        private bool TryConvertParameter(string value, ICommandParameter parameter, CommandContext ctx, out object converted)
        {
            converted = null;
            MethodInfo method = ConvertGeneric.MakeGenericMethod(parameter.Type);
            try
            {
                object[] parameters = new object[] { value, parameter, ctx, null };
                if ((bool)method.Invoke(this, parameters))
                {
                    converted = parameters[3];
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

        public bool TryGetCommand(string commandName, out ICommand command) =>
            (command = Commands.FirstOrDefault(c => c.Triggers.Contains(commandName))) != null;

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;

            ulong id = _botCoreModuleInstance.DiscordClient.CurrentUser.Id;
            string mentionString1 = $"<@{id}>", mentionString2 = $"<@!{id}>";

            string[] messageParts = e.Message.Content.Split(" ");

            string aliasUsed;

            if (!string.IsNullOrWhiteSpace(CommandPrefix) && messageParts[0].StartsWith(CommandPrefix))
                aliasUsed = messageParts[0][CommandPrefix.Length..].ToLowerInvariant();
            else if (messageParts.Length > 1 && (messageParts[0] == mentionString1 || messageParts[0] == mentionString2))
            {
                aliasUsed = messageParts[1].ToLowerInvariant();
                messageParts = messageParts.Skip(1).ToArray();
            }
            else
                return;

            if (!TryGetCommand(aliasUsed, out ICommand command))
                return;

            if (e.Channel.IsPrivate && command.DisableDMs)
                return;

            if (command.PermissionLevel == BotPermissionLevel.HostOwner && e.Author.Id != _botCoreModuleInstance.HostOwnerID)
            {
                await e.Channel.SendMessageAsync($"{e.Author.Mention} You are not authorised to use this command!");
                return;
            }

            CommandContext ctx = await CreateCommandContext(e, aliasUsed, string.Join(" ", messageParts.Skip(1)));

            if (command.PermissionLevel == BotPermissionLevel.Admin && !ctx.ChannelPermissions.HasFlag(Permissions.Administrator))
            {
                await e.Channel.SendMessageAsync($"{e.Author.Mention} This command can only be used by an administrator!");
                return;
            }

            if (command.Permissions != Permissions.None && !(
                e.Guild.Owner.Id == e.Author.Id ||
                ctx.ChannelPermissions.HasFlag(Permissions.Administrator) ||
                ctx.ChannelPermissions.HasFlag(command.Permissions)))
            {
                await e.Channel.SendMessageAsync($"{e.Author.Mention} You do not have the required permissions to use this command!");
                return;
            }

            (bool convertedParameters, IEnumerable<object> parameters) = await TryConvertParameters(command, ctx);

            if (!convertedParameters)
                return;

            await InvokeCommand(command, ctx, parameters.ToArray());
        }

        private async Task<CommandContext> CreateCommandContext(MessageCreateEventArgs e, string aliasUsed, string argumentString)
        {
            if (e.Channel.IsPrivate)
                return new CommandContext(e, _botCoreModuleInstance, null, Permissions.None, aliasUsed, argumentString);

            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
            return new CommandContext(e, _botCoreModuleInstance, member, e.Channel.PermissionsFor(member), aliasUsed, argumentString);
        }

        private async Task<(bool, IEnumerable<object>)> TryConvertParameters(ICommand command, CommandContext ctx)
        {
            IList<object> parameters = new List<object>();

            Queue<string> messageParts = new Queue<string>(ctx.ArgumentString.Split(" ").Where(item => !string.IsNullOrWhiteSpace(item)));

            bool foundRemainingText = false;

            foreach (ICommandParameter param in command.Parameters)
            {
                if (param.FromServices)
                {
                    parameters.Add(_services.GetRequiredService(param.Type));
                    continue;
                }
                else if (foundRemainingText)
                    break;

                if (param.Type == typeof(CommandContext))
                    parameters.Add(ctx);
                else if (!messageParts.Any())
                {
                    if (param.Required)
                    {
                        await ctx.Channel.SendMessageAsync($"{ctx.Author.Mention}, missing parameter `{param.ParameterInfo.Name}`!");
                        return (false, null);
                    }
                    else
                        parameters.Add(param.ParameterInfo.DefaultValue);
                }
                else if (param.Type == typeof(string))
                {
                    if (param.RemainingText)
                    {
                        parameters.Add(string.Join(" ", messageParts));
                        foundRemainingText = true;
                        continue;
                    }
                    else
                        parameters.Add(messageParts.Dequeue());
                }
                else
                {
                    if (TryConvertParameter(messageParts.Dequeue(), param, ctx, out object converted))
                        parameters.Add(converted);
                    else
                    {
                        await ctx.Channel.SendMessageAsync($"{ctx.Author.Mention}, unable to convert parameter `{param.ParameterInfo.Name}` to `{param.Type.Name}`!");
                        return (false, null);
                    }
                }
            }

            return (true, parameters);
        }

        private async Task InvokeCommand(ICommand command, CommandContext ctx, object[] parameters)
        {
            string cmdLogPart = $"Running command " +
                $"{(ctx.AliasUsed == command.Name ? $"`{command.Name}`" : $"`{command.Name}` (alias `{ctx.AliasUsed}`)")} for {ctx.Author.Username}({ctx.Author.Id}) in";

            if (ctx.IsDMs)
                _logger.LogDebug($"{cmdLogPart} DMs");
            else
                _logger.LogDebug($"{cmdLogPart} channel: {ctx.Channel.Name}/{ctx.Channel.Id}, guild: {ctx.Guild.Name}/{ctx.Guild.Id}");

            try
            {
                if (command.CommandMethod.ReturnType == typeof(Task))
                    await (command.MethodDelegate.DynamicInvoke(parameters) as Task);
                else
                    command.MethodDelegate.DynamicInvoke(parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error running command `{command.Name}`");
            }
        }
    }
}
