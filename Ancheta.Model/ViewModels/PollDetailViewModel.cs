using System;
using System.Collections.Generic;

namespace Ancheta.Model.ViewModels
{
    public class PollDetailViewModel
    {

        public string Id { get; set; }
        
        public string Question { get; set; }

        public DateTime CreatedOn { get; set; }

        public TimeSpan Duration { get; set; }

        public IReadOnlyList<AnswerDetailViewModel> Answers { get; set; }
        
    }
}