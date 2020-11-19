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
            return @$"DECLARE
	                    @Country_ID INT = {countryId},  -- тут указать id страны
	                    @Session_ID INT = {sessionId}  -- указать id сессии

	                    -- цифры ниже нужно увеличивать каждый раз когда генерировать коды на 1 т.е. 5760000 = 5770000
	                    -- эти использовались в последний раз (576-577)
                    DECLARE @First_Letter CHAR(1) = 'A'
                    DECLARE @Start_Number INT = 5760000
                    DECLARE @Stop_Number INT = 5770000

                    UPDATE rfi
                    SET
	                    Work_Code = @First_Letter
                    FROM RegForms_Items AS rfi
                    INNER JOIN RegForms_Headers AS rfh
	                    ON rfh.Header_ID = rfi.Header_ID
                    INNER JOIN Exams_Schedule AS sch
	                    ON rfi.Schedule_ID = sch.Schedule_ID
                    INNER JOIN Cities AS c
	                    ON rfh.City_ID = c.City_ID
                    WHERE 
	                    c.Country_ID = ISNULL(@Country_ID, c.Country_ID)
	                    AND sch.Session_ID = @Session_ID
	                    --AND rfi.IsAttended = 1
	                    --AND Work_Code IS NOT NULL
	                    AND Exam_ID in (SELECT Exam_ID FROM Exams)	

                    DECLARE @res INT = 1

                    WHILE @res <> 0
                    BEGIN

	                    UPDATE rfi
	                    SET
		                    Work_Code = @First_Letter + RIGHT(CAST(100000000 + @Start_Number + ABS(CAST(NEWID() AS BINARY(6)) % (@Stop_Number - @Start_Number)) + 1 AS VARCHAR(100)), 7)
	                    FROM RegForms_Items AS rfi
	                    INNER JOIN RegForms_Headers AS rfh
		                    ON rfh.Header_ID = rfi.Header_ID
	                    INNER JOIN Exams_Schedule AS sch
		                    ON rfi.Schedule_ID = sch.Schedule_ID
	                    INNER JOIN Cities AS c
		                    ON rfh.City_ID = c.City_ID
	                    WHERE 
		                    c.Country_ID = ISNULL(@Country_ID, c.Country_ID)
		                    AND sch.Session_ID = @Session_ID
		                    --AND rfi.IsAttended = 1
		                    --AND Work_Code IS NULL
		                    AND Exam_ID in (SELECT Exam_ID FROM Exams)		
	                    
	                    IF NOT EXISTS (
		                    SELECT Work_Code
		                    FROM RegForms_Items AS rfi
		                    INNER JOIN RegForms_Headers AS rfh
			                    ON rfh.Header_ID = rfi.Header_ID
		                    INNER JOIN Exams_Schedule AS sch
			                    ON rfi.Schedule_ID = sch.Schedule_ID
		                    INNER JOIN Cities AS c
			                    ON rfh.City_ID = c.City_ID
		                    WHERE 
			                    c.Country_ID = ISNULL(@Country_ID, c.Country_ID)
			                    AND sch.Session_ID = @Session_ID
		                    GROUP BY 
			                    Work_Code
		                    HAVING 
			                    COUNT(*) > 1
	                    )
		                    SELECT @res = 0
                    END

                    SELECT
	                    p.Full_Name, rfi.Work_Code, e.Exam_Name, es.Session_Name, c.City_Name
                    FROM RegForms_Items AS rfi
                    INNER JOIN RegForms_Headers AS rfh
	                    ON rfh.Header_ID = rfi.Header_ID
                    INNER JOIN Exams_Schedule AS sch
	                    ON rfi.Schedule_ID = sch.Schedule_ID
                    INNER JOIN Cities AS c
	                    ON rfh.City_ID = c.City_ID
                    JOIN People p
	                    ON p.Person_ID=rfh.Person_ID
                    JOIN Exams_Sessions es
	                    ON es.Session_ID=sch.Session_ID
                    JOIN Exams e
	                    ON e.Exam_ID=sch.Exam_ID
                    WHERE 
	                    c.Country_ID = ISNULL(@Country_ID, c.Country_ID)
	                    AND sch.Session_ID = @Session_ID
	                    --AND rfi.IsAttended = 1
	                    --AND Work_Code IS NOT NULL
	                    AND e.Exam_ID in (SELECT Exam_ID FROM Exams)";
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
