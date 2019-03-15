using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Volte.Data;
using Volte.Data.Objects;
using Volte.Discord;
using Volte.Extensions;

namespace Volte.Services
{
    [Service("Guild", "The main Service that handles guild-related Discord gateway events.")]
    public sealed class GuildService
    {
        private readonly LoggingService _logger;

        public GuildService(LoggingService loggingService)
        {
            _logger = loggingService;
        }

        public async Task OnJoinAsync(IGuild guild)
        {
            if (Config.BlacklistedOwners.Contains(guild.OwnerId))
            {
                await _logger.Log(LogSeverity.Warning, LogSource.Volte,
                    $"Left guild \"{guild.Name}\" owned by blacklisted owner {await guild.GetOwnerAsync()}.");
                await guild.LeaveAsync();
                return;
            }

            var owner = await guild.GetOwnerAsync();

            var embed = new EmbedBuilder()
                .WithTitle("Hey there!")
                .WithAuthor(await guild.GetOwnerAsync())
                .WithColor(Config.SuccessColor)
                .WithDescription("Thanks for inviting me! Here's some basic instructions on how to set me up.")
                .AddField("Set your admin role", "$adminrole {roleName}", true)
                .AddField("Set your moderator role", "$modrole {roleName}", true)
                .AddField("Permissions",
                    "It is recommended to give me admin permission, to avoid any permission errors that may happen." +
                    "\nYou *can* get away with just send messages, ban members, kick members, and the like if you don't want to give me admin.")
                .AddField("Support Server", "[Join my support Discord here](https://discord.gg/H8bcFr2)");

            try
            {
                await embed.SendToAsync(owner);
            }
            catch (HttpException ignored) when (ignored.DiscordCode.Equals(50007))
            {
                var c = (await guild.GetTextChannelsAsync()).FirstOrDefault();
                if (c != null) await embed.SendToAsync(c);
            }

            if (Config.JoinLeaveLog.Enabled)
            {
                var joinLeave = Config.JoinLeaveLog;
                var logger = VolteBot.GetRequiredService<LoggingService>();
                if (joinLeave.GuildId.Equals(0) || joinLeave.ChannelId.Equals(0))
                {
                    await logger.Log(LogSeverity.Error, LogSource.Service,
                        "Invalid value set for the GuildId or ChannelId in the JoinLeaveLog config option. " +
                        "To fix, set Enabled to false, or correctly fill in your options.");
                    return;
                }

                var channel = VolteBot.Client.GetGuild(joinLeave.GuildId).GetTextChannel(joinLeave.ChannelId);
                var users = (await guild.GetUsersAsync()).Where(u => !u.IsBot).ToList();
                var bots = (await guild.GetUsersAsync()).Where(u => u.IsBot).ToList();

                var e = new EmbedBuilder()
                    .WithAuthor(owner)
                    .WithTitle("Joined Guild")
                    .AddField("Name", guild.Name, true)
                    .AddField("ID", guild.Id, true)
                    .WithThumbnailUrl(guild.IconUrl)
                    .WithCurrentTimestamp()
                    .AddField("Users", users.Count, true)
                    .AddField("Bots", bots.Count, true);
                try
                {
                    if (bots.Count > users.Count)
                        await channel.SendMessageAsync(
                            $"<@{Config.Owner}>: Joined a guild with more bots than users.", false,
                            e.WithColor(0x00FF00).Build());
                    else
                        await channel.SendMessageAsync("", false, e.WithColor(0x00FF00).Build());
                }
                catch (NullReferenceException ex)
                {
                    await logger.Log(LogSeverity.Error, LogSource.Service,
                        "Invalid JoinLeaveLog.GuildId/JoinLeaveLog.ChannelId configuration.", ex);
                }
            }
        }

        public async Task OnLeaveAsync(SocketGuild guild)
        {
            if (Config.JoinLeaveLog.Enabled)
            {
                var logger = VolteBot.GetRequiredService<LoggingService>();
                var joinLeave = Config.JoinLeaveLog;
                if (joinLeave.GuildId.Equals(0) || joinLeave.ChannelId.Equals(0))
                {
                    await logger.Log(LogSeverity.Error, LogSource.Service,
                        "Invalid value set for the GuildId or ChannelId in the JoinLeaveLog config option. " +
                        "To fix, set Enabled to false, or correctly fill in your options.");
                    return;
                }

                var channel = VolteBot.Client.GetGuild(joinLeave.GuildId).GetTextChannel(joinLeave.ChannelId);
                try
                {
                    var e = new EmbedBuilder()
                        .WithAuthor(guild.Owner)
                        .WithTitle("Left Guild")
                        .AddField("Name", guild.Name, true)
                        .AddField("ID", guild.Id, true)
                        .WithThumbnailUrl(guild.IconUrl)
                        .WithColor(0xFF0000)
                        .SendToAsync(channel);
                }
                catch (NullReferenceException e)
                {
                    await logger.Log(LogSeverity.Error, LogSource.Service,
                        "Invalid JoinLeaveLog.GuildId/JoinLeaveLog.ChannelId configuration.", e);
                }
            }
        }
    }
}