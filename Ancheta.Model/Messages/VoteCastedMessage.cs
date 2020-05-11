using System;
using Ancheta.Model.Services;

namespace Ancheta.Model.Messages
{
    public class VoteCastedMessage : ITinyMessage
    {
        public Guid PollId { get; set; }
        public Guid AnswerId { get; set; }

        public object Sender { get; }

        public VoteCastedMessage(Guid pollId, Guid answerId)
        {
            AnswerId = answerId;
            PollId = pollId;
        }

        public VoteCastedMessage(object sender, Guid pollId, Guid answerId) : this(pollId, answerId)
        {
            Sender = sender;
        }

    }
}