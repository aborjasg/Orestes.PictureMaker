using Npgsql;
using System.Data;
using Dapper;
using AspireApp.Libraries.Models;

namespace AspireApp.ApiService.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class TableAccess<T> : DataAccessBase<T>
    {
        protected string tableName;

        /// <summary>
        /// 
        /// </summary>
        public TableAccess(string connectionString) : base(connectionString) 
        {
            tableName = $"{typeof(T).Name!}s";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetRow(int id)
        {
            var dbConnection = new NpgsqlConnection(base.GetConnectionString());
            var response = dbConnection.Query<T>($"select * from \"{tableName}\" where \"Id\"={id}");            
            return response.FirstOrDefault()!;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        public int SaveRow(T record)
        {
             var dataSource = NpgsqlDataSource.Create(base.GetConnectionString());
            int result = 0;
            switch (tableName)
            {
                case "RunImages":
                    {
                        var row = record as RunImage;
                        var sql = @"INSERT INTO public.""RunImages"" (""Name"", ""Metadata"", ""DataSource"", ""Content"", ""CreatedDate"")" +
                                        "VALUES(@Name, @Metadata, @DataSource, @Content, @CreatedDate)";
                        var cmd = dataSource.CreateCommand(sql);
                        cmd.Parameters.AddWithValue("@Name", row!.Name!);
                        cmd.Parameters.AddWithValue("@Metadata", row!.Metadata!);
                        cmd.Parameters.AddWithValue("@DataSource", row!.DataSource!);
                        cmd.Parameters.AddWithValue("@Content", row!.Content!);
                        cmd.Parameters.AddWithValue("@CreatedDate", row!.CreatedDate!);
                        result = cmd.ExecuteNonQuery();
                        break;
                    }
            }
            return result;
        }
    }
}
