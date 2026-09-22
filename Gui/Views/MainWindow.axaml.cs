using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using IESEViewModel.Messages;
using Avalonia.Controls;
using IESEViewModel.InazumaEleven.Games;
using System;
using IESEViewModel.InazumaEleven.Logic;
using IESEViewModel.ViewModels;

namespace IESE.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            return;

        RegisterMessages();
    }

    private void RegisterMessages()
    {
        WeakReferenceMessenger.Default.Register<MainWindow, ShowSaveInfoMessage>(this, static (window, message) =>
        {
            SaveInfoWindow dialog = new()
            {
                DataContext = message.ViewModel
            };
            message.Reply(dialog.ShowDialog<bool>(window));
        });

        FilePickerFileType fileType = new("Inazuma Eleven Save")
        {
            Patterns = GameVersionHelper.GetAllFileEndings()
        };

        WeakReferenceMessenger.Default.Register<MainWindow, OpenSaveMessage>(this, (window, message) =>
        {
            message.Reply(OpenSave(window, fileType));
        });

        WeakReferenceMessenger.Default.Register<MainWindow, SaveStateMessage>(this, (window, message) =>
        {
            message.Reply(SaveState(window, message, fileType));
        });

        WeakReferenceMessenger.Default.Register<MainWindow, ShowMessageComboBoxMessage>(this, static (window, message) =>
        {
            MessageComboBoxView dialog = new()
            {
                DataContext = new MessageComboBoxViewModel(message.Title, message.Text, message.Players, message.ConfirmText)
            };
            message.Reply(dialog.ShowDialog<Player?>(window));
        });
    }

    private static async Task<bool> SaveState(Window window, SaveStateMessage message, FilePickerFileType fileType)
    {
        IStorageFile? file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save your modified State",
            FileTypeChoices = [fileType],
            DefaultExtension = message.FileEnding,
        });

        if (file is null)
        {
            return false;
        }

        await using Stream stream = await file.OpenWriteAsync();
        await stream.WriteAsync(message.Content.AsMemory(0, message.Content.Length));
        return true;
    }

    private static async Task<Stream?> OpenSave(Window window, FilePickerFileType fileType)
    {
        IReadOnlyList<IStorageFile> files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select your Save File",
            FileTypeFilter = [fileType],
            AllowMultiple = false
        });

        if (files.Count != 1)
        {
            return null;
        }

        IStorageFile file = files[0];

        return await file.OpenReadAsync();
    }
}
