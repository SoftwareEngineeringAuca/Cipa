using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Cipa.Helpers;
using Cipa.Interfaces;
using Study.Common.Results;

namespace Cipa.Repositories
{
    public class ExamsSessionsRepository: Base, IExamsSessionsRepository
    {
        private readonly ICipaSystemRepository _cipaSystemRepository;

        public ExamsSessionsRepository(ICipaSystemRepository cipaSystemRepository)
        {
            _cipaSystemRepository = cipaSystemRepository;
        }

        public ExecuteResult CreateSession(string sessionName, DateTime sessionStart, DateTime sessionEnd)
        {
            //generate new ACTIVE_SESSION_REGISTRATION
            var responseOfNewActiveSession = _cipaSystemRepository.GenerateNewSession();
            if (!responseOfNewActiveSession.IsSuccess)
            {
                return new ExecuteResult
                {
                    Message = responseOfNewActiveSession.Message,
                    State = ExecuteState.Error
                };
            }

            var activeSessionRegistrationId = _cipaSystemRepository.GetActiveSessionId().Cast<ModelResult<int>>().Model;
            var sql = $" INSERT INTO Exams_Sessions VALUES ({activeSessionRegistrationId}, @SessionName, '{ParseDate(sessionStart)}', '{ParseDate(sessionEnd)}') ";
            var totalRowsAffected = 0;
            using var con = new SqlConnection(ConnectionString);
            using var command = con.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 0;
            command.Parameters.AddWithValue("@SessionName", sessionName);

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

            return new ModelResult<int>
            {
                State = ExecuteState.Success,
                Model = totalRowsAffected
            };
        }

        private string ParseDate(DateTime date)
        {
            return date.Year + "-" + date.Month + "-" + date.Day;
        }

        public ExecuteResult AddExamSchedule(int examId, DateTime examDate)
        {
            //get current session id
            var activeSessionRegistrationId = _cipaSystemRepository.GetActiveSessionId().Cast<ModelResult<int>>().Model;
            var sql = $" INSERT INTO Exams_Schedule VALUES ({activeSessionRegistrationId}, {examId}, '{ParseDate(examDate)}', NULL, NULL, 0) ";
            var totalRowsAffected = 0;
            using var con = new SqlConnection(ConnectionString);
            using var command = con.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 0;

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

            return new ModelResult<int>
            {
                State = ExecuteState.Success,
                Model = totalRowsAffected
            };
        }
    }
}
