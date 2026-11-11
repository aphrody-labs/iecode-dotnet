using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Avalonia.Controls;

namespace IECODE.Gui.Tools;

public partial class ToolboxTextDecoderView : UserControl
{
    private static int _encodingProviderRegistered;

    public ToolboxTextDecoderView()
    {
        InitializeComponent();
    }

    private void OnDecodeClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!TryParseHex(EncodedBytesTextBox.Text, out var bytes, out var error))
        {
            SetStatus(error);
            return;
        }

        try
        {
            EnsureCodePagesEncoding();
            DecodedTextTextBox.Text = Inazuma_Eleven_Toolbox.Logic.TextDecoder.Decode(bytes);
            SetStatus($"Decoded {bytes.Length} byte{(bytes.Length == 1 ? string.Empty : "s")}.");
        }
        catch (Exception exception) when (exception is ArgumentException or IndexOutOfRangeException or KeyNotFoundException)
        {
            SetStatus($"Decode failed: {exception.Message}");
        }
    }

    private void OnEncodeClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            EnsureCodePagesEncoding();
            var bytes = Inazuma_Eleven_Toolbox.Logic.TextDecoder.Encode(PlainTextTextBox.Text ?? string.Empty);
            EncodedOutputTextBox.Text = Convert.ToHexString(bytes);
            SetStatus($"Encoded {bytes.Length} byte{(bytes.Length == 1 ? string.Empty : "s")}.");
        }
        catch (Exception exception) when (exception is ArgumentException or EncoderFallbackException)
        {
            SetStatus($"Encode failed: {exception.Message}");
        }
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }

    private static void EnsureCodePagesEncoding()
    {
        if (Interlocked.Exchange(ref _encodingProviderRegistered, 1) == 0)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }

    private static bool TryParseHex(string? text, out byte[] bytes, out string error)
    {
        var tokens = (text ?? string.Empty)
            .Split(new[] { ' ', '\t', '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
        {
            bytes = Array.Empty<byte>();
            error = "Enter hexadecimal bytes first.";
            return false;
        }

        bytes = new byte[tokens.Length];
        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                token = token[2..];
            }

            if (token.Length is < 1 or > 2 ||
                !byte.TryParse(token, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out bytes[i]))
            {
                bytes = Array.Empty<byte>();
                error = $"Invalid byte '{tokens[i]}'. Use values from 00 to FF.";
                return false;
            }
        }

        error = string.Empty;
        return true;
    }
}
