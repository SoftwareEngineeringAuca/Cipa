using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cipa.BusinessModels
{
    public class DuplicationMergeResponseModel
    {
        public string MainCode { get; set; }
        public string NotMainCode { get; set; }
        public string FullName { get; set; }
        public int MainId { get; set; }
        public int? NotMainId { get; set; }
    }
}
