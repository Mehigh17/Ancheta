using System;
using Ancheta.Model.Data;
using Ancheta.Model.Services;

namespace Ancheta.Model.Messages
{
    public class VoteCastedMessage : ITinyMessage
    {
        public Guid PollId { get; set; }
        public Vote Vote { get; set; }

        public object Sender { get; }

        public VoteCastedMessage(Guid pollId, Vote vote)
        {
            Vote = vote ?? throw new ArgumentNullException(nameof(vote));
            PollId = pollId;
        }

        public VoteCastedMessage(string pollId, Vote vote)
        {
            Vote = vote ?? throw new ArgumentNullException(nameof(vote));

            if (Guid.TryParse(pollId, out Guid guid))
            {
                PollId = guid;
            }
            else
            {
                throw new ArgumentException("Poll ID is invalid.", nameof(pollId));
            }
        }

        public VoteCastedMessage(object sender, Guid pollId, Vote vote) : this(pollId, vote)
        {
            Sender = sender;
        }

        public VoteCastedMessage(object sender, string pollId, Vote vote) : this(pollId, vote)
        {
            Sender = sender;
        }

    }
}