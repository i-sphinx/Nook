using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nook.Models;

namespace Nook.ViewModels;

public partial class CategorySummaryViewModel : ObservableObject
{
    public FileCategory Category { get; }

    [ObservableProperty] private int _fileCount;

    public CategorySummaryViewModel(FileCategory category)
    {
        Category = category;
        _fileCount = 0;
    }

    public string Name => Category.Name;
    public string Icon => Category.Icon;
    public string AccentColor => Category.AccentColor;
    public string FolderName => Category.FolderName;
}

public partial class MainWindowViewModel : ViewModelBase
{
    // ── Observable state ──────────────────────────────────────────────────────

    [ObservableProperty] private string _selectedFolderPath = string.Empty;
    [ObservableProperty] private bool   _hasFolder          = false;
    [ObservableProperty] private bool   _isBusy             = false;
    [ObservableProperty] private bool   _isDragOver         = false;
    [ObservableProperty] private double _progress           = 0;
    [ObservableProperty] private string _statusText         = "Drop a folder or click Browse to get started";
    [ObservableProperty] private int    _totalFiles         = 0;
    [ObservableProperty] private int    _movedFiles         = 0;
    [ObservableProperty] private bool   _canUndo            = false;
    [ObservableProperty] private bool   _includeSubfolders  = false;
    [ObservableProperty] private bool   _showPreviewMode    = false;

    public ObservableCollection<OrganizeLogEntry> Logs    { get; } = new();
    public ObservableCollection<CategorySummaryViewModel> Categories { get; } = new();

    // Undo: maps destination path → original path
    private readonly List<(string Dest, string Source)> _undoMap = new();
    private CancellationTokenSource? _cts;

    // ── Constructor ───────────────────────────────────────────────────────────

    public MainWindowViewModel()
    {
        foreach (var cat in FileCategory.All)
            Categories.Add(new CategorySummaryViewModel(cat));
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task BrowseFolder()
    {
        // We raise an event that the View listens to (pattern keeps VM testable)
        FolderPickRequested?.Invoke(this, EventArgs.Empty);
        await Task.CompletedTask;
    }

    public event EventHandler? FolderPickRequested;

    public void SetFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return;

        SelectedFolderPath = path;
        HasFolder          = true;
        CanUndo            = false;
        _undoMap.Clear();
        Logs.Clear();
        Progress    = 0;
        MovedFiles  = 0;
        StatusText  = $"Ready — {Path.GetFileName(path)}";

        _ = ScanFolderAsync(path);
    }

