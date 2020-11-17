using Cipa.Helpers;

namespace Study.Common.Results
{
    public class ModelResult<T> : ExecuteResult
    {
        public T Model { get; set; }
    }
}
