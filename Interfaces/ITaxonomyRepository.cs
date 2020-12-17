using System.Threading.Tasks;
using Cipa.Helpers;
using Microsoft.AspNetCore.Http;

namespace Cipa.Interfaces
{
    public interface ITaxonomyRepository
    {
        ExecuteResult CreateTaxonomyExam(int examId, string taxonomyName);
        Task<ExecuteResult> AddTaxonomy(IFormFile file, int headerId);
        bool ValidateFile(IFormFile file);
    }
}