# UnityHub

Open-source & cross-platform alternative to Unity's official Hub — for managing Unity editor installs and projects, built with [Avalonia UI](https://avaloniaui.net/) and .NET.

> ⚠️ Early-stage project (`v0.1.0`). Expect rough edges and breaking changes.

## Features

- **Editor management** — discover, install, and track multiple Unity Editor versions and modules across custom install locations.
- **Project management** — list, open, and create Unity projects, including creating new projects from templates/packages.
- **Tagging & collections** — organize projects into color-coded collections (e.g. In Development, Archived, Released) and apply custom tags for fast filtering and sorting.
- **Project thumbnails/images** — projects display with their own image/thumbnail so you can recognize them at a glance instead of hunting through folder names.
- **Multiple layouts** — switch between different views of your project list (image card grid, compact list, and table layouts) depending on how you like to browse.
- **Search, filter & sort** — quickly narrow down projects by tag, collection, or editor version.
- **Unity-side integration** — the bundled `com.nexx.unityhublink` Unity package links a project's Editor back to the Hub.

## Screenshots
<img alt="image" src="https://github.com/user-attachments/assets/17e1f3f9-ebc2-4c23-8bef-149587debc5d" />

<p align="center">
<img width="400" alt="image" src="https://github.com/user-attachments/assets/540e6165-05ee-41a1-b3e0-5502119c9ffc" />
<img width="400"  alt="image" src="https://github.com/user-attachments/assets/c9c44ced-ad76-4637-a5cc-e2e1ff16a4d4" />
<img width="400"  alt="image" src="https://github.com/user-attachments/assets/ffd48dcf-07e3-4a2a-9e84-9fcc0ad9f6c1" />
<img width="400" alt="image" src="https://github.com/user-attachments/assets/a06f7abc-17b0-43f2-bc48-3eb05d96c8c5" />
  </p>


## Requirements

- [.NET SDK](https://dotnet.microsoft.com/) 9.0/10.0 or later
- `make` (for the provided build targets)
- `appimagetool` on your `PATH` if building the Linux AppImage

## Building

Clone the repository (it includes submodules):

```bash
git clone --recurse-submodules https://github.com/NeXx42/UnityHub.git
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
