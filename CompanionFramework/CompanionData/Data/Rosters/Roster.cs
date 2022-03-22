using CompanionFramework.IO.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace Companion.Data
{
	public class Roster : XmlData, INameable, IIdentifiable
	{
		public string id;
		public string gameSystemId;
		public string gameSystemName;
		public string gameSystemRevision;
		public string name;
		public string battleScribeVersion;

		public List<Force> forces;
		public List<Cost> costs;
		public List<CostLimit> costLimits;

		public Roster(XmlNode node) : base(node)
		{
		}

		protected override void OnParseNode()
		{
			id = node.GetAttribute("id");
			gameSystemId = node.GetAttribute("gameSystemId");
			gameSystemName = node.GetAttribute("gameSystemName");
			gameSystemRevision = node.GetAttribute("gameSystemRevision");
			battleScribeVersion = node.GetAttribute("battleScribeVersion");
			name = node.GetAttribute("name");

			forces = ParseXmlList<Force>(node.GetNodesFromPath("forces", "force"));
			costs = ParseXmlList<Cost>(node.GetNodesFromPath("costs", "cost"));
			costLimits = ParseXmlList<CostLimit>(node.GetNodesFromPath("costLimits", "costLimit"));
		}

        public override void WriteXml(XmlWriter writer)
        {
			writer.WriteStartElement("roster");
			writer.WriteAttributeString("id", id);
			writer.WriteAttributeString("name", name);
			writer.WriteAttributeString("battleScribeVersion", battleScribeVersion);
			writer.WriteAttributeString("xmlns", "http://www.battlescribe.net/schema/rosterSchema");

			foreach (Force force in forces)
            {
				force.WriteXml(writer);
            }

			foreach (Cost cost in costs)
			{
				cost.WriteXml(writer);
			}

			foreach (CostLimit costLimit in costLimits)
            {
				costLimit.WriteXml(writer);
            }

			writer.WriteEndElement();
		}

        public List<Selection> GetUnits()
		{
			List<Selection> selections = new List<Selection>();

			foreach (Force force in forces)
			{
				foreach (Selection selection in force.selections)
				{
					if (selection.IsUnitSelection())
					{
						selections.Add(selection);
					}
				}
			}

			return selections;
		}

		public static Roster LoadRoster(string rosterPath)
		{
			//byte[] data = FileUtils.ReadFileSimple(rosterPath);

			//// unzip data
			//byte[] uncompressedData = DecompressRoster(data);

			//string text = FileUtils.GetString(uncompressedData);

			//XmlDocument xmlDocument = new XmlDocument();
			//xmlDocument.LoadXml(text);

			XmlDocument xmlDocument = DecompressRosterXml(rosterPath);

			return new Roster(xmlDocument.GetNode("roster"));
		}

		public static byte[] DecompressRoster(byte[] data)
		{
			return CompressionUtils.DecompressFileFromZip(data, ".ros");
		}

		public static XmlDocument DecompressRosterXml(byte[] data)
		{
			return CompressionUtils.DecompressXmlDocumentFromZip(data, ".ros");
		}

		public static XmlDocument DecompressRosterXml(string path)
		{
			return CompressionUtils.DecompressXmlDocumentFromZipFile(path, ".ros");
		}

		public static Roster LoadRosterXml(byte[] data)
		{
			// unzip data
			//byte[] uncompressedData = DecompressRoster(data);

			//string xml = FileUtils.GetString(uncompressedData);

			//XmlDocument xmlDocument = new XmlDocument();
			//xmlDocument.LoadXml(xml);

			XmlDocument xmlDocument = DecompressRosterXml(data);

			return new Roster(xmlDocument.GetNode("roster"));
		}

		public string GetName()
		{
			return name;
		}

		public string GetId()
		{
			return id;
		}

		public void AddForce(Force force)
		{
			if (forces == null)
				forces = new List<Force>();

			forces.Add(force);
		}

		/// <summary>
		/// Checks if any costs have a value larger than zero.
		/// </summary>
		/// <returns>True if it has valid costs</returns>
		public bool HasValidCosts()
		{
			if (costs == null)
				return false;

			foreach (Cost cost in costs)
			{
				if (cost.value > 0)
					return true;
			}

			return false;
		}

		protected override void InitFields()
		{
			base.InitFields();

			forces = new List<Force>();
			costs = new List<Cost>();
			costLimits = new List<CostLimit>();

			// allow searching
			AddField(forces);
			AddField(costs);
			AddField(costLimits);
		}
	}
}
