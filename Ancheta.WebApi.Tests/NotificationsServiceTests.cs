using System;
using System.Threading;
using System.Threading.Tasks;
using Ancheta.Model.Messages;
using Ancheta.Model.Services;
using Ancheta.WebApi.Hubs;
using Ancheta.WebApi.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace Ancheta.WebApi.Tests
{
    public class NotificationsServiceTests
    {

        private readonly ITinyMessengerHub _messengerHub;
        public NotificationsServiceTests()
        {
            _messengerHub = new TinyMessengerHub();
        }

        [Fact]
        public async Task Invokes_NotifyVoteCasted_When_VoteCastedMessagePublished()
        {
            var hubContextMock = new Mock<IHubContext<NotificationsHub, INotificationsClient>>();

            var pollId = Guid.NewGuid();
            var answerId = Guid.NewGuid();

            var callCounter = 0;
            hubContextMock.Setup(s => s.Clients.Group(pollId.ToString()).NotifyVoteCasted(pollId.ToString(), answerId.ToString()))
                          .Callback(() => callCounter++)
                          .Returns(Task.CompletedTask);

            var service = new NotificationsService(hubContextMock.Object, _messengerHub);
            await service.StartAsync(new CancellationToken());

            _messengerHub.Publish(new VoteCastedMessage(pollId, answerId));

            // We expect the notifications service to call NotifyVoteCasted only once when it has received the message from the message hub.
            Assert.Equal(1, callCounter);
        }

    }
}