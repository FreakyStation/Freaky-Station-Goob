//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.ServerCurrency;

/// <summary>
/// Thread-safe roulette statistics storage.
/// </summary>
public static class RouletteStats
{
    private static readonly object Sync = new();

    private static int _allTimeDepositedCoins;
    private static int _roundDepositedCoins;
    private static int _roundPayoutCoins;

    public static void RecordSpin(int bet, int payout)
    {
        lock (Sync)
        {
            _allTimeDepositedCoins += Math.Max(0, bet);
            _roundDepositedCoins += Math.Max(0, bet);
            _roundPayoutCoins += Math.Max(0, payout);
        }
    }

    public static int GetAllTimeDepositedCoins()
    {
        lock (Sync)
        {
            return _allTimeDepositedCoins;
        }
    }

    public static int GetRoundLostCoins()
    {
        lock (Sync)
        {
            return Math.Max(0, _roundDepositedCoins - _roundPayoutCoins);
        }
    }

    public static void ResetRound()
    {
        lock (Sync)
        {
            _roundDepositedCoins = 0;
            _roundPayoutCoins = 0;
        }
    }
}
