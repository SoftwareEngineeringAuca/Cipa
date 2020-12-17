using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Study.Common.Results;

namespace Cipa.Repositories
{
    public class TaxonomyRepository: Base, ITaxonomyRepository
    {
        public ExecuteResult CreateTaxonomyExam(int examId, string taxonomyName)
        {
            var script = $@"insert into Exams_Content_Headers values(@ExamId, @TaxonomyName);
                                    select top(1) Header_ID from Exams_Content_Headers 
                                    where Header_Name = @TaxonomyName order by Header_ID desc";
            var headerId = 0;
            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand(script, connection);
                command.Parameters.AddWithValue("@ExamId", examId);
                command.Parameters.AddWithValue("@TaxonomyName", taxonomyName);
                try
                {
                    connection.Open();
                    command.CommandTimeout = 0;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        /*headerId = reader.IsDBNull(reader.GetOrdinal("Header_ID"))
                            ? reader.GetFieldValue<int>("Header_ID")
                            : 0;*/
                        headerId = reader.GetFieldValue<int>("Header_ID");

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

            return new ModelResult<int>
            {
                State = ExecuteState.Success,
                Model = headerId
            };
        }

        public async Task<ExecuteResult> AddTaxonomy(IFormFile file, int headerId)
        {
            
            var script = new StringBuilder();
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets[1];

                    var rowCount = worksheet.Dimension.End.Row;
                    var counter = 1;
                    for (var row = 2; row <= rowCount; row++)
                    {
                        var dataObject = worksheet.Cells[row, 1].Value;
                        if (dataObject != null)
                        {
                            var data = dataObject.ToString()?.Split('.');
                            var value = data[^1];
                            var pos = data.Length;
                            var theme = worksheet.Cells[row, 2].Value.ToString();
                            script.AppendLine($@" INSERT [Exams_Content_Items] 
                                ([Header_ID], [Item_Path], [Item_Code], [Item_OutlineLevel], [Item_Subject], [Item_Weight], [Item_Level], [IsLeaf], [IsLeafForReport], [PriorityOrder])
                                 SELECT {headerId}, hierarchyid::Parse ('/0/'), '{int.Parse(value)}', {pos}, '{theme}', NULL, NULL, 0, 0, {counter++}"
                            );

                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            //add additional scripts
            script.AppendLine(GetAdditionalScript(headerId));

            var totalRowsAffected = 0;
            using var con = new SqlConnection(ConnectionString);
            using var command = con.CreateCommand();
            command.CommandText = script.ToString();
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

        private string GetAdditionalScript(int headerId)
        {
            return $@"
                UPDATE [Exams_Content_Items]
                SET [Item_Path] = hierarchyid::Parse ('/' + CAST(Item_ID AS VARCHAR(32)) + '/')
                WHERE
	                [Header_ID] = {headerId} AND [Item_OutlineLevel] = 1

                UPDATE ch
                SET [Item_Path] = hierarchyid::Parse ((SELECT TOP 1 CAST(Item_Path AS nvarchar(4000)) FROM [Exams_Content_Items] WHERE [Header_ID] = {headerId} AND [Item_OutlineLevel] = ch.[Item_OutlineLevel] - 1 AND PriorityOrder < ch.PriorityOrder ORDER BY PriorityOrder DESC) + CAST(Item_ID AS VARCHAR(32)) + '/')
                FROM [Exams_Content_Items] AS ch
                WHERE
	                [Header_ID] = {headerId} AND [Item_OutlineLevel] = 2

                UPDATE ch
                SET [Item_Path] = hierarchyid::Parse ((SELECT TOP 1 CAST(Item_Path AS nvarchar(4000)) FROM [Exams_Content_Items] WHERE [Header_ID] = {headerId} AND [Item_OutlineLevel] = ch.[Item_OutlineLevel] - 1 AND PriorityOrder < ch.PriorityOrder ORDER BY PriorityOrder DESC) + CAST(Item_ID AS VARCHAR(32)) + '/')
                FROM [Exams_Content_Items] AS ch
                WHERE
	                [Header_ID] = {headerId} AND [Item_OutlineLevel] = 3

                update pr 
                SET IsLeaf = 1
                from [Exams_Content_Items] as pr
                WHERE
	                [Header_ID] = {headerId}
	                AND NOT EXISTS(SELECT * FROM [Exams_Content_Items] WHERE Item_Path.IsDescendantOf(pr.Item_Path) = 1 AND [Header_ID] = {headerId} AND Item_ID <> pr.Item_ID)

                update pr 
                SET IsLeafForReport = 1
                from [Exams_Content_Items] as pr
                WHERE
	                [Header_ID] = {headerId}
	                AND (
		                Item_OutlineLevel = 2
		                OR (Item_OutlineLevel = 1 AND IsLeaf = 1)
	                )";
        }

        public bool ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0 || file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                return false;
            }

            return true;
        }
    }
}