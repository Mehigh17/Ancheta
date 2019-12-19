using Ancheta.Model.Data;
using Ancheta.Model.ViewModels;
using AutoMapper;

namespace Ancheta.Model.MappingProfiles
{
    public class ViewModelProfile : Profile
    {
        public ViewModelProfile()
        {
            CreateMap<Answer, AnswerDetailViewModel>().ForMember(dest => dest.VoteCount,
                                                                 opt => opt.MapFrom(a => a.Votes.Count));
            CreateMap<Poll, PollDetailViewModel>();
        }
    }
}