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
    public class WorkCodeRepository: Base, IWorkCodeRepository
    {
        public ICipaSystemRepository _cipaSystemRepository;
        public WorkCodeRepository(ICipaSystemRepository cipaSystemRepository)
        {
            _cipaSystemRepository = cipaSystemRepository;
        }
        public ExecuteResult ExecuteWorkCodeQuery(int countryId)
        {
            var sessionId = _cipaSystemRepository.GetActiveSessionId().Cast<ModelResult<int>>().Model;
            var workCode = GetLastWorkCode(sessionId); //
            //parse work code if [1-3] * 100000 >= max int in db then => char(workCode[0]+1) as acii codes should be next in alphabet and [1-3] is 100000!

            int startNumber = 1;

            var query = GetQuery(countryId, sessionId, startNumber);
            var totalRowsAffected = 0;
            using var con = new SqlConnection(ConnectionString);
            using var command = con.CreateCommand();
            command.CommandText = query;
            try
            {
                con.Open();
                totalRowsAffected = command.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                con.Close();
                return new ExecuteResult
                {
                    Message = e.Message,
                    State = ExecuteState.Error
                };
            }

            return new ModelResult<int>
            {
                State = ExecuteState.Success,
                Model = totalRowsAffected
            };
        }

        private string GetQuery(in int countryId, in int sessionId, int startNumber)
        {
            return "";
        }

        private string GetLastWorkCode(int sessionId)
        {
            var script = @$"select TOP 1 Work_Code from RegForms_Items rfi
                            join RegForms_Headers rfh on rfi.Header_ID = rfh.Header_ID
                            where Session_ID = {sessionId}
                            order by Form_Date desc";
            var workCode = "";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (var da = new SqlDataAdapter(script, connection))
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

                                workCode = data.Field<string>("Work_Code");
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

            return workCode;
        }
    }
}
