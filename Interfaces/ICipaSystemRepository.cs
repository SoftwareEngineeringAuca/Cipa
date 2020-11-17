using Cipa.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cipa.Interfaces
{
    public interface ICipaSystemRepository
    {
        ExecuteResult GetActiveSession();
    }
}
