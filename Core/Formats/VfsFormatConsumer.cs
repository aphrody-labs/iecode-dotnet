using System;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace IECODE.Core.Formats;

/// <summary>
/// Authoritative VFS Format Identifiers matching the canonical 28 VFS formats registry.
/// </summary>
public enum VfsFormatId : uint
{
    Unknown = 0,
    G4tx = 1,
    G4md = 2,
    G4mg = 3,
    G4mt = 4,
    G4sk = 5,
    G4pk = 6,
    G4ra = 7,
    Agi = 8,
    Objb = 9,
    Col = 10,
    CfgBin = 11,
    Mevs = 12,
    G4cm = 13,
    Cpk = 14,
    Awb = 15,
    Utf = 16,
    Dds = 17,
    Png = 18,
    Riff = 19,
    Xpck = 20,
    Xfsa = 21,
    CriLayla = 22,
    Acb = 23,
    Usm = 24,
    G4nv = 25,
    ColMesh = 26,
    Cols = 27,
    Live = 28
}

/// <summary>
/// Immutable, strongly-typed format descriptor for Victory Road VFS assets.
/// </summary>
public sealed record VfsFormatDescriptor(
    VfsFormatId Id,
    string Name,
    string MagicHex,
    string MagicAscii,
    string Extension,
    string MimeType,
    string Category,
    bool IsBigEndian,
    bool EncryptionSupported,
    bool CompressionSupported,
    string Description
);

/// <summary>
/// Cross-language VFS format consumer and validator for .NET 10.
/// </summary>
public static class VfsFormatConsumer
{
    private static readonly FrozenDictionary<VfsFormatId, VfsFormatDescriptor> Registry;
    private static readonly VfsFormatDescriptor UnknownDescriptor;

