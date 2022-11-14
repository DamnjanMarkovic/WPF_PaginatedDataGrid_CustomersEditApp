using RepositoryLayer.Interfaces;
using RepositoryLayer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Implementation
{
    public class DbHandler : IDbHandler
    {
        private NLogLogger _nLogger;
        public DbHandler()
        {
            _nLogger = (NLogLogger)LogManager.GetLog(typeof(NLogLogger));
        }
        public List<T> GetAll<T>(
               SqlCommand cmd,
               string connectionString,
               List<SqlParameter> parameters,
               Func<IDataReader, T> readerToRow)
        {

            _nLogger.Info($"Entered func {nameof(GetAll)}");
            List<T> returnObjects = new List<T>();

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    cmd.Connection = sqlConnection;

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        returnObjects.Add(readerToRow(reader));
                    }
                }
                catch (Exception ex)
                {
                    _nLogger.Warn($"Error while retriving data, func {nameof(GetAll)}. Error: ");
                    _nLogger.Error(ex);

                    //
                    throw;
                }
                finally
                {
                    if (null != sqlConnection)
                    {
                        if (sqlConnection.State != System.Data.ConnectionState.Closed)
                        {
                            sqlConnection.Close();
                        }
                        sqlConnection.Dispose();
                    }
                }

            }
            return returnObjects;
        }

        public int GetNumberOfRows(SqlCommand cmd, string connectionString)
        {
            _nLogger.Info($"Entered func {nameof(GetNumberOfRows)}");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    cmd.Connection = sqlConnection;

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        return reader.GetInt32(0);
                        //return readerToRow(reader);
                    }
                }
                catch (Exception ex)
                {
                    _nLogger.Warn($"Error while retriving data, func {nameof(GetAll)}. Error: ");
                    _nLogger.Error(ex);

                    //
                    throw;
                }
                finally
                {
                    if (null != sqlConnection)
                    {
                        if (sqlConnection.State != System.Data.ConnectionState.Closed)
                        {
                            sqlConnection.Close();
                        }
                        sqlConnection.Dispose();
                    }
                }

            }
            return -1;
        }

        public bool UpdateCustomer(SqlCommand cmd, string connectionString)
        {
            _nLogger.Info($"Entered func {nameof(UpdateCustomer)}");
            bool result = false;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    cmd.Connection = sqlConnection;
                    cmd.Transaction = cmd.Connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
                    int count = cmd.ExecuteNonQuery();
                    if (count > 0)
                        result = true;
                    cmd.Transaction.Commit();
                }
                catch (Exception ex)
                {
                    _nLogger.Warn($"Error while updating customer, func {nameof(UpdateCustomer)}. Error: ");
                    _nLogger.Error(ex);
                    throw;
                }
                finally
                {
                    if (null != sqlConnection)
                    {
                        if (sqlConnection.State != System.Data.ConnectionState.Closed)
                        {
                            sqlConnection.Close();
                        }
                        sqlConnection.Dispose();
                    }
                }

            }
            return result;
        }
    }
}
