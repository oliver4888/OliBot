using System;
using DSharpPlus;
using System.Linq;
using Steam.Models;
using Common.Attributes;
using Common.Interfaces;
using SteamHelper.Models;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using System.Threading.Tasks;
using Steam.Models.SteamCommunity;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace SteamHelper
{
    [Module]
    public class SteamHelperModule
    {
        readonly ILogger<SteamHelperModule> _logger;
        readonly IBotCoreModule _botCoreModule;

        const string ClientLinkPrefix_CommunityFilePage = "steam://url/CommunityFilePage/";
        const string ClientLinkPrefix_StorePage = "steam://url/StoreAppPage/";
        const string WebLinkPrefix_FileDetails = "https://steamcommunity.com/sharedfiles/filedetails/?id=";
        const string WebLinkPrefix_StorePage = "https://store.steampowered.com/app/";

        const string _regexString =
            @"(?:(?:https?|steam):\/\/)?(?:url|store.steampowered.com|steamcommunity.com)\/(StoreAppPage|app|sharedfiles|CommunityFilePage|workshop)\/(?:filedetails\/\?id=)?(\d+)\/?(?:[^ \/\n]+\/?)?";
        readonly Regex _steamRegex = new Regex(_regexString, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        readonly ISteamWebApiHelper _steamWebApiHelper;

        public SteamHelperModule(ILoggerFactory loggerFactory, IBotCoreModule botCoreModule, ISteamWebApiHelper steamWebApiHelper)
        {
            _logger = loggerFactory.CreateLogger<SteamHelperModule>();
            _botCoreModule = botCoreModule;

            _botCoreModule.CommandHandler.RegisterCommands<SteamCommands>();
            _botCoreModule.DiscordClient.MessageCreated += (client, e) =>
            {
                Task.Run(async () =>
                {
                    await OnMessageCreated(client, e);
                });

                return Task.CompletedTask;
            };

            _steamWebApiHelper = steamWebApiHelper;
        }

        private async Task OnMessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || string.IsNullOrWhiteSpace(e.Message.Content)) return;

            MatchCollection matches = _steamRegex.Matches(e.Message.Content.ToLowerInvariant());

            if (!matches.Any()) return;

            Match match = matches.First();
            (string page, string stringId) = (match.Groups[1].Value, match.Groups[2].Value);

            (bool success, DiscordEmbed embed) embedResult = (false, null);

            switch (page)
            {
                case "storeapppage":
                case "app":
                    embedResult = await TryGenStoreEmbed(await CreateEmptyEmbedForEvent(e), uint.Parse(stringId));
                    break;
                case "sharedfiles":
                case "workshop":
                case "communityfilepage":
                    embedResult = await TryGenWorkshopEmbed(await CreateEmptyEmbedForEvent(e), ulong.Parse(stringId));
                    break;
            }

            if (!embedResult.success)
            {
                _logger.LogDebug($"Unable to generate embed for {page}/{stringId}");
                return;
            }

            _logger.LogDebug($"Generating Steam embed {page}:{stringId} for {e.Author.Username}({e.Author.Id}) in " +
                $"{(e.Channel.IsPrivate ? "DMs" : $"channel: {e.Channel.Name}/{e.Channel.Id}, guild: {e.Guild.Name}/{e.Guild.Id}")}");

            await e.Channel.SendMessageAsync(embed: embedResult.embed);

            if (e.Channel.IsPrivate)
                return;

            if (matches.Count() == 1 && e.Message.Content.Trim().ToLowerInvariant() == match.Value)
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

        public async Task<(bool, DiscordEmbed)> TryGenStoreEmbed(DiscordEmbedBuilder baseEmbed, uint id)
        {
            SteamAppDetails data = await _steamWebApiHelper.GetStoreDetails(id);

            baseEmbed
                .WithTitle(data.Name)
                .WithUrl(WebLinkPrefix_StorePage + data.SteamAppId)
                .WithThumbnail(data.HeaderImage)
                .WithDescription(data.ShortDescription.Length > 250 ? data.ShortDescription.Substring(0, 250).Trim() + "..." : data.ShortDescription);

            if (data.ReleaseDate.ComingSoon)
                baseEmbed.AddField("Release Date", data.ReleaseDate.Date, true);

            if (data.PriceOverview != null)
                baseEmbed.AddField("Price", data.IsFree ? "Free" : data.PriceOverview.FinalFormatted, true);

            if (data.Recommendations != null)
                baseEmbed.AddField("Recommendatons", data.Recommendations.Total.ToString(), true);

            if (data.Metacritic != null)
                baseEmbed.AddField("Metacritic", $"[{data.Metacritic.Score}]({data.Metacritic.Url})", true);

            baseEmbed.AddField("Steam Client Link", ClientLinkPrefix_StorePage + data.SteamAppId, true);

            return (true, baseEmbed.Build());
        }

        public async Task<(bool, DiscordEmbed)> TryGenWorkshopEmbed(DiscordEmbedBuilder baseEmbed, ulong id)
        {
            PublishedFileDetailsModel response = await _steamWebApiHelper.GetPublishedFileDetails(id);

            if (response == null || response.Result == 9) // Friends Only / Private
                return (false, null);

            PlayerSummaryModel userResponse = await _steamWebApiHelper.GetPlayerSummary(response.Creator);

            string description = string.Join(" ", Regex.Replace(response.Description, @"\[[^]]+\]", "").Split(Environment.NewLine));

            if (response.PreviewUrl != null)
                baseEmbed.WithThumbnail(response.PreviewUrl);

            if (!string.IsNullOrWhiteSpace(description))
                baseEmbed.WithDescription(description.Length > 200 ? description.Substring(0, 200).Trim() + "..." : description);

            baseEmbed
                .WithTitle($"{response.Title} by {userResponse.Nickname}")
                .WithUrl(WebLinkPrefix_FileDetails + response.PublishedFileId.ToString())
                .AddField("Last Updated", response.TimeUpdated.ToString(), true)
                .AddField("Views", string.Format("{0:n0}", response.Views), true);

            if (response.Tags.Any())
                baseEmbed.AddField("Tags", string.Join(", ", response.Tags), true);

            baseEmbed
                .AddField("Steam Client Link", ClientLinkPrefix_CommunityFilePage + response.PublishedFileId.ToString(), true);

            return (true, baseEmbed.Build());
        }

        public async Task<DiscordEmbedBuilder> CreateEmptyEmbedForEvent(MessageCreateEventArgs e)
        {
            if (e.Channel.IsPrivate)
                return new DiscordEmbedBuilder();
            else
            {
                DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);

                return new DiscordEmbedBuilder().WithTimestamp(e.Message.Id).WithColor(member.Color).WithFooter($"{member.Username}", member.AvatarUrl);
            }
        }
    }
}