    static VfsFormatConsumer()
    {
        UnknownDescriptor = new VfsFormatDescriptor(
            VfsFormatId.Unknown,
            "?",
            "0x00000000",
            "NONE",
            ".bin",
            "application/octet-stream",
            "system",
            false,
            false,
            false,
            "Unknown or unparsed binary format"
        );

        var list = new Dictionary<VfsFormatId, VfsFormatDescriptor>
        {
            [VfsFormatId.G4tx] = new(VfsFormatId.G4tx, "G4TX", "0x58543447", "G4TX", ".g4tx", "image/x-level5-g4tx", "texture", false, false, true, "Level-5 Texture Container (DDS BCn textures + atlas sprite subregions)"),
            [VfsFormatId.G4md] = new(VfsFormatId.G4md, "G4MD", "0x444D3447", "G4MD", ".g4md", "model/x-level5-g4md", "model", false, false, true, "Level-5 3D Model Data (meshes, bone indices, vertex buffers, materials)"),
            [VfsFormatId.G4mg] = new(VfsFormatId.G4mg, "G4MG", "0x474D3447", "G4MG", ".g4mg", "model/x-level5-g4mg", "model", false, false, true, "Level-5 Model Group (collection of submeshes and LOD levels)"),
            [VfsFormatId.G4mt] = new(VfsFormatId.G4mt, "G4MT", "0x544D3447", "G4MT", ".g4mt", "model/x-level5-g4mt", "model", false, false, false, "Level-5 Material Properties (shaders, blend modes, diffuse/specular colors)"),
            [VfsFormatId.G4sk] = new(VfsFormatId.G4sk, "G4SK", "0x4B533447", "G4SK", ".g4sk", "model/x-level5-g4sk", "model", false, false, false, "Level-5 Skeleton & Bone Hierarchy (quaternions, bind poses, transform trees)"),
            [VfsFormatId.G4pk] = new(VfsFormatId.G4pk, "G4PK", "0x4B503447", "G4PK", ".g4pk", "application/x-level5-g4pk", "container", false, false, true, "Level-5 Package Container (multi-resource bundle for characters and animations)"),
            [VfsFormatId.G4ra] = new(VfsFormatId.G4ra, "G4RA", "0x41523447", "G4RA", ".g4ra", "application/x-level5-g4ra", "container", false, false, true, "Level-5 Resource Archive (compact asset bundle indexed in nie.exe)"),
            [VfsFormatId.Agi] = new(VfsFormatId.Agi, "AGI", "0x20494741", "AGI ", ".agi", "application/x-level5-agi", "model", false, false, true, "Level-5 Animation Graphic Interface (timeline tracks, animation nodes)"),
            [VfsFormatId.Objb] = new(VfsFormatId.Objb, "OBJB", "0x424A424F", "OBJB", ".objb", "application/x-level5-objb", "config", false, false, true, "Level-5 Object Binary (scene layout descriptors, actor placements)"),
            [VfsFormatId.Col] = new(VfsFormatId.Col, "PXCL", "0x4C435850", "PXCL", ".pxcl", "application/x-level5-pxcl", "collision", false, false, false, "Level-5 Pixel Collision & Physics Hull (hitboxes, boundary colliders)"),
            [VfsFormatId.CfgBin] = new(VfsFormatId.CfgBin, "RDBN", "0x4E424452", "RDBN", ".cfg.bin", "application/x-level5-rdbn", "config", false, true, true, "Level-5 Relational Database Binary (T2B structured binary data sheets)"),
            [VfsFormatId.Mevs] = new(VfsFormatId.Mevs, "MEVS", "0x5346454D", "MEVS", ".mevbin", "application/x-level5-mevs", "script", false, false, true, "Level-5 Motion Event Script (choreographed cutscenes, camera paths, audio cues)"),
            [VfsFormatId.G4cm] = new(VfsFormatId.G4cm, "G4CM", "0x4D433447", "G4CM", ".g4cm", "application/x-level5-g4cm", "script", false, false, false, "Level-5 Camera Track & Spline Path (cinematic parameters, FOV transitions)"),
            [VfsFormatId.Cpk] = new(VfsFormatId.Cpk, "CPK", "0x204B5043", "CPK ", ".cpk", "application/x-crifs-cpk", "container", true, true, true, "CRI Middleware File Package (TOC, ITOC, ETOC, @UTF directory descriptors)"),
            [VfsFormatId.Awb] = new(VfsFormatId.Awb, "AFS2", "0x32534641", "AFS2", ".afs2", "audio/x-cri-afs2", "audio", false, true, false, "Criware Audio Archive (AWB wavebank containers)"),
            [VfsFormatId.Utf] = new(VfsFormatId.Utf, "@UTF", "0x00465455", "@UTF", ".utf", "application/x-cri-utf", "container", true, true, false, "CRI Universal Table Format (schema, column flags, string pools, row vectors)"),
            [VfsFormatId.Dds] = new(VfsFormatId.Dds, "DDS", "0x20534444", "DDS ", ".dds", "image/vnd-ms.dds", "texture", false, false, false, "DirectDraw Surface (raw BC1/BC2/BC3/BC4/BC5/BC7 compressed texture payload)"),
            [VfsFormatId.Png] = new(VfsFormatId.Png, "PNG", "0x474E5089", "PNG\r", ".png", "image/png", "texture", true, false, true, "Portable Network Graphics (decoded atlas glyphs, UI elements, web exports)"),
            [VfsFormatId.Riff] = new(VfsFormatId.Riff, "RIFF", "0x46464952", "RIFF", ".wav", "audio/wav", "audio", false, false, false, "Resource Interchange File Format (PCM and ADPCM audio tracks)"),
            [VfsFormatId.Xpck] = new(VfsFormatId.Xpck, "XPCK", "0x4B533447", "XPCK", ".xpck", "application/x-level5-xpck", "container", false, false, true, "Level-5 XPCK Package (legacy DS/3DS/Switch container format)"),
            [VfsFormatId.Xfsa] = new(VfsFormatId.Xfsa, "XFSA", "0x41534658", "XFSA", ".xfsa", "application/x-level5-xfsa", "container", false, false, true, "Level-5 XFSA Archive (file system archive with hash-addressed records)"),
            [VfsFormatId.CriLayla] = new(VfsFormatId.CriLayla, "CRILAYLA", "0x4159414C", "LAYL", ".crilayla", "application/x-cri-layla", "container", false, false, true, "CRI Layla Reverse-Bitstream LZSS Compressed Container (16-byte header)"),
            [VfsFormatId.Acb] = new(VfsFormatId.Acb, "@ACB", "0x46424341", "@ACB", ".acb", "audio/x-cri-acb", "audio", true, true, false, "Criware Audio Cue Sheet (cue mappings, sound triggers, DSP effects)"),
            [VfsFormatId.Usm] = new(VfsFormatId.Usm, "USM", "0x20534D43", "CRID", ".usm", "video/x-cri-usm", "video", true, true, false, "CRI Movie 2 Container (video stream, multi-track audio, subtitle packets)"),
            [VfsFormatId.G4nv] = new(VfsFormatId.G4nv, "NAVM", "0x4D56414E", "NAVM", ".g4nv", "application/x-level5-g4nv", "system", false, false, false, "Level-5 Navigation Mesh (stadium and field locomotion polygons)"),
            [VfsFormatId.ColMesh] = new(VfsFormatId.ColMesh, "COL", "0x004C4F43", "COL\0", ".col", "application/x-level5-col", "collision", false, false, false, "Level-5 Collision mesh data"),
            [VfsFormatId.Cols] = new(VfsFormatId.Cols, "COLS", "0x534C4F43", "COLS", ".cols", "application/x-level5-cols", "collision", false, false, false, "Level-5 Collision Set (stadium boundaries, goal posts, ball bouncing volumes)"),
            [VfsFormatId.Live] = new(VfsFormatId.Live, "LIVE", "0x9DCE66C3", "LIVE", ".live", "application/x-level5-live", "system", false, true, false, "Level-5 Lives Save Container (CRC32-keyed XOR cipher, 0xEEFF blobs)")
        };

        Registry = list.ToFrozenDictionary();
    }

