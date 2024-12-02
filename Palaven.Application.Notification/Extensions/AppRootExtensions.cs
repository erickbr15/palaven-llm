using Microsoft.Extensions.DependencyInjection;
using Palaven.Application.Abstractions.Notifications;

namespace Palaven.Application.Notification.Extensions;

public static class AppRootExtensions
{
    public static IServiceCollection AddNotificationService(this IServiceCollection services)
    {
        services.AddSingleton<INotificationService, NotificationService>();
        return services;
    }
}
