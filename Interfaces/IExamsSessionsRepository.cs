using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cipa.Helpers;

namespace Cipa.Interfaces
{
    public interface IExamsSessionsRepository
    {
        ExecuteResult CreateSession(string sessionName, DateTime sessionStart, DateTime sessionEnd);
        ExecuteResult AddExamSchedule(int examId, DateTime examDate);
    }
}
