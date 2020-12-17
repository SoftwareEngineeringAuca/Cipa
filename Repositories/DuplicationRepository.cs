using System;
using System.Data;
using System.Data.SqlClient;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Study.Common.Results;

namespace Cipa.Repositories
{
    public class DuplicationRepository: Base, IDuplicationRepository
    {
        public ExecuteResult ExecuteWorkCodeQuery(string userCode)
        {
            var script = GetScript();
            var responseData = new DuplicationMergeResponseModel();
            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand(script, connection);
                command.Parameters.AddWithValue("@UserCode", userCode);
                try
                {
                    connection.Open();
                    command.CommandTimeout = 0;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        responseData.MainCode = reader.IsDBNull(reader.GetOrdinal("MainCode")) ? reader.GetFieldValue<string>("MainCode") : "NULL";
                        responseData.NotMainCode = reader.IsDBNull(reader.GetOrdinal("NotMainCode")) ? reader.GetFieldValue<string>("NotMainCode") : "NULL";
                        responseData.FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? reader.GetFieldValue<string>("FullName") : "NULL";
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

            return new ModelResult<DuplicationMergeResponseModel>
            {
                State = ExecuteState.Success,
                Model = responseData
            };
        }

        private string GetScript()
        {
            return @$"
                    declare @mainCode nvarchar(10) = @UserCode, 
		                    @notMainCode nvarchar(10),
		                    @fullName nvarchar(max),
		                    @mainId int,
		                    @notMainId int

                    select @fullName = Full_Name from People
                    where Person_Code = @mainCode

                    select @notMainCode = Person_Code from People
                    where Full_Name = @fullName and Person_Code != @mainCode

                    select @mainId = Person_ID from People
                    where Person_Code = @mainCode
                    select @notMainId = Person_ID from People
                    where Person_Code = @notMainCode

                    select @mainCode as MainCode, @notMainCode as NotMainCode, @fullName as FullName, @mainId as MainId, @notMainId as NotMainId

                    update RegForms_Headers
                    set Person_ID = @mainId
                    where Person_ID = @notMainId
                    update Pending_RegForms_Headers
                    set Person_ID = @mainId
                    where Person_ID = @notMainId
                    update Mail_Delivery
                    set Person_ID = @mainId
                    where Person_ID = @notMainId
                    update LearningCourses
                    set Person_ID = @mainId
                    where Person_ID = @notMainId
                    update People_Photos
                    set Person_ID = @mainId
                    where Person_ID = @notMainId

                    delete from People_Contacts
                    where Person_ID = @notMainId
                    delete from People
                    where Person_ID = @notMainId
                    ";
		}
    }
}
