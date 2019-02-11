﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using NLog;
using RedditSharp;
using RedditSharp.Things;

namespace OliBot.Classes.Helpers.Sites.Reddit
{
    public static class RedditHelper
    {
        private static RedditSharp.Reddit _redditClient;

        public static string Pattern = "^r/[a-zA-Z0-9][a-zA-Z0-9_]{0,20}";
        public static string RedditUrl = "https://old.reddit.com";

        public static int MaxResponsesPerMessage = 2;

        public static void Login(string username, string password, string clientId, string secret)
        {
            try
            {
                // That redirectURI was used in an example and just seems to work, so I'm not touching it
                _redditClient = new RedditSharp.Reddit(new BotWebAgent(username, password, clientId, secret, "http://127.0.0.1:65010"), false);
            }
            catch (Exception ex)
            {
                OliBotCore.Log.Fatal(ex);
            }
        }

        private async static Task<Subreddit> GetSubreddit(string subreddit)
        {
            try
            {
                return subreddit == "r/all" ? _redditClient.RSlashAll : await _redditClient.GetSubredditAsync(subreddit);
            }
            catch (Exception ex)
            {
                OliBotCore.Log.Fatal(ex);
                return null;
            }
        }

        public async static Task<DiscordEmbedBuilder> GetSubredditEmbeded(string subreddit, DiscordMember author)
        {
            Subreddit sub = await GetSubreddit(subreddit);
            if (sub == null)
                return null;
            //OliBotCore.Log.Debug($"Seen subreddit {subreddit}");
            string description = sub.Description;
            if (description?.Length > 300)
            {
                description = description.Substring(0, 300) + "...";
            }
            //OliBotCore.Log.Debug(description);
            var embed = new DiscordEmbedBuilder
            {
                Color = author.Color,
                Title = $"{sub.Title}",
                Description = description ?? "",
                Timestamp = DateTime.UtcNow,
                ThumbnailUrl = sub.HeaderImage
            };

            if (subreddit == "r/all")
            {
                embed.AddField("Links", $"[r/all]({RedditUrl}/r/all)", true);
            }
            else
            {
                embed
                    .AddField("Subscribers", string.Format("{0:n0}", sub.Subscribers), true)
                    .AddField("Active Users", string.Format("{0:n0}", sub.ActiveUsers), true)
                    .AddField("Links", $"[r/{sub.DisplayName}]({RedditUrl}{sub.Url})", true)
                    .AddField("Created", sub.Created?.ToString("dd/MM/yyyy hh:mm tt"), true);
            }

            IEnumerable<Post> posts = sub.GetTop(FromTime.Week).Take(3);
            string topPostsTxt = "";
            foreach (Post post in posts)
            {
                string title = post.Title;
                title = title.Length > 50 ? title.Substring(0, 47) + "..." : title;

                topPostsTxt += $"{(post.NSFW ? "NSFW: " : "")}[{title}]({RedditUrl}/comments/{post.Id}/) by [u/{post.AuthorName}]({RedditUrl}/u/{post.AuthorName}) {Environment.NewLine}";
            }
            embed.AddField("Top 3 posts (Weekly)", topPostsTxt);
            embed.WithFooter(author.Username, author.AvatarUrl);
            return embed;
        }
    }
}