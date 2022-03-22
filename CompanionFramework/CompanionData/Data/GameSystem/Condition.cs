using System;
using System.Xml;

namespace Companion.Data
{
	public class Condition : XmlData
	{
		public string field;
		public string scope;
		public double value;
		public bool percentValue;
		public bool shared;
		public bool includeChildSelections;
		public bool includeChildForces;
		public string childId;
		public string type;

		public Condition(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			field = node.GetAttribute("field");
			scope = node.GetAttribute("scope");
			value = node.GetAttributeDouble("value");
			percentValue = node.GetAttributeBool("percentValue");
			shared = node.GetAttributeBool("shared");
			includeChildSelections = node.GetAttributeBool("includeChildSelections");
			includeChildForces = node.GetAttributeBool("includeChildForces");
			childId = node.GetAttribute("childId");
			type = node.GetAttribute("type");
		}

        public override void WriteXml(XmlWriter writer)
        {
			writer.WriteStartElement("condition");
			writer.WriteAttribute("field", field);
			writer.WriteAttribute("scope", scope);
			writer.WriteAttribute("value", value);
			writer.WriteAttribute("percentValue", percentValue);
			writer.WriteAttribute("shared", shared);
			writer.WriteAttribute("includeChildSelections", includeChildSelections);
			writer.WriteAttribute("includeChildForces", includeChildForces);
			writer.WriteAttribute("childId", childId);
			writer.WriteAttribute("type", type);
			writer.WriteEndElement();
        }

        public bool IsConditionMet(GameSystem gameSystem, Roster roster, Selection selection)
		{
			if (field == "selections")
			{
				// just handle specific
				if (scope != "force")
				{
					Selection scopedSelection = selection.GetSelectionByEntryId(scope);
					if (scopedSelection == null)
						scopedSelection = selection.entryId == scope ? selection : null;

					if (scopedSelection != null)
					{
						Selection childSelection = scopedSelection.GetSelectionByEntryId(childId);
						if (childSelection != null)
						{
							if (CompareSelectionValue(childSelection.number))
							{

							}
						}
					}
				}

				return false;
			}
			else
			{
				// TODO: not handled
				return false;
			}
		}

		private bool CompareSelectionValue(double selectionValue)
		{
			if (type == "greaterThan")
			{
				return selectionValue > value;
			}
			else if (type == "equalTo")
			{
				return selectionValue == value;
			}
			else if (type == "lessThan")
			{
				return selectionValue < value;
			}
			else
			{
				// TODO: not handled type
				return false;
			}
		}
	}
}
