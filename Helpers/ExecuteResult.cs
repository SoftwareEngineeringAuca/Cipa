using System;
using System.Diagnostics;

namespace Cipa.Helpers
{
    public class ExecuteResult
    {
        public ExecuteState State { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsSuccess => State == ExecuteState.Success;

        public static ExecuteResult Success()
        {
            return new ExecuteResult { State = ExecuteState.Success };
        }

        public static ExecuteResult Success(string message)
        {
            return new ExecuteResult { State = ExecuteState.Success, Message = message };
        }

        public static ExecuteResult Error(string errorMessage)
        {
            return new ExecuteResult { State = ExecuteState.Error, Message = errorMessage };
        }

        public static TR Error<TR>(string errorMessage) where TR : ExecuteResult, new()
        {
            return new TR { State = ExecuteState.Error, Message = errorMessage };
        }
        public static TR Error<TR>(Exception exception) where TR : ExecuteResult, new()
        {
            return Error<TR>(exception.Message);
        }

        public static TR Success<TR>(Action<TR> action) where TR : ExecuteResult, new()
        {
            var result = new TR { State = ExecuteState.Success };
            action(result);
            return result;
        }

        public static ExecuteResult Error(Exception exception)
        {
            return Error(exception.Message);
        }
    }

    public static class ExecuteResultHelper
    {
        public static void Check(this ExecuteResult result)
        {
            if (result.State == ExecuteState.Error) throw new Exception(result.Message);
        }

        public static T Cast<T>(this ExecuteResult result) where T : ExecuteResult
        {
            result.Check();
            var cast = result as T;
            if (cast != null) return (T)result;

            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType?.FullName;
            throw new Exception($"Wrong call method Cast in {type}");
        }


        public static TReturn Get<TExecuteResult, TReturn>(this TExecuteResult result, Func<TExecuteResult, TReturn> getFunc) where TExecuteResult : ExecuteResult
        {
            result.Check();
            return getFunc(result);
        }
    }
}
