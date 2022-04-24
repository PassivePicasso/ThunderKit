using System.Collections.Generic;

namespace AssetsExporter.YAML
{
	public abstract class YAMLNode
	{
		internal virtual void Emit(Emitter emitter)
		{
			bool isWrote = false;
			if (!CustomTag.IsEmpty)
			{
				emitter.Write(CustomTag.ToString()).WriteWhitespace();
				isWrote = true;
			}
			if (Anchor.Length > 0)
			{
				emitter.Write("&").Write(Anchor).WriteWhitespace();
				isWrote = true;
			}

			if (isWrote)
			{
				if (IsMultiline)
				{
					emitter.WriteLine();
				}
			}
		}

		public void AddExtraNodeForParent(YAMLNode node)
        {
			extraNodesInParent = extraNodesInParent ?? new List<YAMLNode>();
			extraNodesInParent.Add(node);
		}

		public void AddExtraNamedNodeForParent(string name, YAMLNode node)
		{
			extraNamedNodesInParent = extraNamedNodesInParent ?? new Dictionary<string, YAMLNode>();
			extraNamedNodesInParent.Add(name, node);
		}

		public abstract YAMLNodeType NodeType { get; }
		public abstract bool IsMultiline { get; }
		public abstract bool IsIndent { get; }
		
		public string Tag
		{
			get => CustomTag.Content;
			set => CustomTag = new YAMLTag(YAMLWriter.DefaultTagHandle, value);
		}
		public YAMLTag CustomTag { get; set; }
		public string Anchor { get; set; } = string.Empty;
		internal List<YAMLNode> extraNodesInParent;
		internal Dictionary<string, YAMLNode> extraNamedNodesInParent;
	}
}
