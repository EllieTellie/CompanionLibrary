# CompanionLibrary
Simple library to read battle scribe data files in C#
This is the library I wrote for using in my own Unity project, making it available in case people are interested in using it.
Available for use in non-commercial projects, please read the license for more information.

## Code Examples Project
There is now a working console application included for examples. It's located in Examples/CodeExamples.sln. It's a Visual Studio solution targeting .NET 6.0 (made with Visual Studio 2022). It contains only a single class: [Example.cs](Examples/CodeExamples/CodeExamples/Example.cs).

## Usage
Loading Roster from path:
```
byte[] data = FileUtils.ReadFileSimple(rosterPath);
Roster roster = Roster.LoadRosterXml(data);
```

Loading Game System/Catalogue from path:
```
GameSystem gameSystem = GameSystem.LoadGameSystem("path_to_gstz_file");
Catalogue catalogue = Catalogue.LoadCatalogue("path_to_catz_file");
```

Loading GameSystems from path:
```
// Listen to the active game system changing
SystemManager systemManager = SystemManager.Instance;
systemManager.OnActiveGameSystemChanged += OnActiveGameSystemChanged; // callback for when it completed loading 

// Load it asynchronously
GameSystemLoading loadingState = SystemManager.LoadActiveGameSystemAsync("path_to_folder")
loading.OnProgressUpdate += UpdateProgressBar;
loading.OnLoadingCompleted += OnLoadingCompleted; // alternative way to detect if loading completed successfully
loading.OnLoadingFailed += OnLoadingFailed; // callback when loading failed
```

Retrieving game systems from SystemManager once loaded:
```
GameSystem gameSystem = SystemManager.Instance.GetGameSystemByName("Warhammer 40,000 9th Edition");
GameSystem gameSystem = SystemManager.Instance.GetGameSystemById("28ec-711c-d87f-3aeb");
GameSystem gameSystem = SystemManager.Instance.GetActiveGameSystem();
```

Converting a copy paste roster to classes (slow operation because there's a lot of searching involved)
```
SystemManager.Instance.LoadGameSystems("C:\Users\YourUser\BattleScribe\data\Warhammer 40,000 9th Edition\");
string rosterText = FileUtils.ReadTextFile(inputPath);

RosterReader reader = new RosterReader(rosterText);
Roster roster = reader.Parse();
if (roster != null) // if it is null it failed to parse
{
  Console.WriteLine("Points: " + roster.costs.GetByName("pts").value);
}
```

You may need to call dispatch on the message queue on your main thread if you use the async/event functionality.
This is primarily so Unity calls are guaranteed to be on the Unity main thread. If you don't use Unity you can find your main thread and execute it on its tick/update loop.
If the message handler is not created and dispatch is never called any events will happen on the thread they were called on.
```
public void Update()
{
  MessageQueue.Get().Dispatch();
}

```
If you don't need it to run on the main thread you can use a timer to call MessageQueue.Get().Dispatch()

## Updating
You can now update from the online repositories. Just replace <your_repo_url>, <repo_name> and <save_data_here>.
```
RepositoryData repositoryData = new RepositoryData();

RetrieveRepositoryIndexProcess process = new RetrieveRepositoryIndexProcess(<your_repo_url>, false); // for example "https://battlescribedata.appspot.com/repos"
process.LoadingComplete += (object result) =>
{
  RepositoryIndex repositoryIndex = (RepositoryIndex)result;

  Repository repo = repositoryIndex.GetRepositoryByName(<repo_name>); // for example wh40k

  RetrieveDataIndexProcess indexProcess = new RetrieveDataIndexProcess(repo, false);
  indexProcess.LoadingComplete += (object r) =>
  {
    GameSystemData data = (GameSystemData)r;

    UpdateGameSystemProcess updateProcess = new UpdateGameSystemProcess(repo, data.dataIndex, <save_data_here>, false); // for example C:\Users\<your username>\BattleScribe\data\wh40k
    updateProcess.LoadingAborted += (UpdateError error, string message) => { Console.WriteLine("Aborted"); };
    updateProcess.LoadingComplete += (object r2) => { Console.WriteLine("Update Completed"); };
    updateProcess.Execute(repositoryData);
  };
  indexProcess.Execute(repositoryData);
};
process.Execute(repositoryData);
```

You can also employ the UpdateManager to update for you.
```
UpdateManager updateManager = new UpdateManager();

updateManager.OnRepositoryIndexFailed += OnFailed;
updateManager.OnDataIndexFailed += OnFailed;
updateManager.OnRepositoryIndexFailed += OnFailed;

updateManager.OnRepositoryIndexAdded += (object sender, EventArgs e) =>
{
  RepositoryIndexSuccessEventArgs successEventArgs = (RepositoryIndexSuccessEventArgs)e;
  updateManager.RetrieveRepositoryDataIndex(successEventArgs.repositoryData, <repo_name>, false); // for example wh40k
};

updateManager.OnDataIndexAdded += (object sender, EventArgs e) =>
{
  DataIndexSuccessEventArgs successEventArgs = (DataIndexSuccessEventArgs)e;
  updateManager.UpdateFromRepository(successEventArgs.repositoryData, successEventArgs.repository, <save_data_here>, false); // for example C:\Users\<your username>\BattleScribe\data\wh40k
};

updateManager.OnUpdateSucceeded += (object sender, EventArgs e) =>
{
  Console.WriteLine("Success");
};

updateManager.RetrieveRepositoryIndex(<your_repo_url>, false); // for example "https://battlescribedata.appspot.com/repos"
```

## Building
To help deploying the library files in the .csproj files there is a post build event:
```
"$(SolutionDir)DeployHelper.exe" -path="C:\\Projects\\" -lib="$(TargetDir)$(ProjectName).dll" -deploy="true" -debug="true" -tools="false"
```
This has a hard coded path so if it fails to build you can remove it or change it to an applicable path.
