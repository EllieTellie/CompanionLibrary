using System.Collections.Generic;
using System.Xml;

namespace Companion.Data
{
	public class CategoryLink : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string name;
		public bool hidden;
		public string targetId;
		public bool primary;
		public string publicationId; // it's very possible this may also have a string page attribute alongside it but seems not set for wh40k

		public List<Modifier> modifiers;
		public List<Constraint> constraints;

		protected XmlData cachedTarget;

		public CategoryLink(XmlNode node) : base(node)
		{
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			hidden = node.GetAttributeBool("hidden");
			targetId = node.GetAttribute("targetId");
			primary = node.GetAttributeBool("primary");
			publicationId = node.GetAttribute("publicationId");

			modifiers = ParseXmlList<Modifier>(node.GetNodesFromPath("modifiers", "modifier"));
			constraints = ParseXmlList<Constraint>(node.GetNodesFromPath("constraints", "constraint"));
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
