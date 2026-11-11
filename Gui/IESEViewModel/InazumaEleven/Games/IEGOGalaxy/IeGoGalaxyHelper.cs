using System.Runtime.InteropServices;

namespace IESEViewModel.InazumaEleven.Games.IEGOGalaxy;

public static class IeGoGalaxyHelper
{
    public const long NameOffset = 0x3C;

    public const long TeamNameOffset = 0x5C;

    public const long TimeOffset = 0x20;

    public const long LinkOffset = 0x90B4;

    public const long ChapterOffset = 0x9F1C;

    public const long MoneyOffset = 0x268D0;

    public const long CoinOffset = 0x26CC8;

    public const long PlayRecordsOffset = 0x8FEB;

    public const long PlayerDataOffset = 0xF83C;

    public const long PlayerIndexOffset = 0x26E28;

    public const int MaximumPlayer = 336;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MoveBlock
    {
        public uint MoveID;
        public byte MoveLevel;
        public byte MoveUsage;
        public bool MoveLearned;
        public short MoveUnk;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerBlock
    {
        public uint TotemID;
        public int Index;
        public uint ID;
        public int MiximaxIndex;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x0C)]
        public byte[] EmptyBlock1;
        public short GP;
        public short TP;
        public short Freedom;
        public byte Level;
        public byte Unk1;
        public byte MixiMaxMove1Index;
        public byte MixiMaxMove2Index;
        public byte InvokeStatus;
        public byte Style;
        public short ParticipationPoint;
        public short ScorePoint;
        public short WonBattle;
        public short Unk2;
        public int Unk3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public short[] InvestedPoint;
        public uint AvatarID;
        public byte AvatarLevel;
        public byte AvatarUsage;
        public byte Unk4;
        public byte Unk5;
        public int BootsIndex;
        public int BraceletIndex;
        public int PendantIndex;
        public int GlovesIndex;
        public int Unk6;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public MoveBlock[] Moves;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x64)]
        public byte[] Unk7;
    }

    public const long ItemBlockGroup1Offset = 0xA394;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemBlockGroup1
    {
        public int Index;
        public uint ID;
        public int Quantity;
    }

    public const long ItemBlockGroup2Offset = 0xC020;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemBlockGroup2
    {
        public int Index;
        public uint ID;
        public int Quantity;
        public int QuantityEquiped;
    }

    public const long ItemBlockGroup3Offset = 0xDC6C;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemBlockGroup3
    {
        public int Index;
        public uint ID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Team
    {
        public int CoachIndex;
        public int FormationIndex;
        public int KitIndex;
        public int EmblemIndex;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] PlayersFormationIndex;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] PlayerKitNumber;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TeamPlayer
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public int[] PlayersIndex;
    }
}