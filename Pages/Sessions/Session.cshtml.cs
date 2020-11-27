using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Study.Common.Results;

namespace Cipa.Pages.Sessions
{
    public class SessionModel : PageModel
    {
        private readonly ILogger<SessionModel> _logger;
        private readonly ICipaSystemRepository _cipaSystemRepository;
        private readonly IExamsSessionsRepository _examsSessionsRepository;
        private readonly IExamsRepository _examsRepository;


        public int ActiveSessionNumber { get; set; }
        public string Message { get; set; } = "";
        public IEnumerable<ExamViewModel> ExamsList { get; set; } = new List<ExamViewModel>();
        public IEnumerable<ExamViewModel> ExamsInSessionList { get; set; } = new List<ExamViewModel>();
        [BindProperty] 
        public string SessionName { get; set; }

        [BindProperty] 
        [DataType(DataType.Date)]
        public DateTime SessionStart { get; set; }

        [BindProperty]
        [DataType(DataType.Date)]
        public DateTime SessionEnd { get; set; }
        [BindProperty]
        public int ExamId { get; set; }
        [BindProperty]
        public DateTime ExamDate { get; set; }

        public SessionModel(ICipaSystemRepository cipaSystemRepository, ILogger<SessionModel> logger, IExamsSessionsRepository examsSessionsRepository, IExamsRepository examsRepository)
        {
            _cipaSystemRepository = cipaSystemRepository;
            _logger = logger;
            _examsSessionsRepository = examsSessionsRepository;
            _examsRepository = examsRepository;
        }

        public void OnGet()
        {
            ActiveSessionNumber = _cipaSystemRepository.GetActiveSessionId().Cast<ModelResult<int>>().Model;
            ExamsList = _examsRepository.GetExams();
            ExamsInSessionList = _examsRepository.GetCurrentSessionExams();
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation($"SessionModel->onPost: GenerateNewSession");
            var state = _cipaSystemRepository.GenerateNewSession();
            if (state.IsSuccess)
            {
                var affectedRowsAmount = state.Cast<ModelResult<int>>().Model;
                _logger.LogInformation($"SessionModel->onPost: GenerateNewSession, rows affected = {affectedRowsAmount}");
                ActiveSessionNumber = _cipaSystemRepository.GetActiveSessionId().Cast<ModelResult<int>>().Model;
                return RedirectToPage("/Sessions/Session");
            }

            Message = "Query Executed with Error: " + state.Message;
            return Page();

        }
        public IActionResult OnPostRevert()
        {
            _logger.LogInformation($"SessionModel->OnPostRevert: RevertSession");
            var state = _cipaSystemRepository.RevertSession();
            if (state.IsSuccess)
            {
                var affectedRowsAmount = state.Cast<ModelResult<int>>().Model;
                _logger.LogInformation($"SessionModel->onPost: GenerateNewSession, rows affected = {affectedRowsAmount}");
                Message = $"Query Executed, {affectedRowsAmount} rows affected!";
                return RedirectToPage("/Sessions/Session");
            }
            Message = "Query Executed with Error: " + state.Message;
            return Page();

        }
        public IActionResult OnPostAddSession()
        {
            _logger.LogInformation($"SessionModel -> OnPostAddSession: AddSession");
            var state = _examsSessionsRepository.CreateSession(SessionName, SessionStart, SessionEnd);
            if (state.IsSuccess)
            {
                var affectedRowsAmount = state.Cast<ModelResult<int>>().Model;
                _logger.LogInformation($"SessionModel->OnPostAddSession: AddSession, rows affected = {affectedRowsAmount}");
                Message = $"Query Executed, {affectedRowsAmount} rows affected!";
                return RedirectToPage("/Sessions/Session");
            }
            Message = "Query Executed with Error: " + state.Message;
            return Page();

        }
        public IActionResult OnPostAddExam()
        {
            _logger.LogInformation($"SessionModel -> OnPostAddExam: AddExam");
            var state = _examsSessionsRepository.AddExamSchedule(ExamId, ExamDate);
            if (state.IsSuccess)
            {
                var affectedRowsAmount = state.Cast<ModelResult<int>>().Model;
                _logger.LogInformation($"SessionModel->OnPostAddExam: AddExam, rows affected = {affectedRowsAmount}");
                Message = $"Query Executed, {affectedRowsAmount} rows affected!";
                return RedirectToPage("/Sessions/Session");
            }
            Message = "Query Executed with Error: " + state.Message;
            return Page();

        }
    }
}
