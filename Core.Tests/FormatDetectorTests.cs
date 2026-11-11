using IECODE.Core.Formats;
using Xunit;

namespace IECODE.Core.Tests;

public class FormatDetectorTests
{
    [Theory]
    [InlineData(new byte[] { 0x52, 0x44, 0x42, 0x4e }, FormatDetector.Format.CFGBIN)]
    [InlineData(new byte[] { 0x40, 0x55, 0x54, 0x46 }, FormatDetector.Format.UTF)]
    [InlineData(new byte[] { 0x1f, 0x8b }, FormatDetector.Format.GZIP)]
    [InlineData(new byte[] { 0x1f, 0x8b, 8, 0 }, FormatDetector.Format.GZIP)]
    [InlineData(new byte[] { 0x47, 0x34, 0x53, 0x4b }, FormatDetector.Format.G4SK)]
    [InlineData(new byte[] { 0x47, 0x34, 0x50, 0x4b }, FormatDetector.Format.G4PK)]
    public void DetectsMagic(byte[] data, FormatDetector.Format expected)
    {
        Assert.Equal(expected, FormatDetector.Detect(data).Format);
    }
}
