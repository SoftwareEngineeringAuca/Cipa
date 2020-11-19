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
    public class DuplicationRepository: Base, IDuplicationRepository
    {
        public ExecuteResult ExecuteWorkCodeQuery(string userCode)
        {
            var script = GetScript(userCode);
            var totalRowsAffected = 0;
            using var con = new SqlConnection(ConnectionString);
            using var command = con.CreateCommand();
            command.CommandText = script;
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
            finally
            {
                con.Close();
                con.Dispose();
            }

            return new ModelResult<int>
            {
                State = ExecuteState.Success,
                Model = totalRowsAffected
            };
        }

        private string GetScript(string userCode)
        {
            return @$"
                    declare @mainCode nvarchar(10) = '{userCode}', 
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

                    select @mainCode, @notMainCode, @fullName, @mainId, @notMainId

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
