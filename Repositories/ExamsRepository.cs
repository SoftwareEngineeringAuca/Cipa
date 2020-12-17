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
    public class ExamsRepository: Base, IExamsRepository
    {
        public IEnumerable<ExamViewModel> GetExams()
        {
            var responseData = new List<ExamViewModel>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var query = "SELECT [Exam_ID],[Exam_Name] FROM Exams";
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

                                responseData.Add(new ExamViewModel
                                {
                                    ExamId = data.Field<int>("Exam_ID"),
                                    ExamName = data.Field<string>("Exam_Name")
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

        public IEnumerable<ExamViewModel> GetCurrentSessionExams()
        {
            var responseData = new List<ExamViewModel>();
            //get Active sessionId 
            var session = new CipaSystemRepository();
            var sessionId = session.GetActiveSessionId().Cast<ModelResult<int>>().Model;
            using (var connection = new SqlConnection(ConnectionString))
            {
                var query = @" SELECT e.Exam_ID, e.Exam_Name, es.Exam_Date FROM Exams_Schedule AS es
                                    INNER JOIN Exams AS e ON e.Exam_ID = es.Exam_ID
                                    WHERE es.Session_ID = " + sessionId;

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

                                responseData.Add(new ExamViewModel
                                {
                                    ExamId = data.Field<int>("Exam_ID"),
                                    ExamName = data.Field<string>("Exam_Name"),
                                    ExamDate = data.Field<DateTime>("Exam_Date")
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
    }
}
