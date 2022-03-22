using Companion.Data.Utils;
using CompanionFramework.Core.Log;
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
			writer.WriteStartElement("roster", "http://www.battlescribe.net/schema/rosterSchema"); // keeping the namespace even if it's a dead link, probably can be https too
			writer.WriteAttribute("id", id);
			writer.WriteAttribute("name", name);
			writer.WriteAttribute("battleScribeVersion", battleScribeVersion);
			writer.WriteAttribute("gameSystemId", gameSystemId);
			writer.WriteAttribute("gameSystemName", gameSystemName);
			writer.WriteAttribute("gameSystemRevision", gameSystemRevision);

			WriteXmlList(writer, costs, "costs");
			WriteXmlList(writer, costLimits, "costLimits");
			WriteXmlList(writer, forces, "forces");

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

		/// <summary>
		/// Save roster xml.
		/// </summary>
		/// <returns>Return byte data of text</returns>
		public byte[] SaveRosterXml()
        {
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = FileUtils.DefaultEncoding;
			settings.Indent = true;

			using (MemoryStream stream = new MemoryStream())
			{
				using (StreamWriter streamWriter = new StreamWriter(stream, FileUtils.DefaultEncoding))
				{
					using (XmlWriter writer = XmlWriter.Create(streamWriter, settings))
					{
						writer.WriteStartDocument(true);

						WriteXml(writer);
					}
				}

				return stream.ToArray();
			}
		}

		/// <summary>
		/// Save this roster as xml to a .rosz file.
		/// </summary>
		/// <param name="path">Folder to save to</param>
		/// <param name="name">File name minus the extension</param>
		/// <param name="overwrite">Whether to overwrite the file</param>
		/// <returns>Return true if successful</returns>
		public bool SaveRosterXml(string path, string name, bool overwrite = false)
		{
			try
			{
				byte[] data = SaveRosterXml();

				if (data == null)
					return false;

				string targetFile = Path.Combine(path, name + ".rosz");

				if (!overwrite && File.Exists(targetFile))
				{
					return false;
				}

				CompressionUtils.CompressXmlDocumentToZipFile(targetFile, name + ".ros", data);

				return true;
			}
			catch (Exception e)
            {
				FrameworkLogger.Exception(e);
				return false;
            }
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

		/// <summary>
		/// Merge any selections in the forces if they match. This will go over every selection. This uses <see cref="SortingUtils.MergeSelections(List{Selection})"/>. This cannot be undone.
		/// </summary>
		public void MergeSelections()
        {
			foreach (Force force in forces)
            {
				MergeSelectionRecursive(force.selections);
            }
        }

        private void MergeSelectionRecursive(List<Selection> selections)
        {
			if (selections == null) // just in case
				return;

			// merge them if required
			SortingUtils.MergeSelections(selections);

			// go recursive
			foreach (Selection selection in selections)
            {
				if (selection.selections != null && selection.selections.Count > 0)
				{
					MergeSelectionRecursive(selection.selections);
				}
            }
        }
    }
}
