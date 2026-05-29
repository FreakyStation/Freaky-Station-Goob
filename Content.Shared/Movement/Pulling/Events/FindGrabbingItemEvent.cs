// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Movement.Pulling.Events;

[ByRefEvent]
public record struct FindGrabbingItemEvent(EntityUid Uid)
{
    public EntityUid? GrabbingItem { get; set; }
}
