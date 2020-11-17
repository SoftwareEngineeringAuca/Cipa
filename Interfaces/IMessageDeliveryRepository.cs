using Cipa.BusinessModels;
using Cipa.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Cipa.Interfaces
{
    public interface IMessageDeliveryRepository
    {
        ExecuteResult ExecuteQuery(int countryId, int cityId, int batchId);
        IEnumerable<MailBatchesViewModel> GetMailBatches();
    }
}