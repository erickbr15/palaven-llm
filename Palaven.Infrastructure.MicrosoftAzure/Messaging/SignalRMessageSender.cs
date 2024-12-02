using Palaven.Infrastructure.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Palaven.Infrastructure.MicrosoftAzure.Messaging;

public class SignalRMessageSender : IMessageSender
{

    public SignalRMessageSender()
    {        
    }

    public Task BroadcastAsync(string target, object message)
    {
        throw new NotImplementedException();
    }

    public Task BroadcastAsync(string target, object message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SendToGroupAsync(string groupName, string target, object message)
    {
        throw new NotImplementedException();
    }

    public Task SendToGroupAsync(string groupName, string target, object message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SendToUserAsync(string userId, string target, object message)
    {
        throw new NotImplementedException();
    }

    public Task SendToUserAsync(string userId, string target, object message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
