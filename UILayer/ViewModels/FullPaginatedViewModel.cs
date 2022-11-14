using Caliburn.Micro;
using RepositoryLayer;
using RepositoryLayer.Interfaces;
using RepositoryLayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using UILayer.Helpers;
using UILayer.Models;
using UILayer.ViewModels;

namespace UILayer.ViewModels
{
    public class FullPaginatedViewModel : PropertyChangedBase, IFullPaginatedViewModel, IHandle<object>
    {
        #region Props
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private readonly IRepositoryHandler _respositoryHandler;

        //  This helps returning to the page that was visible
        private int _currentPagePosition;

        private int _numberOfRowsTotal = -1;
        private NLogLogger _nLogger;

        //  Determines number of rows visible on the page
        private readonly int _numberOfRowsShowing = 20;

        //  Props used to set geometry to buttons (FirstPage, PageForward, PageBackward, LastPage)
        public Grid ContentPresenterFirstPage { get; set; } = new Grid();
        public Grid ContentPresenterStepBack { get; set; } = new Grid();
        public Grid ContentPresenterStepForward { get; set; } = new Grid();
        public Grid ContentPresenterLastPage { get; set; } = new Grid();

        //Collection provides pagination
        private ObservableCollection<Customer> _customersList;
        public ObservableCollection<Customer> CustomersList
        {
            get => _customersList;
            set
            {
                _customersList = value;
                NotifyOfPropertyChange(() => CustomersList);
            }
        }

        private ObservableCollection<string> States;
        //  Preserve states here
        //  Eliminates need to get them from the db multiple times
        private ObservableCollection<string> CashedStates;

