using System;
using System.Linq;
using Steam.Models;
using Common.Attributes;
using Common.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using System.Threading.Tasks;
using Steam.Models.SteamCommunity;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace SteamHelperModule
{
    [Module]
    public class SteamHelperModule
    {
        readonly ILogger<SteamHelperModule> _logger;
        readonly IConfigurationSection _config;
        readonly IBotCoreModule _botCoreModule;

        const string SteamClientLinkAffix = "steam://url/CommunityFilePage/";
        const string SteamWebLinkAffix = "https://steamcommunity.com/sharedfiles/filedetails/?id=";

        const string _regexString = @"(http(s)?:\/\/)?steam(community\.com\/sharedfiles\/filedetails\/\?id=|:\/\/url\/CommunityFilePage\/)(\d{9,10})";
        readonly Regex _steamRegex = new Regex(_regexString, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        readonly SteamWebApiHelper SteamWebApiHelper;

        public SteamHelperModule(ILoggerFactory loggerFactory, IConfiguration configuration, IBotCoreModule botCoreModule)
        {
            _logger = loggerFactory.CreateLogger<SteamHelperModule>();
            _config = configuration.GetSection("SteamHelper");
            _botCoreModule = botCoreModule;

            _botCoreModule.CommandHandler.RegisterCommands<SteamCommands>();
            _botCoreModule.DiscordClient.MessageCreated += OnMessageCreated;

            SteamWebApiHelper = new SteamWebApiHelper(loggerFactory, _config["Token"], int.Parse(_config["SlidingExpirationHours"]), _config["FileCachePath"]);
        }

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || string.IsNullOrWhiteSpace(e.Message.Content)) return;

            MatchCollection matches = _steamRegex.Matches(e.Message.Content);

            if (!matches.Any()) return;

            ulong itemId = ulong.Parse(matches[0].Groups.Last().Value);

            _logger.LogDebug($"Generating Steam embed wsID:{itemId} for {e.Author.Username}({e.Author.Id}) in " +
                $"{(e.Channel.IsPrivate ? "DMs" : $"channel: {e.Channel.Name}/{e.Channel.Id}, guild: {e.Guild.Name}/{e.Guild.Id}")}");

            PublishedFileDetailsModel response = await SteamWebApiHelper.GetPublishedFileDetails(itemId);

            if (response.Result == 9) // Friends Only / Private
            {
                await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":x:"));
                return;
            }

            PlayerSummaryModel userResponse = await SteamWebApiHelper.GetPlayerSummary(response.Creator);

            await e.Channel.SendMessageAsync(embed: BuildEmbedForItem(await CreateEmptyEmbedForEvent(e), response, userResponse));

            if (!e.Channel.IsPrivate && matches.Count() == 1 && e.Message.Content.Trim() == matches[0].Value)
            {
                try
                {
                    await e.Message.DeleteAsync();
                }
                catch (NotFoundException)
                {
                    _logger.LogDebug($"Attempted to remove deleted message {e.Message.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting message {e.Message.Id}");
                }
            }
            else
                await e.Message.ModifyEmbedSuppressionAsync(true);
        }

        public async Task<DiscordEmbedBuilder> CreateEmptyEmbedForEvent(MessageCreateEventArgs e)
        {
            if (e.Channel.IsPrivate)
            {
                return new DiscordEmbedBuilder();
            }
            else
            {
                DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);

                return new DiscordEmbedBuilder().WithTimestamp(e.Message.Id).WithColor(member.Color).WithFooter($"{member.Username}", member.AvatarUrl);
            }
        }

        private DiscordEmbed BuildEmbedForItem(DiscordEmbedBuilder builder, PublishedFileDetailsModel model, PlayerSummaryModel userModel)
        {
            string description = string.Join(" ", Regex.Replace(model.Description, @"\[[^]]+\]", "").Split(Environment.NewLine));

            if (model.PreviewUrl != null)
                builder.WithThumbnail(model.PreviewUrl);

            if (!string.IsNullOrWhiteSpace(description))
                builder.WithDescription(description.Length > 200 ? description.Substring(0, 200).Trim() + "..." : description);

            builder
                .WithTitle($"{model.Title} by {userModel.Nickname}")
                .WithUrl(SteamWebLinkAffix + model.PublishedFileId.ToString())
                .AddField("Last Updated", model.TimeUpdated.ToString(), true)
                .AddField("Views", string.Format("{0:n0}", model.Views), true);

            if (model.Tags.Count > 0)
                builder.AddField("Tags", string.Join(", ", model.Tags), true);

            return builder
                .AddField("Steam Client Link", SteamClientLinkAffix + model.PublishedFileId.ToString())
                .Build();
        }
    }
}
