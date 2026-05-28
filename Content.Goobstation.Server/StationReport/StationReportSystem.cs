using Content.Server.GameTicking;
using Content.Goobstation.Common.StationReport;
using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Server.ServerCurrency;
using Content.Shared.Paper;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.StationReportSystem;

public sealed class StationReportSystem : EntitySystem
{
    [Dependency] private readonly ICommonCurrencyManager _currency = default!;

    //this is shitcode?

    public override void Initialize()
    {
        //subscribes to the endroundevent
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent args)
    {
        //locates the first entity with StationReportComponent then stops
        string? stationReportText = null;
        var query = EntityQueryEnumerator<StationReportComponent>();
        while (query.MoveNext(out var uid, out var tablet))//finds the first entity with stationreport
        {
            if (!TryComp<PaperComponent>(uid, out var paper))
               return;
            
            stationReportText = paper.Content;
            break;
        }

        var lost = RouletteStats.GetRoundLostCoins();
        var rouletteLine = Loc.GetString("gs-roulette-station-report-lost", ("amount", _currency.Stringify(lost)));

        stationReportText = string.IsNullOrWhiteSpace(stationReportText)
            ? rouletteLine
            : $"{stationReportText}\n\n{rouletteLine}";

        BroadcastStationReport(stationReportText);
    }

    //sends a networkevent to tell the client to update the stationreporttext when recived
    public void BroadcastStationReport(string? stationReportText)
    {
        RaiseNetworkEvent(new StationReportEvent(stationReportText));//to send to client
        RaiseLocalEvent(new StationReportEvent(stationReportText));//to send to discord intergration
    }
}
