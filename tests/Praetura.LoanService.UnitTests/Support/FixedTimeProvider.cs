namespace Praetura.LoanService.UnitTests.Support;

/// <summary>
/// A <see cref="TimeProvider"/> that always returns the same instant, so timestamps in tests are predictable.
/// </summary>
internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}
