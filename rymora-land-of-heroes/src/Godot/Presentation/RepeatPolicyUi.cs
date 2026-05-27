using System.Globalization;
using Godot;
using RymoraLandOfHeroes.Core.Automation;

namespace RymoraLandOfHeroes.GodotAdapter.Presentation;

internal static class RepeatPolicyUi
{
    public static void Configure(OptionButton? mode)
    {
        if (mode is null || mode.ItemCount > 0)
        {
            return;
        }

        mode.AddItem("Once", (int)RepeatMode.Once);
        mode.AddItem("Forever", (int)RepeatMode.Forever);
        mode.AddItem("Count", (int)RepeatMode.Count);
        mode.AddItem("Duration", (int)RepeatMode.Duration);
        mode.Select(0);
    }

    public static bool TryRead(OptionButton? mode, LineEdit? value, out RepeatPolicy repeat)
    {
        repeat = RepeatPolicy.Once;
        var selectedMode = GetSelectedMode(mode);
        switch (selectedMode)
        {
            case RepeatMode.Once:
                repeat = RepeatPolicy.Once;
                return true;
            case RepeatMode.Forever:
                repeat = RepeatPolicy.Forever;
                return true;
            case RepeatMode.Count:
                return TryReadCount(value?.Text, out repeat);
            case RepeatMode.Duration:
                return TryReadDuration(value?.Text, out repeat);
            default:
                GD.Print($"Unknown repeat mode: {selectedMode}.");
                return false;
        }
    }

    public static void Write(OptionButton? mode, LineEdit? value, RepeatPolicy repeat)
    {
        if (mode is not null)
        {
            for (var i = 0; i < mode.ItemCount; i++)
            {
                if (mode.GetItemId(i) == (int)repeat.Mode)
                {
                    mode.Select(i);
                    break;
                }
            }
        }

        if (value is null)
        {
            return;
        }

        value.Text = repeat.Mode switch
        {
            RepeatMode.Count => repeat.RepeatCount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            RepeatMode.Duration => repeat.Seconds?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty,
            _ => string.Empty
        };
    }

    public static string Format(RepeatPolicy repeat)
    {
        return repeat.Mode switch
        {
            RepeatMode.Count => repeat.RepeatCount?.ToString(CultureInfo.InvariantCulture) ?? "count",
            RepeatMode.Duration => $"{repeat.Seconds?.ToString("0.##", CultureInfo.InvariantCulture)}s",
            _ => repeat.Mode.ToString()
        };
    }

    private static RepeatMode GetSelectedMode(OptionButton? mode)
    {
        if (mode is null || mode.Selected < 0)
        {
            return RepeatMode.Once;
        }

        return (RepeatMode)mode.GetItemId(mode.Selected);
    }

    private static bool TryReadCount(string? text, out RepeatPolicy repeat)
    {
        repeat = RepeatPolicy.Once;
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) || count <= 0)
        {
            GD.Print("Count repeat requires a positive whole number.");
            return false;
        }

        repeat = RepeatPolicy.Count(count);
        return true;
    }

    private static bool TryReadDuration(string? text, out RepeatPolicy repeat)
    {
        repeat = RepeatPolicy.Once;
        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
            || !float.IsFinite(seconds)
            || seconds <= 0)
        {
            GD.Print("Duration repeat requires positive seconds.");
            return false;
        }

        repeat = RepeatPolicy.Duration(seconds);
        return true;
    }
}
