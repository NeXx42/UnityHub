# UnityHub

Open-source & cross-platform alternative to Unity's official Hub — for managing Unity editor installs and projects, built with [Avalonia UI](https://avaloniaui.net/) and .NET.

> ⚠️ Early-stage project. Expect rough edges and breaking changes.

## Features

- **Editor management** — discover, install, and track multiple Unity Editor versions and modules across custom install locations.
- **Project management** — list, open, and create Unity projects, including creating new projects from templates/packages.
- **Tagging & collections** — organize projects into color-coded collections (e.g. In Development, Archived, Released) and apply custom tags for fast filtering and sorting.
- **Project thumbnails/images** — projects display with their own image/thumbnail so you can recognize them at a glance instead of hunting through folder names.
- **Multiple layouts** — switch between different views of your project list (image card grid, compact list, and table layouts) depending on how you like to browse.
- **Search, filter & sort** — quickly narrow down projects by tag, collection, or editor version.
- **Unity-side integration** — the bundled `com.nexx.unityhublink` Unity package links a project's Editor back to the Hub.

## Screenshots
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/5f59b09b-45f2-456e-843f-cdfb0bf56758" />
<img width="7680" height="1080" alt="image" src="https://github.com/user-attachments/assets/f2f8179b-c5fd-449a-add5-37d3ba0e329d" />

## Languages
> ⚠️ Not completed & May be incorrect.
- English (Native)
- Japanese

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/) 10.0 or later
- `make` (for the provided build targets)
- `appimagetool` on your `PATH` if building the Linux AppImage

## Building

Clone the repository:

```bash
cd UnityHub
```

Build:

```bash
make build
```

Build output is written to `Build/Output/`.

Alternatively, run it directly with the .NET CLI:

```bash
dotnet run --project UI/UI.csproj
```

## The Unity Link Package

`com.nexx.unityhublink` a small Unity package (Editor-only tooling) that ties an open Unity project back to the Hub, allowing the Hub to communicate with/track the running Editor instance. Is automatically injected into the packages (unless manually disabled).
