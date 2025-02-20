namespace Volte.Commands.Text.Modules;

public sealed partial class ModerationModule
{
    [Command("Softban")]
    [Description(
        "Softbans the mentioned user, kicking them and deleting the last x (where x is defined by the daysToDelete parameter) days of messages.")]
    public async Task<ActionResult> SoftBanAsync(
        [CheckHierarchy, EnsureNotSelf, Description("The member to softban.")]
        SocketGuildUser user, [Description("The amount of days of messages to delete. Defaults to 7.")]
        int daysToDelete = 0,
        [Remainder, Description("The reason for the softban.")]
        string reason = "Softbanned by a Moderator.")
    {
        var e = Context.CreateEmbedBuilder(
                $"You've been softbanned from **{Context.Guild.Name}** for **{reason}**.")
            .Apply(Context.GuildData);

        if (!await user.TrySendMessageAsync(embed: e.Build()))
            Warn(LogSource.Module, $"encountered a 403 when trying to message {user}!");

        try
        {
            await user.BanAsync(daysToDelete == 0 ? 7 : daysToDelete, reason);
            await Context.Guild.RemoveBanAsync(user);

            return Ok($"Successfully softbanned **{user.Username}#{user.Discriminator}**.", _ =>
                ModerationService.OnModActionCompleteAsync(ModActionEventArgs
                    .InContext(Context)
                    .WithTarget(user)
                    .WithReason(reason))
            );
        }
        catch
        {
            return BadRequest(
                "An error occurred softbanning that user. Do I have permission; or are they higher than me in the role list?");
        }
    }
}