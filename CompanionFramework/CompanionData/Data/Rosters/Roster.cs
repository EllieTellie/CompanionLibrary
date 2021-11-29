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
			name = node.GetAttribute("name");

			forces = ParseXmlList<Force>(node.GetNodesFromPath("forces", "force"));
			costs = ParseXmlList<Cost>(node.GetNodesFromPath("costs", "cost"));
			costLimits = ParseXmlList<CostLimit>(node.GetNodesFromPath("costLimits", "costLimit"));
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
			byte[] data = FileUtils.ReadFileSimple(rosterPath);

			// unzip data
			byte[] uncompressedData = DecompressRoster(data);

			string text = FileUtils.GetString(uncompressedData);

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(text);

			return new Roster(xmlDocument.GetNode("roster"));
		}

		public static byte[] DecompressRoster(byte[] data)
		{
			return CompressionUtils.DecompressFileFromZip(data, ".ros");
		}

		public static Roster LoadRosterXml(byte[] data)
		{
			// unzip data
			byte[] uncompressedData = DecompressRoster(data);

			string xml = FileUtils.GetString(uncompressedData);

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);

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
