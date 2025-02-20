﻿using System.Text.Json.Serialization;

namespace Volte.Entities;

public sealed class ModerationOptions
{
    [JsonPropertyName("mod_log_channel")]
    public ulong ModActionLogChannel { get; set; }

    [JsonPropertyName("mod_role")]
    public ulong ModRole { get; set; }

    [JsonPropertyName("admin_role")]
    public ulong AdminRole { get; set; }
        
    [JsonPropertyName("check_account_age_on_join")]
    public bool CheckAccountAge { get; set; }
        
    [JsonPropertyName("unverified_role")]
    public ulong UnverifiedRole { get; set; }
        
    [JsonPropertyName("verified_role")]
    public ulong VerifiedRole { get; set; }
        
    [JsonPropertyName("show_moderator")]
    public bool ShowResponsibleModerator { get; set; }
        
    public override string ToString()
        => JsonSerializer.Serialize(this, Config.JsonOptions);
}

public sealed class WelcomeOptions
{
    public static Dictionary<string, string> ValidPlaceholders => new Dictionary<string, string>
    {
        {"GuildName", "The name of the guild."},
        {"UserName", "The user's name."},
        {"UserMention", "The user's full @."}, 
        {"OwnerMention", "The guild owner's full @."},
        {"UserTag", "The user's discriminator (the numbers after their #)."},
        {"MemberCount", "The amount of people in the guild."},
        {"UserString", "A user's full username#discriminator; i.e. Greem#1337."}
    };
        
    [JsonPropertyName("welcome_channel")]
    public ulong WelcomeChannel { get; set; }

    [JsonPropertyName("welcome_message")]
    public string WelcomeMessage { get; set; }

    [JsonPropertyName("leaving_message")]
    public string LeavingMessage { get; set; }

    [JsonPropertyName("welcome_color")]
    public uint WelcomeColor { get; set; }

    [JsonPropertyName("welcome_dm_message")]
    public string WelcomeDmMessage { get; set; }

    public string FormatWelcomeMessage(SocketGuildUser user)
        => WelcomeMessage.ReplaceIgnoreCase("{ServerName}", user.Guild.Name)
            .ReplaceIgnoreCase("{GuildName}", user.Guild.Name)
            .ReplaceIgnoreCase("{UserName}", user.Username)
            .ReplaceIgnoreCase("{UserMention}", user.Mention)
            .ReplaceIgnoreCase("{OwnerMention}", user.Guild.Owner.Mention)
            .ReplaceIgnoreCase("{UserTag}", user.Discriminator)
            .ReplaceIgnoreCase("{MemberCount}", user.Guild.MemberCount)
            .ReplaceIgnoreCase("{UserString}", user);

    public string FormatLeavingMessage(SocketGuild guild, SocketUser user)
        => LeavingMessage.ReplaceIgnoreCase("{ServerName}", guild.Name)
            .ReplaceIgnoreCase("{GuildName}", guild.Name)
            .ReplaceIgnoreCase("{UserName}", user.Username)
            .ReplaceIgnoreCase("{UserMention}", user.Mention)
            .ReplaceIgnoreCase("{OwnerMention}", guild.Owner.Mention)
            .ReplaceIgnoreCase("{UserTag}", user.Discriminator)
            .ReplaceIgnoreCase("{MemberCount}", guild.MemberCount)
            .ReplaceIgnoreCase("{UserString}", user);

    public string FormatDmMessage(SocketGuildUser user)
        => WelcomeDmMessage.ReplaceIgnoreCase("{ServerName}", user.Guild.Name)
            .ReplaceIgnoreCase("{GuildName}", user.Guild.Name)
            .ReplaceIgnoreCase("{UserName}", user.Username)
            .ReplaceIgnoreCase("{UserMention}", user.Mention)
            .ReplaceIgnoreCase("{OwnerMention}", user.Guild.Owner.Mention)
            .ReplaceIgnoreCase("{UserTag}", user.Discriminator)
            .ReplaceIgnoreCase("{MemberCount}", user.Guild.MemberCount)
            .ReplaceIgnoreCase("{UserString}", user);
        
    public override string ToString()
        => JsonSerializer.Serialize(this, Config.JsonOptions);
}

public sealed class StarboardOptions
{
    [JsonPropertyName("starboard_channel")]
    public ulong StarboardChannel { get; set; }
        
    [JsonPropertyName("starboard_enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("number_of_required_stars")]
    public int StarsRequiredToPost { get; set; } = 1;
        
    [JsonPropertyName("delete_invalid_stars")]
    public bool DeleteInvalidStars { get; set; }
}