using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Interfaces
{
    public interface IRepositoryHandler
    {
        Task<List<Customer>> GetAllCustomers(string connectionString);
        Task<List<Customer>> GetAllCustomersFullPagination(string connectionString, int numberOfRowsShowing, int offset);
        Task<bool> UpdateCustomer(string connectionString, Customer customer);
        Task<List<string>> GetCountriesCodes(string connectionString);
        Task<int> GetTotalNumberOfRows(string connectionString);

    }
}
