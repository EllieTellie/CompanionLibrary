﻿using Companion.Data.Utils;
using CompanionFramework.Core.Log;
using System;
using System.Collections.Generic;

namespace Companion.Data
{
    public class RosterReader : AbstractRosterReader
	{
		public RosterReader(string text) : base(text)
		{
		}

		/// <summary>
		/// Attempt to parse the roster and detect the selections. If no <see cref="GameSystem"/> is provided we attempt to detect the game system.
		/// </summary>
		/// <param name="gameSystem">Game system, optional</param>
		/// <returns>Roster or null if unable to parse</returns>
		public override Roster Parse(GameSystem gameSystem = null)
		{
			// detect game system here if not provided
			if (gameSystem == null)
				gameSystem = DetectGameSystem();

			if (gameSystem == null) // if no game systems are loaded etc
				return null;

			SystemManager manager = SystemManager.Instance;

			Roster roster = new Roster(null);
			roster.battleScribeVersion = gameSystem.battleScribeVersion;
			roster.gameSystemId = gameSystem.id;
			roster.gameSystemName = gameSystem.name;
			roster.gameSystemRevision = gameSystem.revision;
			roster.id = gameSystem.GenerateUniqueId();

			// the active force we are reading into
			Force activeForce = null;

			// the active selection we are reading into
			SelectionStack selectionStack = new SelectionStack();

			SelectionResult parentSelectionResult = null;

			GameSystemGroup gameSystemGroup = new GameSystemGroup(gameSystem);

			RosterToken rosterToken;
			while ((rosterToken = tokenReader.ReadRosterToken()) != null)
			{
				FrameworkLogger.Message("Type: " + rosterToken.tokenType + " Content: " + rosterToken.content);

				if (rosterToken.tokenType == RosterTokenType.Force)
				{
					ForceToken forceToken = ForceToken.ParseToken(rosterToken);

					if (forceToken != null)
					{
						Force force = new Force(null);
						force.id = gameSystem.GenerateUniqueId();

						// TODO: battle scribe takes rules from catalogue and info links and adds them to the force (where applicable)

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

						FrameworkLogger.Message("Found selection: " + selectionToken.name);

						// check if we have a parent
						bool hasParent = selectionStack.Peek() != null && tokenIndex > 0; // if token index is > 0 and we have a parent then we know we are a child

						// find the selection entry if possible
						SelectionResult selectionResult;
						if (hasParent && parentSelectionResult != null)
						{
							// we allow partial matches here because of cases like "*Custom Craftworld*" which are written out as "Custom Craftworld" for some reason
							selectionResult = parentSelectionResult.GetSelectionEntryByName(gameSystemGroup, selectionName, true, parentSelectionResult.selectionEntry);

							if (selectionResult != null)
							{
								FrameworkLogger.Message("Found selection entry in parent: " + selectionToken.name);
							}
							else
                            {
								// this should now almost never happen
								FrameworkLogger.Message("Did not find selection entry in parent: " + selectionToken.name);

								// fallback to the full search
								selectionResult = SearchSelectionEntry(gameSystemGroup, selectionName, hasParent);
							}
						}
						else
						{
							selectionResult = SearchSelectionEntry(gameSystemGroup, selectionName, hasParent);
						}

						if (selectionResult != null && selectionResult.selectionEntry != null)
						{
							// store parent selection entry
							if (!hasParent)
								parentSelectionResult = selectionResult;

							// maybe selection name needs to be the full name including the 9x Dire Avengers (depending).
							Selection selection = CreateSelection(gameSystemGroup, selectionResult, selectionName, selectionNumber);

							// handle sub selection
							if (selectionToken.entries != null)
							{
								foreach (string entry in selectionToken.entries)
								{
									int subSelectionNumber = ReaderUtils.GetSelectionNumberFromName(entry, out string subSelectionName);

									// search in the parent selection result first
									SelectionResult subentry = null;
									if (parentSelectionResult != null)
                                    {
										subentry = parentSelectionResult.GetSelectionEntryByName(gameSystemGroup, subSelectionName, true, parentSelectionResult.selectionEntry);
                                    }

									// try without contains first
									// ignore root selection entry because they might have the same name
									if (subentry == null) // this should now almost never happen
									{
										// use the root of this selection entry to search
										SelectionEntry rootSelectionEntry = selectionResult.selectionEntry.GetRootSelection();

										subentry = rootSelectionEntry.GetSelectionEntryByName(gameSystemGroup, subSelectionName, false, rootSelectionEntry);
										if (subentry == null)
											subentry = rootSelectionEntry.GetSelectionEntryByName(gameSystemGroup, subSelectionName, true, rootSelectionEntry); // has to be contains because sometimes there's a space in the name at the end?
									}

									if (subentry != null)
									{
										Selection subselection = CreateSelection(gameSystemGroup, subentry, subSelectionName, subSelectionNumber);
										selection.AddSelection(subselection, true);
									}
									else
									{
										FrameworkLogger.Message("Unable to find: " + subSelectionName);
									}
								}
							}

							// add the selection to the appropriate place
							if (tokenIndex == 0)
                            {
								// reset stack
								selectionStack.Clear();
                            }

							// grab parent from stack
							// this must be done before Push() otherwise we might pop ourselves
							Selection parent = selectionStack.PopToParent(tokenIndex);
							if (parent != null)
								parent.AddSelection(selection);
							else
								activeForce.AddSelection(selection);

							// always add every selection to the stack at the end
							selectionStack.Push(selection);
						}
                        else
                        {
                            FrameworkLogger.Message("Unable to find: " + selectionToken.name);
                        }
                    }
				}
			}

			CalculateCosts(gameSystem, roster);

			// just give it a random name
			roster.name = DetermineRosterName(gameSystemGroup);

			// fire event
			FireRosterParsedEvent(roster);

			return roster;
		}
	}
}
