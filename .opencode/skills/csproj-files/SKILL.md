---
name: csproj-files
description: "How to change a .csproj here — a data file copied to the build output, a project reference, a package, a property. Read before editing any .csproj or Directory.Build.props, and when a file is missing from bin/ at runtime. Not for C# code or EF migrations."
---

# Project files

Reference: `GreenGenius.App/GreenGenius.App.csproj`. Shared settings live once in `Directory.Build.props` at the root: `TargetFramework`, `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`, `EnforceCodeStyleInBuild`. Never repeat them in a `.csproj`, never turn one off.

## Which change

| You need | Write |
|---|---|
| A data file read at runtime, in a web project (`Sdk="Microsoft.NET.Sdk.Web"`): `.json` files are already `Content` | `<Content Update="Data\Plants\**" CopyToOutputDirectory="PreserveNewest" />` |
| The same in a library or a test project | `<None Include="Data\**" CopyToOutputDirectory="PreserveNewest" />` |
| A file the tests or the app rewrite in `bin/`, reset at each build | `CopyToOutputDirectory="IfDifferent"` |
| Code of another project | `<ProjectReference Include="..\<Project>\<Project>.csproj" />`, only in the direction of the root `AGENTS.md` |
| A package | Stop and report: never add or upgrade a package yourself |
| A new `.cs` file | Nothing: the SDK includes every `.cs` of the folder |

- A folder needs the glob: `Data\Plants\**`. `Data\Plants` alone matches no file and nothing is copied.
- `PreserveNewest`, never `Always`: `Always` copies at every build.
- At runtime the file is under `AppContext.BaseDirectory`: `Path.Combine(AppContext.BaseDirectory, "Data", "Plants")`. Never a path from the repository root, never an absolute path.

## Steps

1. Edit the `.csproj`: one `ItemGroup` per kind of item, next to the existing one.
2. `dotnet build GreenGenius.slnx -v q --nologo 2>&1 | tail -20`, without warnings.
3. A copied file: check it arrived, `ls GreenGenius.App/bin/Debug/net10.0/Data/Plants`.
4. A test that reads the file reads it from its own output directory: the test project copies it too, or the test builds its data in a temporary folder.

## Never

- List `.cs` files by hand, or restate an SDK default (`OutputType` of a library, `LangVersion`).
- An absolute path, or a path outside the repository.
- `NoWarn`, `TreatWarningsAsErrors=false` or a `#pragma` to get a green build.
- A `<Reference>` with a `HintPath`: a package is a `PackageReference`.
