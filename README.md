# Frontier-Command
A real-time strategy game where players build bases, gather resources, command armies, expand across a large strategic map, and defeat rival factions through economy, technology, and tactical combat.

## Run the CI checks locally

Install the .NET 8 SDK and Godot 4.7.2 .NET. Put the Godot executable on your PATH as `godot`.
Download [GdUnit4 v6.2.1](https://github.com/godot-gdunit-labs/gdUnit4/releases/tag/v6.2.1)
and copy its `addons/gdUnit4` directory into this project. The plugin and test reports are ignored by Git.

From the project directory, run:

```sh
dotnet restore "Frontier Command.csproj"
dotnet build "Frontier Command.csproj" --no-restore --warnaserror
dotnet format "Frontier Command.csproj" --no-restore --verify-no-changes --severity warn --exclude addons
godot --headless --editor --path . --import
godot --path . --rendering-method gl_compatibility --audio-driver Dummy -s res://addons/gdUnit4/bin/GdUnitCmdTool.gd -a res://__tests__ -c -rd reports
```

To fix formatting, run the `dotnet format` command without `--verify-no-changes`.
The lint check enforces C# formatting and analyzer diagnostics at warning severity or above.

Tests run in Godot, including the client tests that load scenes and simulate input.
On Linux without a display, prefix the test command with `xvfb-run --auto-servernum`.
Do not add `--headless` to the test command. HTML and JUnit reports appear under `reports/`.

## Require CI before merging

[CI](.github/workflows/ci.yml) runs on every branch push and on pull requests targeting `main`.
A push checks the latest commit in that push. A branch with an open pull request gets both workflow runs.
The workflow compiles the project, runs every test under `__tests__`, and checks lint.
The `Tests` job uploads its reports for 14 days, including reports from failed tests.

After the workflow has run on GitHub:

1. Open the repository's **Settings > Rules > Rulesets** and create an active branch ruleset targeting `main`.
2. Enable **Require a pull request before merging**.
3. Enable **Require status checks to pass** and add `Build`, `Tests`, and `Lint` from GitHub Actions.
4. Save the ruleset.

The workflow file does not configure these repository settings. Until the ruleset is active, failed CI does not block merges.
