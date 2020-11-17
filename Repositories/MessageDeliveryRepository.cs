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
    public class MessageDeliveryRepository : Base, IMessageDeliveryRepository
    {
        public IEnumerable<MailBatchesViewModel> GetMailBatches()
        {
            var responseData = new List<MailBatchesViewModel>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var query = "select top 10 * from Mail_Batches order by Batch_ID Desc";
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

                                responseData.Add(new MailBatchesViewModel
                                {
                                    BatchId = data.Field<int>("Batch_ID"),
                                    BatchName = data.Field<string>("Batch_Name")
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

        public ExecuteResult ExecuteQuery(int countryId, int cityId, int batchId)
        {
            
            var innerQuery = getInnerQuery(countryId, cityId);
            var sql = "insert into Mail_Delivery (Person_ID, Person_Mail, Batch_ID, IsDelivered) " +
                       $" select distinct p.Person_ID, pc.Value, {batchId}, 0  from People p " +
                       " join RegForms_Headers rfh on p.Person_ID=rfh.Person_ID " +
                       " join People_Contacts pc on p.Person_ID=pc.Person_ID " +
                       " where pc.Type_ID=4 and rfh.City_ID in (" + innerQuery + ") " +
                       " and pc.Value not in (select * from Auxiliary_BrokenEmails) ";

            var totalRowsAffected = 0;
            using var con = new SqlConnection(ConnectionString);
            using var command = con.CreateCommand();
            command.CommandText = sql;
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

        private string getInnerQuery(int countryId, int cityId)
        {
            return countryId > 0 ? $"select City_ID from Cities where Country_ID = {countryId}" : $" {cityId} ";
        }
    }
}

