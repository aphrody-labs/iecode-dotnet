using CommunityToolkit.Mvvm.Messaging.Messages;

namespace IESEViewModel.Messages;

public class SaveStateMessage(byte[] content, string fileEnding) : AsyncRequestMessage<bool>
{
    public byte[] Content { get; } = content;
    public string FileEnding { get; } = fileEnding;
}