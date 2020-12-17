using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Cipa.Pages.WorkCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Study.Common.Results;

namespace Cipa.Pages.UsersSession
{
    public class UsersSessionModel : PageModel
    {
        private readonly ILogger<UsersSessionModel> _logger;
        private readonly ICountryRepository _countryRepository;
        private readonly IUsersSessionRepository _usersSessionRepository;
        public UsersSessionModel(ICountryRepository countryRepository, IUsersSessionRepository usersSessionRepository, ILogger<UsersSessionModel> logger)
        {
            _countryRepository = countryRepository;
            _usersSessionRepository = usersSessionRepository;
            _logger = logger;
        }

        [BindProperty]
        public int CountryId { get; set; }
        [BindProperty]
        public int CityId { get; set; }
        public string Message { get; set; }
        public IEnumerable<CountryViewModel> Countries { get; set; } = new List<CountryViewModel>();
        public IEnumerable<CityViewModel> Cities { get; set; } = new List<CityViewModel>();
        public IActionResult OnGet()
        {
            Countries = _countryRepository.GetCountries();

            Cities = _countryRepository.GetCities();
            return Page();
        } 
        
        public IActionResult OnPost()
        {
            _logger.LogInformation($"UsersSessionModel -> OnPost countryId={CountryId}, cityId={CityId}");
            var response = _usersSessionRepository.ExecuteScript(CountryId, CityId);
            if (response.IsSuccess)
            {
                var totalAffectedRows = response.Cast<ModelResult<int>>().Model;
                _logger.LogInformation($"UsersSessionModel -> OnPost; Success totalAffectedRows={totalAffectedRows}");
                Message = $"Запрос выполнился успешно, колличество измененных данных = {totalAffectedRows}";
                return OnGet();
            }

            _logger.LogInformation($"UsersSessionModel -> OnPost; Error: " + response.Message);
            Message = "Запрос выполнился с ошибкой: " + response.Message;
            return Page();
        }
    }
}
