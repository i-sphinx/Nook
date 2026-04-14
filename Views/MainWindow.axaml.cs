using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Nook.ViewModels;

namespace Nook.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Enable drag-drop at the window level
        AddHandler(DragDrop.DropEvent,      DropZone_Drop,      RoutingStrategies.Bubble);
        AddHandler(DragDrop.DragEnterEvent, DropZone_DragEnter, RoutingStrategies.Bubble);
        AddHandler(DragDrop.DragLeaveEvent, DropZone_DragLeave, RoutingStrategies.Bubble);

        DataContextChanged += (_, _) => AttachViewModel();
        AttachViewModel();
    }

    private void AttachViewModel()
    {
        if (DataContext is MainWindowViewModel vm)
            vm.FolderPickRequested += async (_, _) => await PickFolderAsync(vm);
    }

    // ── Folder Picker ──────────────────────────────────────────────────────
    private async System.Threading.Tasks.Task PickFolderAsync(MainWindowViewModel vm)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title         = "Select a folder to organize",
            AllowMultiple = false,
        });

        if (folders is { Count: > 0 })
            vm.SetFolder(folders[0].Path.LocalPath);
    }

    // ── Drag & Drop ────────────────────────────────────────────────────────
    private void DropZone_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Formats.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy;
            DropZoneBorder.Tag = "hover";
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void DropZone_DragLeave(object? sender, DragEventArgs e)
    {
        DropZoneBorder.Tag = null;
    }

    private void DropZone_Drop(object? sender, DragEventArgs e)
    {
        DropZoneBorder.Tag = null;

        if (!e.DataTransfer.Formats.Contains(DataFormat.File))
            return;
        if (DataContext is not MainWindowViewModel vm)
            return;

        var items = e.DataTransfer.TryGetFiles();
        var list = items?.ToList();
        if (list is not { Count: > 0 }) return;

        var path = list[0].Path.LocalPath;

        // If a file was dropped, use its parent directory
        if (File.Exists(path))
            path = Path.GetDirectoryName(path)!;

        if (Directory.Exists(path))
            vm.SetFolder(path);
    }

    // ── Custom Title Bar ───────────────────────────────────────────────────
    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void MinimizeBtn_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseBtn_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}