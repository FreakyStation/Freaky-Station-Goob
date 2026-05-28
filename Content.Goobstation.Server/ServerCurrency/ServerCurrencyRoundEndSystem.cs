//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.ServerCurrency;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;

namespace Content.Goobstation.Server.ServerCurrency;

public sealed class ServerCurrencyRoundEndSystem : EntitySystem
{
    [Dependency] private readonly ICommonCurrencyManager _currency = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        var lost = RouletteStats.GetRoundLostCoins();
        ev.AddLine(Loc.GetString("gs-roulette-round-end-lost", ("amount", _currency.Stringify(lost))));
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        RouletteStats.ResetRound();
    }
}
