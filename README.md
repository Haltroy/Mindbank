# Mindbank

Something on your mind? Don't lose it, save it to Mindbank.

This program is released under GNU GPL v3. [Click here](./LICENSE) to read more.

## Features

- Keep multiple notes in a bank.
- Tag notes, search notes via tags.
- Edit and manage notes.
- Random pick notes.
- Import notes.
- Cross-platform, available in:
   - Windows (64-bit)
   - Linux (distribution packages are coming soon)
   - Android
   - macOS (Unofficial, build files exist)
   - iOS (Unofficial, build files exist)

Please refer to the appropiate OS version from the project file for that specific platform (ex. FluxionViewer.Android/FluxionViewer.Android.csproj).

## Build

Requires [.NET SDK](https://dotnet.microsoft.com) (latest available).

If using another .NET version, please change the "TargetFramework" versions of each .csproj file accordingly. We prefer
using the latest LTS version of .NET.

To build, head to the folder of any trget system (ex. BlueLabel.Linux) and run "dotnet publish" to build it (ex. dotnet
build -r linux-x64 -c Release). The application should be inside the bin folder of that folder.

***NOTE: PLEASE DO NOT SKIP ANY STEPS UNLESS IT IS TOLD TO OK TO SKIP IT.***

1. Get the code. Either use the green "Code" button on the GitHub page or use `git clone https://github.com/haltroy/Mindbank.git` command to get the code.
2. Either use the FluxionViewer.sln file in your IDE (Vİsual Studio, VSCode, Rider etc.) and build using that IDE or open up a terminal inside the platform you want to build and run `dontet build.
3. For production-level build, use `dotnet publish -c Release -r <Runtime Identifier>`.
    - `win-x64`: Windows Intel/AMD 64-bit
    - `win-arm64`: Windows ARM 64-bit
    - `linux-x64`: Linux Intel/AMD 64-bit
    - `linux-musl-x64`: Linux (distributions that use MUSL instead) Intel/AMD 64-bit
    - `linux-arm64`: Linux ARM 64-bit
    - `android-arm64`: Android phone
    - `iOS-arm64`: iOS device (iPhone, iPad etc.)
    - `osx-x64`: macOS Intel 64-bit
    - `osx-arm64`: macOS ARM 64-bit
4. Build files should be in the `bin` folder of that specific platform.
