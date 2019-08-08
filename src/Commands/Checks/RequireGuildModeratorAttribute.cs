using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Gommon;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using ICommandContext = Qmmands.ICommandContext;

namespace Volte.Commands.Checks
{
    public sealed class RequireGuildModeratorAttribute : CheckBaseAttribute
    {
        public override async Task<CheckResult> CheckAsync(ICommandContext context, IServiceProvider provider)
        {
            var ctx = context.Cast<VolteContext>();
            if (ctx.User.IsModerator(provider.Cast<ServiceProvider>())) return CheckResult.Successful;

            await ctx.ReactFailureAsync();
            return CheckResult.Unsuccessful("Insufficient permission.");
        }
    }
}