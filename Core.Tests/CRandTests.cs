using System;
using IECODE.Core.Crypto;
using Xunit;

namespace IECODE.Core.Tests;

public class CRandTests
{
    [Fact]
    public void DefaultSeed_GeneratesCanonicalMT19937Sequence()
    {
        // Vector matching seed 5489 ground truth across Rust, Python, Bun/TS, C++
        var rand = new CRand(5489);

        Assert.Equal(3499211612u, rand.NextU32());
        Assert.Equal(581869302u, rand.NextU32());
        Assert.Equal(3890346734u, rand.NextU32());
    }

    [Fact]
    public void FromU64_FoldsSeedAndGeneratesDeterministicSequence()
    {
        ulong seed64 = 0x123456789ABCDEF0UL;
        uint folded = (uint)(seed64 ^ (seed64 >> 32));

        var rand1 = CRand.FromU64(seed64);
        var rand2 = new CRand(folded);

        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(rand2.NextU32(), rand1.NextU32());
        }
    }

    [Fact]
    public void Bounded_ProducesValuesWithinSpecifiedRange()
    {
        var rand = new CRand(12345);
        uint bound = 17;

        for (int i = 0; i < 100; i++)
        {
            uint val = rand.Bounded(bound);
            Assert.True(val < bound, $"Expected val < {bound}, got {val}");
        }
    }

    [Fact]
    public void Bounded_WithZeroBound_ReturnsRawNextU32()
    {
        var rand1 = new CRand(42);
        var rand2 = new CRand(42);

        // In nie.exe, bound == 0 returns the raw next uint
        uint raw = rand1.NextU32();
        uint boundedZero = rand2.Bounded(0);

        Assert.Equal(raw, boundedZero);
    }

    [Fact]
    public void NextF32_ProducesValuesInUnitInterval()
    {
        var rand = new CRand(9999);

        for (int i = 0; i < 100; i++)
        {
            float f = rand.NextF32();
            Assert.True(f >= 0.0f && f < 1.0f, $"Expected f in [0.0, 1.0), got {f}");
        }
    }
}
