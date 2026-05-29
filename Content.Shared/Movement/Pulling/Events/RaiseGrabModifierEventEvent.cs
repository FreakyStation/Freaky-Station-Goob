// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Movement.Pulling.Events;

[ByRefEvent]
public record struct RaiseGrabModifierEventEvent(EntityUid Uid, int Stage)
{
    public int? NewStage { get; set; }
    public float Multiplier { get; set; } = 1f;
    public float Modifier => Multiplier;
    public float SpeedMultiplier { get => Multiplier; set => Multiplier = value; }
}
