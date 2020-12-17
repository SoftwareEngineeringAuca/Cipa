using Cipa.Helpers;
using Cipa.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Cipa.BusinessModels;
using Study.Common.Results;

namespace Cipa.Repositories
{
    public class CipaSystemRepository: Base, ICipaSystemRepository
    {
        public ExecuteResult GetActiveSessionId()
        {
            var responseData = new SystemSettingsModel();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var query = "SELECT [SettingsName],[SettingsValue] FROM [SystemSettings] WHERE SettingsName = 'ACTIVE_SESSION_REGISTRATION'";
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

                                responseData = new SystemSettingsModel
                                {
                                    SettingsName = data.Field<string>("SettingsName"),
                                    SettingsValue = data.Field<string>("SettingsValue")
                                };
                            }
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
            }
            
            return new ModelResult<int>
            {
                State = ExecuteState.Success,
                Model = int.Parse(responseData.SettingsValue)
            };
        }

        public ExecuteResult GenerateNewSession()
        {
            var script = @" Declare @ActiveSessionId INT =  CONVERT(INT, (select SettingsValue from SystemSettings where SettingsName = 'ACTIVE_SESSION_REGISTRATION'));
                            update SystemSettings set SettingsValue = @ActiveSessionId where SettingsName = 'ACTIVE_SESSION_CHECK'
                            update SystemSettings set SettingsValue = @ActiveSessionId + 1 where SettingsName = 'ACTIVE_SESSION_REGISTRATION'";

            var totalRowsAffected = 0;
            using var con = new SqlConnection(ConnectionString);
            using var command = con.CreateCommand();
            command.CommandTimeout = 0;
            command.CommandText = script;
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
        public ExecuteResult RevertSession()
        {
            var script = @" Declare @ActiveSessionId INT =  CONVERT(INT, (select SettingsValue from SystemSettings where SettingsName = 'ACTIVE_SESSION_CHECK'));
                            update SystemSettings set SettingsValue = @ActiveSessionId - 1 where SettingsName = 'ACTIVE_SESSION_CHECK'
                            update SystemSettings set SettingsValue = @ActiveSessionId where SettingsName = 'ACTIVE_SESSION_REGISTRATION'";
            var totalRowsAffected = 0;
            using var con = new SqlConnection(ConnectionString);
            using var command = con.CreateCommand();
            command.CommandTimeout = 0;
            command.CommandText = script;
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
