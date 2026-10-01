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

    public static string GetVersion() => "2.0.4-dotnet10";
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
            return CriLayla.IsCompressed(p, out _);
        }
    }

    public static unsafe byte[]? DecompressCrilayla(byte[] data)
    {
        if (data.Length < 16) return null;
        fixed (byte* p = data)
        {
            if (!CriLayla.IsCompressed(p, out int uncompressedSize))
                return null;

            int uncompSizeOfCompData = *(int*)(p + 8);
            int uncompHeaderOffset = *(int*)(p + 12);
            int total = uncompSizeOfCompData + CriLayla.UncompressedDataSize;

            var outArr = new byte[total];
            fixed (byte* outPtr = outArr)
            {
                CriLayla.Decompress(p, outPtr, uncompSizeOfCompData, uncompHeaderOffset);
            }
            return outArr;
        }
    }

    public static byte[] DecryptCpkBlock(byte[] data, ulong offset, uint key = NativeCrypto.IEVRCriKey)
    {
        var copy = (byte[])data.Clone();
        NativeCrypto.DecryptBlock(copy.AsSpan(), (long)offset, key);
        return copy;
    }
}
