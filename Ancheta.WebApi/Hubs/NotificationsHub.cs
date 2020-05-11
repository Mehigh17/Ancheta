
using System;
using System.Threading.Tasks;
using Ancheta.Model.Repositories;
using Ancheta.Model.Services;
using Microsoft.AspNetCore.SignalR;

namespace Ancheta.WebApi.Hubs
{
    public class NotificationsHub : Hub<INotificationsClient>
    {
        private readonly IPollRepository _pollRepository;

        public ITinyMessengerHub MessengerHub { get; }

        public NotificationsHub(ITinyMessengerHub messengerHub,
                                IPollRepository pollRepository)
        {
            MessengerHub = messengerHub ?? throw new ArgumentNullException(nameof(messengerHub));
            _pollRepository = pollRepository ?? throw new ArgumentNullException(nameof(pollRepository));
        }

        /// <summary>
        /// Method called by the client when it subscribes to a poll events.
        /// </summary>
        /// <param name="pollId">The id of the poll.</param>
        public async Task SubscribePoll(string pollId)
        {
            if (Guid.TryParse(pollId, out var id))
            {
                var poll = _pollRepository.GetById(id);
                if (poll != null)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, pollId);
                }
            }
        }

        /// <summary>
        /// Method called by the client when it unsubscribes to a poll events.
        /// </summary>
        /// <param name="pollId">The id of the poll.</param>
        public async Task UnsubscribePoll(string pollId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, pollId);
        }

    }
}