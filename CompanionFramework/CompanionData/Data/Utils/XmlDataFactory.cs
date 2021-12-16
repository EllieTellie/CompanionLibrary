using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Companion.Data.Utils
{
	/// <summary>
	/// Speed up xml serialization by creating the constructors manually.
	/// </summary>
	public class XmlDataFactory
	{
		public delegate XmlData XmlConstructor(XmlNode node);

		public Dictionary<Type, XmlConstructor> constructors = new Dictionary<Type, XmlConstructor>();

		protected XmlDataFactory()
		{
			// could be generated automatically
			constructors.Add(typeof(Catalogue), (XmlNode node) => { return new Catalogue(node); });
			constructors.Add(typeof(CatalogueLink), (XmlNode node) => { return new CatalogueLink(node); });
			constructors.Add(typeof(CategoryEntry), (XmlNode node) => { return new CategoryEntry(node); });
			constructors.Add(typeof(CategoryLink), (XmlNode node) => { return new CategoryLink(node); });
			constructors.Add(typeof(CharacteristicType), (XmlNode node) => { return new CharacteristicType(node); });
			constructors.Add(typeof(Condition), (XmlNode node) => { return new Condition(node); });
			constructors.Add(typeof(ConditionGroup), (XmlNode node) => { return new ConditionGroup(node); });
			constructors.Add(typeof(Constraint), (XmlNode node) => { return new Constraint(node); });
			constructors.Add(typeof(CostType), (XmlNode node) => { return new CostType(node); });
			constructors.Add(typeof(EntryLink), (XmlNode node) => { return new EntryLink(node); });
			constructors.Add(typeof(ForceEntry), (XmlNode node) => { return new ForceEntry(node); });
			constructors.Add(typeof(GameSystem), (XmlNode node) => { return new GameSystem(node); });
			constructors.Add(typeof(InfoGroup), (XmlNode node) => { return new InfoGroup(node); });
			constructors.Add(typeof(InfoLink), (XmlNode node) => { return new InfoLink(node); });
			constructors.Add(typeof(Modifier), (XmlNode node) => { return new Modifier(node); });
			constructors.Add(typeof(ModifierGroup), (XmlNode node) => { return new ModifierGroup(node); });
			constructors.Add(typeof(ProfileType), (XmlNode node) => { return new ProfileType(node); });
			constructors.Add(typeof(Publication), (XmlNode node) => { return new Publication(node); });
			constructors.Add(typeof(Repeat), (XmlNode node) => { return new Repeat(node); });
			constructors.Add(typeof(SelectionEntry), (XmlNode node) => { return new SelectionEntry(node); });
			constructors.Add(typeof(SelectionEntryGroup), (XmlNode node) => { return new SelectionEntryGroup(node); });
			constructors.Add(typeof(Category), (XmlNode node) => { return new Category(node); });
			constructors.Add(typeof(Characteristic), (XmlNode node) => { return new Characteristic(node); });
			constructors.Add(typeof(Cost), (XmlNode node) => { return new Cost(node); });
			constructors.Add(typeof(CostLimit), (XmlNode node) => { return new CostLimit(node); });
			constructors.Add(typeof(Force), (XmlNode node) => { return new Force(node); });
			constructors.Add(typeof(Profile), (XmlNode node) => { return new Profile(node); });
			constructors.Add(typeof(Roster), (XmlNode node) => { return new Roster(node); });
			constructors.Add(typeof(Rule), (XmlNode node) => { return new Rule(node); });
			constructors.Add(typeof(Selection), (XmlNode node) => { return new Selection(node); });
			constructors.Add(typeof(DataIndex), (XmlNode node) => { return new DataIndex(node); });
			constructors.Add(typeof(DataIndexEntry), (XmlNode node) => { return new DataIndexEntry(node); });
		}

		protected static XmlDataFactory instance;
		public static XmlDataFactory Instance
		{
			get
			{
				if (instance == null)
					instance = new XmlDataFactory();

				return instance;
			}
		}

		/// <summary>
		/// Create a new xml data class of the type specified. This will attempt to use an existing constructor if defined or use reflection last.
		/// </summary>
		/// <typeparam name="T">Type to create</typeparam>
		/// <param name="node">Node to pass into constructor</param>
		/// <returns>New XmlData class</returns>
		public T Create<T>(XmlNode node) where T : XmlData
		{
			Type t = typeof(T);

			if (constructors.TryGetValue(t, out XmlConstructor constructor))
			{
				return (T)constructor(node);
			}
			else
			{
				return (T)Activator.CreateInstance(typeof(T), new object[] { node });
			}
		}
	}
}
