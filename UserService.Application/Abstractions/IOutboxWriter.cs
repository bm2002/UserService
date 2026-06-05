namespace UserService.Application.Abstractions;

public interface IOutboxWriter
{
    Task WriteAsync<TEvent>(TEvent @event, CancellationToken cancellation);
}