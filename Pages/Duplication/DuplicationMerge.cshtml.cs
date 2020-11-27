using Cipa.BusinessModels;
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
        public string Message { get; set; }
        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            if (UserCode.Length > 10)
            {
                Message = "Введенные данные неправильные";
                return Page();
            }
            var result = _duplicationRepository.ExecuteWorkCodeQuery(UserCode);

            if (!result.IsSuccess)
            {
                Message = "Произошла ошибка: " + result.Message;
                return Page();
            }
            var response = result.Cast<ModelResult<DuplicationMergeResponseModel>>().Model;
            _logger.LogInformation($"DuplicationMergeModel: userCode = {UserCode}, Name = {response.FullName}, MainCode = {response.MainCode}");
            Message = $"Запрос выполнился правильно, объединили {response.FullName}, старый ИН {response.NotMainCode} оставили ИН {response.MainCode}.";
            return Page();
        }
    }
}
