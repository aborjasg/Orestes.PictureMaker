using Npgsql;

namespace AspireApp.ApiService.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class DataAccessBase<T>
    {
        protected string ConnectionString;

        public DataAccessBase(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <returns></returns>
        public string GetConnectionString()
        {
            return ConnectionString;
        }

    }
}