        //  This Customer will be eventually sent to editing
        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                NotifyOfPropertyChange(() => SelectedCustomer);
            }
        }

        //  Shows datagrid only when ready (data retrieved)
        private bool _dataGridVisible = false;
        public bool DataGridVisible
        {
            get => _dataGridVisible;
            set
            {
                _dataGridVisible = value;
                NotifyOfPropertyChange(() => DataGridVisible);
            }
        }

        //  Shows datagrid only when ready (data retrieved)
        private bool _loadingAnimationVisible = false;
        public bool LoadingAnimationVisible
        {
            get => _loadingAnimationVisible;
            set
            {
                _loadingAnimationVisible = value;
                NotifyOfPropertyChange(() => LoadingAnimationVisible);
            }
        }

        #endregion

        #region Constructor
        public FullPaginatedViewModel(IEventAggregator eventAggregator, IRepositoryHandler repositoryHandler, IWindowManager windowManager)
        {
            _eventAggregator = eventAggregator;
            _respositoryHandler = repositoryHandler;
            _windowManager = windowManager;
            _eventAggregator.Subscribe(this);


            //  Logging implemented - Log is in the Bin/Debug
            _nLogger = (NLogLogger)RepositoryLayer.Models.LogManager.GetLog(typeof(NLogLogger));
            _nLogger.Info("Entered Full-Pagination Mode.");

            SetButtonImages();
        }


        #endregion


        #region Funcs

        //  Initiates fetching data from db
        //  Here, we are fetching data from the db every time we change page (but only customers which will be shown). 
        public async Task LoadCustomers()
        {
            LoadingAnimationVisible = true;
            _nLogger.Info($"Entered func {nameof(LoadCustomers)}");
            await GetCustomers(0);
        }


        public async Task GetCustomers(int offset)
        {
            _nLogger.Info($"Entered func {nameof(GetCustomers)}");
            bool fetchCustomers = await Task.Run(async () =>
            {
                try
                {
                    //  Try fetch connection string from Configuration
                    string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;

                    if (ConfigurationManager.ConnectionStrings["DefaultConnection"] == null || string.IsNullOrEmpty(connectionString))
                    {
                        _nLogger.Info($"ConnectionString not retrieved from Configuration.");
                        return false;
                    }

                    //  Check if total number of rows is fetched
                    //  If not, fetch it from db
                    if (_numberOfRowsTotal == -1)
                    {
                        //  Fetch number of rows
                        bool fetchNumberOfRows = await Task.Run(async () =>
                        {
                            try
                            {
                                _numberOfRowsTotal = await _respositoryHandler.GetTotalNumberOfRows(connectionString);

                                _nLogger.Info($"total number of rows: {_numberOfRowsTotal}");
                                return true;
                            }
                            catch (Exception ex)
                            {
                                _nLogger.Warn($"Error while retriving data, func {nameof(GetCustomers)}. Error: ");
                                _nLogger.Error(ex);
                            }
                            return false;
                        });

                        if (!fetchNumberOfRows)
                        {
                            _nLogger.Warn($"Could not fetch number of rows.");
                        }
                    }
                    List<Customer> list = await _respositoryHandler.GetAllCustomersFullPagination(connectionString, _numberOfRowsShowing, offset);
                    CustomersList = new ObservableCollection<Customer>(list);

                    _nLogger.Info($"CustomersList populated. Number of values: {list.Count}");
                    return true;
                }
                catch (Exception ex)
                {
                    _nLogger.Warn($"Error while retriving data, func {nameof(GetCustomers)}. Error: ");
                    _nLogger.Error(ex);
                }
                return false;
            });

            //  Whe list is populated, show grid
            if (fetchCustomers)
            {
                LoadingAnimationVisible = false;
                DataGridVisible = true;
            }
        }

        //Provides pagination - fast moving between First, Last, Next, Previous page
        public async void ChangeButtonClicked(PageHandlingEnum pageHandlingEnum)
        {
            switch (pageHandlingEnum)
            {
                case PageHandlingEnum.FirstPage:
                    _currentPagePosition = 0;
                    break;
                case PageHandlingEnum.PreviousPage:
                    if (_currentPagePosition > 0)
                        _currentPagePosition--;
                    break;
                case PageHandlingEnum.NextPage:
                    if (_currentPagePosition < (_numberOfRowsTotal / _numberOfRowsShowing))
                        _currentPagePosition++;
                    break;
                case PageHandlingEnum.Lastpage:
                    _currentPagePosition = _numberOfRowsTotal / _numberOfRowsShowing;
                    break;
                default:
                    break;
            }

            int offset = _currentPagePosition * _numberOfRowsShowing;
            await GetCustomers(offset);

        }


        //  Will show dialog = EditCustomerView 
        public async Task EditCustomerAsync()
        {
            _nLogger.Info($"Entered func {nameof(EditCustomerAsync)}");
            //  If this is the first time we are editing any of the customers, 
            //  we should get states from db
            if (CashedStates == null)
            {
                var fetchCountries = await Task.Run(async () =>
                {
                    try
                    {
                        //  Try fetch connection string from Configuration
                        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
                        List<string> list = await _respositoryHandler.GetCountriesCodes(connectionString);
                        States = new ObservableCollection<string>(list);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _nLogger.Error(ex);
                    }
                    return false;
                });
                //Save this, so we don't fetch it more then once
                if (fetchCountries)
                {
                    CashedStates = States;
                }
            }
            else
            {
                States = CashedStates;
            }
            var editCustomerView = new EditCustomerViewModel(_eventAggregator, _respositoryHandler, SelectedCustomer, States, _nLogger);
            _windowManager.ShowDialog(editCustomerView, null, null);
        }





        //  Provides geometry for images - appearance wise
        private void SetButtonImages()
        {
            _nLogger.Info($"Entered func {nameof(SetButtonImages)}.");
            //Move to First page
            Geometry data2 = ImagePaths.btnStepBackGeometry;
            Path path2 = new Path();
            path2.Data = data2;
            path2.Fill = Brushes.White;
            path2.Stretch = Stretch.Uniform;
            path2.Margin = new System.Windows.Thickness(2);
            ContentPresenterFirstPage.Children.Add(path2);

            //Move to Previous page
            Geometry data = ImagePaths.btnGoToEnd;
            Path path = new Path();
            path.Data = data;
            path.Fill = Brushes.White;
            path.Stretch = Stretch.Uniform;
            path.Margin = new System.Windows.Thickness(2);
            ContentPresenterStepBack.Children.Add(path);

            //Move to Previous page
            Geometry data1 = ImagePaths.btnGoToStart;
            Path path1 = new Path();
            path1.Data = data1;
            path1.Fill = Brushes.White;
            path1.Stretch = Stretch.Uniform;
            path1.Margin = new System.Windows.Thickness(2);
            ContentPresenterStepForward.Children.Add(path1);

            //Move to Previous page
            Geometry data3 = ImagePaths.btnStepForwardGeometry;
            Path path3 = new Path();
            path3.Data = data3;
            path3.Fill = Brushes.White;
            path3.Stretch = Stretch.Uniform;
            path3.Margin = new System.Windows.Thickness(2);
            ContentPresenterLastPage.Children.Add(path3);

        }

        //  Handle events
        //  If saving update is cancelled, set previusly set values to the customer
        public void Handle(object message)
        {
            _nLogger.Info($"Event - canceling of the customer which is being edited occured.");
            Customer removeChanges = message as Customer;

            var item = CustomersList.ToList().FirstOrDefault(i => i.CustomerId == removeChanges.CustomerId);
            if (item != null)
            {
                item.FirstName = removeChanges.FirstName;
                item.LastName = removeChanges.LastName;
                item.Address1 = removeChanges.Address1;
                item.City = removeChanges.City;
                item.State = removeChanges.State;
                item.Zip = removeChanges.Zip;
                item.Phone = removeChanges.Phone;
                item.Age = removeChanges.Age;
                item.Sales = removeChanges.Sales;
            }


            //Refresh elements in dataGrid
            CustomersList = new ObservableCollection<Customer>(CustomersList.ToList());

        }  

        #endregion
    }
}
