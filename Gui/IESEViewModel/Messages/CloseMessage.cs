using CommunityToolkit.Mvvm.Messaging.Messages;
using IESEViewModel.InazumaEleven.Logic;

namespace IESEViewModel.Messages;

public class CloseMessage(Player? result)
{
    public Player? Result { get; } = result;
}