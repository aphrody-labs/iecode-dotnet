using System.Runtime.InteropServices;
using Microsoft.JavaScript.NodeApi;
using IECODE.Core.Crypto;
using IECODE.Core.Formats;
using IECODE.Core.Formats.Criware.CriFs.Compression;
using IECODE.Core.Formats.Level5.CfgBin.Rdbn;
using IECODE.Core.Native;
using IECODE.Core.Util;

namespace IECODE.Ffi;

/// <summary>Runtime metadata and JSExport methods for official node-api-dotnet host integration.</summary>
[JSExport]
public static class NodeApiBridge
{
    public const string NpmPackage = "node-api-dotnet";
    public const string NuGetPackage = "Microsoft.JavaScript.NodeApi";
    public const string NativeContract = "iecode.ffi/v1";

    public static string GetVersion() => Native.IecodeNative.RuntimeVersion;
    public static string GetSchema() => NativeContract;
    public static int GetStatus() => 1;

    public static uint ComputeLevel5Crc32(byte[] data) => Level5Hash.Hash(data);

    public static int DetectFormat(byte[] data) => (int)FormatDetector.Detect(data).Format;

    public static string DetectFormatName(byte[] data) => FormatDetector.Detect(data).Format.ToString();

    public static bool IsRdbn(byte[] data) => RdbnReader.IsRdbn(data);

    public static unsafe bool IsCrilayla(byte[] data)
    {
        if (data.Length < 16) return false;
        fixed (byte* p = data)
        {
            return CriLayla.IsCompressed(data, out _);
        }
    }

    public static byte[]? DecompressCrilayla(byte[] data)
    {
        try { return CriLayla.DecompressToArray(data); }
        catch (InvalidDataException) { return null; }
    }

    public static byte[] DecryptCpkBlock(byte[] data, ulong offset, uint key = NativeCrypto.IEVRCriKey)
    {
        var copy = (byte[])data.Clone();
        NativeCrypto.DecryptBlock(copy.AsSpan(), (long)offset, key);
        return copy;
    }
}
