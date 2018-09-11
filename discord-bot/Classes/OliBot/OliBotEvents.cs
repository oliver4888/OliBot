﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace discord_bot.Classes
{
    public class OliBotEvents
    {
        public async Task OliBot_Ready(ReadyEventArgs e)
        {
            OliBotCore.Log.Info("General| Bot ready!");
#if DEBUG
            await OliBotCore.Instance.SetStatus("Getting an upgrade");
#else
            await OliBotCore.Instance.SetRandomStatus();

            OliBotCore.Instance.StatusTimer.Elapsed += async (sender, args) => await OliBotCore.Instance.SetRandomStatus();
            OliBotCore.Instance.StatusTimer.Start();
#endif
            await OliBotCore.Instance.EnsureOliInGuilds();

            // TODO: Check through all text channels for the muted role and ensure that it is denied the Permission.SendMessages
        }

        public async Task OliBot_GuildCreated(GuildCreateEventArgs e)
        {
            OliBotCore.Log.Info($"Added to guild {e.Guild.Name}({e.Guild.Id})");
            bool oliInGuild = await OliBotCore.Instance.EnsureOliInGuild(e.Guild);

            if (!oliInGuild) return;

            DiscordRole muted = await OliBotCore.Instance.GetMutedRole(e.Guild);

            foreach (DiscordChannel channel in e.Guild.Channels)
            {
                if (channel.Type != ChannelType.Text)
                    continue;

                await channel.AddOverwriteAsync(muted, Permissions.None, Permissions.SendMessages);
            }
        }

        public async Task OliBot_GuildDeleted(GuildDeleteEventArgs e)
        {
            OliBotCore.Log.Info($"Removed from guild {e.Guild.Name}({e.Guild.Id})");
        }

        public async Task OliBot_MessageCreated(MessageCreateEventArgs e)
        {
#if DEBUG
            if (e.Channel.Id != OliBotCore.DevChannelId)
#else
            if (e.Channel.Id == OliBotCore.DevChannelId)
#endif
                return;

            OliBotCore.Log.Info($"NewMessage| {e.Guild.Name}/{e.Channel.Name}: {OliBotCore.Instance.GetUserNameFromDiscordUser(e.Guild, e.Message.Author)} said: {e.Message.Content}");

            if (e.Author.IsBot)
                return;

            string[] words = e.Message.Content.Split(new char[] { ' ', ',', '.', ':', '\t' });

            bool correctedOli = false;

            foreach (string word in words)
            {
                if (!correctedOli && (word == "olly" || word == "ollie"))
                {
                    await e.Message.RespondAsync($"{e.Author.Mention} the correct spelling is \"Oli\"");
                    correctedOli = true;
                }
                else if (Regex.Match(word, RedditHelper.Pattern).Success)
                {
                    DiscordEmbedBuilder embed = null;
                    try
                    {
                        embed = await RedditHelper.GetSubredditEmbeded(word);
                    }
                    catch (Exception)
                    { }

                    if (embed != null)
                    {
                        embed.WithFooter(e.Author.Username, e.Author.AvatarUrl);
                        await e.Message.RespondAsync(embed: embed.Build());
                    }
                    else
                    {
                        await e.Message.RespondAsync($"I tried to get {word}, but something went wrong D:");
                    }
                }
            }
        }

        public async Task OliBot_GuildMemberAdded(GuildMemberAddEventArgs e)
        {
            OliBotCore.Log.Info($"GuildMemberAdded| {e.Guild.Name}({e.Guild.Id}) {OliBotCore.Instance.GetUserNameFromDiscordUser(e.Guild, e.Member)}");
        }

        public async Task OliBot_GuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
            OliBotCore.Log.Info($"GuildMemberRemoved| {e.Guild.Name}({e.Guild.Id}) {e.Member.Username}#{e.Member.Discriminator}");

            if (e.Member.Id == OliBotCore.Oliver4888Id) await OliBotCore.Instance.UnauthorisedBotUse(e.Guild);
        }

        public async Task OliBotCore_ChannelCreated(ChannelCreateEventArgs e)
        {
            if (e.Channel.Type != ChannelType.Text)
                return;

            DiscordRole muted = await OliBotCore.Instance.GetMutedRole(e.Guild);

            await e.Channel.AddOverwriteAsync(muted, Permissions.None, Permissions.SendMessages);

            DiscordMessage message =  await e.Channel.SendMessageAsync($"Updated channel permissions for {muted.Mention}");

            await Task.Delay(5000);
            
            await message.DeleteAsync();
        }
    }
}
