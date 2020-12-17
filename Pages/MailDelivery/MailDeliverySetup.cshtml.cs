using System.Collections.Generic;
using System.Linq;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Study.Common.Results;

namespace Cipa.Pages.MailDelivery
{
    public class MailDeliverySetup : PageModel
    {
        private readonly ILogger<MailDeliverySetup> _logger;
        private readonly ICountryRepository _countryRepository;
        private readonly IMessageDeliveryRepository _messageDeliveryRepository;
        private readonly IExamsRepository _examsRepository;

        public MailDeliverySetup(ILogger<MailDeliverySetup> logger, ICountryRepository countryRepository, IMessageDeliveryRepository messageDeliveryRepository, IExamsRepository examsRepository)
        {
            _logger = logger;
            _countryRepository = countryRepository;
            _messageDeliveryRepository = messageDeliveryRepository;
            _examsRepository = examsRepository;
        }
        
        [BindProperty]
        public int CountryId { get; set; }
        [BindProperty]
        public int CityId { get; set; }
        [BindProperty]
        public int BatchId { get; set; }
        public string Message { get; set; }
        public IEnumerable<CountryViewModel> Countries { get; set; } = new List<CountryViewModel>();
        public IEnumerable<CityViewModel> Cities { get; set; } =  new List<CityViewModel>();
        public IEnumerable<MailBatchesViewModel> MailBatches { get; set; } = new List<MailBatchesViewModel>();
        public IEnumerable<ExamViewModel> Exams { get; set; } = new List<ExamViewModel>();

        public IActionResult OnGet()
        {
            Countries = _countryRepository.GetCountries();

            Cities = _countryRepository.GetCities();

            MailBatches = _messageDeliveryRepository.GetMailBatches();

            Exams = _examsRepository.GetCurrentSessionExams();
            return Page();
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation($"MailDeliverySetup->onPost: Picked country id = {CountryId}");
            var state = _messageDeliveryRepository.ExecuteQuery(CountryId, CityId, BatchId);
            if (state.IsSuccess)
            {
                var affectedRowsAmount = state.Cast<ModelResult<int>>().Model;
                _logger.LogInformation($"MailDeliverySetup->onPost: IsSuccess = true, rows affected = {affectedRowsAmount}");
                Message = $"Запрос выполнился успешно, {affectedRowsAmount} полей добавилось!";
                return OnGet();
            }

            Message = "Произошла ошибка: " + state.Message;
            return Page();

        }
        public IActionResult OnPostShowExamResult()
        {
            var state = _messageDeliveryRepository.ExecuteQuery(CountryId, CityId, BatchId);
            if (state.IsSuccess)
            {
                var affectedRowsAmount = state.Cast<ModelResult<int>>().Model;
                _logger.LogInformation($"MailDeliverySetup->OnPostShowExamResult: IsSuccess = true, rows affected = {affectedRowsAmount}");
                Message = $"Запрос выполнился успешно, {affectedRowsAmount} полей добавилось!";
                return OnGet();
            }
            Message = "Произошла ошибка: " + state.Message;
            return Page();

        }
    }
}
