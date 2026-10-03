using ndsSharp.Core.Extensions;
using ndsSharp.Core.Mathematics;
using ndsSharp.Core.Objects.Exports.Textures.Blocks;
using ndsSharp.Core.Plugins.BW2.Map.Data;
using Xunit;

namespace IECODE.Core.Tests;

public class NdsMalformedInputTests
{
    [Fact]
    public void TerrainEqualityIsTypeSafeAndHashConsistent()
    {
        var first = new BW2TerrainTile { X = 2, Y = 3, Height = 4, Slope = 5 };
        var second = new BW2TerrainTile { X = 2, Y = 3, Height = 4, Slope = 5 };
        Assert.False(first.Equals(null));
        Assert.False(first.Equals(new object()));
        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.True(new HashSet<BW2TerrainTile> { first }.Contains(second));
    }

    [Fact]
    public void UnknownTextureFormatsAreRejected()
    {
        var invalid = (TextureFormat)255;
        Assert.Throws<InvalidDataException>(() => invalid.BitsPerPixel());
        Assert.Throws<InvalidDataException>(() => invalid.PaletteSize());
        Assert.Equal(0, TextureFormat.None.BitsPerPixel());
        Assert.Equal(16, TextureFormat.A1BGR5.BitsPerPixel());
    }

    [Fact]
    public void UnknownPivotSelectionIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Matrix3x3.CreatePivot(9, 0, 1, 0));
        Matrix3x3.CreatePivot(8, 0, 1, 0);
    }
}
