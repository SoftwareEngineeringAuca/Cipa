using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Study.Common.Results;

namespace Cipa.Repositories
{
    public class GradingRepository: Base, IGradingRepository
    {
        private readonly ICipaSystemRepository _cipaSystemRepository;
        public GradingRepository(ICipaSystemRepository cipaSystemRepository)
        {
            _cipaSystemRepository = cipaSystemRepository;
        }
        public ExecuteResult AddCountryToGrading(int gradingId, int countryId)
        {
            //add gradings to all session countries
            var sql = "insert into EvalForms_Headers_Countries values ";
            var gradingData = GetGradingById(gradingId);
            if (countryId == 0)
            {
                var countryIds = GetAllSessionCountries();
                for (int i = 0; i < countryIds.Count; i++)
                {
                    //insert without last comma ','
                    if (countryIds.Count - 1 == i)
                    {
                        sql += $"({gradingId}, {gradingData.ScheduleId}, {countryIds[i]}, {gradingData.ExamId})";
                    }
                    else
                    {
                        sql += $"({gradingId}, {gradingData.ScheduleId}, {countryIds[i]}, {gradingData.ExamId}), ";
                    }
                }
            }
            else //country Id is picked, generate script for one country.
            {
                sql += $"({gradingId}, {gradingData.ScheduleId}, {countryId}, {gradingData.ExamId})";
            }
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

        public IEnumerable<GradingViewModel> GetGradings()
        {
            var responseData = new List<GradingViewModel>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var query = "select top 20 Header_ID, Schedule_ID, Header_Name, Exam_ID from EvalForms_Headers order by Header_ID desc";
                using (var da = new SqlDataAdapter(query, connection))
                {
                    var tblPromotion = new DataTable();
                    try
                    {
                        connection.Open();
                        da.Fill(tblPromotion);
                        if (tblPromotion.Rows.Count != 0)
                        {
                            for (var i = 0; i < tblPromotion.Rows.Count; i++)
                            {
                                var data = tblPromotion.Rows[i];

                                responseData.Add(new GradingViewModel
                                {
                                    GradingId = data.Field<int>("Header_ID"),
                                    GradingName = data.Field<string>("Header_Name")
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            return responseData;
        }
        private GradingViewModel GetGradingById(int headerId)
        {
            var responseData = new GradingViewModel();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var query = $"select Header_ID, Schedule_ID, Header_Name, Exam_ID from EvalForms_Headers where Header_ID = {headerId}";
                using (var da = new SqlDataAdapter(query, connection))
                {
                    var tblPromotion = new DataTable();
                    try
                    {
                        connection.Open();
                        da.Fill(tblPromotion);
                        if (tblPromotion.Rows.Count != 0)
                        {
                            for (var i = 0; i < tblPromotion.Rows.Count; i++)
                            {
                                var data = tblPromotion.Rows[i];

                                responseData = new GradingViewModel
                                {
                                    GradingId = data.Field<int>("Header_ID"),
                                    GradingName = data.Field<string>("Header_Name"),
                                    ScheduleId = data.Field<int>("Schedule_ID"),
                                    ExamId = data.Field<int>("Exam_ID")
                                };
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            return responseData;
        }
        private List<int> GetAllSessionCountries()
        {
            //get the session ID
            var sessionId = _cipaSystemRepository.GetActiveSessionId().Cast<ModelResult<int>>().Model;
            var countryIds = new List<int>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var query = $"select Distinct Country_ID from Cities where City_ID in(select City_ID from RegForms_Headers where Session_ID = {sessionId})";
                using (var da = new SqlDataAdapter(query, connection))
                {
                    var tblPromotion = new DataTable();
                    try
                    {
                        connection.Open();
                        da.Fill(tblPromotion);
                        if (tblPromotion.Rows.Count != 0)
                        {
                            for (var i = 0; i < tblPromotion.Rows.Count; i++)
                            {
                                var data = tblPromotion.Rows[i];

                                countryIds.Add(data.Field<int>("Country_ID"));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            return countryIds;
        }
    }
}
