using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cipa.BusinessModels
{
    public class ExamViewModel
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public DateTime? ExamDate { get; set; }
    }
}
