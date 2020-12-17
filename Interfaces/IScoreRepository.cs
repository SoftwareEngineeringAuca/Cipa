using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cipa.Helpers;

namespace Cipa.Interfaces
{
    public interface IScoreRepository
    {
        ExecuteResult GetEvalFormList();
        ExecuteResult GetAllCheckers();
        ExecuteResult ExecuteScoreCalculation(int evalFormHeaderId, int factPass, int factMax, int gap, int checkerId);
        ExecuteResult PublishResults(int evalFormHeaderId);
    }
}
