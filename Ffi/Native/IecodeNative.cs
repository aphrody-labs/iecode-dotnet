using System.Runtime.InteropServices;

namespace IECODE.Ffi.Native;

/// <summary>Small source-generated P/Invoke surface over the stable IECODE ABI.</summary>
public static partial class IecodeNative
{
    [LibraryImport("iecode_ffi", EntryPoint = "iecode_core_version")]
    private static partial nint CoreVersionPointer();

    [LibraryImport("iecode_ffi", EntryPoint = "iecode_ffi_schema")]
    private static partial nint SchemaPointer();

    public static string CoreVersion() => ReadUtf8(CoreVersionPointer());

    public static string Schema() => ReadUtf8(SchemaPointer());

    private static string ReadUtf8(nint pointer) =>
        Marshal.PtrToStringUTF8(pointer) ?? throw new InvalidOperationException("Native IECODE returned a null string.");
}
