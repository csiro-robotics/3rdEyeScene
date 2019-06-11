# 3esCore build instructions

This page describes how to build the 3esCore solution. This solution provides core 3es functionality for .Net projects including for the Unity 3D 3rdEyeScene client.

# Pre-requisites

- A .Net build system, recommended:
  - [Visual Studio for Windows or Mac](https://visualstudio.microsoft.com/) (not Visual Studio Code) compatible with the .Net 4.61 framework
  - [dotnet core 2.2+](https://dotnet.microsoft.com/download)
- [Unity3D](https://unity3d.com/) editor installation.
  - A UnityHub installation is recommended.
    - [Experimental Linux Build](https://forum.unity.com/threads/unity-hub-v-1-3-2-is-now-available.594139/)
  - Minimum version: `2018.4`
- [Python 3](https://www.python.org/) recommended for `unity-marshal.py`

# Build instructions

- Ensure the path to your Unity Engine DLL is set in the environmeet variable `UNITY_DLL_PATH` and set as shown below
  - Windows: typically `C:\Program Files\Unity\Hub\Editor\<Unity-version>\Editor\Data\Managed`
  - MacOS: typically `'/<install-path>/Unity.app/Contents/Managed`
  - Linux: `<Unity-hub-path>/Hub/Editor/<Unity-version>/Editor/Data/Managed`
  - *Note:* Do not set the path to `<Unity-version>\Editor\Data\Managed\UnityEngine` (note the last directory). The UnityEngine.dll is different in that DLL to the one in the parent directory. Using the wrong path will result in unresolved references from `UnityEngine.CoreModule.dll`, `UnityEngine.IMGUIModule.dll` and `UnityEngine.TextRenderingModule.dll`.
- Build using either `dotnet` (recommended) or using Visual Studio (instructions below)
- Marshal the 3esRuntime DLLs for the 3rdEyeScene Unity 3D project.
  - Building with `dotnet`, you can use `unity-marshal.py`
    - Open a command prompt and change into the directory containing `unity-marshal.py`
    - Run the marshaling script (no arguments required)
      - `python unity-marshal.py`
  - Building with Visual Studio
    - Navigate to `3esRuntime/bin/Release`
    - Find the 3esRuntime.DLL. This will be under the `net461` directory for Visual Studio builds.
    - Copy the 3esRuntime.DLL, all other DLLs in the same folder and all .PDB files with the of the same name into the 3rdEyeScene Unity project, under `Assets/plugins`.

## Visual Studio

- Open `3esCore.sln`
- Select the build configuration (e.g., Release, Any CPU)
- Select Build All in the menus

## Visual Studio Code

A tasks.json file has been provided for building in Visual Studio Code using the `dotnet core`. This includes the following tasks:

- `build Debug` : builds debug assemblies
  - This is the default build task executed by the VSCode command: `Tasks: Run Build Task`
- `build Release` : build release assemblies
- `publish Debug` : generate debug mode executables using `netcoreapp2.2` target.
  - Output directory: `${workspaceFolder}/build/Debug`
- `publish Release` : generate release mode executables using `netcoreapp2.2` target.
  - Output directory: `${workspaceFolder}/build/Release`
- `marshal Release` : build and marshal the release assemblies for use by the 3rdEyeScene Unity project. Requires a `python` available on the path.
- `marshal Debug` : build and marshal the debug assemblies for use by the 3rdEyeScene Unity project. Requires a `python` available on the path.

## Command line dotnet build

- Open a command prompt
- Ensure the `dotnet` command is available on the path by running `dotnet --info`
- Navigate into the folder containing `3esCore.sln`
- Build for development including Unity development
  - `dotnet build -c Debug`
  - `dotnet build -c Release`
- Building for running utilities (3esinfo, 3esrec)
  - Linux and MacOS (bash shell):
    - `dotnet publish -f netcoreapp2.2 -c Release -o $PWD/build/Release`
    - `dotnet publish -f netcoreapp2.2 -c Debug -o $PWD/build/Debug`
  - Windows:
    - `dotnet publish -f netcoreapp2.2 -c Release -o %CD%\build\Release`
    - `dotnet publish -f netcoreapp2.2 -c Debug -o %CD%\build\build\Debug`

Note: If you receive an error message like the one quoted below (`MSB4126`), then you may need to change the environment variable `PLATFORM`. Either clear this variable or set it explicitly to `"Any CPU"`.

```
...\3esCore.sln.metaproj : error MSB4126: The specified solution configuration "Debug|x64" is invalid. Please specify a valid solution configuration using the Configuration and Platform properties (e.g. MSBuild.exe Solution.sln /p:Configuration=Debug /p:Platform="Any CPU") or leave those properties blank to use the default solution configuration. [D:\Users\Kazys\source\3rdEyeScene\3esCore\3esCore.sln]
    0 Warning(s)
    1 Error(s)
```

### Running utilities ###

The utility programs may be executed using the `dotnet` command followed by the path of the utility DLL. For example, use `dotnet <path>/3esrec.dll` to run 3esrec.

It is possible to build executable programs using the publish commands listed above and specifying the runtime. The examples below show the recommended runtime specifications:

- Windows:
  - `dotnet publish -f netcoreapp2.2 -c Release -o %CD%\build\Release -r win10-x64`
- Linux (bash shell):
  - `dotnet publish -f netcoreapp2.2 -c Release -o $PWD/build/Release -r linux-x64`
- MacOS (bash shell):
  - `dotnet publish -f netcoreapp2.2 -c Release -o $PWD/build/Release -r osx-x64`

# Notes on build setup

This section highlights some issues encountered in setting up the build for cross platform development and explains why things are as they are.

- Build setup is targeted at supporting `dotnet` pipeline over Visual Studio. This is for cross platform compatibility.
- The `.csproj` files support either `dotnet` or Visual Studio builds by adding conditions to `TargetFramework` tags. For Visual Studio, only the `net461` target is selected, for `dotnet` both `netstandard2.0` and `netcoreapp2.2` targets are enabled.
- Visual Studio only uses `net461` to avoid issues where `dotnet` is not installed.
- `dotnet` uses both `netstandard2.0` and `netcoreapp2.2` to support Unity import and command line execution.
  - `netstandard2.0` is supported by Unity, while `netcoreapp2.2` is not currently compatible with Unity.
    - Only `netstandard2.0` DLLs should be marshaled for Unity
    - Tested with `Unity 2018.4.1f1`
  - `netcoreapp2.2` is required for `dotnet publish` commands in order to build runnable utilities including executable files.
