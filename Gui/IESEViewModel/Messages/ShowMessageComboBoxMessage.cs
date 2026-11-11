using CommunityToolkit.Mvvm.Messaging.Messages;
using IESEViewModel.InazumaEleven.Logic;

namespace IESEViewModel.Messages;

public class ShowMessageComboBoxMessage(string title, string text, List<Player> players, string confirmText) : AsyncRequestMessage<Player?>
{
    public string Title { get; } = title;
    public string Text { get; } = text;
    public List<Player> Players { get; } = players;
    public string ConfirmText { get; } = confirmText;
}