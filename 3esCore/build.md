# 3esCore Build Instructions

This page describes how to build the 3esCore solution. This solution provides core 3es functionality for .Net projects including for the Unity 3D 3rdEyeScene client.

## Pre-requisites

- A .Net build system, recommended:
  - [Visual Studio for Windows or Mac](https://visualstudio.microsoft.com/) (not Visual Studio Code) compatible with the .Net 4.61 framework
  - [dotnet core 2.0+](https://dotnet.microsoft.com/download)
- [Python 3](https://www.python.org/) recommended for `unity-marshal.py`

## Build Instructions

- Build using either the Visual Studio or dotnet instructions below
- Marshal the 3esRuntime DLLs for the 3rdEyeScene Unity 3D project
  - Open a command prompt and change into the directory containing `unity-marshal.py`
  - Run the marshalling script (no arguments required)
    - `python unity-marshal.py`


Note: instead of using Python to run `unity-marshal.py`, you may instead copy 3esRuntime.DLL and support DLLs manually.

- Navigate to `3esRuntime/bin/Release`
- Find the 3esRuntime.DLL. This will be under the `netstandard2.0` directory for `dotnet` builds.
- Copy the 3esRuntime.DLL, all other DLLs in the same folder and all .PDB files with the of the same name into the 3rdEyeScene Unity project, under `Assets/plugins`.

### Visual Studio

- Open `3esCore.sln`
- Select the build configuration (e.g., Release, Any CPU)
- Select Build All in the menus

### dotnet build

- Open a command prompt
- Ensure the `dotnet` command is available on the path by running `dotnet --info`
- Nativate into the folder containing `3esCore.sln`
- Building the project:
  - `dotnet build -f netstandard2.0 -c Release .`
