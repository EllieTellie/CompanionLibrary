using Companion.Data.Utils;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Companion.Data
{
	public class Selection : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string entryId;
		public string entryGroupId;
		public string name;
		public string type;
		public string publicationId;

		public string page; // can be multiple pages, so cannot be an integer
		public int number;

		public List<Cost> costs;
		public List<Selection> selections;
		public List<Profile> profiles;
		public List<Category> categories;
		public List<Rule> rules;

		/// <summary>
		/// Parent selection if available.
		/// </summary>
		public Selection parent;

		public Selection(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			name = node.GetAttribute("name");
			type = node.GetAttribute("type");
			entryId = node.GetAttribute("entryId");
			entryGroupId = node.GetAttribute("entryGroupId");
			publicationId = node.GetAttribute("publicationId");
			page = node.GetAttribute("page");

			number = node.GetAttributeInt("number", 1);

			costs = ParseXmlList<Cost>(node.GetNodesFromPath("costs", "cost"));
			selections = ParseXmlList<Selection>(node.GetNodesFromPath("selections", "selection"));
			categories = ParseXmlList<Category>(node.GetNodesFromPath("categories", "category"));
			profiles = ParseXmlList<Profile>(node.GetNodesFromPath("profiles", "profile"));
			rules = ParseXmlList<Rule>(node.GetNodesFromPath("rules", "rule"));

			// sort it at parse time
			categories.Sort(SortingUtils.CategoriesByPrimary); // TODO: Move this out of the parsing block and into the front end

			SetupParent();
			//Debug.Log("Pre: " + GetCombinedSelectionsText());
			SortingUtils.MergeSelections(selections);  // TODO: Move this out of the parsing block and into the front end
			//Debug.Log("Post: " + GetCombinedSelectionsText());
		}

        public override void WriteXml(XmlWriter writer)
        {
			writer.WriteStartElement("selection");
			writer.WriteAttribute("id", id);
			writer.WriteAttribute("name", name);
			writer.WriteAttribute("entryId", entryId);
			writer.WriteAttribute("entryGroupId", entryGroupId);
			writer.WriteAttribute("publicationId", publicationId);
			writer.WriteAttribute("page", page);
			writer.WriteAttribute("number", number);
			writer.WriteAttribute("type", type);

			WriteXmlList(writer, rules, "rules");
			WriteXmlList(writer, profiles, "profiles");
			WriteXmlList(writer, selections, "selections");
			WriteXmlList(writer, costs, "costs");
			WriteXmlList(writer, categories, "categories");

			writer.WriteEndElement();
		}

        private void SetupParent()
		{
			foreach (Selection selection in selections)
			{
				selection.SetParent(this);
			}
		}

		private void SetParent(Selection parent)
		{
			this.parent = parent;
		}

		public bool HasProfileOfType(string type)
		{
			foreach (Profile profile in profiles)
			{
				if (profile.typeName == type)
				{
					return true;
				}
			}

			return false;
		}

		public string GetCombinedCategoryText()
		{
			StringBuilder builder = new StringBuilder();

			foreach (Category category in categories)
			{
				if (builder.Length > 0)
					builder.Append(", ");

				builder.Append(category.name);
			}

			return builder.ToString();
		}

		public string GetCombinedSelectionsText()
		{
			StringBuilder builder = new StringBuilder();

			foreach (Selection selection in selections)
			{
				if (builder.Length > 0)
					builder.Append(", ");

				builder.Append(CommonTextUtils.GetSelectionName(selection));
			}

			return builder.ToString();
		}

		public bool HasCategories()
		{
			return categories != null && categories.Count > 0;
		}

		public Profile GetProfileOfType(string type, bool includeSubselections = true)
		{
			foreach (Profile profile in profiles)
			{
				if (profile.typeName == type)
				{
					return profile;
				}
			}

			if (includeSubselections)
			{
				foreach (Selection selection in selections)
				{
					Profile profile = selection.GetProfileOfType(type, includeSubselections);
					if (profile != null)
						return profile;
				}
			}

			return null;
		}

		public List<Profile> GetProfilesOfType(string type, bool includeSubselections = true)
		{
			List<Profile> profilesFound = new List<Profile>();
			foreach (Profile profile in profiles)
			{
				if (profile.typeName == type)
				{
					profilesFound.Add(profile);
				}
			}

			if (includeSubselections)
			{
				foreach (Selection selection in selections)
				{
					List<Profile> subProfiles = selection.GetProfilesOfType(type, includeSubselections);
					profilesFound.AddRange(subProfiles);
				}
			}

			return profilesFound;
		}

		public bool IsUnitSelection()
		{
			if (type == "model" || type == "unit")
			{
				return true;
			}
			else if (type == "upgrade")
			{
				// search for units within
				if (HasProfileOfType("Unit"))
					return true;
				else if (HasSelectionsOfType("model") || HasSelectionsOfType("unit"))
					return true;
			}

			return false;
		}

		public bool HasWeaponProfiles()
		{
			if (parent != null && parent != this) // guard against recursion
			{
				if (parent.HasWeaponProfiles())
					return true;
			}

			if (HasProfileOfType("Weapon"))
				return true;

			foreach (Selection subselection in selections)
			{
				if (subselection.HasProfileOfType("Weapon"))
					return true;
			}

			return false;
		}

		public bool HasAbilities()
		{
			if (parent != null && parent != this) // guard against recursion
			{
				if (parent.HasAbilities())
					return true;
			}

			if (HasProfileOfType("Abilities"))
				return true;

			foreach (Selection subselection in selections)
			{
				if (subselection.HasProfileOfType("Abilities"))
					return true;
			}

			return false;
		}

		public bool HasRules()
		{
			if (parent != null)
			{
				if (parent.rules.Count > 0)
					return true;
			}

			return rules.Count > 0;
		}

		public bool HasCost()
		{
			foreach (Cost cost in costs)
			{
				if (cost.value > 0)
					return true;
			}

			return false;
		}

		public double GetPoints()
		{
			return GetCostValue("pts");
		}

		public double GetCommandPoints()
		{
			return GetCostValue("CP");
		}

		public double GetPowerLevel()
		{
			return GetCostValue(" PL");
		}

		public double GetCostValue(string name)
		{
			foreach (Cost cost in costs)
			{
				if (cost.name == name)
					return cost.value;
			}

			return 0.0;
		}

		public Category GetPrimaryCategory()
		{
			foreach (Category category in categories)
			{
				if (category.primary)
					return category;
			}

			return null;
		}

		public int GetSelectionCount(string type)
		{
			int selectionCount = 0;
			foreach (Selection selection in selections)
			{
				if (selection.type == type)
					selectionCount++;
			}

			return selectionCount;
		}

		public int GetSelectionNumber(string type)
		{
			int selectionCount = 0;
			foreach (Selection selection in selections)
			{
				if (selection.type == type)
					selectionCount += selection.number;
			}

			return selectionCount;
		}

		public bool HasSelectionsOfType(string type)
		{
			foreach (Selection selection in selections)
			{
				if (selection.type == type)
					return true;
			}

			return false;
		}

		public List<Selection> GetSelectionsOfType(string type)
		{
			List<Selection> foundSelections = new List<Selection>();

			foreach (Selection selection in selections)
			{
				if (selection.type == type)
					foundSelections.Add(selection);
			}

			return foundSelections;
		}


		public List<Selection> GetSelectionsById(string id)
		{
			List<Selection> foundSelections = new List<Selection>();

			foreach (Selection selection in selections)
			{
				if (selection.id == id)
					foundSelections.Add(selection);
			}

			return foundSelections;
		}

		public Selection GetSelectionById(string id)
		{
			foreach (Selection selection in selections)
			{
				if (selection.id == id)
					return selection;
			}

			return null;
		}

		public Selection GetSelectionByType(string type)
		{
			foreach (Selection selection in selections)
			{
				if (selection.type == type)
					return selection;
			}

			return null;
		}

		public Selection GetSelectionByEntryId(string entryId)
		{
			foreach (Selection selection in selections)
			{
				if (selection.entryId == entryId)
					return selection;
			}

			return null;
		}

		public bool IsSame(Selection selection)
		{
			if (selection.entryId != entryId)
				return false;

			if (selection.entryGroupId != entryGroupId)
				return false;

			if (selection.selections.Count != selections.Count)
				return false;

			for (int i=0; i<selection.selections.Count; i++)
			{
				Selection a = selection.selections[i];
				Selection b = selections[i];

				if (!a.IsSame(b))
					return false;
			}

			return true;
		}

		public int GetIndex()
		{
			return GetIndexRecursive(0);
		}

		private int GetIndexRecursive(int currentIndex)
		{
			if (parent == null)
			{
				return currentIndex;
			}
			else
			{
				currentIndex++;
				return parent.GetIndexRecursive(currentIndex);
			}
		}

		public void AddSelection(Selection selection, bool addCosts = false)
		{
			if (selections == null)
				selections = new List<Selection>();

			// set parent
			selection.parent = this;

			if (addCosts)
			{
				foreach (Cost cost in selection.costs)
				{
					Cost existingCost = costs.GetByName(cost.name);
					if (existingCost != null)
						existingCost.value += cost.value;
				}
			}

			selections.Add(selection);
		}

		public Selection GetLastSelection()
		{
			if (selections == null || selections.Count == 0)
				return null;

			return selections[selections.Count - 1];
		}

		public string GetName()
		{
			return name;
		}

		public string GetId()
		{
			return id;
		}

		protected override void InitFields()
		{
			base.InitFields();

			costs = new List<Cost>();
			selections = new List<Selection>();
			profiles = new List<Profile>();
			categories = new List<Category>();
			rules = new List<Rule>();

			AddField(costs);
			AddField(selections);
			AddField(profiles);
			AddField(categories);
			AddField(rules);
		}

		public override string ToString()
		{
			return name;
		}
	}
}
