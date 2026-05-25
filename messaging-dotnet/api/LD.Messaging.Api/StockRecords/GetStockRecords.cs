using FastEndpoints;
using LD.Messaging.DataInterfaces.Queries;
using LD.Messaging.DataInterfaces.StockRecords;
using LD.Messaging.Domain;

namespace LD.Messaging.Api.StockRecords;

public class GetStockRecords(
    IQueryHandler<GetStockRecordsQuery, IReadOnlyList<StockRecord>> queryHandler,
    ILogger<GetStockRecords> logger) : Endpoint<StockRecordRequest>
{
    public override void Configure()
    {
        Get("/api/stockrecords");
        AllowAnonymous();
    }

    public override async Task HandleAsync(StockRecordRequest req, CancellationToken ct)
    {
        logger.LogInformation(
            "GET /api/stockrecords called with Symbol={Symbol}, Exchange={Exchange}, RecordDate={RecordDate}, PageSize={PageSize}, PageOffset={PageOffset}",
            req.Symbol,
            req.Exchange,
            req.RecordDate,
            req.PageSize,
            req.PageOffset);

        var query = new GetStockRecordsQuery(
            Symbol: req.Symbol,
            Exchange: req.Exchange,
            RecordDate: req.RecordDate,
            PageSize: req.PageSize,
            PageOffset: req.PageOffset);

        var records = await queryHandler.HandleAsync(query, ct);

        await Send.OkAsync(records, ct);
    }
}