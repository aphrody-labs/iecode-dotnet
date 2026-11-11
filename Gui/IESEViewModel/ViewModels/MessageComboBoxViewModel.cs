using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IESEViewModel.InazumaEleven.Logic;
using IESEViewModel.Messages;

namespace IESEViewModel.ViewModels;

public partial class MessageComboBoxViewModel(string title, string text, List<Player> players, string confirmText) : ObservableObject
{
    private string _title = title;
    private string _text = text;
    private List<Player> _players = players;
    private string _confirmText = confirmText;
    private Player? _selectedPlayer;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public List<Player> Players
    {
        get => _players;
        set => SetProperty(ref _players, value);
    }

    public string ConfirmText
    {
        get => _confirmText;
        set => SetProperty(ref _confirmText, value);
    }

    public Player? SelectedPlayer
    {
        get => _selectedPlayer;
        set
        {
            if (SetProperty(ref _selectedPlayer, value))
            {
                ConfirmCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool HasSelectedPlayer => SelectedPlayer is not null;

    [RelayCommand(CanExecute = nameof(HasSelectedPlayer))]
    private void Confirm()
    {
        if (SelectedPlayer is null) return;
        WeakReferenceMessenger.Default.Send(new CloseMessage(SelectedPlayer));
    }

    [RelayCommand]
    private static void Cancel()
    {
        WeakReferenceMessenger.Default.Send(new CloseMessage(null));
    }

    // Only for previewer in an IDE
    public MessageComboBoxViewModel() : this("Title", "Text", [], "Confirm")
    { }
}