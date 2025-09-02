namespace SSH.Classes.Config;

public record ConfigNode(string Host, Dictionary<string, string> Properties);