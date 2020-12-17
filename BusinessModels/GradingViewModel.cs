using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cipa.BusinessModels
{
    public class GradingViewModel
    {
        public int GradingId { get; set; }
        public int ExamId{ get; set; }
        public int ScheduleId { get; set; }
        public string GradingName { get; set; }
    }
}
