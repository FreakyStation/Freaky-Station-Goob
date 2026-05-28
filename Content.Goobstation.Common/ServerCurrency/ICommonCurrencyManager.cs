// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Network;

namespace Content.Goobstation.Common.ServerCurrency;

public interface ICommonCurrencyManager
{
    public event Action? ClientBalanceChange;
    public event Action<PlayerBalanceChangeEvent>? BalanceChange;

    public void Initialize();

    public void Shutdown();

    public bool CanAfford(NetUserId? userId, int amount, out int balance);

    public string Stringify(int amount);

    public int AddCurrency(NetUserId userId, int amount);

    public int RemoveCurrency(NetUserId userId, int amount);

    public (int, int) TransferCurrency(NetUserId sourceUserId, NetUserId targetUserId, int amount);

    public int SetBalance(NetUserId userId, int amount);

    public int GetBalance(NetUserId? userId = null);
}
