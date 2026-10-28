using WoWRandomizer.Domain;

namespace WoWRandomizer.UnitTests;

internal sealed class FakeComboProvider : IComboProvider
{
    private readonly IReadOnlyList<Combo> _combos;

    public FakeComboProvider(params Combo[] combos)
    {
        _combos = combos;
    }

    public IReadOnlyList<Combo> GetAll() => _combos;
}

/// <summary>Always picks the last valid index and remembers the upper bound it was asked for.</summary>
internal sealed class LastIndexRandom : Random
{
    public int RequestedUpperBound { get; private set; }

    public override int Next(int maxValue)
    {
        RequestedUpperBound = maxValue;
        return maxValue - 1;
    }
}

/// <summary>Answers every request with a canned response and remembers the URIs it was asked for.</summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        _respond = respond;
    }

    public List<string> RequestedUris { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestedUris.Add(request.RequestUri!.AbsoluteUri);

        return Task.FromResult(_respond(request));
    }
}
