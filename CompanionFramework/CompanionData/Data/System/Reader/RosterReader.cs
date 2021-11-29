using Companion.Data;
using Companion.Data.Utils;
using CompanionFramework.Core.Log;
using CompanionFramework.Core.Threading.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Companion.Data
{
	public class RosterReader
	{
		protected readonly string text;
		protected RosterTokenReader tokenReader;

		/// <summary>
		/// Dispatched on the main unity thread. Source is roster. EventArgs is null.
		/// </summary>
		public event EventHandler OnRosterParsed;

		public RosterReader(string text)
		{
			this.text = text;
			tokenReader = new RosterTokenReader(text);
		}

		public GameSystem DetectGameSystem()
		{
			List<GameSystem> gameSystems = SystemManager.Instance.GetGameSystems();

			List<RosterToken> tokens = tokenReader.ReadAllTokens();

			foreach (GameSystem gameSystem in gameSystems)
			{
				foreach (RosterToken token in tokens)
				{
					if (token.tokenType == RosterTokenType.Force)
					{
						ForceToken forceToken = ForceToken.ParseToken(token);

						if (forceToken != null && gameSystem.SearchByName(forceToken.name, true) != null)
						{
							return gameSystem;
						}
					}
				}
			}

			return null;
		}

		public void ParseAsync()
		{
			ThreadPool.QueueUserWorkItem(ParseAsync);
		}

		private void ParseAsync(object state)
		{
			Stopwatch stopwatch = new Stopwatch();

			stopwatch.Start();
			Parse();
			stopwatch.Stop();

			FrameworkLogger.Message("Roster Parsed: " + stopwatch.ElapsedMilliseconds);
		}

		public Roster Parse()
		{
			RosterToken rosterToken;

			// could detect game system here
			//GameSystem gameSystem = SystemManager.Instance.GetGameSystemByName("Warhammer 40,000 9th Edition");
			GameSystem gameSystem = DetectGameSystem();

			if (gameSystem == null) // if no game systems are loaded etc
				return null;

			SystemManager manager = SystemManager.Instance;

			Roster roster = new Roster(null);
			roster.gameSystemId = gameSystem.id;
			roster.gameSystemName = gameSystem.name;
			roster.gameSystemRevision = gameSystem.revision;
			roster.id = Guid.NewGuid().ToString();

			// the active force we are reading into
			Force activeForce = null;

			// the active selection we are reading into
			Stack<Selection> selectionStack = new Stack<Selection>();

			GameSystemGroup gameSystemGroup = new GameSystemGroup(gameSystem);

			while ((rosterToken = tokenReader.ReadRosterToken()) != null)
			{
				Console.WriteLine("Type: " + rosterToken.tokenType + " Content: " + rosterToken.content);

				if (rosterToken.tokenType == RosterTokenType.Force)
				{
					ForceToken forceToken = ForceToken.ParseToken(rosterToken);

					if (forceToken != null)
					{
						Force force = new Force(null);

						// search catalogue in system manager
						Catalogue catalogue = manager.SearchByName<Catalogue>(gameSystem, forceToken.faction);
						if (catalogue != null)
						{
							force.catalogueId = catalogue.id;
							force.catalogueName = catalogue.name;
							force.catalogueRevision = catalogue.revision;
						}

						// store for speed
						gameSystemGroup.AddCatalogue(catalogue);

						// we can search for the force in game system only
						gameSystem.SearchByName<EntryLink>(forceToken.name);
						ForceEntry forceEntry = gameSystem.SearchByName<ForceEntry>(forceToken.name, true);
						if (forceEntry == null)
						{
							// probably should be the default anyway
							forceEntry = gameSystem.SearchByName<ForceEntry>(forceToken.name + " " + forceToken.cost);
						}

						if (forceEntry != null)
						{
							force.entryId = forceEntry.id;
							force.name = forceEntry.name;
						}

						roster.AddForce(force);

						activeForce = force;
						selectionStack.Clear();
					}
				}
				else if (rosterToken.tokenType == RosterTokenType.Selection)
				{
					int tokenIndex = ReaderUtils.GetSelectionIndex(rosterToken.content);

					SelectionToken selectionToken = SelectionToken.ParseToken(rosterToken);

					if (selectionToken != null)
					{
						string selectionName;
						int selectionNumber = ReaderUtils.GetSelectionNumberFromName(selectionToken.name, out selectionName);

						Console.WriteLine("Found selection: " + selectionToken.name);

						Selection parent = selectionStack.Count > 0 ? selectionStack.Peek() : null;
						bool hasParent = parent != null && tokenIndex > 0; // if token index is > 0 and we have a parent then we know we are a child

						SelectionResult selectionResult = SearchSelectionEntry(gameSystemGroup, selectionName, hasParent, true);

						if (selectionResult != null && selectionResult.selectionEntry != null)
						{
							// maybe selection name needs to be the full name including the 9x Dire Avengers (depending).
							Selection selection = CreateSelection(gameSystemGroup, selectionResult, selectionName, selectionNumber);

							// handle sub selection
							if (selectionToken.entries != null)
							{
								foreach (string entry in selectionToken.entries)
								{
									int subSelectionNumber = ReaderUtils.GetSelectionNumberFromName(entry, out string subSelectionName);

									// use the root of this selection entry to search
									SelectionEntry rootSelectionEntry = selectionResult.selectionEntry.GetRootSelection();
									
									// try without contains first
									// ignore root selection entry because they might have the same name
									SelectionResult subentry = rootSelectionEntry.GetSelectionEntryByName(gameSystemGroup, subSelectionName, false, rootSelectionEntry);
									if (subentry == null)
										subentry = rootSelectionEntry.GetSelectionEntryByName(gameSystemGroup, subSelectionName, true, rootSelectionEntry); // has to be contains because sometimes there's a space in the name at the end?

									if (subentry != null)
									{
										Selection subselection = CreateSelection(gameSystemGroup, subentry, subSelectionName, subSelectionNumber);
										selection.AddSelection(subselection, true);
									}
									else
									{
										//List<SelectionEntry> selections = selectionResult.selectionEntry.GetSelectionEntries(gameSystem);

										//SelectionEntry selectionEntry = selections.GetContainingName(subSelectionName);
										//if (selectionEntry != null)
										//{
										//	Selection subselection = CreateSelection(gameSystemGroup, new SelectionResult(selectionEntry), subSelectionName, 1);
										//	selection.AddSelection(subselection, true);
										//}
										//else
										//{
											Console.WriteLine("Unable to find: " + subSelectionName);
										//}
									}
								}
							}

							// add the selection to the appropriate place
							if (parent != null)
							{
								int selectionIndex = parent.GetIndex();

								if (tokenIndex == 0)
								{
									// reset and push current
									selectionStack.Clear();
									selectionStack.Push(selection); // always push selection every time

									// add to force
									activeForce.AddSelection(selection);
								}
								else if (tokenIndex < selectionIndex)
								{
									int pops = selectionIndex - tokenIndex;
									for (int i = 0; i < pops; i++)
									{
										selectionIndex--;
										selectionStack.Pop();
									}

									// add to this one
									parent = selectionStack.Peek();
									parent.AddSelection(selection, true);
								}
								else if (tokenIndex > selectionIndex + 1) // if it is more than 2 we need to push to the stack, hence the +1
								{
									// add as child to the last selection
									parent = parent.GetLastSelection();
									parent.AddSelection(selection, true);

									// push the parent as the new parent
									selectionStack.Push(parent);
								}
								else // if the token index is 1 more this is just a child
								{
									// add as child
									parent.AddSelection(selection, true);
								}
							}
							else
							{
								selectionStack.Push(selection);

								// add to force
								activeForce.AddSelection(selection);
							}
						}
						else
						{
							Console.WriteLine("Unable to find: " + selectionToken.name);
						}
					}
				}
			}

			CalculateCosts(gameSystem, roster);

			MessageQueue.Invoke(OnRosterParsed, roster);

			return roster;
		}

		private void ApplyModifiers(GameSystem gameSystem, Roster roster, Selection selection, SelectionEntry selectionEntry)
		{
			foreach (Modifier modifier in selectionEntry.modifiers)
			{
				foreach (Condition condition in modifier.conditions)
				{
					if (condition.IsConditionMet(gameSystem, roster, selection))
					{
						Console.WriteLine("Met condition!");

						//modifier.Apply(gameSystem, selection, selectionEntry);
					}
				}
			}
		}

		private void CalculateCosts(GameSystem gameSystem, Roster roster)
		{
			List<Cost> newCosts = new List<Cost>();

			List<Cost> costs = GetRosterSelectionCosts(roster);

			foreach (CostType costType in gameSystem.costTypes)
			{
				Cost cost = new Cost(null);
				cost.name = costType.name;
				cost.value = 0.0;

				foreach (Cost selectionCost in costs)
				{
					if (selectionCost.value > 0 && selectionCost.name == cost.name)
						cost.value += selectionCost.value;
				}

				newCosts.Add(cost);
			}

			// add later so we don't recursively search ourselves
			foreach (Cost cost in newCosts)
			{
				roster.costs.Add(cost);
			}
		}

		private List<Cost> GetRosterSelectionCosts(Roster roster)
		{
			List<Cost> costs = new List<Cost>();

			costs.AddRange(roster.costs);

			foreach (Force force in roster.forces)
			{
				foreach (Selection selection in force.selections)
				{
					costs.AddRange(selection.costs);
				}
			}

			return costs;
		}

		private SelectionResult SearchSelectionEntry(GameSystemGroup gameSystemGroup, string selectionName, bool hasParent, bool recursive)
		{
			// we can do a faster lookup by using the entry links
			if (!hasParent)
			{
				List<Catalogue> catalogues = gameSystemGroup.GetCatalogues();
				foreach (Catalogue catalogue in catalogues)
				{
					// go fast
					foreach (EntryLink entryLink in catalogue.entryLinks)
					{
						XmlData target = entryLink.GetTarget(gameSystemGroup);
						if (target != null)
						{
							SelectionEntry selectionEntry = target.SearchByName<SelectionEntry>(selectionName, false);
							if (selectionEntry != null && selectionEntry.GetParent() == null)
								return new SelectionResult(selectionEntry, entryLink);
						}
					}
				}
			}

			// this could potentially be faster by modifying GetSelectionEntryByName on GameSystemGroup to only search one level
			SelectionResult result = gameSystemGroup.GetSelectionEntryByName(selectionName, false); // do one without contains first
			if (result == null)
				result = gameSystemGroup.GetSelectionEntryByName(selectionName, true); // do contains afterwards

			return result;

			//// search for all selection entries
			//// we can improve this by searching through entry links only if we have no parents
			//List<SelectionEntry> selectionEntries = gameSystemGroup.SearchAllByName<SelectionEntry>(selectionName, recursive);

			//// we cannot return the first because we may find the same models in different units
			//foreach (SelectionEntry entry in selectionEntries)
			//{
			//	if (hasParent)
			//	{
			//		return new SelectionResult(entry); // just return first
			//	}
			//	else if (entry.GetParent() == null) // skip any that have parents
			//	{
			//		return new SelectionResult(entry);
			//	}
			//}

			//if (selectionEntries.Count > 0)
			//	return new SelectionResult(selectionEntries[0]);
			//return null;
		}

		private Selection CreateSelection(GameSystemGroup gameSystemGroup, SelectionResult selectionResult, string selectionName, int selectionNumber)
		{
			SelectionEntry selectionEntry = selectionResult.selectionEntry;

			Selection selection = new Selection(null);
			selection.name = selectionName;
			selection.number = selectionNumber;
			selection.entryId = selectionEntry.id; // this needs to be a chain
			selection.type = selectionEntry.type;

			selection.profiles.AddRange(selectionEntry.profiles);
			selection.rules.AddRange(selectionEntry.rules);
			selection.costs.AddRange(selectionEntry.costs.Multiply(selection.number));

			foreach (InfoLink infoLink in selectionEntry.infoLinks)
			{
				XmlData target = infoLink.GetTarget(gameSystemGroup);

				if (target is Rule rule)
				{
					selection.rules.Add(rule);
				}
				else if (target is Profile profile)
				{
					selection.profiles.Add(profile);
				}
			}

			foreach (CategoryLink categoryLink in selectionEntry.categoryLinks)
			{
				//if (categoryLink.hidden)
				//	continue;

				XmlData target = categoryLink.GetTarget(gameSystemGroup);

				if (target is CategoryEntry categoryEntry)
				{
					Category category = new Category(null);
					category.name = categoryEntry.name;
					category.id = categoryEntry.id;
					category.primary = categoryLink.primary;

					selection.categories.Add(category);
				}
			}

			if (selectionResult.entryLink != null && selectionResult.entryLink.HasCosts())
			{
				foreach (Cost cost in selectionResult.entryLink.costs)
				{
					// we can just use the reference as we should have created new costs in multiply above
					Cost selectionCost = selection.costs.GetByName(cost.name);
					if (selectionCost != null)
						selectionCost.value += cost.value;
				}
			}

			return selection;
		}

		private Selection GetParent(Selection activeSelection, int tokenIndex)
		{
			if (activeSelection != null)
			{
				int selectionIndex = activeSelection.GetIndex();

				if (tokenIndex == 0)
				{
					// add it to the root
					return null;
				}
				else if (tokenIndex > selectionIndex)
				{
					// add it as child
					Selection lastSelection = activeSelection.GetLastSelection();
					return lastSelection ?? activeSelection;
				}
				else
				{
					// add it as sibling
					return activeSelection;
				}
			}
			else
			{
				// add it to root
				return null;
			}
		}
	}
}
