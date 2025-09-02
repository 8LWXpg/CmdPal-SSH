using GlobExpressions;

namespace SSH.Classes.Config;

public class Parser
{
	// exclude ',' and '!' here, as they only appear inside brackets
	private static readonly char[] GlobCharacters = ['*', '?', '[', ']', '{', '}'];

	/// <summary>
	///     Convert Lexer nodes to ConfigNode
	/// </summary>
	public Parser(string configPath)
	{
		Nodes = [];
		List<ConfigNode> globNodes = [];
		var lexer = new Lexer(configPath);
		Includes = lexer.Includes;

		ConfigNode? current = null;
		foreach (KeyValuePair<string, string> node in lexer.Nodes)
		{
			// First node must be Host
			if (node.Key == "Host")
			{
				AddNode(current, Nodes, globNodes);

				current = new ConfigNode(node.Value, []);
				// {
				// 	Host = ,
				// 	Properties = []
				// };
			}
			else
				_ = current!.Properties.TryAdd(node.Key, node.Value);
		}

		AddNode(current, Nodes, globNodes);

		foreach (ConfigNode globNode in globNodes)
		{
			foreach (ConfigNode node in Nodes)
			{
				var glob = new Glob(globNode.Host);
				if (!glob.IsMatch(node.Host)) continue;

				foreach (KeyValuePair<string, string> property in globNode.Properties)
					_ = node.Properties.TryAdd(property.Key, property.Value);
			}
		}
	}

	public List<ConfigNode> Nodes { get; }
	public HashSet<string> Includes { get; }

	private static void AddNode(ConfigNode? node, List<ConfigNode> nodes, List<ConfigNode> globNodes)
	{
		if (node == null) return;

		if (HasGlobPattern(node.Host))
			globNodes.Add(node);
		else
			nodes.Add(node);
	}

	private static bool HasGlobPattern(string pattern) => pattern.Any(c => GlobCharacters.Contains(c));
}