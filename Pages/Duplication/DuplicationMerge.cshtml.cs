using Cipa.Helpers;
using Cipa.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Study.Common.Results;

namespace Cipa.Pages.Duplication
{
    public class DuplicationMergeModel : PageModel
    {
        private readonly ILogger<DuplicationMergeModel> _logger;
        private readonly IDuplicationRepository _duplicationRepository;
        public DuplicationMergeModel(ILogger<DuplicationMergeModel> logger, IDuplicationRepository duplicationRepository)
        {
            _logger = logger;
            _duplicationRepository = duplicationRepository;
        }

        [BindProperty]
        public string UserCode { get; set; }
        
        public IActionResult OnPost()
        {
            var result = _duplicationRepository.ExecuteWorkCodeQuery(UserCode);

            if (!result.IsSuccess) return RedirectToPage("/Error");
            var rowsAffected = result.Cast<ModelResult<int>>().Model;
            _logger.LogInformation($"DuplicationMergeModel: userCode = {UserCode}, totalRowsAffected = {rowsAffected}");

            return RedirectToPage("/Index");
        }
    }
}
