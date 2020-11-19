using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cipa.Helpers;

namespace Cipa.Interfaces
{
    public interface IWorkCodeRepository
    {
        ExecuteResult ExecuteWorkCodeQuery(int countryId);
    }
}
