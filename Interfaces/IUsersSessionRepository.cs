using Cipa.Helpers;

namespace Cipa.Interfaces
{
    public interface IUsersSessionRepository
    {
        ExecuteResult ExecuteScript(int countryId, int cityId);
    }
}
