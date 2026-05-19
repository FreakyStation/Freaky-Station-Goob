using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.OfferItem;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedOfferItemSystem))]
public sealed partial class OfferItemComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool IsInOfferMode;

    [DataField]
    public bool IsInReceiveMode;

    [DataField]
    public string? Hand;

    [DataField]
    public EntityUid? Item;

    [DataField]
    public EntityUid? Target;

    [DataField]
    public float MaxOfferDistance = 2f;
}

[Serializable, NetSerializable]
public sealed class OfferItemComponentState : ComponentState
{
    public bool IsInOfferMode;
    public bool IsInReceiveMode;
    public string? Hand;
    public NetEntity? Item;
    public NetEntity? Target;

    public OfferItemComponentState(bool isInOfferMode, bool isInReceiveMode, string? hand, NetEntity? item, NetEntity? target)
    {
        IsInOfferMode = isInOfferMode;
        IsInReceiveMode = isInReceiveMode;
        Hand = hand;
        Item = item;
        Target = target;
    }
}
