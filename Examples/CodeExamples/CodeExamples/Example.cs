using Companion.Data;
using Companion.Data.System.Update;
using CompanionFramework.Core.Log;
using CompanionFramework.IO.Utils;

namespace CodeExamples
{
    public enum ExampleMenu
    {
        Invalid = 0,
        ReadGameSystem = 1,
        ReadRoster = 2,
        ListRespositories = 3,
        UpdateRepository = 4,
        ConvertTextFileToRoster = 5
    }

    /// <summary>
    /// Using .NET 6.0 because it's the default
    /// </summary>
    internal class Example
    {
        static void Main()
        {
            ShowMenu();
        }

        private static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Library Code Examples Menu");
            Console.WriteLine("1. Read Game System from folder");
            Console.WriteLine("2. Read Roster from path");
            Console.WriteLine("3. List repositories");
            Console.WriteLine("4. Update repository");
            Console.WriteLine("5. Convert Text file to Roster");
            Console.WriteLine("Pick an option: [1-5]:");
            string? option = Console.ReadLine();

            ExampleMenu menuOption = ParseOption(option);
            if (!HandleMenuOption(menuOption))
            {
                // just go again
                ShowMenu();
            }
        }

        private static bool HandleMenuOption(ExampleMenu menuOption)
        {
            switch (menuOption)
            {
                case ExampleMenu.ReadGameSystem:
                    ExampleReadGameSystem();
                    break;
                case ExampleMenu.ReadRoster:
                    ExampleReadRoster();
                    break;
                case ExampleMenu.ListRespositories:
                    ExampleListRepositories();
                    break;
                case ExampleMenu.UpdateRepository:
                    ExampleUpdateRepository();
                    break;
                case ExampleMenu.ConvertTextFileToRoster:
                    ExampleConvertTextToRoster();
                    break;

                case ExampleMenu.Invalid:
                default:
                    Console.Error.WriteLine("Invalid menu option or not supported. Use 1 to 5 to pick an option");
                    return false;
            }

            return true;
        }

        private static void ExampleReadRoster()
        {
            // read some input
            Console.WriteLine("Please give a path to the roster:");
            string? textPath = Console.ReadLine();

            if (textPath == null || !File.Exists(textPath))
            {
                Abort("Invalid roster text path: " + textPath);
                return;
            }

            // action
            Roster roster = Roster.LoadRoster(textPath);
            if (roster == null)
            {
                Abort("Unable to parse roster: " + textPath);
                return;
            }

            DisplayRosterAndShowMenu(roster);
        }

        private static void ExampleListRepositories()
        {
            // read some input
            Console.WriteLine("Provide http/https repository (or press enter for battlescribe default repo):");
            string? url = Console.ReadLine();

            if (string.IsNullOrEmpty(url))
            {
                url = "https://battlescribedata.appspot.com/repos";
            }

            // get repository index
            UpdateManager.Instance.RetrieveRepositoryIndex(url, false); // async false because we are in a console

            // action
            RepositoryData repositoryData = UpdateManager.Instance.GetRepositoryDataByUrl(url);
            if (repositoryData == null)
            {
                Abort("Unable to find repository");
                return;
            }

            // we've got the index
            RepositoryIndex repositoryIndex = repositoryData.repositoryIndex;

            Console.WriteLine("Repositories found:");

            // list them all (spam incoming)
            foreach (Repository repository in repositoryIndex.repositories)
            {
                Console.WriteLine("> Repository: " + repository.description + ", " + repository.version);
            }

            Success("Repositories listed");
        }

