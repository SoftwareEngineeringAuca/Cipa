using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cipa.BusinessModels;
using Cipa.Helpers;

namespace Cipa.Interfaces
{
    public interface IGradingRepository
    {
        ExecuteResult AddCountryToGrading(int gradingId, int countryId);
        IEnumerable<GradingViewModel> GetGradings();
    }
}
