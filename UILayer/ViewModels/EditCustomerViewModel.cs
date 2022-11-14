using Caliburn.Micro;
using RepositoryLayer;
using RepositoryLayer.Interfaces;
using RepositoryLayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UILayer.Helpers;
using UILayer.Models;

namespace UILayer.ViewModels
{
    public class EditCustomerViewModel : PropertyChangedBase, IEditCustomerViewModel
    {
        public delegate void EventCustomerUpdatedDelegate(Customer customerUpdated);
        public EventCustomerUpdatedDelegate EventCustomerUpdated;

        private readonly IEventAggregator _events;
        private readonly IRepositoryHandler _respositoryHandler;

        //  Cashed customer editing - wil be used if editing is canceled
        private Customer _currentCustomerCashe;
        private NLogLogger _nLogger;


        //  Customer selected on the previous page - to be edited
        private Customer _currentCustomer;
        public Customer CurrentCustomer
        {
            get => _currentCustomer;
            set
            {
                _currentCustomer = value;
                NotifyOfPropertyChange(() => CurrentCustomer);
            }
        }

        //  Observable collectino used to populate combo box with states
        private ObservableCollection<string> _states;
        public ObservableCollection<string> States
        {
            get => _states;
            set
            {
                _states = value;
                NotifyOfPropertyChange(() => States);
            }
        }

        //  This prop, in accordance with CloseWindowBehavior class,
        //  will provide closing of the Popup menu from the view model.
        private bool closeTrigger;
        public bool CloseTrigger
        {
            get => closeTrigger;
            set
            {
                closeTrigger = value;
                NotifyOfPropertyChange(() => CloseTrigger);
            }
        }

        public EditCustomerViewModel(IEventAggregator events, IRepositoryHandler respositoryHandler,
                                    Customer selectedCustomer, ObservableCollection<string> states, NLogLogger nLogLogger)
        {
            _events = events;
            _respositoryHandler = respositoryHandler;
            _states = states;
            _currentCustomer = selectedCustomer;
            _states = states;
            _currentCustomerCashe = new Customer(_currentCustomer);
            _nLogger = nLogLogger;

            _nLogger.Info($"Editing Customer initiated. Customer ID: {CurrentCustomer.CustomerId}, Customer Name: {CurrentCustomer.FirstName}");
        }

        //  Func that saves updated customer to the db
        //  If all values are entered (apart from Adress2) there will be no error, and updating will be possible
        public async Task UpdateCustomer()
        {
            _nLogger.Info($"Entered func {nameof(UpdateCustomer)}. Saving customer with Customer ID: {CurrentCustomer.CustomerId}, Customer Name: {CurrentCustomer.FirstName}.");

            if (string.IsNullOrEmpty(CurrentCustomer.Error))
            {
                var saveCustomerToDb = await Task.Run(() =>
                {
                    try
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;

                        if (ConfigurationManager.ConnectionStrings["DefaultConnection"] == null || string.IsNullOrEmpty(connectionString))
                        {
                            _nLogger.Info($"ConnectionString not retrieved from Configuration.");
                            return false;
                        }
                        var updated = _respositoryHandler.UpdateCustomer(connectionString, CurrentCustomer).Result;
                        if (updated)
                        {
                            _nLogger.Info($"Customer updated.");
                            return true;
                        }
                        else
                        {
                            _nLogger.Warn($"Customer NOT updated.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _nLogger.Warn($"Error while retriving data, func {nameof(UpdateCustomer)}. Error: ");
                        _nLogger.Error(ex);
                    }
                    return false;
                });

            }

            //  If some of the values are empty, message will be shown to user
            else
            {
                var cust = CurrentCustomer;
                MessageBox.Show("All fields must have values. \nCustomer NOT SAVED!");
            }

            CloseTrigger = true;
        }

        //  If updating is cancelled - values for that customer will be set to the one before editing (from cash)
        public void CancelSaving()
        {
            _nLogger.Info($"Entered func {nameof(CancelSaving)}.");
            _events.PublishOnBackgroundThread(new Customer(_currentCustomerCashe));
            CloseTrigger = true;
        }

    }
}
