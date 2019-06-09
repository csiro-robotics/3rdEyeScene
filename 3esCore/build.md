# 3esCore Build Instructions

This page describes how to build the 3esCore solution. This solution provides core 3es functionality for .Net projects including for the Unity 3D 3rdEyeScene client.

## Pre-requisites

- A .Net build system, recommended:
  - [Visual Studio for Windows or Mac](https://visualstudio.microsoft.com/) (not Visual Studio Code) compatible with the .Net 4.61 framework
  - [dotnet core 2.2+](https://dotnet.microsoft.com/download)
- [Unity3D](https://unity3d.com/) editor installation.
  - A UnityHub installation is recommended.
    - [Experimental Linux Build](https://forum.unity.com/threads/unity-hub-v-1-3-2-is-now-available.594139/)
  - At least version 2018.2.x is required to support dotnet core builds.
- [Python 3](https://www.python.org/) recommended for `unity-marshal.py`

## Build Instructions

- Ensure the path to your Unity Engine DLL is set in the environmnet variable `UNITY_DLL_PATH` and set as shown below
  - Windows: typically `C:\Program Files\Unity\Hub\Editor\<Unity-version>\Editor\Data\Managed\UnityEngine`
  - MacOS: typically `'/<install-path>/Unity.app/Contents/Managed`
  - Linux: `<Unity-hub-path>/Hub/Editor/<Unity-version>/Editor/Data/Managed`
- Build using either the Visual Studio or dotnet instructions below
- Marshal the 3esRuntime DLLs for the 3rdEyeScene Unity 3D project
  - Open a command prompt and change into the directory containing `unity-marshal.py`
  - Run the marshalling script (no arguments required)
    - `python unity-marshal.py`


Note: instead of using Python to run `unity-marshal.py`, you may instead copy 3esRuntime.DLL and support DLLs manually.

- Navigate to `3esRuntime/bin/Release`
- Find the 3esRuntime.DLL. This will be under the `netcoreapp2.2` directory for `dotnet` builds.
- Copy the 3esRuntime.DLL, all other DLLs in the same folder and all .PDB files with the of the same name into the 3rdEyeScene Unity project, under `Assets/plugins`.

### Visual Studio

- Open `3esCore.sln`
- Select the build configuration (e.g., Release, Any CPU)
- Select Build All in the menus

### dotnet build

- Open a command prompt
- Ensure the `dotnet` command is available on the path by running `dotnet --info`
- Navigate into the folder containing `3esCore.sln`
- Build for development including Unity development
  - `dotnet build -c Debug`
  - `dotnet build -c Release`
- Building for running utilities (3esinfo, 3esrec)
  - Linux:
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

### Running Utilities ###

The utility programs may be executed using the `dotnet` command followed by the path of the utility DLL. For example, use `dotnet <path>/3esrec.dll` to run 3esrec.

It is possible to build executable programs using the publish commands listed above and specifying the runtime. The examples below show the recommended runtime specifications:

- Windows:
  - `dotnet publish -f netcoreapp2.2 -c Release -o %CD%\build\Release -r win10-x64`
- Linux:
  - `dotnet publish -f netcoreapp2.2 -c Release -o $PWD/build/Release -r linux-x64`
- MacOS:
  - `dotnet publish -f netcoreapp2.2 -c Release -o $PWD/build/Release -r osx-x64`
