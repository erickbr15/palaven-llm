namespace Palaven.Infrastructure.Model.Messaging;

public class Message<TBody> where TBody : class
{
    public string MessageId { get; set; } = default!;
    public string PopReceipt { get; set; } = default!;
    public DateTimeOffset? NextVisibleOn { get; set; }
    public DateTimeOffset? InsertedOn { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }
    public long DequeueCount { get; set; }
    public TBody Body { get; set; } = default!;
}
