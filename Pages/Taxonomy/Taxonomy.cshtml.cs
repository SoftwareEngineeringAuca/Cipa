using System.Collections.Generic;
using System.Threading.Tasks;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Study.Common.Results;

namespace Cipa.Pages.Taxonomy
{
    public class TaxonomyModel : PageModel
    {
        
        private readonly ILogger<TaxonomyModel> _logger;
        private readonly ITaxonomyRepository _taxonomyRepository;
        private readonly IExamsRepository _examsRepository;
        public TaxonomyModel(ILogger<TaxonomyModel> logger, ITaxonomyRepository taxonomyRepository, IExamsRepository examsRepository)
        {
            _logger = logger;
            _taxonomyRepository = taxonomyRepository;
            _examsRepository = examsRepository;
        }
        public IEnumerable<ExamViewModel> ExamsList { get; set; } = new List<ExamViewModel>();
        [BindProperty]
        public int ExamId { get; set; }
        
        [BindProperty]
        public string TaxonomyName { get; set; }
        
        [BindProperty]
        public string Message { get; set; }

        public IActionResult OnGet()
        {
            ExamsList = _examsRepository.GetCurrentSessionExams();
            return Page();
        }

        public async Task<IActionResult> OnPost(IFormFile file)
        {
            _logger.LogInformation($"TaxonomyModel->onPost: Create Taxonomy");
            var validation = _taxonomyRepository.ValidateFile(file);
            if (!validation)
            {
                Message = "Выберите excel файл с расширением '(.xlsx)' \n Перезагрузите страницу!";
                return Page();
            }

            var createTaxonomyHeader = _taxonomyRepository.CreateTaxonomyExam(ExamId, TaxonomyName);
            if (!createTaxonomyHeader.IsSuccess)
            {
                Message = "Создание Таксономии по имени не получилось, произошла ошибка: " +
                          createTaxonomyHeader.Message;
                _logger.LogInformation(Message);
                return Page();
            }
            var state = await _taxonomyRepository.AddTaxonomy(file, createTaxonomyHeader.Cast<ModelResult<int>>().Model);
            if (state.IsSuccess)
            {
                var affectedRowsAmount = state.Cast<ModelResult<int>>().Model;
                _logger.LogInformation($"TaxonomyModel->onPost: Create Taxonomy, rows affected = {affectedRowsAmount}");
                Message = "Запрос выполнился успешно!";
                return Page();
            }

            Message = "Запрос выполнился с ошибкой: " + state.Message;
            _logger.LogInformation($"TaxonomyModel->onPost: Create Taxonomy, " + Message);
            return Page();
        }
    }
}