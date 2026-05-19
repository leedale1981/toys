using System.Collections;

namespace LD.Messaging.Domain;

public record StockRecord(
    string Symbol,
    string Name,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    long Volume,
    decimal ChangePercent
);

public static class StockRecordsExtensions
{
    public static IEnumerable<string> GetSymbols(this StockRecords records)
    {
        foreach (StockRecord record in records)
        {
            yield return record.Symbol;
        }
    }
    
    public static async IAsyncEnumerable<string> GetSymbolsAsync(this StockRecordsAsync records)
    {
        await foreach (StockRecord record in records)
        {
            yield return record.Symbol;
        }
    }
}

public abstract class StockRecordsAsync(StockRecord[] records) : IAsyncEnumerable<StockRecord>
{
    public IAsyncEnumerator<StockRecord> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}

public abstract class StockRecords(StockRecord[] records) : IEnumerable<StockRecord>
{
    public IEnumerator<StockRecord> GetEnumerator()
    {
        return new StockRecordEnumerator(records);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class StockRecordEnumerator(StockRecord[] records) : IEnumerator<StockRecord>
{
    private int _position = -1;

    public bool MoveNext()
    {
        _position++;
        return _position < records.Length;
    }

    public void Reset()
    {
        _position = -1;
    }

    StockRecord IEnumerator<StockRecord>.Current => records[_position];

    object? IEnumerator.Current => records[_position];

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public class StockRecordEnumeratorAsync(StockRecord[] records) : IAsyncEnumerator<StockRecord>
{
    public StockRecord Current { get; }
    private int _position = -1;

    public void Reset()
    {
        _position = -1;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        _position++;
        return ValueTask.FromResult(_position < records.Length);
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}