using System;
using System.Text;
using IECODE.Core.Formats;
using Xunit;

namespace IECODE.Core.Tests;

public class VfsFormatConsumerTests
{
    [Fact]
    public void ListAll_ContainsAll28CanonicalFormats()
    {
        var formats = VfsFormatConsumer.ListAll();
        Assert.Equal(28, formats.Count);

        var g4tx = VfsFormatConsumer.Get(VfsFormatId.G4tx);
        Assert.Equal("G4TX", g4tx.MagicAscii);
        Assert.Equal(".g4tx", g4tx.Extension);
        Assert.Equal("texture", g4tx.Category);
        Assert.False(g4tx.IsBigEndian);

        var cpk = VfsFormatConsumer.Get(VfsFormatId.Cpk);
        Assert.Equal("CPK ", cpk.MagicAscii);
        Assert.Equal(".cpk", cpk.Extension);
        Assert.True(cpk.IsBigEndian);
        Assert.True(cpk.EncryptionSupported);

        var live = VfsFormatConsumer.Get(VfsFormatId.Live);
        Assert.Equal("LIVE", live.Name);
        Assert.Equal("0x9DCE66C3", live.MagicHex);
    }

    [Fact]
    public void Detect_IdentifiesFormatByMagicBytes()
    {
        // G4TX
        byte[] g4tx = Encoding.ASCII.GetBytes("G4TX\x20\x00\x00\x00");
        Assert.Equal(VfsFormatId.G4tx, VfsFormatConsumer.Detect(g4tx).Id);

        // CPK
        byte[] cpk = Encoding.ASCII.GetBytes("CPK \x00\x00\x00\x00");
        Assert.Equal(VfsFormatId.Cpk, VfsFormatConsumer.Detect(cpk).Id);

        // RDBN
        byte[] rdbn = Encoding.ASCII.GetBytes("RDBN\x50\x00\x01\x00");
        Assert.Equal(VfsFormatId.CfgBin, VfsFormatConsumer.Detect(rdbn).Id);

        // CRILAYLA
        byte[] layla = Encoding.ASCII.GetBytes("CRILAYLA\x00\x10\x00\x00");
        Assert.Equal(VfsFormatId.CriLayla, VfsFormatConsumer.Detect(layla).Id);

        // LIVE save (0x9DCE66C3 LE)
        byte[] live = new byte[] { 0xC3, 0x66, 0xCE, 0x9D, 0x01, 0x00, 0x00, 0x00 };
        Assert.Equal(VfsFormatId.Live, VfsFormatConsumer.Detect(live).Id);

        // Unknown
        byte[] unknown = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        Assert.Equal(VfsFormatId.Unknown, VfsFormatConsumer.Detect(unknown).Id);
    }

    [Fact]
    public void Validate_ValidatesExpectedFormat()
    {
        byte[] g4tx = Encoding.ASCII.GetBytes("G4TX\x20\x00\x00\x00");
        Assert.True(VfsFormatConsumer.Validate(VfsFormatId.G4tx, g4tx));
        Assert.False(VfsFormatConsumer.Validate(VfsFormatId.Cpk, g4tx));
    }
}