    private async Task ScanFolderAsync(string path)
    {
        await Task.Run(() =>
        {
            var counts = FileCategory.All.ToDictionary(c => c.Type, _ => 0);

            var searchOption = IncludeSubfolders
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            var files = Directory.EnumerateFiles(path, "*.*", searchOption);

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);
                var cat = FileCategory.GetCategory(ext);
                counts[cat.Type]++;
            }

            Dispatcher.UIThread.Post(() =>
            {
                int total = 0;
                foreach (var vm in Categories)
                {
                    vm.FileCount = counts[vm.Category.Type];
                    total += vm.FileCount;
                }
                TotalFiles = total;
                StatusText = total == 0
                    ? "No files found in this folder"
                    : $"Found {total} file{(total == 1 ? "" : "s")} — click Organize to sort them";
            });
        });
    }

    [RelayCommand(CanExecute = nameof(CanOrganize))]
    private async Task Organize()
    {
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;

        IsBusy    = true;
        Progress  = 0;
        MovedFiles = 0;
        _undoMap.Clear();
        CanUndo   = false;
        Logs.Clear();
        _cts = new CancellationTokenSource();

        AddLog($"Starting organization of: {SelectedFolderPath}", LogLevel.Info);

        try
        {
            await Task.Run(async () =>
            {
                var searchOption = IncludeSubfolders
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly;

                var files = Directory.EnumerateFiles(SelectedFolderPath, "*.*", searchOption)
                                     .ToList();

                int total = files.Count;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TotalFiles = total;
                    StatusText  = $"Organizing {total} files…";
                });

                for (int i = 0; i < files.Count; i++)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    var file = files[i];
                    var ext  = Path.GetExtension(file);
                    var cat  = FileCategory.GetCategory(ext);

                    // Skip files already inside a category sub-folder
                    var parentDir = Path.GetDirectoryName(file)!;
                    if (FileCategory.All.Any(c => parentDir.EndsWith(c.FolderName, StringComparison.OrdinalIgnoreCase)))
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                            AddLog($"Skipped (already organized): {Path.GetFileName(file)}", LogLevel.Warning));
                        continue;
                    }

                    var destDir = Path.Combine(SelectedFolderPath, cat.FolderName);
                    Directory.CreateDirectory(destDir);

                    var destFile = GetUniqueDestination(destDir, Path.GetFileName(file));

                    if (ShowPreviewMode)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                            AddLog($"[Preview] {Path.GetFileName(file)} → {cat.FolderName}/", LogLevel.Info));
                    }
                    else
                    {
                        File.Move(file, destFile);
                        _undoMap.Add((destFile, file));

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            MovedFiles++;
                            AddLog($"{Path.GetFileName(file)} → {cat.FolderName}/", LogLevel.Success);
                        });
                    }

                    double pct = (double)(i + 1) / total * 100;
                    await Dispatcher.UIThread.InvokeAsync(() => Progress = pct);

                    await Task.Delay(10, _cts.Token); // small delay so UI breathes
                }
            }, _cts.Token);

            AddLog(ShowPreviewMode
                ? $"Preview complete — {TotalFiles} file(s) would be moved."
                : $"Done! {MovedFiles} file(s) organized.", LogLevel.Success);

            StatusText = ShowPreviewMode
                ? $"Preview complete — {TotalFiles} file(s) would be moved"
                : $"Organized {MovedFiles} of {TotalFiles} file(s) successfully";

            CanUndo = !ShowPreviewMode && _undoMap.Count > 0;

            await ScanFolderAsync(SelectedFolderPath);
        }
        catch (OperationCanceledException)
        {
            AddLog("Organization cancelled.", LogLevel.Warning);
            StatusText = "Cancelled";
        }
        catch (Exception ex)
        {
            AddLog($"Error: {ex.Message}", LogLevel.Error);
            StatusText = "An error occurred — check the log";
        }
        finally
        {
            IsBusy   = false;
            Progress = 100;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private bool CanOrganize() => HasFolder && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanUndoOrganize))]
    private async Task UndoOrganize()
    {
        if (_undoMap.Count == 0) return;

        IsBusy = true;
        AddLog("Undoing organization…", LogLevel.Info);

        try
        {
            await Task.Run(() =>
            {
                foreach (var (dest, source) in Enumerable.Reverse(_undoMap))
                {
                    try
                    {
                        if (File.Exists(dest))
                        {
                            var sourceDir = Path.GetDirectoryName(source)!;
                            Directory.CreateDirectory(sourceDir);
                            var safeDest = GetUniqueDestination(sourceDir, Path.GetFileName(source));
                            File.Move(dest, safeDest);
                            Dispatcher.UIThread.Post(() =>
                                AddLog($"Restored: {Path.GetFileName(dest)}", LogLevel.Success));
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.UIThread.Post(() =>
                            AddLog($"Could not restore {Path.GetFileName(dest)}: {ex.Message}", LogLevel.Error));
                    }
                }
            });

            _undoMap.Clear();
            CanUndo    = false;
            StatusText = "Undo complete — files restored to original locations";

            await ScanFolderAsync(SelectedFolderPath);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanUndoOrganize() => CanUndo && !IsBusy;

    [RelayCommand]
    private void CancelOrganize()
    {
        _cts?.Cancel();
    }

    [RelayCommand]
    private void ClearLog()
    {
        Logs.Clear();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void AddLog(string message, LogLevel level = LogLevel.Info)
    {
        Logs.Insert(0, new OrganizeLogEntry { Message = message, Level = level, Timestamp = DateTime.Now });
    }

    private static string GetUniqueDestination(string directory, string fileName)
    {
        var dest = Path.Combine(directory, fileName);
        if (!File.Exists(dest)) return dest;

        var name      = Path.GetFileNameWithoutExtension(fileName);
        var ext       = Path.GetExtension(fileName);
        int counter   = 1;

        while (File.Exists(dest))
        {
            dest = Path.Combine(directory, $"{name} ({counter}){ext}");
            counter++;
        }
        return dest;
    }
}
