using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Cipa.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Study.Common.Results;

namespace Cipa.Pages
{
    public class GetAllCountriesModel : PageModel
    {
        private readonly ILogger<GetAllCountriesModel> _logger;
        private readonly ICountryRepository _countryRepository;
        private readonly IMessageDeliveryRepository _messageDeliveryRepository;

        public GetAllCountriesModel(ILogger<GetAllCountriesModel> logger, ICountryRepository countryRepository, IMessageDeliveryRepository messageDeliveryRepository)
        {
            _logger = logger;
            _countryRepository = countryRepository;
            _messageDeliveryRepository = messageDeliveryRepository;
        }
        
        [BindProperty]
        public int CountryId { get; set; }
        [BindProperty]
        public int CityId { get; set; }
        [BindProperty]
        public int BatchId { get; set; }
        public IEnumerable<CountryViewModel> Countries { get; set; }
        public IEnumerable<CityViewModel> Cities { get; set; }
        public IEnumerable<MailBatchesViewModel> MailBatches { get; set; }

        public void OnGet()
        {
            var countriesList = _countryRepository.GetCountries().ToList();
            Countries = countriesList;

            var citiesList = _countryRepository.GetCities().ToList();
            Cities = citiesList;

            var mailBatchesList = _messageDeliveryRepository.GetMailBatches();
            MailBatches = mailBatchesList;

        }

        public IActionResult OnPost()
        {
            _logger.LogInformation($"CountriesController->onPost: Picked country id = {CountryId}");
            var state = _messageDeliveryRepository.ExecuteQuery(CountryId, CityId, BatchId);
            if (state.IsSuccess)
            {
                var affectedRowsAmount = state.Cast<ModelResult<int>>().Model;
                _logger.LogInformation($"CountriesController->onPost: IsSuccess = true, rows affected = {affectedRowsAmount}");
            }
            return RedirectToPage("/Index");

        }
    }
}
