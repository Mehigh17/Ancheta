using System.Threading.Tasks;

namespace Ancheta.WebApi.Hubs
{
    public interface INotificationsClient
    {
        /// <summary>
        /// Procedure called on the client side when a vote has been casted.
        /// </summary>
        /// <param name="pollId">The id of the poll affected by the vote.</param>
        /// <param name="answerId">The id of the answer where the vote has been casted.</param>
        Task NotifyVoteCasted(string pollId, string answerId);
    }
}