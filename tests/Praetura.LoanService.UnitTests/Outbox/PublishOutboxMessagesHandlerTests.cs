using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Praetura.LoanService.API.Features.Outbox.PublishOutboxMessages;
using Praetura.LoanService.Domain.Entities;
using Praetura.LoanService.Domain.Messaging;
using Praetura.LoanService.Domain.Repositories;
using Praetura.LoanService.UnitTests.Support;

namespace Praetura.LoanService.UnitTests.Outbox;

public class PublishOutboxMessagesHandlerTests
{
    private static readonly DateTimeOffset Now = new(2025, 5, 27, 12, 0, 0, TimeSpan.Zero);

    private readonly IOutboxRepository _outboxRepository = Substitute.For<IOutboxRepository>();
    private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();

    [Fact]
    public async Task GIVEN_HandleAsync_WHEN_MessagesAreUnpublished_THEN_EachIsPublishedAndMarkedPublished()
    {
        var first = CreateMessage();
        var second = CreateMessage();
        SetUnpublished(first, second);

        var handler = CreateHandler();
        var published = await handler.HandleAsync(CancellationToken.None);

        Assert.Equal(2, published);
        await _publisher.Received(1).PublishAsync(first, Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(second, Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).MarkPublishedAsync(first.Id, Now.UtcDateTime, Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).MarkPublishedAsync(second.Id, Now.UtcDateTime, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GIVEN_HandleAsync_WHEN_NoMessagesAreUnpublished_THEN_NothingIsPublished()
    {
        SetUnpublished();

        var handler = CreateHandler();
        var published = await handler.HandleAsync(CancellationToken.None);

        Assert.Equal(0, published);
        await _publisher.DidNotReceiveWithAnyArgs().PublishAsync(default!, default);
    }

    [Fact]
    public async Task GIVEN_HandleAsync_WHEN_PublishingOneMessageFails_THEN_ItIsNotMarkedAndTheRestAreStillPublished()
    {
        var failing = CreateMessage();
        var healthy = CreateMessage();
        SetUnpublished(failing, healthy);

        _publisher
            .PublishAsync(failing, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Publish failed."));

        var handler = CreateHandler();
        var published = await handler.HandleAsync(CancellationToken.None);

        Assert.Equal(1, published);
        await _outboxRepository.DidNotReceive().MarkPublishedAsync(
            failing.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).MarkPublishedAsync(
            healthy.Id, Now.UtcDateTime, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GIVEN_HandleAsync_WHEN_MarkingOneMessageFails_THEN_TheRestAreStillPublishedAndMarked()
    {
        var failing = CreateMessage();
        var healthy = CreateMessage();
        SetUnpublished(failing, healthy);

        _outboxRepository
            .MarkPublishedAsync(failing.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Mark failed."));

        var handler = CreateHandler();
        var published = await handler.HandleAsync(CancellationToken.None);

        Assert.Equal(1, published);
        await _publisher.Received(1).PublishAsync(healthy, Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).MarkPublishedAsync(
            healthy.Id, Now.UtcDateTime, Arg.Any<CancellationToken>());
    }

    private PublishOutboxMessagesHandler CreateHandler() =>
        new(
            _outboxRepository,
            _publisher,
            Options.Create(new OutboxOptions { BatchSize = 10 }),
            new FixedTimeProvider(Now),
            NullLogger<PublishOutboxMessagesHandler>.Instance);

    private void SetUnpublished(params OutboxMessage[] messages) =>
        _outboxRepository
            .GetUnpublishedAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<OutboxMessage>>(messages));

    private static OutboxMessage CreateMessage() =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = "LoanApplicationApproved",
            Payload = "{}",
            CreatedAt = new DateTime(2025, 5, 27, 11, 0, 0, DateTimeKind.Utc)
        };
}
