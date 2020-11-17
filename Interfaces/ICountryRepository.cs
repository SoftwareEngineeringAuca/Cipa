using System.Collections.Generic;
using Cipa.BusinessModels;

namespace Cipa.Interfaces
{
    public interface ICountryRepository
    {
        IEnumerable<CountryViewModel> GetCountries();
        IEnumerable<CityViewModel> GetCities();
    }
}