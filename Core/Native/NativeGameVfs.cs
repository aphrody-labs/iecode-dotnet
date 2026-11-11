using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace IECODE.Core.Native;

/// <summary>Thin managed adapter over the maintained Rust NIE VFS C ABI.</summary>
public sealed class NativeGameVfs : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeBytes { public nint Pointer; public nuint Length; public nuint Capacity; }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint Open([MarshalAs(UnmanagedType.LPUTF8Str)] string directory);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Free(nint handle);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void FreeBytes(nint pointer, nuint length, nuint capacity);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ulong CountEntries(nint handle);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ListRange(nint handle, ulong offset, ulong limit, out NativeBytes bytes);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ReadFile(nint handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, out NativeBytes bytes);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DecodeBytes(nint pointer, nuint length, out NativeBytes bytes);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void LastError(out NativeBytes bytes);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ResolveGameSource([MarshalAs(UnmanagedType.LPUTF8Str)] string? hint, out NativeBytes bytes);

    private nint _library;
    private nint _handle;
    private readonly Free _free;
    private readonly FreeBytes _freeBytes;
    private readonly CountEntries _count;
    private readonly ListRange _list;
    private readonly ReadFile _read;
    private readonly DecodeBytes _decode;
    private readonly LastError _lastError;

    public string LibraryPath { get; }
    public string DataPath { get; }
    public ulong Count { get { EnsureOpen(); return _count(_handle); } }

    /// <summary>Resolve through Rust without mounting archives or contacting a remote host.</summary>
    public static JsonElement Locate(string? hint = null)
    {
        nint library = NativeLibrary.Load(ResolveLibrary());
        try
        {
            T Symbol<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library, name));
            var free = Symbol<FreeBytes>("nie_bytes_free_fields");
            Symbol<ResolveGameSource>("nie_game_source_resolve_out")(hint, out var bytes);
            if (bytes.Pointer == 0)
            {
                Symbol<LastError>("nie_last_error_out")(out var error);
                throw new DirectoryNotFoundException(Encoding.UTF8.GetString(CopyAndFree(error, free)));
            }
            using var document = JsonDocument.Parse(CopyAndFree(bytes, free));
            return document.RootElement.Clone();
        }
        finally { NativeLibrary.Free(library); }
    }

    public static string ResolveSource(string? hint = null) => Locate(hint).GetProperty("source").GetString()
        ?? throw new InvalidDataException("Native discovery returned no source.");

    public static bool IsRemoteSource(string source) => Uri.TryCreate(source, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https" or "ssh" or "sftp";

    public NativeGameVfs(string? gamePath = null)
    {
        DataPath = ResolveSource(gamePath);
        LibraryPath = ResolveLibrary();
        _library = NativeLibrary.Load(LibraryPath);
        try
        {
            var open = Symbol<Open>("nie_vfs_open");
            _free = Symbol<Free>("nie_vfs_free");
            _freeBytes = Symbol<FreeBytes>("nie_bytes_free_fields");
            _count = Symbol<CountEntries>("nie_vfs_count");
            _list = Symbol<ListRange>("nie_vfs_list_range_json_out");
            _read = Symbol<ReadFile>("nie_vfs_read_out");
            _decode = Symbol<DecodeBytes>("nie_decode_json_out");
            _lastError = Symbol<LastError>("nie_last_error_out");
            _handle = open(DataPath);
            if (_handle == 0) throw new InvalidDataException(Error("Unable to mount the installed game VFS."));
        }
        catch { NativeLibrary.Free(_library); _library = 0; throw; }
    }

    private T Symbol<T>(string name) where T : Delegate =>
        Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(_library, name));

    private static string ResolveLibrary()
    {
        string? configured = Environment.GetEnvironmentVariable("NIE_FFI_PATH") ?? Environment.GetEnvironmentVariable("IECODE_LIB_PATH");
        if (!string.IsNullOrWhiteSpace(configured))
            return File.Exists(configured) ? Path.GetFullPath(configured)
                : throw new FileNotFoundException("NIE_FFI_PATH or IECODE_LIB_PATH must name an existing native library.", configured);
        string name = OperatingSystem.IsWindows() ? "iecode.dll"
            : OperatingSystem.IsMacOS() ? "libiecode.dylib" : "libiecode.so";
        var directories = new[] { AppContext.BaseDirectory,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin") }
            .Concat((Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator).Where(p => p.Length > 0));
        foreach (string directory in directories)
        {
            string path = Path.Combine(directory, name);
            if (File.Exists(path)) return Path.GetFullPath(path);
        }
        throw new FileNotFoundException("Install the Rust NIE native library in the executable directory or PATH, or set NIE_FFI_PATH.");
    }

    private byte[] CopyAndFree(NativeBytes bytes) => CopyAndFree(bytes, _freeBytes);

    private static byte[] CopyAndFree(NativeBytes bytes, FreeBytes freeBytes)
    {
        try
        {
            if (bytes.Length == 0) return [];
            if (bytes.Pointer == 0 || bytes.Length > int.MaxValue)
                throw new InvalidDataException("Invalid or oversized native VFS buffer.");
            var result = new byte[(int)bytes.Length];
            Marshal.Copy(bytes.Pointer, result, 0, result.Length);
            return result;
        }
        finally { if (bytes.Pointer != 0) freeBytes(bytes.Pointer, bytes.Length, bytes.Capacity); }
    }

    private string Error(string fallback)
    {
        _lastError(out var bytes);
        string error = Encoding.UTF8.GetString(CopyAndFree(bytes));
        return error.Length == 0 ? fallback : error;
    }

    public IEnumerable<JsonElement> Entries()
    {
        EnsureOpen();
        ulong count = Count;
        for (ulong offset = 0; offset < count; offset += 1000)
        {
            _list(_handle, offset, Math.Min(1000, count - offset), out var bytes);
            using var document = JsonDocument.Parse(CopyAndFree(bytes));
            foreach (var entry in document.RootElement.EnumerateArray()) yield return entry.Clone();
        }
    }

    public byte[] Read(string path)
    {
        EnsureOpen();
        if (path.Contains('\0')) throw new ArgumentException("Virtual paths cannot contain NUL.", nameof(path));
        _read(_handle, path.Replace('\\', '/'), out var bytes);
        bool failed = bytes.Pointer == 0;
        byte[] result = CopyAndFree(bytes);
        if (failed) throw new IOException(Error($"Virtual file could not be read: {path}"));
        return result;
    }

    public unsafe string Decode(byte[] data, bool typedConfig = false)
    {
        EnsureOpen();
        DecodeBytes decode = typedConfig ? Symbol<DecodeBytes>("nie_cfgbin_typed_json_out") : _decode;
        fixed (byte* pointer = data)
        {
            decode((nint)pointer, (nuint)data.Length, out var bytes);
            byte[] decoded = CopyAndFree(bytes);
            if (decoded.Length == 0) throw new InvalidDataException(Error("The virtual file format cannot be decoded."));
            return Encoding.UTF8.GetString(decoded);
        }
    }

    private void EnsureOpen() => ObjectDisposedException.ThrowIf(_handle == 0, this);
    public void Dispose()
    {
        if (_handle != 0) { _free(_handle); _handle = 0; }
        if (_library != 0) { NativeLibrary.Free(_library); _library = 0; }
    }
}
