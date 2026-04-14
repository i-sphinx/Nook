# Nook

Nook is a simple, straightforward desktop application designed to help you quickly organize and categorize your files. Built with C# and Avalonia UI, it scans a selected directory and moves your files into clean, categorized folders.

## Features

- **Drag & Drop Workflow**: Easily drag and drop any folder into the app to instantly scan its contents.
- **Smart Categorization**: Automatically groups files into general categories such as Images, Videos, Audio, Documents, Spreadsheets, Presentations, Code, Archives, and Fonts based on their extensions.
- **Flexible Organization Options**:
  - Move categorized files directly to your standard OS folders (e.g., Pictures, Videos, Music, Documents).
  - Alternatively, create neatly grouped subfolders within your target directory.
- **Preview Mode**: Run an organization pass without moving any actual files to see what will happen first.
- **Undo Support**: Accidentally organized a folder? You can undo the last organization action to securely restore files to their original locations.
- **Recursive Scan**: Option to scan top-level files only or recursively include all subfolders.

## Tech Stack

- C# / .NET 
- Avalonia UI (for cross-platform desktop UI)
- CommunityToolkit.Mvvm

## Getting Started

Make sure you have the .NET SDK installed.

1. Clone the repository.
2. Navigate to the project directory.
3. Run `dotnet restore` to resolve dependencies.
4. Run `dotnet run` to launch the application.
