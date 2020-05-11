using System.Threading;
using System.Threading.Tasks;
using Ancheta.Model.Messages;
using Ancheta.Model.Services;
using Ancheta.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace Ancheta.WebApi.Services
{
    public class NotificationsService : IHostedService
    {
        private readonly IHubContext<NotificationsHub, INotificationsClient> _hubContext;
        private readonly ITinyMessengerHub _messengerHub;
        private TinyMessageSubscriptionToken _subscriptionToken;

        public NotificationsService(IHubContext<NotificationsHub, INotificationsClient> hubContext,
                                    ITinyMessengerHub messengerHub)
        {
            _hubContext = hubContext ?? throw new System.ArgumentNullException(nameof(hubContext));
            _messengerHub = messengerHub ?? throw new System.ArgumentNullException(nameof(messengerHub));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscriptionToken = _messengerHub.Subscribe<VoteCastedMessage>((message) =>
            {
                _hubContext.Clients.Group(message.PollId.ToString()).NotifyVoteCasted(message.PollId.ToString(), message.AnswerId.ToString());
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _messengerHub.Unsubscribe(_subscriptionToken);

            return Task.CompletedTask;
        }
    }
}