using Cipa.BusinessModels;
using Cipa.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Cipa.Interfaces
{
    public interface IMessageDeliveryRepository
    {
        ExecuteResult ExecuteQuery(int countryId, int cityId, int batchId);
        IEnumerable<MailBatchesViewModel> GetMailBatches();
    }
}