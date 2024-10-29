using System;
using System.Globalization;

namespace Mindbank.Backend.Exceptions;

public class MindbankException(string messageID, params string[] args)
    : Exception
{
    public override string Message
    {
        get
        {
            var s = Lang.Lang.ResourceManager.GetString("Error_" + messageID, CultureInfo.CurrentCulture) ??
                    $"Unknown error \"{messageID} (args: {args})";
            for (var i = 0; i < args.Length; i++)
                s = s.Replace($"{{{i}}}", args[i]);
            return s;
        }
    }
}

public class MindbankEndOfFileException(int type, params string[] args)
    : MindbankException("EndOfFileReached" + type, args);
