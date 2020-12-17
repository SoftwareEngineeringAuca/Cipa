using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Study.Common.Results;

namespace Cipa.Repositories
{
    public class ScoreRepository: Base, IScoreRepository
    {
        public ExecuteResult GetEvalFormList()
        {
            var responseData = new List<LookUpModel>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var query = "select top(20) Header_ID, Header_Name from EvalForms_Headers order by Header_ID Desc";
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

                                responseData.Add(new LookUpModel
                                {
                                    Id = data.Field<int>("Header_ID"),
                                    Name = data.Field<string>("Header_Name")
                                });
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
                }
            }
            return new ModelResult<List<LookUpModel>>
            {
                Model = responseData,
                State = ExecuteState.Success
            };

        }

        public ExecuteResult GetAllCheckers()
        {
            var responseData = new List<LookUpModel>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var query = @" SELECT [Checker_ID],[Checker_Name] FROM [Results_Checkers] ";
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

                                responseData.Add(new LookUpModel
                                {
                                    Id = data.Field<int>("Checker_ID"),
                                    Name = data.Field<string>("Checker_Name")
                                });
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
                }
            }
            return new ModelResult<List<LookUpModel>>
            {
                Model = responseData,
                State = ExecuteState.Success
            };
        }

        public ExecuteResult ExecuteScoreCalculation(int evalFormHeaderId, int factPass, int factMax, int gap, int checkerId)
        {
            var sql = $@" 				
			                    UPDATE EvalForms_Headers
				                    SET Fact_Pass='{factPass}', Fact_Max='{factMax}', Gap='{gap}'
				                    WHERE Header_ID='{evalFormHeaderId}';

				                    EXEC SetContext 'CUSTOM'

				                    DECLARE MyCursor Cursor FOR
				                    SELECT 
				                    rh.Header_ID
				                    FROM Results_Headers rh
				                    INNER JOIN RegForms_Items rfi
				                    ON rfi.Item_ID=rh.RegForm_Item_ID
				                    INNER JOIN RegForms_Headers rfh
				                    ON rfh.Header_ID=rfi.Header_ID
				                    WHERE EvalForm_Header_ID IN ({evalFormHeaderId})
				                    AND rh.Fact_Score IS NOT NULL
				                    AND rh.Notify_Score IS NULL
				                    AND rh.Checker_ID = {checkerId}

				                    DECLARE @headerId INT

				                    OPEN MyCursor
				                    FETCH NEXT FROM MyCursor INTO @headerId

				                    WHILE @@FETCH_STATUS = 0
				                    BEGIN
					                    EXEC dbo.Results_Headers_NotifyScoreCalculate @headerID
					                    FETCH NEXT FROM MyCursor INTO @headerId
				                    END

				                    CLOSE MyCursor
				                    DEALLOCATE MyCursor

					                    exec SetContext 'CUSTOM'
					                    UPDATE Results_Headers
					                    SET IsFinal ='1' 
					                    WHERE EvalForm_Header_ID = '{evalFormHeaderId}'                                
                              ";
           
            var totalRowsAffected = 0;
            using var con = new SqlConnection(ConnectionString);
            con.Open();
            using var calculateScoreCommand = con.CreateCommand();

            SqlTransaction transaction;

            // Start a local transaction.
            transaction = con.BeginTransaction("ScoreTransaction");

            // Must assign both transaction object and connection
            // to Command object for a pending local transaction
            calculateScoreCommand.Connection = con;
            calculateScoreCommand.Transaction = transaction;
            
            try
            {
                calculateScoreCommand.CommandText = sql;
                calculateScoreCommand.CommandTimeout = 0;
                totalRowsAffected = calculateScoreCommand.ExecuteNonQuery();
                // Attempt to commit the transaction.
                transaction.Commit();
            }
            catch (Exception e)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception e2)
                {
                    return new ExecuteResult
                    {
                        Message = e.Message + "; " + e2.Message,
                        State = ExecuteState.Error
                    };
                }
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

        public ExecuteResult PublishResults(int evalFormHeaderId)
        {
            var sql = $"exec SetContext 'CUSTOM'; UPDATE Results_Headers SET IsForPublish = '1' WHERE EvalForm_Header_ID = {evalFormHeaderId}";
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
