using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Cipa.BusinessModels;
using Cipa.Helpers;
using Cipa.Interfaces;

namespace Cipa.Repositories
{
    public class CountryRepository: Base, ICountryRepository
    {
        public IEnumerable<CountryViewModel> GetCountries()
        {
            var responseData = new List<CountryViewModel>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var query = "select * from Countries";
                using (var da = new SqlDataAdapter(query, connection))
                {
                    var tblPromotion = new DataTable();
                    try
                    {
                        connection.Open();
                        da.Fill(tblPromotion);
                        if (tblPromotion.Rows.Count != 0)
                        {
                            for(var i = 0; i < tblPromotion.Rows.Count; i++)
                            {
                                var data = tblPromotion.Rows[i];
                                
                                responseData.Add(new CountryViewModel
                                {
                                    CountryId = data.Field<int>("Country_ID"),
                                    CountryName = data.Field<string>("Country_Name")
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

        public IEnumerable<CityViewModel> GetCities()
        {
            var responseData = new List<CityViewModel>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var query = "select * from Cities where IsEnable = 1; ";
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

                                responseData.Add(new CityViewModel
                                {
                                    CityId = data.Field<int>("City_ID"),
                                    CityName = data.Field<string>("City_Name")
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