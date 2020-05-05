﻿using System;
using System.Linq;
using Steam.Models;
using System.Net.Http;
using Common.Attributes;
using Common.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using System.Threading.Tasks;
using SteamWebAPI2.Utilities;
using SteamWebAPI2.Interfaces;
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

        readonly SteamWebInterfaceFactory SteamWebInterfaceFactory;
        readonly SteamRemoteStorage SteamRemoteStorage;
        readonly SteamUser SteamUser;

        public SteamHelperModule(ILogger<SteamHelperModule> logger, IConfiguration configuration, IBotCoreModule botCoreModule)
        {
            _logger = logger;
            _config = configuration.GetSection("SteamHelper");
            _botCoreModule = botCoreModule;

            _botCoreModule.DiscordClient.MessageCreated += OnMessageCreated;

            SteamWebInterfaceFactory = new SteamWebInterfaceFactory(_config["Token"]);
            SteamRemoteStorage = SteamWebInterfaceFactory.CreateSteamWebInterface<SteamRemoteStorage>(new HttpClient());
            SteamUser = SteamWebInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());
        }

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || string.IsNullOrWhiteSpace(e.Message.Content)) return;

            MatchCollection matches = _steamRegex.Matches(e.Message.Content);

            if (!matches.Any()) return;

            ulong itemId = ulong.Parse(matches[0].Groups.Last().Value);

            _logger.LogDebug($"Generating Steam embed wsID:{itemId} for {e.Author.Username}({e.Author.Id}) in channel: {e.Channel.Name}/{e.Channel.Id}, guild: {e.Guild.Name}/{e.Guild.Id}");

            PublishedFileDetailsModel response = (await SteamRemoteStorage.GetPublishedFileDetailsAsync(itemId)).Data;
            PlayerSummaryModel userResponse = (await SteamUser.GetPlayerSummaryAsync(response.Creator)).Data;

            if (matches.Count() == 1 && e.Message.Content.Trim() == matches[0].Value)
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

            await e.Channel.SendMessageAsync(embed: BuildEmbedForItem(await CreateEmptyEmbedForEvent(e), response, userResponse));
        }

        public async Task<DiscordEmbedBuilder> CreateEmptyEmbedForEvent(MessageCreateEventArgs e)
        {
            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);

            return new DiscordEmbedBuilder().WithTimestamp(e.Message.Id).WithColor(member.Color).WithFooter($"{member.Username}", member.AvatarUrl);
        }

        private DiscordEmbed BuildEmbedForItem(DiscordEmbedBuilder builder, PublishedFileDetailsModel model, PlayerSummaryModel userModel)
        {
            string description = Regex.Replace(model.Description, @"\[[^]]+\]", "");

            return builder
                .WithTitle(model.Title)
                .WithUrl(SteamWebLinkAffix + model.PublishedFileId.ToString())
                .WithDescription(description.Length > 200 ? description.Substring(0, 200).Trim() + "..." : description)
                .AddField("Author", userModel.Nickname, true)
                .AddField("Created", model.TimeCreated.ToString(), true)
                .AddField("Last Updated", model.TimeUpdated.ToString(), true)
                .AddField("Views", string.Format("{0:n0}", model.Views), true)
                .AddField("Favorited", string.Format("{0:n0}", model.Favorited), true)
                .AddField("Subscriptions", string.Format("{0:n0}", model.Subscriptions), true)
                .AddField("Tags", string.Join(", ", model.Tags))
                .AddField("Steam Client Link", SteamClientLinkAffix + model.PublishedFileId.ToString())
                .WithImageUrl(model.PreviewUrl)
                .Build();
        }
    }
}
