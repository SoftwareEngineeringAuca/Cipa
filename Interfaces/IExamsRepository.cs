using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cipa.BusinessModels;

namespace Cipa.Interfaces
{
    public interface IExamsRepository
    {
        IEnumerable<ExamViewModel> GetExams();
        IEnumerable<ExamViewModel> GetCurrentSessionExams();
    }
}
