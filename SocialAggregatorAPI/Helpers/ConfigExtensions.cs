public static class ConfigExtensions
{
    public static int ParseInt(this Dictionary<string, string> dict, string key)
    {
        return dict.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : 0;
    }

    public static bool ParseBool(this Dictionary<string, string> dict, string key)
    {
        return dict.TryGetValue(key, out var value) && bool.TryParse(value, out var result) && result;
    }

    public static List<string> ParseList(this Dictionary<string, string> dict, string key)
    {
        return dict.TryGetValue(key, out var value) ? value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() : new List<string>();
    }
}
