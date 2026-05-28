// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Server.Worldgen.Systems.Debris;
using NUnit.Framework;
using Robust.UnitTesting;

namespace Content.Tests.Server.Worldgen;

[TestFixture]
public sealed class DebrisFeaturePlacerSystemTests : RobustUnitTest
{
    [Test]
    public void IsWithinSpawnDistance_InsideLimit_ReturnsTrue()
    {
        var result = DebrisFeaturePlacerSystem.IsWithinSpawnDistance(new Vector2(3000f, 4000f), 10_000f);
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsWithinSpawnDistance_OutsideLimit_ReturnsFalse()
    {
        var result = DebrisFeaturePlacerSystem.IsWithinSpawnDistance(new Vector2(10001f, 0f), 10_000f);
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsWithinSpawnDistance_DisabledLimit_ReturnsTrue()
    {
        var result = DebrisFeaturePlacerSystem.IsWithinSpawnDistance(new Vector2(50000f, 50000f), 0f);
        Assert.That(result, Is.True);
    }
}
