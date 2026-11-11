using System.Text;
using System.Text.RegularExpressions;
using ndsSharp.Core.Data;
using ndsSharp.Core.Objects.Files;

namespace ndsSharp.Core.Plugins;

public class PluginFileTypeAssociation(Type fileType, string extension, string[]? pathMatches = null, Regex[]? regexMatches = null, string[]? magicMatches = null)
{
    public Type FileType = fileType;
    public string Extension = extension;
    public string[] PathMatches = pathMatches ?? [];
    public Regex[] RegexMatches = regexMatches ?? [];
    public string[] MagicMatches = magicMatches ?? [];

    public bool Match(RomFile file)
    {
        var matched = false;
        matched |= PathMatches.Any(match => file.Path.Contains(match));
        matched |= RegexMatches.Any(regex => regex.IsMatch(file.Path));
        matched |= MagicMatches.Any(magic =>
        {
            if (file.Pointer.Owner is null || string.IsNullOrWhiteSpace(magic))
            {
                return false;
            }

            using var reader = file.Pointer.Load();
            var data = reader.GetBuffer();
            var expected = ParseMagic(magic);
            return data.AsSpan().StartsWith(expected);
        });
        return matched;
    }

    private static byte[] ParseMagic(string magic)
    {
        var compact = magic.Replace(" ", string.Empty, StringComparison.Ordinal);
        if (compact.Length > 0 && compact.Length % 2 == 0 && compact.All(Uri.IsHexDigit))
        {
            return Convert.FromHexString(compact);
        }

        return Encoding.ASCII.GetBytes(magic);
    }
}