        private static void ExampleUpdateRepository()
        {
            // read some input
            Console.WriteLine("Please give a folder where the game system is stored (.gstz):");
            string? folderPath = Console.ReadLine();

            if (folderPath == null || !Directory.Exists(folderPath))
            {
                Abort("Invalid game system folder: " + folderPath);
                return;
            }

            List<string> gameSystems = SystemManager.Instance.DetectGameSystems(folderPath);

            string? gameSystemPath = gameSystems != null && gameSystems.Count > 0 ? gameSystems[0] : null;

            if (gameSystemPath == null)
            {
                Abort("No game system found!");
                return;
            }

            DataIndexVersionInfo versionInfo = GameSystem.GetVersionInfo(gameSystemPath);
            if (versionInfo == null)
            {
                Abort("Invalid game system!");
                return;
            }

            // get repo index
            Console.WriteLine("Provide http/https repository (or press enter for battlescribe default repo):");
            string? url = Console.ReadLine();

            if (string.IsNullOrEmpty(url))
            {
                url = "https://battlescribedata.appspot.com/repos";
            }

            // get repository index
            UpdateManager.Instance.RetrieveRepositoryIndex(url, false); // async false because we are in a console

            RepositoryData repositoryData = UpdateManager.Instance.GetRepositoryDataByUrl(url);

            if (repositoryData == null)
            {
                Abort("Unable to retrieve repository index");
                return;
            }

            Console.WriteLine("Repositories found:");

            // list them all
            for (int i = 0; i < repositoryData.repositoryIndex.repositories.Count; i++)
            {
                Repository repository = repositoryData.repositoryIndex.repositories[i];
                Console.WriteLine("> " + i+ "  Repository: " + repository.description + ", " + repository.version);
            }

            Console.WriteLine("Select Repository for this system: [0/{0}]", repositoryData.repositoryIndex.repositories.Count - 1);
            string? repositoryIndex = Console.ReadLine();

            if (!int.TryParse(repositoryIndex, out int result))
            {
                Abort("Invalid repository index: " + repositoryIndex);
                return;
            }

            Repository selectedRepository = repositoryData.repositoryIndex.repositories[result];

            // get data index
            UpdateManager.Instance.RetrieveRepositoryDataIndex(repositoryData.repositoryIndex, selectedRepository, false);

            // updated
            UpdateManager.Instance.UpdateFromRepository(repositoryData, selectedRepository, folderPath, false);

            Success("Updated: " + selectedRepository.name);
        }

        private static void ExampleReadGameSystem()
        {
            // read some input
            Console.WriteLine("Please give a folder where the game system is stored (.gstz):");
            string? folderPath = Console.ReadLine();

            if (folderPath == null || !Directory.Exists(folderPath))
            {
                Abort("Invalid game system folder: " + folderPath);
                return;
            }

            Console.WriteLine("Loading game systems: " + folderPath);

            // action
            SystemManager.Instance.LoadGameSystems(folderPath);

            List<GameSystem> gameSystems = SystemManager.Instance.GetGameSystems();
            Success("Game Systems loaded: " + gameSystems.Count);
        }

        private static void ExampleConvertTextToRoster()
        {
            // read some input
            Console.WriteLine("Please give a path to a text file containing roster text:");
            string? textPath = Console.ReadLine();

            if (textPath == null || !File.Exists(textPath))
            {
                Abort("Invalid roster text path: " + textPath);
                return;
            }

            // convert a text file into a roster class
            // this must be exported from BattleScribe with the Facebook option
            string text = FileUtils.ReadTextFileSimple(textPath);
            if (text == null)
            {
                Abort("Unable to read text file: " + textPath);
                return;
            }

            // create a reader and parse it
            RosterReader reader = new RosterReader(text);
            Roster roster = reader.Parse();

            if (roster != null) // if it is null it failed to parse
            {
                DisplayRosterAndShowMenu(roster);
            }
            else
            {
                Abort("Unable to parse the roster, it might not be supported (Facebook export only supported)");
            }
        }

        private static void DisplayRosterAndShowMenu(Roster roster)
        {
            // just show something like points if it exists
            Cost pointsCost = roster.costs.GetByName("pts");
            if (pointsCost != null)
            {
                Success(roster.name + ", Points: " + pointsCost.value);
            }
            else
            {
                Success(roster.name);
            }
        }

        private static void Abort(string errorText)
        {
            Console.Error.WriteLine(errorText);

            // default to menu
            ShowMenu();
        }

        private static void Success(string successText)
        {
            Console.WriteLine(successText);

            // default to menu
            ShowMenu();
        }

        private static ExampleMenu ParseOption(string? option)
        {
            if (option == null)
                return ExampleMenu.Invalid;

            if (int.TryParse(option, out int result))
            {
                return (ExampleMenu)result;
            }
            else
            {
                return ExampleMenu.Invalid;
            }

        }
    }
}