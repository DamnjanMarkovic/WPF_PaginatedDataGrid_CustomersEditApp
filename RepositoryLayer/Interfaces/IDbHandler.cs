using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Interfaces
{
    public interface IDbHandler
    {
        List<T> GetAll<T>(
                    SqlCommand cmd,
                    string connectionString,
                    List<SqlParameter> parameters,
                    Func<IDataReader, T> readerToRow);

        bool UpdateCustomer(
            SqlCommand cmd,
            string connectionString);

        int GetNumberOfRows(
            SqlCommand cmd,
            string connectionString);
    }
}
