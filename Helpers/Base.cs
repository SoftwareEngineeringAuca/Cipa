using System.Configuration;

namespace Cipa.Helpers
{
    public class Base
    {
        public static string ConnectionString => "Server=(local);Database=CIPA;User Id=sa;Password=1;MultipleActiveResultSets=true";//ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
    }
}