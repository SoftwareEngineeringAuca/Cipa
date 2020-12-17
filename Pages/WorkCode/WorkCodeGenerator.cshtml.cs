using System.Collections.Generic;
using System.Linq;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Study.Common.Results;

namespace Cipa.Pages.WorkCode

{
    public class WorkCodeGeneratorModel : PageModel
    {

        private readonly ILogger<WorkCodeGeneratorModel> _logger;
        private readonly IWorkCodeRepository _workCodeRepository;
        private readonly ICountryRepository _countryRepository;

        public WorkCodeGeneratorModel(ILogger<WorkCodeGeneratorModel> logger, IWorkCodeRepository workCodeRepository, ICountryRepository countryRepository)
        {
            _logger = logger;
            _workCodeRepository = workCodeRepository;
            _countryRepository = countryRepository;
        }

        [BindProperty]
        public int CountryId { get; set; }

        public string Message { get; set; }

        public IEnumerable<CountryViewModel> Countries { get; set; } = new List<CountryViewModel>();
        public IActionResult OnGet()
        {
            //load countries
            Countries = _countryRepository.GetCountries();
            return Page();
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation("WorkCodeGeneratorModel -> OnPost");
            //generate work_code for country.
            var response = _workCodeRepository.ExecuteWorkCodeQuery(CountryId);
            if (response.IsSuccess)
            {
                var totalRowsAffected = response.Cast<ModelResult<int>>().Model;
                _logger.LogInformation($"WorkCodeGeneratorModel -> OnPost; Success: {totalRowsAffected}");
                Message = $"Запрос выполнился успешно, количество сгенерированных строк: {totalRowsAffected}";
                return OnGet();
            }
            _logger.LogInformation("WorkCodeGeneratorModel -> OnPost; Error: " + response.Message);
            Message = $"Произошла ошибка: {response.Message}";
            return OnGet();
        }
    }
}
