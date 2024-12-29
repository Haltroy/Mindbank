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
   - Windows (64-bit Intel/AMD/ARM)
   - Linux (generic, for both GCC and MUSL, 64-bit Intel/AMD/ARM)
   - Debian-based distributions (64-bit Intel/AMD/ARM)
   - Red Hat/Fedora based distributions (64-bit Intel/AMD/ARM)
   - Arch Linux-based distributions (using PKGBUILD, 64-bit Intel/AMD/ARM)
   - Android
   - macOS (not officially, need to build the app)
   - iOS (not officially, need to build the app)

Please refer to the appropiate OS version from the project file for that specific platform (ex. Mindbank.Android/Mindbank.Android.csproj).

## Installation

Releases for Windows (Intel, AMD, ARM), Android (as APK) and generic binaries and distribution-specific packages for Linux are available on [Releases](https://github.com/Haltroy/Mindbank/releases).

For Arch-based Linux users, download the PKGBUILD files and put them in a folder. Then run `makepkg -i` inside that folder to build & install Mindbank.
 - [Normal](https://raw.githubusercontent.com/Haltroy/Mindbank/refs/heads/main/linux/arch/main/PKGBUILD) (builds the latest release) [`mindbank`]
 - [Binary](https://raw.githubusercontent.com/Haltroy/Mindbank/refs/heads/main/linux/arch/bin/PKGBUILD) (skips building the latest release, downloads and repackages the generic binaries) [`mindbank-bin`]

Mindbank will check for updates for you. However, installing it is a task up to the you (the user).

## Build

Requires [.NET SDK](https://dotnet.microsoft.com) (latest LTS available).

If using another .NET version, please change the "TargetFramework" versions of each .csproj file accordingly. We prefer
using the latest LTS version of .NET.

To build, head to the folder of any trget system (ex. BlueLabel.Linux) and run "dotnet publish" to build it (ex. dotnet
build -r linux-x64 -c Release). The application should be inside the bin folder of that folder.

***NOTE: PLEASE DO NOT SKIP ANY STEPS UNLESS IT IS TOLD TO SKIP IT.***

1. Get the code. Either use the green "Code" button on the GitHub page or use `git clone https://github.com/haltroy/Mindbank.git` command to get the code.
2. Either use the FluxionViewer.sln file in your IDE (Vİsual Studio, VSCode, Rider etc.) and build using that IDE or open up a terminal inside the platform you want to build and run `dontet build.
3. For production-level build, use `dotnet publish -c Release -r <Runtime Identifier>`.
    - `win-x64`: Windows Intel/AMD 64-bit
    - `win-arm64`: Windows ARM 64-bit
    - `linux-x64`: Linux Intel/AMD 64-bit
    - `linux-musl-x64`: Linux (distributions that use MUSL instead) Intel/AMD 64-bit
    - `linux-arm64`: Linux ARM 64-bit
    - `linux-musl-arm64`: Linux (distributions that use MUSL instead) ARM 64-bit
    - `android-arm64`: Android phone
    - `ios-arm64`: iOS device (iPhone, iPad etc.)
    - `osx-x64`: macOS Intel 64-bit
    - `osx-arm64`: macOS ARM 64-bit
4. Build files should be in the `bin` folder of that specific platform.


## Development

You can help the development of Mindbank by:
 - Reporting any issues (bugs, vulnerabilities, feature requests etc.)
 - Submitting PRs
    - NOTE: The Linux and Windows workloads DO take some time since Windows workload has to publish it (with NativeAOT and everything) for 2 different platforms while Linux has to publish it for 4 platforms. This isn't a bug. Patience is a virtue.
    
Your contributions will be rewarded in the credits screen in the About section of this program (only if you want to).
