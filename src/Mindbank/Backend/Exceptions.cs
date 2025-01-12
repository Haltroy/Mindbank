using System;
using System.Globalization;

namespace Mindbank.Backend.Exceptions;

public class MindbankException(string messageId, params string[] args)
    : Exception
{
    public override string Message
    {
        get
        {
            var s = Lang.Lang.ResourceManager.GetString("Error_" + messageId, CultureInfo.CurrentCulture) ??
                    $"Unknown error \"{messageId} (args: {args})";
            for (var i = 0; i < args.Length; i++)
                s = s.Replace($"{{{i}}}", args[i]);
            return s;
        }
    }
}

public class MindbankEndOfFileException(int type, params string[] args)
    : MindbankException("EndOfFileReached" + type, args);
