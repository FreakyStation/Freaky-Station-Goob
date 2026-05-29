// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Heretic;

[ByRefEvent]
public record struct HereticCheckEvent
{
    public EntityUid Uid;
    public bool Result;
}
