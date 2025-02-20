﻿using Volte.Interactive;

namespace Volte.Commands.Text.Modules;

public sealed partial class ModerationModule : VolteModule
{
    [Command("Warn", "W")]
    [Description("Warns the target member for the given reason.")]
    public async Task<ActionResult> WarnAsync([CheckHierarchy, EnsureNotSelf, Description("The member to warn.")]
        SocketGuildUser member, [Remainder, Description("The reason for the warn.")]
        string reason)
    {
        await member.WarnAsync(Context, reason);

        return Ok($"Successfully warned **{member}** for **{reason}**.",
            _ => ModerationService.OnModActionCompleteAsync(ModActionEventArgs.InContext(Context)
                .WithActionType(ModActionType.Warn)
                .WithTarget(member)
                .WithReason(reason))
        );
    }

    [Command("Warns", "Ws")]
    [Description("Shows all the warns for the given member.")]
    public Task<ActionResult> WarnsAsync([Remainder, Description("The member to list warns for.")]
        SocketGuildUser member)
    {
        var warns = Db.GetData(Context.Guild).Extras.Warns.Where(x => x.User == member.Id)
            .Select(x => $"{Format.Bold(x.Reason)}, on {Format.Bold(x.Date.FormatDate())}");
        return Ok(PaginatedMessage.Builder.New()
            .WithPages(warns)
            .WithTitle($"Warns for {member}")
            .SplitPages(8)
            .WithDefaults(Context));
    }

    [Command("ClearWarns", "Cw")]
    [Description("Clears the warnings for the given member.")]
    public async Task<ActionResult> ClearWarnsAsync(
        [Remainder, EnsureNotSelf, Description("The member who you want to clear warns for.")]
        SocketGuildUser member)
    {
        var warnCount = Context.GuildData.Extras.Warns.RemoveWhere(x => x.User == member.Id);
        Db.Save(Context.GuildData);

        var e = Context
            .CreateEmbedBuilder(
                $"Your {"warn".ToQuantity(warnCount)} in {Format.Bold(Context.Guild.Name)} have been cleared. Hooray!")
            .Apply(Context.GuildData);

        if (!await member.TrySendMessageAsync(embed: e.Build()))
            Warn(LogSource.Volte, $"encountered a 403 when trying to message {member}!");

        return Ok($"Cleared **{warnCount}** warnings for **{member}**.", _ =>
            ModerationService.OnModActionCompleteAsync(ModActionEventArgs
                .InContext(Context)
                .WithActionType(ModActionType.ClearWarns)
                .WithTarget(member))
        );
    }
}