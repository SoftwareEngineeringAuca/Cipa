using System;
using System.Data.SqlClient;
using Cipa.Helpers;
using Cipa.Interfaces;
using Study.Common.Results;

namespace Cipa.Repositories
{
    public class UsersSessionRepository: Base, IUsersSessionRepository
    {

        public ICipaSystemRepository _cipaSystemRepository;
        public UsersSessionRepository(ICipaSystemRepository cipaSystemRepository)
        {
            _cipaSystemRepository = cipaSystemRepository;
        }

        public ExecuteResult ExecuteScript(int countryId, int cityId)
        {

            var sessionId = _cipaSystemRepository.GetActiveSessionId().Cast<ModelResult<int>>().Model;
            var isFirstRun = IsFirstRun(sessionId);
            var updateScriptWhenFirstRun = " ";
            if (!isFirstRun.IsSuccess)
            {
                return new ExecuteResult
                {
                    Message = isFirstRun.Message,
                    State = ExecuteState.Error
                };
            }
            if (isFirstRun.Cast<ModelResult<bool>>().Model)
            {
                //Execute update RegForms_Items set IsPublished = 0 when run the first time in Session.
                updateScriptWhenFirstRun = " update RegForms_Items set IsPublished = 0; ";
            }

            var innerQuery = getInnerQuery(countryId, cityId);
            var sql = $@" {updateScriptWhenFirstRun} update RegForms_Items
                        set IsPublished = 1
                        from RegForms_Items rfi
                        join RegForms_Headers rfh on rfi.Header_ID = rfh.Header_ID
                        join People p on p.Person_ID = rfh.Person_ID
                        left join People_Contacts pc on p.Person_ID = pc.Person_ID and pc.Type_ID = 4
                        join Exams_Schedule es on rfi.Schedule_ID = es.Schedule_ID
                        where rfh.City_ID IN ({innerQuery}) and es.Session_ID = {sessionId}";
            using var con = new SqlConnection(ConnectionString);
            using var command = con.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 0;
            var totalRowsAffected = 0;
            try
            {
                con.Open();
                totalRowsAffected = command.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                return new ExecuteResult
                {
                    Message = e.Message,
                    State = ExecuteState.Error
                };
            }
            finally
            {
                con.Close();
            }

            return new ModelResult<int>()
            {
                State = ExecuteState.Success,
                Model = totalRowsAffected
            };
        }

        private string getInnerQuery(int countryId, int cityId)
        {
            return countryId > 0 ? $"select City_ID from Cities where Country_ID = {countryId}" : $" {cityId} ";
        }

        private ExecuteResult IsFirstRun(int sessionId)
        {
            var script = $@"select top 1 Person_ID from RegForms_Items as rfi
                                    inner join RegForms_Headers rfh on rfi.Header_ID = rfh.Header_ID
                                    where rfi.IsPublished = 1 and rfh.Session_ID = {sessionId}";
            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand(script, connection);
                try
                {
                    connection.Open();
                    command.CommandTimeout = 0;
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        return new ModelResult<bool>
                        {
                            State = ExecuteState.Success,
                            Model = false
                        };
                    }
                }
                catch (Exception e)
                {
                    return new ExecuteResult
                    {
                        Message = e.Message,
                        State = ExecuteState.Error
                    };
                }
                finally
                {
                    connection.Close();
                }
            }

            return new ModelResult<bool>
            {
                State = ExecuteState.Success,
                Model = true
            };
        }
    }
}
