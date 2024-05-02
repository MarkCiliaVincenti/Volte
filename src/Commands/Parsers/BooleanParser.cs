﻿namespace Volte.Commands;

[InjectTypeParser(true)]
public sealed class BooleanParser : VolteTypeParser<bool>
{
    private static string[] _trueValues =
    [
        "true", "y",
        "yes", "ye",
        "yep", "yeah",
        "sure", "affirmative",
        "yar", "aff",
        "ya", "da",
        "yas", "enable",
        "yip", "positive",
        "1"
    ];

    private static readonly string[] _falseValues =
    [
        "false", "n",
        "no", "nah",
        "na", "nej",
        "nope", "nop",
        "neg", "negatory",
        "disable", "nay",
        "negative", "0"
    ];

    public override ValueTask<TypeParserResult<bool>> ParseAsync(string value, VolteContext _) => Parse(value, _);
    
    public static TypeParserResult<bool> Parse(string value, VolteContext _)
    {
        if (_trueValues.ContainsIgnoreCase(value))
            return Success(true);

        if (_falseValues.ContainsIgnoreCase(value))
            return Success(false);

        return bool.TryParse(value, out var result)
            ? Success(result)
            : Failure($"Failed to parse a {typeof(bool)} (true/false) value. Try using true or false.");
    }
}