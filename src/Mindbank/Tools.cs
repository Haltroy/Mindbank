using System;
using Avalonia.Media;

namespace Mindbank;

public static class Tools
{
    internal static Color ShiftBrightness(Color c, int value, bool shiftAlpha = false)
    {
        return new Color(
            shiftAlpha
                ? !IsTransparencyHigh(c)
                    ? (byte)AddIfNeeded(c.A, value, byte.MaxValue)
                    : (byte)SubtractIfNeeded(c.A, value)
                : c.A,
            !IsBright(c) ? (byte)AddIfNeeded(c.R, value, byte.MaxValue) : (byte)SubtractIfNeeded(c.R, value),
            !IsBright(c) ? (byte)AddIfNeeded(c.G, value, byte.MaxValue) : (byte)SubtractIfNeeded(c.G, value),
            !IsBright(c) ? (byte)AddIfNeeded(c.B, value, byte.MaxValue) : (byte)SubtractIfNeeded(c.B, value));
    }

    internal static int Brightness(Color c)
    {
        return (int)Math.Sqrt(
            c.R * c.R * .241 +
            c.G * c.G * .691 +
            c.B * c.B * .068);
    }

    internal static bool IsTransparencyHigh(Color c)
    {
        return c.A < 130;
    }

    internal static bool IsBright(Color c)
    {
        return Brightness(c) > 130;
    }

    internal static int SubtractIfNeeded(int number, int subtract, int limit = 0)
    {
        return limit == 0 ? number > subtract ? number - subtract : number :
            number - subtract < limit ? number : number - subtract;
    }

    internal static int AddIfNeeded(int number, int add, int limit = int.MaxValue)
    {
        return number + add > limit ? number : number + add;
    }
}