    /// <summary>
    /// Detects format from magic bytes in a read-only byte span.
    /// </summary>
    public static VfsFormatDescriptor Detect(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
            return UnknownDescriptor;

        // 8-byte magics
        if (data.Length >= 8)
        {
            if (data.StartsWith("CRILAYLA"u8))
                return Registry[VfsFormatId.CriLayla];
            if (data.SequenceEqual(new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A }))
                return Registry[VfsFormatId.Png];
        }

        // 4-byte magics
        if (data.StartsWith("G4TX"u8)) return Registry[VfsFormatId.G4tx];
        if (data.StartsWith("G4MD"u8)) return Registry[VfsFormatId.G4md];
        if (data.StartsWith("G4MG"u8)) return Registry[VfsFormatId.G4mg];
        if (data.StartsWith("G4MT"u8)) return Registry[VfsFormatId.G4mt];
        if (data.StartsWith("G4SK"u8)) return Registry[VfsFormatId.G4sk];
        if (data.StartsWith("G4PK"u8)) return Registry[VfsFormatId.G4pk];
        if (data.StartsWith("G4RA"u8)) return Registry[VfsFormatId.G4ra];
        if (data.StartsWith("AGI "u8)) return Registry[VfsFormatId.Agi];
        if (data.StartsWith("OBJB"u8)) return Registry[VfsFormatId.Objb];
        if (data.StartsWith("PXCL"u8)) return Registry[VfsFormatId.Col];
        if (data.StartsWith("RDBN"u8)) return Registry[VfsFormatId.CfgBin];
        if (data.StartsWith("MEVS"u8)) return Registry[VfsFormatId.Mevs];
        if (data.StartsWith("G4CM"u8)) return Registry[VfsFormatId.G4cm];
        if (data.StartsWith("CPK "u8)) return Registry[VfsFormatId.Cpk];
        if (data.StartsWith("AFS2"u8)) return Registry[VfsFormatId.Awb];
        if (data.StartsWith("@UTF"u8)) return Registry[VfsFormatId.Utf];
        if (data.StartsWith("DDS "u8)) return Registry[VfsFormatId.Dds];
        if (data.StartsWith("RIFF"u8)) return Registry[VfsFormatId.Riff];
        if (data.StartsWith("XPCK"u8)) return Registry[VfsFormatId.Xpck];
        if (data.StartsWith("XFSA"u8)) return Registry[VfsFormatId.Xfsa];
        if (data.StartsWith("@ACB"u8)) return Registry[VfsFormatId.Acb];
        if (data.StartsWith("CRID"u8)) return Registry[VfsFormatId.Usm];
        if (data.StartsWith("NAVM"u8)) return Registry[VfsFormatId.G4nv];
        if (data.StartsWith("COLS"u8)) return Registry[VfsFormatId.Cols];
        if (data.StartsWith("COL\0"u8)) return Registry[VfsFormatId.ColMesh];

        // Integer magic for Live saves
        uint magicLe = BinaryPrimitives.ReadUInt32LittleEndian(data);
        if (magicLe == 0x9DCE66C3)
            return Registry[VfsFormatId.Live];

        return UnknownDescriptor;
    }

    /// <summary>
    /// Gets descriptor by format ID.
    /// </summary>
    public static VfsFormatDescriptor Get(VfsFormatId id)
    {
        return Registry.TryGetValue(id, out var desc) ? desc : UnknownDescriptor;
    }

    /// <summary>
    /// Returns all 28 canonical VFS format descriptors.
    /// </summary>
    public static IReadOnlyCollection<VfsFormatDescriptor> ListAll()
    {
        return Registry.Values;
    }

    /// <summary>
    /// Validates whether a byte span matches the expected format header.
    /// </summary>
    public static bool Validate(VfsFormatId id, ReadOnlySpan<byte> data)
    {
        var detected = Detect(data);
        return detected.Id == id;
    }
}
