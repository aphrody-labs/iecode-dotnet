using System;
using System.Collections.Generic;

namespace IECODE.Core;

/// <summary>
/// Verified cryptographic constants and polynomial values in nie.exe.
/// </summary>
public sealed record NieCryptoConstant(
    string Name,
    string HexValue,
    uint IntValue,
    string Usage,
    bool VerifiedInGameDump
);

/// <summary>
/// Key function entry point and program counter in nie.exe.
/// </summary>
public sealed record NieMemoryOffset(
    string Symbol,
    string AddressHex,
    ulong RuntimePc,
    string Subsystem,
    string Description,
    string CallingConvention,
    string Signature
);

/// <summary>
/// Reconstructed RTTI class identified in nie.exe.
/// </summary>
public sealed record NieRttiClass(
    string Name,
    string Namespace,
    string VtableOffsetHex,
    IReadOnlyList<string> BaseClasses,
    string Description
);

/// <summary>
/// Critical runtime memory structure layout in nie.exe.
/// </summary>
public sealed record NieMemoryStructure(
    string Name,
    string SizeHex,
    int SizeBytes,
    string Description,
    IReadOnlyDictionary<string, string> Fields
);

/// <summary>
/// Authoritative, factual reverse-engineering specification for nie.exe (Inazuma Eleven: Victory Road).
/// Cross-language contract matching Rust aphrody-nie-re, TypeScript @rosegriffon/inagle, and Python niepy.
/// </summary>
public sealed class NieReMap
{
    public string TargetBinary { get; init; } = "nie.exe";
    public string InternalName { get; init; } = "nie1v2.exe";
    public string Architecture { get; init; } = "x86_64";
    public string Format { get; init; } = "PE32+";
    public string BaseAddressHex { get; init; } = "0x140000000";
    public ulong BaseAddress { get; init; } = 0x140000000;
    public int TotalFunctionsCount { get; init; } = 117068;
    public int TotalStringsCount { get; init; } = 72151;
    public string Compiler { get; init; } = "MSVC 19.38 / VS 2022 v17.8";

    public IReadOnlyList<NieCryptoConstant> CryptoConstants { get; init; } = new List<NieCryptoConstant>
    {
        new(
            "VIOLA_FIXED_KEY",
            "0x1717E18E",
            0x1717E18Eu,
            "Fixed CRI XOR encryption key for cpk_list.cfg.bin and standalone Level-5 packages (discovered by Viola).",
            true
        ),
        new(
            "LEVEL5_CRC32_POLYNOMIAL",
            "0xEDB88320",
            0xEDB88320u,
            "Standard IEEE 802.3 reflected CRC32 polynomial for RDBN hash dictionaries and CPK filename key derivation.",
            true
        ),
        new(
            "CRI_ENCRYPTED_TABLE_MAGIC",
            "0xF5F39E1F",
            0xF5F39E1Fu,
            "Magic header sentinel indicating an encrypted CRI @UTF table block.",
            true
        ),
        new(
            "LIVES_SAVE_HEADER_MAGIC",
            "0x9DCE66C3",
            0x9DCE66C3u,
            "Sentinel u32 LE placed at offset 0x00 of decrypted Lives save containers (002AB8F4-*).",
            true
        ),
        new(
            "LIVES_SAVE_CONST2",
            "0x2EC3031F",
            0x2EC3031Fu,
            "Constant u32 LE at offset 0x08 of decrypted Lives save header (bytes: 1F 03 C3 2E).",
            true
        ),
        new(
            "LIVES_BLOB_MAGIC",
            "0xEEFF",
            0xEEFFu,
            "Big-endian u16 sentinel starting every concatenated payload blob in Lives saves.",
            true
        ),
        new(
            "STEAM_APP_ID",
            "0x2AB8F4",
            2799860u,
            "Official Steam App ID for Inazuma Eleven: Victory Road (hexadecimal prefix for save files: 002AB8F4).",
            true
        )
    };

    public IReadOnlyList<NieMemoryOffset> CriticalOffsets { get; init; } = new List<NieMemoryOffset>
    {
        new(
            "GameTickLoop",
            "0x1406d5840",
            0x1406d5840UL,
            "game-loop",
            "Main game tick loop orchestrating 60 Hz physics update, animation blending, and entity state synchronization.",
            "__fastcall",
            "void __fastcall GameTickLoop(void* engineContext, float deltaTime)"
        ),
        new(
            "CharaControllerUpdate",
            "0x1404ecd60",
            0x1404ecd60UL,
            "controller",
            "Player movement, kinematic resolution, ball possession calculations, and user input mapping.",
            "__fastcall",
            "void __fastcall CharaControllerUpdate(void* playerEntity, void* inputState, float dt)"
        ),
        new(
            "CriFsStreamDecryptBlock",
            "0x1405a1120",
            0x1405a1120UL,
            "crypto",
            "Position-aware XOR stream cipher implementation decrypting 4-byte quads based on file offset.",
            "__fastcall",
            "void __fastcall CriFsStreamDecryptBlock(uint8_t* buffer, size_t length, uint64_t fileOffset, uint32_t key)"
        ),
        new(
            "CriLaylaDecompressStream",
            "0x1405b3890",
            0x1405b3890UL,
            "vfs",
            "Reverse bitstream LZSS decompressor processing byte-by-byte from end of stream into preallocated buffer.",
            "__fastcall",
            "int32_t __fastcall CriLaylaDecompressStream(const uint8_t* input, uint8_t* output, size_t uncompressedSize)"
        ),
        new(
            "RdbnTableLookup",
            "0x1403c8040",
            0x1403c8040UL,
            "vfs",
            "Binary search in RDBN table header resolving 32-bit CRC hashes to column offsets and row pointers.",
            "__fastcall",
            "void* __fastcall RdbnTableLookup(const uint8_t* rdbnBuffer, uint32_t tableHash, uint32_t entryHash)"
        ),
        new(
            "G4txDecodeTexture",
            "0x1404b2a80",
            0x1404b2a80UL,
            "render",
            "G4TX header parsing, DDS texture extraction, and GPU resource registration.",
            "__fastcall",
            "void* __fastcall G4txDecodeTexture(const uint8_t* g4txData, size_t dataSize, int32_t textureIndex)"
        ),
        new(
            "LuaHostInit",
            "0x14072f910",
            0x14072f910UL,
            "lua",
            "Initializes Lua 5.1/5.2 C host environment for UI animations, dialogue branching, and menu state machines.",
            "__fastcall",
            "void* __fastcall LuaHostInit(void* memoryPool)"
        ),
        new(
            "CRand_MT19937_Next",
            "0x14081c200",
            0x14081c200UL,
            "prng",
            "Deterministic Mersenne Twister MT19937 pseudorandom number generator step function.",
            "__fastcall",
            "uint32_t __fastcall CRand_MT19937_Next(void* crandState)"
        )
    };

