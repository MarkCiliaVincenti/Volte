﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Gommon;
using Qmmands;
using ICommandContext = Qmmands.ICommandContext;

namespace Volte.Commands.TypeParsers
{
    [VolteTypeParser]
    public sealed class GuildParser : TypeParser<SocketGuild>
    {
        public override Task<TypeParserResult<SocketGuild>> ParseAsync(
            Parameter parameter,
            string value,
            ICommandContext context,
            IServiceProvider provider)
        {
            var ctx = context.Cast<VolteContext>();
            SocketGuild guild = default;

            var guilds = ctx.Client.Guilds;

            if (ulong.TryParse(value, out var id))
                guild = guilds.FirstOrDefault(x => x.Id == id);

            if (guild is null)
            {
                var match = guilds.Where(x =>
                    x.Name.EqualsIgnoreCase(value)).ToList();
                if (match.Count > 1)
                    return Task.FromResult(TypeParserResult<SocketGuild>.Unsuccessful(
                        "Multiple guilds found, try using its ID."));

                guild = match.FirstOrDefault();
            }

            return Task.FromResult(guild is null
                ? TypeParserResult<SocketGuild>.Unsuccessful("Guild not found.")
                : TypeParserResult<SocketGuild>.Successful(guild));
        }
    }
}