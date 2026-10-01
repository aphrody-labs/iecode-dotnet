using System.Linq;
using IECODE.Core;
using Xunit;

namespace IECODE.Core.Tests;

public class NieReMapTests
{
    [Fact]
    public void GetAuthoritativeMap_ContainsExecutableMetadata()
    {
        var map = NieReMap.GetAuthoritativeMap();

        Assert.Equal("nie.exe", map.TargetBinary);
        Assert.Equal("nie1v2.exe", map.InternalName);
        Assert.Equal("x86_64", map.Architecture);
        Assert.Equal("PE32+", map.Format);
        Assert.Equal("0x140000000", map.BaseAddressHex);
        Assert.Equal(0x140000000UL, map.BaseAddress);
        Assert.Equal(117068, map.TotalFunctionsCount);
        Assert.Equal(72151, map.TotalStringsCount);
    }

    [Fact]
    public void CryptoConstants_ContainsAll7CanonicalConstants()
    {
        var map = NieReMap.GetAuthoritativeMap();
        Assert.Equal(7, map.CryptoConstants.Count);

        var viola = map.CryptoConstants.First(c => c.Name == "VIOLA_FIXED_KEY");
        Assert.Equal(0x1717E18Eu, viola.IntValue);
        Assert.Equal("0x1717E18E", viola.HexValue);

        var crc = map.CryptoConstants.First(c => c.Name == "LEVEL5_CRC32_POLYNOMIAL");
        Assert.Equal(0xEDB88320u, crc.IntValue);

        var live = map.CryptoConstants.First(c => c.Name == "LIVES_SAVE_HEADER_MAGIC");
        Assert.Equal(0x9DCE66C3u, live.IntValue);

        var steam = map.CryptoConstants.First(c => c.Name == "STEAM_APP_ID");
        Assert.Equal(2799860u, steam.IntValue);
    }

    [Fact]
    public void CriticalOffsets_ContainsAll8CriticalFunctionPointers()
    {
        var map = NieReMap.GetAuthoritativeMap();
        Assert.Equal(8, map.CriticalOffsets.Count);

        var gameTick = map.CriticalOffsets.First(o => o.Symbol == "GameTickLoop");
        Assert.Equal(0x1406d5840UL, gameTick.RuntimePc);
        Assert.Equal("game-loop", gameTick.Subsystem);

        var crandNext = map.CriticalOffsets.First(o => o.Symbol == "CRand_MT19937_Next");
        Assert.Equal(0x14081c200UL, crandNext.RuntimePc);
        Assert.Equal("prng", crandNext.Subsystem);

        var cpkDecrypt = map.CriticalOffsets.First(o => o.Symbol == "CriFsStreamDecryptBlock");
        Assert.Equal(0x1405a1120UL, cpkDecrypt.RuntimePc);
    }

    [Fact]
    public void RttiClasses_ContainsAll8ReconstructedClasses()
    {
        var map = NieReMap.GetAuthoritativeMap();
        Assert.Equal(8, map.RttiClasses.Count);

        var chara = map.RttiClasses.First(c => c.Name == "CharaController");
        Assert.Equal("Level5::Game", chara.Namespace);
        Assert.Contains("Level5::Engine::Object", chara.BaseClasses);

        var sound = map.RttiClasses.First(c => c.Name == "SoundPlayer");
        Assert.Equal("CriWare::Atom", sound.Namespace);
    }

    [Fact]
    public void MemoryStructures_ContainsAll3CriticalStructures()
    {
        var map = NieReMap.GetAuthoritativeMap();
        Assert.Equal(3, map.MemoryStructures.Count);

        var bone = map.MemoryStructures.First(s => s.Name == "BonePalette");
        Assert.Equal(1536, bone.SizeBytes);
        Assert.Equal("0x600", bone.SizeHex);

        var glyph = map.MemoryStructures.First(s => s.Name == "FontGlyph");
        Assert.Equal(12, glyph.SizeBytes);

        var rdbn = map.MemoryStructures.First(s => s.Name == "RdbnHeader");
        Assert.Equal(80, rdbn.SizeBytes);
        Assert.Equal("0x50", rdbn.SizeHex);
    }
}
