using IECODE.Core;
using Xunit;

namespace IECODE.Core.Tests;

public sealed class GameDiscoveryTests
{
    [Fact]
    public void NativeTypedConfigNumbersAreCultureIndependent()
    {
        var previous = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
            var config = new IECODE.Core.Formats.Level5.CfgBin.CfgBin();
            config.ImportJsonString("""{"entries":[{"name":"ENTRY_0","variables":[{"type":"Int","value":"-42"},{"type":"Float","value":"1.25"}],"children":[]}]}""");
            Assert.Equal(-42, config.Entries[0].Variables[0].Value);
            Assert.Equal(1.25f, config.Entries[0].Variables[1].Value);
        }
        finally { System.Globalization.CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResolvesGameRootDataDirectoryAndExecutable(bool nestedExecutable)
    {
        string root = Path.Combine(Path.GetTempPath(), "iecode-discovery-" + Guid.NewGuid());
        string data = Path.Combine(root, "data");
        Directory.CreateDirectory(Path.Combine(data, "packs"));
        string executableDirectory = nestedExecutable ? data : root;
        File.WriteAllBytes(Path.Combine(executableDirectory, "nie.exe"), []);
        File.WriteAllBytes(Path.Combine(data, "cpk_list.cfg.bin"), []);
        try
        {
            foreach (string input in new[] { root, data, Path.Combine(executableDirectory, "nie.exe") })
            {
                using var game = new IEVRGame(input);
                Assert.True(game.IsValid);
                Assert.Equal(data, game.DataPath);
                Assert.Equal(executableDirectory, game.GamePath);
                Assert.Equal(Path.Combine(data, "cpk_list.cfg.bin"), game.CpkListPath);
            }
        }
        finally { Directory.Delete(root, true); }
    }
}
