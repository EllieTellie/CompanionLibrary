using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class InfoLink : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string name;
		public bool hidden;
		public string targetId;
		public string type;
		public string publicationId;
		public string page;

		public List<Modifier> modifiers;

		protected XmlData cachedTarget;

		public InfoLink(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			hidden = node.GetAttributeBool("hidden");
			targetId = node.GetAttribute("targetId");
			type = node.GetAttribute("type");
			publicationId = node.GetAttribute("publicationId");
			page = node.GetAttribute("page");

			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}

		public XmlData GetTarget(GameSystem gameSystem)
		{
			if (cachedTarget != null)
				return cachedTarget;

			cachedTarget = SystemManager.Instance.SearchById(gameSystem, targetId, true);

			return cachedTarget;
		}

		public XmlData GetTarget(GameSystemGroup gameSystemGroup)
		{
			if (cachedTarget != null)
				return cachedTarget;

			cachedTarget = gameSystemGroup.SearchById(targetId, true);

			return cachedTarget;
		}

		public XmlData GetTarget(Catalogue catalogue)
		{
			if (cachedTarget != null)
				return cachedTarget;

			cachedTarget = catalogue.SearchById(targetId, true);

			return cachedTarget;
		}
	}
}