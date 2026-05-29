// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Changeling;

[ByRefEvent]
public record struct ActionAugmentedEyesightEvent
{
    public bool Handled;
}

[ByRefEvent]
public record struct ChangelingRegenerateEvent
{
    public bool Handled;
}

[ByRefEvent]
public record struct ChangelingStasisEvent
{
    public bool Handled;
}
