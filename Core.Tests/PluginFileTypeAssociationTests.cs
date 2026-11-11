using ndsSharp.Core.Data;
using ndsSharp.Core.Plugins;
using ndsSharp.Core.Objects.Files;
using System.Text;
using Xunit;

namespace IECODE.Core.Tests;

public sealed class PluginFileTypeAssociationTests
{
    [Fact]
    public void MatchesHexAndAsciiMagicWithoutMovingTheSourceReader()
    {
        var source = new DataReader([0xDE, 0xAD, 0xBE, 0xEF, 0x01]);
        var file = new RomFile("a/test.bin", new DataPointer(0, 5, source));

        var hex = new PluginFileTypeAssociation(typeof(object), "bin", magicMatches: ["DE AD BE EF"]);
        var asciiSource = new DataReader(Encoding.ASCII.GetBytes("TEST\0"));
        var asciiFile = new RomFile("a/ascii.bin", new DataPointer(0, 5, asciiSource));
        var ascii = new PluginFileTypeAssociation(typeof(object), "bin", magicMatches: ["TEST"]);

        Assert.True(hex.Match(file));
        Assert.True(ascii.Match(asciiFile));
        Assert.Equal(0, source.Position);
        Assert.Equal(0, asciiSource.Position);
    }
}
