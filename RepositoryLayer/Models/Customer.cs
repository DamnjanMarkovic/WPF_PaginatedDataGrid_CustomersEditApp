using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer
{
    //  This class will inherit from IDataErrorInfo
    //  That will provide labeling faulty poipulated fields
    public class Customer : IDataErrorInfo
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public int? Age { get; set; }
        public decimal? Sales { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }

        public Customer(Customer customer)
        {
            CustomerId = customer.CustomerId;
            FirstName = customer.FirstName;
            LastName = customer.LastName;
            Address1 = customer.Address1;
            Address2 = customer.Address2;
            City = customer.City;
            State = customer.State;
            Zip = customer.Zip;
            Phone = customer.Phone;
            Age = customer.Age;
            Sales = customer.Sales;
        }
        public Customer()
        {
        }

        #region Error Handling
        public string Error
        {
            get
            {
                StringBuilder error = new StringBuilder();
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    string propertyError = this[prop.Name];
                    if (propertyError != string.Empty)
                    {
                        _ = error.Append((error.Length != 0 ? ", " : "") + propertyError);
                    }
                }

                return error.Length == 0 ? null : error.ToString();
            }
        }
        public string this[string name]
        {
            get
            {
                string result = null;

                if (name == "FirstName")
                {
                    if (string.IsNullOrEmpty(this.FirstName))
                    {
                        result = "First Name cannot be empty";
                    }
                }
                if (name == "LastName")
                {
                    if (string.IsNullOrEmpty(this.LastName))
                    {
                        result = "Last Name cannot be empty";
                    }
                }
                if (name == "Address1")
                {
                    if (string.IsNullOrEmpty(this.Address1))
                    {
                        result = "Address cannot be empty";
                    }
                }

                if (name == "Address2")
                {
                    if (string.IsNullOrEmpty(this.City))
                    {
                        result = "Address2 cannot be empty";
                    }
                }
                if (name == "City")
                {
                    if (string.IsNullOrEmpty(this.City))
                    {
                        result = "City cannot be empty";
                    }
                }
                if (name == "State")
                {
                    if (string.IsNullOrEmpty(this.State))
                    {
                        result = "State cannot be empty";
                    }
                }
                if (name == "Zip")
                {
                    if (string.IsNullOrEmpty(this.Zip))
                    {
                        result = "Zip cannot be empty";
                    }
                }
                if (name == "Phone")
                {
                    if (string.IsNullOrEmpty(this.Phone))
                    {
                        result = "Phone cannot be empty";
                    }
                }
                if (name == "Age")
                {
                    if (string.IsNullOrEmpty(this.Age.ToString()))
                    {
                        result = "Age must exist and be over 0";
                    }
                }
                if (name == "Sales")
                {
                    if (string.IsNullOrEmpty(this.Sales.ToString()))
                    {
                        result = "Sales must exist and be over 0";
                    }
                }

                return result;
            }
        }

        #endregion
    }
}