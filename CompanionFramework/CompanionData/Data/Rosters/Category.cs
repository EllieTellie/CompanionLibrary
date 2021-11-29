using System.Xml;

namespace Companion.Data
{
	public class Category : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string name;
		public bool primary;

		public Category(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			primary = node.GetAttributeBool("primary");
		}

		public int GetCategoryOrder()
		{
			switch (name)
			{
				case "Primarch | Daemon Primarch | Supreme Commander":
					return 0;
				case "HQ":
					return 1;
				case "Troops":
					return 2;
				case "Elites":
					return 3;
				case "Fast Attack":
					return 4;
				case "Heavy Support":
					return 5;
				case "Lord of War":
					return 6;
				case "Flyer":
					return 7;
				case "Dedicated Transport":
					return 8;
				default:
					return 9;
			}
		}

		public string GetId()
		{
			return id;
		}

		public string GetName()
		{
			return name;
		}
	}
}
