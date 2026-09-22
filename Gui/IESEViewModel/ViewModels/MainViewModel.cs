using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IESEViewModel.InazumaEleven.Games;
using IESEViewModel.InazumaEleven.Logic;
using IESEViewModel.Messages;

namespace IESEViewModel.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private SaveInfoViewModel? _saveInfoViewModel;
    private IGame? _game;
    private bool _isLoading;

    public IGame? Game
    {
        get => _game;
        private set
        {
            if (!SetProperty(ref _game, value)) return;
            IsLoading = false;
            OnPropertyChanged(nameof(GameIsAvailable));
        }
    }

    public bool GameStartedLoading => Game != null || IsLoading;
    public bool GameIsAvailable => Game != null;

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value)) OnPropertyChanged(nameof(GameStartedLoading));
        }
    }

    [RelayCommand]
    private async Task OpenSaveInfoDialogAsync()
    {
        _saveInfoViewModel ??= OpenSaveInfo();
        _ = await WeakReferenceMessenger.Default.Send(new ShowSaveInfoMessage(_saveInfoViewModel));
    }

    [RelayCommand]
    private async Task OpenSaveAsync()
    {
        Stream? stream = await WeakReferenceMessenger.Default.Send(
            new OpenSaveMessage());

        if (stream is null)
        {
            return;
        }

        IsLoading = true;
        Game = await DataHandler.Open(stream);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Game is null) return;

        _ = await WeakReferenceMessenger.Default.Send(
            new SaveStateMessage(
                await DataHandler.SaveAsync(Game),
                GameVersionHelper.GetFileEndingForVersion(Game.Version))
            );
    }

    [RelayCommand]
    private static void OpenNdsViewer()
    {
        // The viewer is a capability window owned by the IECODE Avalonia host.
        // It does not create an application lifetime or spawn a second process.
        new ndsSharp.Viewer.Windows.MainWindow().Show();
    }

    [RelayCommand]
    private static void OpenToolboxTextDecoder()
    {
        new Avalonia.Controls.Window
        {
            Title = "IECODE Toolbox text decoder",
            Width = 760,
            Height = 620,
            Content = new IECODE.Gui.Tools.ToolboxTextDecoderView()
        }.Show();
    }

    [RelayCommand]
    private static void OpenIevrTools()
    {
        if (!IECODE.Gui.Shell.ShellRegistry.Current.Contributions.TryGetValue("ievr", out var contribution))
        {
            return;
        }

        var entry = contribution.NavigationEntries.FirstOrDefault(item => item.Id == "ievr");
        if (entry is null)
        {
            return;
        }

        new Avalonia.Controls.Window
        {
            Title = "IEVR tools",
            Width = 1_120,
            Height = 720,
            MinWidth = 800,
            MinHeight = 560,
            Content = entry.ViewFactory()
        }.Show();
    }

    [RelayCommand]
    private async Task OpenMessageComboBoxAsync(string context)
    {
        if (Game is null) return;

        if (context == "Recruit" && Game.Reserve.Count > Game.MaximumPlayer)
        {
            // TODO: Implement dialog
            throw new NotImplementedException();
        }

        string title = $"{context} Player";
        string text = $"Select the player you want to {context.ToLower()}";

        Player? player = await WeakReferenceMessenger.Default.Send(new ShowMessageComboBoxMessage(
            title,
            text,
            context == "Recruit" ? Game.Reserve /* TODO: use all sorted players*/ : Game.Reserve,
            context)
        );

        if (player is null) return;

        switch (context)
        {
            case "Recruit":
                RecruitPlayer(player);
                break;
            case "Dismiss":
                DismissPlayer(player);
                break;
            case "Export":
                ExportPlayer(player);
                break;
            case "Replace":
                ReplacePlayer(player);
                break;
        }
    }

    private void RecruitPlayer(Player player)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }

    private void DismissPlayer(Player player)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }

    private void ExportPlayer(Player player)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }

    private void ReplacePlayer(Player player)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }

    private static SaveInfoViewModel OpenSaveInfo()
    {
        // TODO: Implement logic to open SaveInfoViewModel
        return new SaveInfoViewModel(10, 30, 10, "", "", 10000, 3, 0, coins: [10, 15, 20, 25, 30]);
    }
}
