// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.GrabIntent;

[ByRefEvent]
public record struct CheckGrabbedEvent
{
    public bool IsGrabbed;
}

[ByRefEvent]
public record struct GrabAttemptEvent(
    EntityUid Puller,
    bool IgnoreCombatMode = false,
    Content.Goobstation.Common.Grab.GrabStage? GrabStageOverride = null,
    float EscapeAttemptModifier = 1f)
{
    public bool Grabbed;
}

[ByRefEvent]
public record struct GrabAttemptReleaseEvent(
    EntityUid? user = null,
    EntityUid? puller = null)
{
    public bool Released;
}
