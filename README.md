# CompanionLibrary
Simple library to read battle scribe data files in C#
This is the library I wrote for using in my own Unity project, making it available in case people are interested in using it.
Available for use in non-commercial projects, please read the license for more information.

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
SystemManager.LoadGameSystemAsync("path_to_folder")
```

Retrieving game systems from SystemManager once loaded:
```
GameSystem gameSystem = SystemManager.Instance.GetGameSystemByName("Warhammer 40,000 9th Edition");
```

Converting a copy paste roster to classes (slow operation because there's a lot of searching involved)
```
SystemManager.Instance.LoadGameSystems("C:\Users\YourUser\BattleScribe\data\Warhammer 40,000 9th Edition\");
string rosterText = FileUtils.ReadTextFile(inputPath);

RosterReader reader = new RosterReader(rosterText);
Roster roster = reader.Parse();
if (roster != null)
{
  Console.WriteLine("Points: " + roster.costs.GetByName("pts").value);
}
```

You will need to call dispatch on the message queue on your main thread if you use the async/event functionality.
This is primarily so Unity calls are guaranteed to be on the Unity main thread. If you don't use Unity you can find your main thread and execute it on its tick/update loop.
This step may become optional in the future.
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
UpdateStateData state = new UpdateStateData();
RetrieveGameSystemIndexProcess process = new RetrieveGameSystemIndexProcess("<your_repo_url>", false);
process.LoadingComplete += (object result) =>
{
  RepositoryIndex repositoryIndex = (RepositoryIndex)result;

  Repository repo = repositoryIndex.GetRepositoryByName("<repo_name>");

  RetrieveRepositoryIndexProcess indexProcess = new RetrieveRepositoryIndexProcess(repo.indexUrl, false);
  indexProcess.LoadingComplete += (object r) =>
  {
    DataIndex dataIndex = (DataIndex)r;

    UpdateGameSystemProcess updateProcess = new UpdateGameSystemProcess(repo, dataIndex, @"%USERPROFILE%\BattleScribe\data\<save_data_here>");
    updateProcess.Execute(state);
  };
  indexProcess.Execute(state);
};
process.Execute(state);
```

## Building
To help deploying the library files in the .csproj files there is a post build event:
"$(SolutionDir)DeployHelper.exe" -path="C:\\Projects\\" -lib="$(TargetDir)$(ProjectName).dll" -deploy="true" -debug="true" -tools="false"
This has a hard coded path so if it fails to build you can remove it or change it to an applicable path.
