using WoWRandomizer.Shared;
using WoWRandomizer.Web.Services;

namespace WoWRandomizer.Web.ComponentTests;

/// <summary>Answers each call with the next scripted response and counts how often it was called.</summary>
internal sealed class FakeComboApiClient : IComboApiClient
{
    private readonly Queue<Func<Task<ComboResponse>>> _responses = new();

    public int CallCount { get; private set; }

    public void Enqueue(Func<Task<ComboResponse>> response) => _responses.Enqueue(response);

    public void EnqueueResult(ComboResponse combo) => Enqueue(() => Task.FromResult(combo));

    public void EnqueueFailure(Exception exception) => Enqueue(() => Task.FromException<ComboResponse>(exception));

    public Task<ComboResponse> GetRandomComboAsync(string? faction = null, CancellationToken cancellationToken = default)
    {
        CallCount++;

        return _responses.Count > 0
            ? _responses.Dequeue()()
            : throw new InvalidOperationException("No response was set up for this call.");
    }
}
