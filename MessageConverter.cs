public class MessageConverter
{
    public static string ToHumanReadable(string message)
    {
        return message.Replace("\u0005", "[:endmsg]")
            .Replace("\f", "[:nextmsg]")
            .Replace("\t", "[:tab]")
            .Replace("\u0022", "[:dq]")
            .Replace("\n", "[:newline]");
    }
}