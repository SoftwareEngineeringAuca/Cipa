using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Study.Common.Results;

namespace Cipa.Pages.Scores
{
    public class ScoreModel : PageModel
    {
        public readonly IScoreRepository _ScoreRepository;

        public ScoreModel(IScoreRepository scoreRepository)
        {
            _ScoreRepository = scoreRepository;
        }

        public List<LookUpModel> CheckersList { get; set; } = new List<LookUpModel>();
        public List<LookUpModel> EvalFormList { get; set; } = new List<LookUpModel>();
        [BindProperty]
        public int SelectedEvalId { get; set; }
        [BindProperty]
        public int SelectedCheckerId { get; set; }
        [BindProperty]
        public int FactMax { get; set; }
        [BindProperty]
        public int FactPass { get; set; }
        [BindProperty]
        public int Gap { get; set; }
        public string Message { get; set; }
        public IActionResult OnGet()
        {
            CheckersList = _ScoreRepository.GetAllCheckers().Cast<ModelResult<List<LookUpModel>>>().Model;
            EvalFormList = _ScoreRepository.GetEvalFormList().Cast<ModelResult<List<LookUpModel>>>().Model;
            return Page();
        }

        public IActionResult OnPost()
        {
            var response = _ScoreRepository.ExecuteScoreCalculation(SelectedEvalId, FactPass, FactMax, Gap, SelectedCheckerId);

            if (response.IsSuccess)
            {
                var totalAffectedRows = response.Cast<ModelResult<int>>().Model;
                Message = $"Запрос выполнился успешно, колличество измененных данных = {totalAffectedRows}";
                return OnGet();
            }

            Message = "Запрос выполнился с ошибкой: " + response.Message;
            return Page();

        }
        public IActionResult OnPostPublish()
        {
            var response = _ScoreRepository.PublishResults(SelectedEvalId);

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
