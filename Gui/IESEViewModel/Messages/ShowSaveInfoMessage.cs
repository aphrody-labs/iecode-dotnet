using CommunityToolkit.Mvvm.Messaging.Messages;
using IESEViewModel.ViewModels;


namespace IESEViewModel.Messages;

public class ShowSaveInfoMessage(SaveInfoViewModel viewModel) : AsyncRequestMessage<bool>
{
    public SaveInfoViewModel ViewModel { get; } = viewModel;
}
