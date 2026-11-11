using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using IESEViewModel.Messages;

namespace IESE.Views;

public partial class MessageComboBoxView : Window
{
    public MessageComboBoxView()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<MessageComboBoxView, CloseMessage>(this, (window, message) =>
        {
            window.Close(message.Result);
        });
    }
}
