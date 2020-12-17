using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Study.Common.Results;

namespace Cipa.Pages.Gradings
{
    public class GradingModel : PageModel
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IGradingRepository _gradingRepository;

        public GradingModel(ICountryRepository countryRepository, IGradingRepository gradingRepository)
        {
            _countryRepository = countryRepository;
            _gradingRepository = gradingRepository;
        }


        public IEnumerable<CountryViewModel> Countries { get; set; } = new List<CountryViewModel>();
        public IEnumerable<GradingViewModel> GradingsList { get; set; } = new List<GradingViewModel>();
        [BindProperty]
        public int GradingId { get; set; }
        [BindProperty]
        public int SelectedCountryId { get; set; }
        public string Message { get; set; }

        public IActionResult OnGet()
        {
            GradingsList = _gradingRepository.GetGradings();

            Countries = _countryRepository.GetCountries();
            return Page();
        }

        public IActionResult OnPost()
        {
            var response = _gradingRepository.AddCountryToGrading(GradingId, SelectedCountryId);
            if (response.IsSuccess)
            {
                var totalAffectedRows = response.Cast<ModelResult<int>>().Model;
                Message = $"Запрос выполнился успешно, колличество измененных данных = {totalAffectedRows}";
                return OnGet();
            }

            Message = "Запрос выполнился с ошибкой: " + response.Message;
            return Page();
        }
    }
}
