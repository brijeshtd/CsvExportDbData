using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using CsvDlDbData.ViewModels;
using System.Text;

namespace CsvDlDbData.Repositories
{
    public class BrandsRepository
    {
        IConfiguration _iconfiguration;

        public BrandsRepository(IConfiguration iconfiguration)
        {
            _iconfiguration = iconfiguration;
        }

        internal IDbConnection Connection
        {
            get
            {
#if DEBUG
                return new NpgsqlConnection(_iconfiguration.GetConnectionString("PSQLDev"));
#else
                return new NpgsqlConnection(_iconfiguration.GetConnectionString("PSQLProd"));
#endif

            }
        }

        public async Task<int> CountAllBrandsAsync()
        {
            StringBuilder query = new StringBuilder();
            query.Append("select count(*) ");
            query.Append("from brands as a ");
            query.Append("where a.record_status = 'active'");

            using (IDbConnection dbConnection = Connection)
            {
                try
                {
                    dbConnection.Open();

                    var result = await dbConnection.QueryAsync<int>(query.ToString());

                    return result.FirstOrDefault();
                }
                catch (Exception)
                {
                    return -1;
                }

            }

        }

        public async Task<List<BrandVM>> AllBrandsAsync(int start, int limit, string order, string dir, string search)
        {
            List<BrandVM> data = new List<BrandVM>();

            StringBuilder query = new StringBuilder();
            if (String.IsNullOrEmpty(search))
            {
                query.Append("select a.id,a.brand_name from brands as a ");
                query.Append("where a.record_status = 'active' ");
                query.Append("order by " + order + " " + dir + " limit @paramLimit offset @paramStart");

            }
            else
            {
                query.Append("select a.id,a.brand_name from brands as a ");
                query.Append("where a.record_status = 'active' and (lower(a.brand_name) like @paramSearch) ");
                query.Append("order by " + order + " " + dir + " limit @paramLimit offset @paramStart");

            }


            using (IDbConnection dbConnection = Connection)
            {
                try
                {
                    dbConnection.Open();

                    var result = await dbConnection.QueryAsync<BrandVM>(query.ToString(),new {
                        paramStart = start,
                        paramLimit = limit,
                        paramSearch = "%" + search + "%"
                    });

                    return result.AsList();
                }
                catch (Exception)
                {
                    return data;
                }
            }

        }

        public async Task<int> AllFilteredBrandsAsync(string search)
        {
            StringBuilder query = new StringBuilder();
            query.Append("select count(*) ");
            query.Append("from brands as a ");
            query.Append("where a.record_status = 'active' and (lower(a.brand_name) like @paramSearch)");

            using (IDbConnection dbConnection = Connection)
            {
                try
                {
                    dbConnection.Open();

                    var result = await dbConnection.QueryAsync<int>(query.ToString(), new
                    {
                        paramSearch = "%" + search + "%"
                    });

                    return result.FirstOrDefault();
                }
                catch (Exception)
                {
                    return -1;
                }

            }

        }

        public async Task<List<BrandVM>> GetAllAsync()
        {
            List<BrandVM> brands = new List<BrandVM>();

            StringBuilder sb = new StringBuilder();
            sb.Append("select id,brand_name,record_status from brands where record_status != 'deleted' order by brand_name");

            using (IDbConnection dbConnection = Connection)
            {
                try
                {
                    dbConnection.Open();

                    var result = await dbConnection.QueryAsync<BrandVM>(sb.ToString());

                    brands = result.AsList();

                    return brands;
                }
                catch (Exception)
                {
                    return brands;
                }

            }

        }
    }
}
