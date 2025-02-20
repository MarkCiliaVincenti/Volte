﻿using System.Globalization;

namespace Volte.Commands.Text;

[InjectTypeParser]
public sealed class ColorParser : VolteTypeParser<Color>
{
    public override ValueTask<TypeParserResult<Color>> ParseAsync(string value, VolteContext _) => Parse(value);
        
    public static TypeParserResult<Color> Parse(string value)
    {
        Color? c = null;
        
        if (uint.TryParse(value.StartsWith('#') ? value[1..] : value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var colorInt))
            c = new Color(colorInt);

        if (c is null)
        {
            try
            {
                var val = value.Split(" ");
                
                var r = val[0].Parse<int>();
                var g = val[1].Parse<int>();
                var b = val[2].Parse<int>();

                if (r > 255 || g > 255 || b > 255)
                {
                    return Failure(
                        "A value in an RGB sequence may not be over the value of 255.");
                }

                c = new Color(r, g, b);
            }
            catch (Exception)
            { 
                c = null;
            }
        }

        return c is null
            ? Failure("A color could not be determined from your input text. Try using a hex value.")
            : Success(c.Value);
    }
}