    public IReadOnlyList<NieRttiClass> RttiClasses { get; init; } = new List<NieRttiClass>
    {
        new(
            "Object",
            "Level5::Engine",
            "0x140AC4000",
            Array.Empty<string>(),
            "Polymorphic base class for all engine actors, components, and resource handles."
        ),
        new(
            "CharaController",
            "Level5::Game",
            "0x140AC68A0",
            new[] { "Level5::Engine::Object" },
            "High-level player controller managing motion states (dash, dribble, slide tackle, shoot charge)."
        ),
        new(
            "BallPhysics",
            "Level5::Game",
            "0x140AC7120",
            new[] { "Level5::Engine::Object" },
            "Aerodynamic and turf friction simulation for the soccer ball, including spin and pass trajectories."
        ),
        new(
            "VfsManager",
            "Level5::Resource",
            "0x140AD1400",
            new[] { "Level5::Engine::Object" },
            "Global filesystem coordinator mounting CPK pack archives and routing relative logical paths."
        ),
        new(
            "CpkArchive",
            "Level5::Resource",
            "0x140AD2200",
            new[] { "Level5::Engine::Object" },
            "CRI Middleware CPK container handle with memory-mapped file views and decryption context."
        ),
        new(
            "RenderScene",
            "Level5::Graphics",
            "0x140AE0100",
            new[] { "Level5::Engine::Object" },
            "Scene graph renderer managing camera viewports, shadow cascades, and lighting buffers."
        ),
        new(
            "SkinnedMesh",
            "Level5::Graphics",
            "0x140AE1880",
            new[] { "Level5::Engine::Object" },
            "GPU skinning mesh processor packing 32 bones into 96 vec4 matrix registers (renderObj + 0x1D98)."
        ),
        new(
            "SoundPlayer",
            "CriWare::Atom",
            "0x140AF4900",
            Array.Empty<string>(),
            "CRI Atom Craft sound playback service managing voice categories, BGM ducking, and 3D positional audio."
        )
    };

    public IReadOnlyList<NieMemoryStructure> MemoryStructures { get; init; } = new List<NieMemoryStructure>
    {
        new(
            "BonePalette",
            "0x600",
            1536,
            "Uniform buffer layout for skinned character meshes (32 bones × 3 vec4 matrices = 96 vec4, 1536 bytes).",
            new Dictionary<string, string>
            {
                ["0x000..0x600"] = "Array of 32 mat3x4 (row-major affine transforms without homogeneous row [0,0,0,1])."
            }
        ),
        new(
            "FontGlyph",
            "0x0C",
            12,
            "Single glyph descriptor in Level-5 font texture atlas.",
            new Dictionary<string, string>
            {
                ["0x00"] = "uint16_t u (atlas X coordinate)",
                ["0x02"] = "uint16_t v (atlas Y coordinate)",
                ["0x04"] = "uint8_t width",
                ["0x05"] = "uint8_t height",
                ["0x06"] = "int8_t offsetX",
                ["0x07"] = "int8_t offsetY",
                ["0x08"] = "int8_t advanceX",
                ["0x09"] = "uint8_t pageIndex (0 to 3)",
                ["0x0A"] = "uint16_t reserved/padding"
            }
        ),
        new(
            "RdbnHeader",
            "0x50",
            80,
            "RDBN / cfg.bin 80-byte file header descriptor.",
            new Dictionary<string, string>
            {
                ["0x00..0x04"] = "uint32_t magic = 0x4E424452 ('RDBN')",
                ["0x04..0x06"] = "uint16_t headerSize = 0x50",
                ["0x06..0x0A"] = "uint32_t version = 0x01",
                ["0x0A..0x0E"] = "uint32_t tableCount",
                ["0x0E..0x12"] = "uint32_t stringPoolSize",
                ["0x12..0x16"] = "uint32_t dataSectionSize",
                ["0x16..0x50"] = "CRC32 checksums, schema flags, and table offset descriptors"
            }
        )
    };

    private static readonly NieReMap Instance = new();

    /// <summary>
    /// Returns the authoritative singleton reverse-engineering map for nie.exe.
    /// </summary>
    public static NieReMap GetAuthoritativeMap() => Instance;
}
