using CompanionFramework.Core.Log;
using CompanionFramework.Core.Threading.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Companion.Data
{
    public abstract class AbstractRosterReader
	{
		protected readonly string text;
		protected RosterTokenReader tokenReader;

		/// <summary>
		/// Dispatched on the main unity thread. Source is roster. EventArgs is null.
		/// </summary>
		public event EventHandler OnRosterParsed;

		public AbstractRosterReader(string text)
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

		/// <summary>
		/// Attempt to parse the roster and detect the selections. If no <see cref="GameSystem"/> is provided we attempt to detect the game system.
		/// </summary>
		/// <param name="gameSystem">Game system, optional</param>
		/// <returns>Roster or null if unable to parse</returns>
		public abstract Roster Parse(GameSystem gameSystem = null);

		protected void FireRosterParsedEvent(Roster roster)
		{
			MessageHandler.InvokeSafe(OnRosterParsed, roster, null);
		}

        protected string DetermineRosterName(GameSystemGroup gameSystemGroup)
        {
			List<Catalogue> catalogues = gameSystemGroup.GetCatalogues();

			if (catalogues.Count > 0)
			{
				Catalogue catalogue = catalogues[0];

				if (catalogue.publications.Count > 0)
                {
					return "Imported - " + catalogue.publications[0].name;
                }

				return "Imported - " + catalogues[0].name;
			}
			else
			{
				return "Imported Roster";
			}
        }

		protected void ApplyModifiers(GameSystem gameSystem, Roster roster, Selection selection, SelectionEntry selectionEntry)
		{
			foreach (Modifier modifier in selectionEntry.modifiers)
			{
				foreach (Condition condition in modifier.conditions)
				{
					if (condition.IsConditionMet(gameSystem, roster, selection))
					{
						FrameworkLogger.Debug("Met condition!");

						//modifier.Apply(gameSystem, selection, selectionEntry);
					}
				}
			}
		}

		protected void CalculateCosts(GameSystem gameSystem, Roster roster)
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

		protected List<Cost> GetRosterSelectionCosts(Roster roster)
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

		protected SelectionResult SearchSelectionEntry(GameSystemGroup gameSystemGroup, string selectionName, bool hasParent)
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
		}

		protected Selection CreateSelection(GameSystemGroup gameSystemGroup, SelectionResult selectionResult, string selectionName, int selectionNumber)
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

		protected Selection GetParent(Selection activeSelection, int tokenIndex)
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
