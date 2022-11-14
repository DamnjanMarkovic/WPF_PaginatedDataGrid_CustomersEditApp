using RepositoryLayer.Interfaces;
using RepositoryLayer.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Implementation
{
    public class RepositoryHandler : IRepositoryHandler
    {
        private readonly IDbHandler _dbHandler;
        private NLogLogger _nLogger;

        public RepositoryHandler(IDbHandler dbHandler)
        {
            _dbHandler = new DbHandler();
            _nLogger = (NLogLogger)LogManager.GetLog(typeof(NLogLogger));
        }
        public async Task<List<Customer>> GetAllCustomers(string connectionString)
        {
            _nLogger.Info($"Entered func {nameof(GetAllCustomers)}");
            List<Customer> customersList = new List<Customer>();
            SqlCommand command = null;
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                command = new SqlCommand();
                string query = @"SELECT * FROM Customer";
                command.CommandText = query;
                await Task.Run(() =>
                {
                    customersList = _dbHandler.GetAll(command, connectionString, null, reader =>
                    new Customer
                    {
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        CustomerId = (int)reader["CustomerId"],
                        Address1 = reader["Address1"].ToString(),
                        Address2 = reader["Address2"].ToString(),
                        City = reader["City"].ToString(),
                        State = reader["State"].ToString(),
                        Zip = reader["Zip"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Age = reader["Age"] == DBNull.Value ? null : (int?)reader["Age"],
                        Sales = reader["Sales"] == DBNull.Value ? null : (decimal?)reader["Sales"]
                    });
                });
                return customersList;

            }
            catch (Exception ex)
            {
                _nLogger.Warn($"Error while retriving data, func {nameof(GetAllCustomers)}. Error: ");
                _nLogger.Error(ex);
                throw;
            }
        }
        public async Task<List<Customer>> GetAllCustomersFullPagination(string connectionString, int numberOfRowsShowing, int offset)
        {
            _nLogger.Info($"Entered func {nameof(GetAllCustomers)}");
            List<Customer> customersList = new List<Customer>();
            SqlCommand command = null;
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                command = new SqlCommand();
                string query = @"SELECT * FROM Customer order by customerid
                                offset @offset rows fetch next @numberOfRowsShowing rows only";


                command.Parameters.Clear();
                command.Parameters.AddWithValue("@offset", offset);
                command.Parameters.AddWithValue("@numberOfRowsShowing", numberOfRowsShowing);
                foreach (SqlParameter parameter in command.Parameters)
                {
                    if (parameter.Value == null) parameter.Value = DBNull.Value;
                }

                command.CommandText = query;
                await Task.Run(() =>
                {
                    customersList = _dbHandler.GetAll(command, connectionString, null, reader =>
                    new Customer
                    {
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        CustomerId = (int)reader["CustomerId"],
                        Address1 = reader["Address1"].ToString(),
                        Address2 = reader["Address2"].ToString(),
                        City = reader["City"].ToString(),
                        State = reader["State"].ToString(),
                        Zip = reader["Zip"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Age = reader["Age"] == DBNull.Value ? null : (int?)reader["Age"],
                        Sales = reader["Sales"] == DBNull.Value ? null : (decimal?)reader["Sales"]
                    });
                });
                return customersList;

            }
            catch (Exception ex)
            {
                _nLogger.Warn($"Error while retriving data, func {nameof(GetAllCustomers)}. Error: ");
                _nLogger.Error(ex);
                throw;
            }
        }

        public async Task<bool> UpdateCustomer(string connectionString, Customer customer)
        {
            _nLogger.Info($"Entered func {nameof(UpdateCustomer)}Customer ID: {customer.CustomerId}, Customer Name: {customer.FirstName}");
            bool result = false;
            SqlCommand command = null;
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                    return false;
                await Task.Run(() =>
                {
                    command = new SqlCommand();
                    string query = @"UPDATE Customer
                                    SET FirstName = @FirstName, LastName = @LastName, Address1 = @Address1, Address2 = @Address2, 
                                    City= @City, State = @State, Zip = @Zip, Age= @Age,
                                    Phone = @Phone, Sales = @Sales, UpdatedTime= @UpdatedTime
                                    WHERE CustomerId = @CustomerId";


                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                    command.Parameters.AddWithValue("@FirstName", customer.FirstName);
                    command.Parameters.AddWithValue("@LastName", customer.LastName);
                    command.Parameters.AddWithValue("@Address1", customer.Address1);
                    command.Parameters.AddWithValue("@Address2", customer.Address2);
                    command.Parameters.AddWithValue("@City", customer.City);
                    command.Parameters.AddWithValue("@State", customer.State);
                    command.Parameters.AddWithValue("@Zip", customer.Zip);
                    command.Parameters.AddWithValue("@Phone", customer.Phone);
                    command.Parameters.AddWithValue("@Sales", customer.Sales);
                    command.Parameters.AddWithValue("@Age", customer.Age);
                    command.Parameters.AddWithValue("@UpdatedTime", DateTime.Now);

                    foreach (SqlParameter parameter in command.Parameters)
                    {
                        if (parameter.Value == null) parameter.Value = DBNull.Value;
                    }

                    command.CommandText = query;

                    _dbHandler.UpdateCustomer(command, connectionString);
                });
            }
            catch (Exception ex)
            {
                _nLogger.Warn($"Error while updating customer, func {nameof(UpdateCustomer)}. Error: ");
                _nLogger.Error(ex);
                throw;
            }
            return result;
        }

        public async Task<List<string>> GetCountriesCodes(string connectionString)
        {
            _nLogger.Info($"Entered func {nameof(GetCountriesCodes)}");
            List<string> countryCodes = new List<string>();
            SqlCommand command = null;
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                command = new SqlCommand();
                string query = @"SELECT DISTINCT state FROM Customer";
                command.CommandText = query;
                await Task.Run(() =>
                {
                    countryCodes = _dbHandler.GetAll(command, connectionString, null, reader =>
                                        reader["state"].ToString());
                });
                return countryCodes;

            }
            catch (Exception ex)
            {
                _nLogger.Warn($"Error while retriving data, func {nameof(GetAllCustomers)}. Error: ");
                _nLogger.Error(ex);
                throw;
            }
        }

        public async Task<int> GetTotalNumberOfRows(string connectionString)
        {
            _nLogger.Info($"Entered func {nameof(GetTotalNumberOfRows)}");
            int totalNumberOfRows = -1;
            SqlCommand command = null;
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                command = new SqlCommand();
                string query = @"SELECT COUNT(*) AS totalrownumber FROM customer";
                command.CommandText = query;
                await Task.Run(() =>
                {
                    totalNumberOfRows = _dbHandler.GetNumberOfRows(command, connectionString);
                });
                return totalNumberOfRows;

            }
            catch (Exception ex)
            {
                _nLogger.Warn($"Error while retriving data, func {nameof(GetAllCustomers)}. Error: ");
                _nLogger.Error(ex);
                return totalNumberOfRows;
                throw;
            }
        }
    }
}
