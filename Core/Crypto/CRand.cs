using System;

namespace IECODE.Core.Crypto;

/// <summary>
/// Authoritative port of the Inazuma Eleven: Victory Road PRNG (lives::CRand / nie.exe:0x14081c200).
/// Canonical 32-bit Mersenne Twister MT19937 with Lemire bounded rejection.
/// </summary>
public sealed class CRand
{
    private const int N = 624;
    private const int M = 397;
    private const uint MatrixA = 0x9908B0DF;
    private const uint UpperMask = 0x80000000;
    private const uint LowerMask = 0x7FFFFFFF;
    private const uint InitMult = 1812433253; // 0x6C078965

    private readonly uint[] _mt = new uint[N];
    private int _index;

    /// <summary>
    /// Initializes the Mersenne Twister generator with a 32-bit seed.
    /// Default seed in nie.exe is 5489 (0x1571).
    /// </summary>
    public CRand(uint seed = 5489)
    {
        _mt[0] = seed;
        for (uint i = 1; i < N; i++)
        {
            uint prev = _mt[i - 1];
            _mt[i] = (InitMult * (prev ^ (prev >> 30)) + i);
        }
        _index = N; // Force generation on first draw
    }

    /// <summary>
    /// Convenience factory folding a 64-bit seed into a 32-bit seed via XOR.
    /// </summary>
    public static CRand FromU64(ulong seed)
    {
        return new CRand((uint)(seed ^ (seed >> 32)));
    }

    private void Generate()
    {
        for (int i = 0; i < N; i++)
        {
            uint y = (_mt[i] & UpperMask) | (_mt[(i + 1) % N] & LowerMask);
            uint next = _mt[(i + M) % N] ^ (y >> 1);
            if ((y & 1) != 0)
            {
                next ^= MatrixA;
            }
            _mt[i] = next;
        }
        _index = 0;
    }

    /// <summary>
    /// Generates the next pseudo-random 32-bit unsigned integer with canonical MT19937 tempering.
    /// </summary>
    public uint NextU32()
    {
        if (_index >= N)
        {
            Generate();
        }

        uint y = _mt[_index++];

        // Canonical MT19937 tempering (d = 0xFFFFFFFF, verified in nie.exe:0x1402bf031)
        y ^= y >> 11;
        y ^= (y << 7) & 0x9D2C5680;
        y ^= (y << 15) & 0xEFC60000;
        y ^= y >> 18;

        return y;
    }

    /// <summary>
    /// Generates a bounded pseudo-random integer in [0, bound) using Lemire's method with rejection.
    /// Ported byte-exactly from lives::CRand::vmethod_4(bound) in nie.exe.
    /// If bound == 0, returns the raw next uint (matching nie.exe behavior).
    /// </summary>
    public uint Bounded(uint bound)
    {
        if (bound == 0)
        {
            return NextU32();
        }

        ulong product = (ulong)NextU32() * bound;
        uint low = (uint)product;

        if (low < bound)
        {
            uint threshold = (uint)((0u - bound) % bound);
            while (low < threshold)
            {
                product = (ulong)NextU32() * bound;
                low = (uint)product;
            }
        }

        return (uint)(product >> 32);
    }

    /// <summary>
    /// Generates a single-precision floating point number in [0.0, 1.0) using the top 24 bits.
    /// </summary>
    public float NextF32()
    {
        uint bits = NextU32() >> 8;
        return (float)bits / 16777216.0f;
    }
}
