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
    public class SemiPaginatedViewModel : PropertyChangedBase, ISemiPaginatedViewModel, IHandle<object>
    {
        #region Props
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private readonly IRepositoryHandler _respositoryHandler;
        private bool _isSortingConducted;
        private string _whenSorting_LastSelectedParam;

        //  This helps returning to the page that was visible
        private int _currentPagePosition;
        private NLogLogger _nLogger;

        //  Determines number of rows visible on the page
        private readonly int _numberOfRowsShowing = 20;

        //  Props used to set geometry to buttons (FirstPage, PageForward, PageBackward, LastPage)
        public Grid ContentPresenterFirstPage { get; set; } = new Grid();
        public Grid ContentPresenterStepBack { get; set; } = new Grid();
        public Grid ContentPresenterStepForward { get; set; } = new Grid();
        public Grid ContentPresenterLastPage { get; set; } = new Grid();

        //Collection provides pagination
        private PagingCollectionView _customersList;
        public PagingCollectionView CustomersList
        {
            get => _customersList;
            set
            {
                _customersList = value;
                NotifyOfPropertyChange(() => CustomersList);
            }
        }

        //  Preserve CustomersList here
        //  Provides list when sorting happens and we should move to the next page
        private PagingCollectionView CashedCustomersList;
        private ObservableCollection<string> States;
        //  Preserve states here
        //  Eliminates need to get them from the list of customers while app is running
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
        public SemiPaginatedViewModel(IEventAggregator eventAggregator, IRepositoryHandler repositoryHandler, IWindowManager windowManager)
        {
            _eventAggregator = eventAggregator;
            _respositoryHandler = repositoryHandler;
            _windowManager = windowManager;
            _eventAggregator.Subscribe(this);


            //  Logging implemented - Log is in the Bin/Debug
            _nLogger = (NLogLogger)RepositoryLayer.Models.LogManager.GetLog(typeof(NLogLogger));
            _nLogger.Info("Entered Semi-Pagination Mode.");

            SetButtonImages();
        }


        #endregion


        #region Funcs


        //  Initiates fetching data from db
        //  I have selected one trip to the db - fetch all customers and manipulate with data. 
        //  This approach is a little bit slowlier at the starting of the app, but eliminates the need for multiple trips to the db (for every page)
        public async Task LoadCustomers()
        {
            LoadingAnimationVisible = true;
            _nLogger.Info($"Entered func {nameof(LoadCustomers)}");
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

                    var list = await _respositoryHandler.GetAllCustomers(connectionString);
                    CustomersList = new PagingCollectionView(list, _numberOfRowsShowing);

                    _nLogger.Info($"CustomersList populated. Number of values: {list.Count}");
                    return true;
                }
                catch (Exception ex)
                {
                    _nLogger.Warn($"Error while retriving data, func {nameof(LoadCustomers)}. Error: ");
                    _nLogger.Error(ex);
                }
                return false;
            });

            //  If list is populated, show grid and cash list
            if (fetchCustomers)
            {
                DataGridVisible = true;
                LoadingAnimationVisible = false;
                CashedCustomersList = CustomersList;
            }
        }


        //  Provides pagination - fast moving between First, Last, Next, Previous page
        public void ChangeButtonClicked(PageHandlingEnum pageHandlingEnum)
        {
            _nLogger.Info($"Selected button {pageHandlingEnum}");
            //  Check if customers have already been fetched 
            //  If not, there is no need for next/previous movement
            if (CustomersList != null && CustomersList.Count > 0)
            {
                //  Check if visual list has been sorted
                //  If yes, we should get customerslist from cash and set currentpage
                if (_isSortingConducted)
                {
                    CustomersList = CashedCustomersList;
                    CustomersList.CurrentPage = _currentPagePosition;
                    _isSortingConducted = false;
                }
                try
                {
                    switch (pageHandlingEnum)
                    {
                        case PageHandlingEnum.FirstPage:
                            CustomersList.MoveToFirstPage();
                            break;
                        case PageHandlingEnum.PreviousPage:
                            CustomersList.MoveToPreviousPage();
                            break;
                        case PageHandlingEnum.NextPage:
                            CustomersList.MoveToNextPage();
                            break;
                        case PageHandlingEnum.Lastpage:
                            CustomersList.MoveToLastPage();
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _nLogger.Error(ex);
                }
            }
        }

        //  On first entry, gets states from CustomersList, later just takes them from cash
        //  Will show dialog = EditCustomerView 
        public async Task EditCustomerAsync()
        {
            _nLogger.Info($"Entered func {nameof(EditCustomerAsync)}");
            //  If this is the first time we are editing customer, 
            //  we should get states from Customers List
            if (CashedStates == null)
            {
                var fetchCountries = await Task.Run(() =>
                {
                    try
                    {
                        //try to get list of states from the list of customers
                        ListCollectionView listView = (ListCollectionView)GetListCollectionView();
                        List<Customer> listCustomers = listView.Cast<Customer>().ToList();

                        List<string> statesList = listCustomers.Select(x => x.State).Distinct().ToList();
                        States = new ObservableCollection<string>(statesList);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _nLogger.Error(ex);
                    }
                    return false;
                });

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

        //  Apparently, pagination doesn't work hand in hand with sorting (by header clicking), without using additional libraries
        //  Hence, I've implemented sorting of the list myself
        //  Sorting depends on the column header selected and it only sorts the visible items
        //  Furthermore - if, after sorting, user goes to the next/previous page, 
        //  items on the new page will be sorted by default (by first column - customer ID)
        //  

        public void CustomerGridSorting(DataGridSortingEventArgs e)
        {
            var sortingParam = e.Column.Header.ToString();
            _nLogger.Info($"Entered func {nameof(CustomerGridSorting)}. Sorting parametar: {sortingParam}. ");

            _currentPagePosition = CustomersList.CurrentPage;
            int positionStart = (CustomersList.CurrentPage - 1) * CustomersList.ItemsPerPage;
            int positionEnd = positionStart + CustomersList.ItemsPerPage;

            ListCollectionView listView = (ListCollectionView)GetListCollectionView();
            List<Customer> listCustomers = listView.Cast<Customer>().ToList().Where((x, i) => i >= positionStart && i < positionEnd).ToList();


            //var field = typeof(Customer).GetField("First Name");


            switch (sortingParam)
            {
                case "Customer Id":

                    //SortListOnInt(ref listCustomers, _whenSorting_LastSelectedParam, "CustomerId", "Customer Id");
                    if (!string.IsNullOrEmpty(_whenSorting_LastSelectedParam) && _whenSorting_LastSelectedParam.Equals("Customer Id") && listCustomers.Count > 1)
                    {
                        if (listCustomers[1].CustomerId > listCustomers[0].CustomerId)
                            listCustomers.Sort((x, y) => y.CustomerId.CompareTo(x.CustomerId));
                        else
                            listCustomers.Sort((y, x) => y.CustomerId.CompareTo(x.CustomerId));
                    }
                    else
                        listCustomers.Sort((x, y) => y.CustomerId.CompareTo(x.CustomerId));
                    _whenSorting_LastSelectedParam = "Customer Id";
                    break;
                case "First Name":
                    if (!string.IsNullOrEmpty(_whenSorting_LastSelectedParam) && _whenSorting_LastSelectedParam.Equals("First Name") && listCustomers.Count > 1)
                    {
                        if (string.Compare(listCustomers[1].FirstName, listCustomers[0].FirstName, StringComparison.CurrentCulture) > 0)
                            listCustomers.Sort((x, y) => y.FirstName.CompareTo(x.FirstName));
                        else
                            listCustomers.Sort((y, x) => y.FirstName.CompareTo(x.FirstName));
                    }
                    else
                        listCustomers.Sort((x, y) => y.FirstName.CompareTo(x.FirstName));
                    _whenSorting_LastSelectedParam = "First Name";
                    break;
                case "Last Name":
                    if (!string.IsNullOrEmpty(_whenSorting_LastSelectedParam) && _whenSorting_LastSelectedParam.Equals("Last Name") && listCustomers.Count > 1)
                    {
                        if (string.Compare(listCustomers[1].LastName, listCustomers[0].LastName, StringComparison.CurrentCulture) > 0)
                            listCustomers.Sort((x, y) => y.LastName.CompareTo(x.LastName));
                        else
                            listCustomers.Sort((y, x) => y.LastName.CompareTo(x.LastName));
                    }
                    else
                        listCustomers.Sort((x, y) => y.LastName.CompareTo(x.LastName));
                    _whenSorting_LastSelectedParam = "Last Name";
                    break;
                case "Age":
                    if (!string.IsNullOrEmpty(_whenSorting_LastSelectedParam) && _whenSorting_LastSelectedParam.Equals("Age") && listCustomers.Count > 1)
                    {
                        if (listCustomers[1].Age > listCustomers[0].Age)
                            listCustomers.Sort((x, y) => y.Age.Value.CompareTo(x.Age.Value));
                        else
                            listCustomers.Sort((y, x) => y.Age.Value.CompareTo(x.Age.Value));
                    }
                    else
                        listCustomers.Sort((x, y) => y.Age.Value.CompareTo(x.Age.Value));
                    _whenSorting_LastSelectedParam = "Age";
                    break;
                case "Address":
                    if (!string.IsNullOrEmpty(_whenSorting_LastSelectedParam) && _whenSorting_LastSelectedParam.Equals("Address") && listCustomers.Count > 1)
                    {
                        if (string.Compare(listCustomers[1].Address1, listCustomers[0].Address1, StringComparison.CurrentCulture) > 0)
                            listCustomers.Sort((x, y) => y.Address1.CompareTo(x.Address1));
                        else
                            listCustomers.Sort((y, x) => y.Address1.CompareTo(x.Address1));
                    }
                    else
                        listCustomers.Sort((x, y) => y.Address1.CompareTo(x.Address1));
                    _whenSorting_LastSelectedParam = "Address";
                    break;
                case "Sales":
                    if (!string.IsNullOrEmpty(_whenSorting_LastSelectedParam) && _whenSorting_LastSelectedParam.Equals("Sales") && listCustomers.Count > 1)
                    {
                        if (listCustomers[1].Sales > listCustomers[0].Sales)
                            listCustomers.Sort((x, y) => y.Sales.Value.CompareTo(x.Sales.Value));
                        else
                            listCustomers.Sort((y, x) => y.Sales.Value.CompareTo(x.Sales.Value));
                    }
                    else
                        listCustomers.Sort((x, y) => y.Sales.Value.CompareTo(x.Sales.Value));
                    _whenSorting_LastSelectedParam = "Sales";
                    break;
                case "State":
                    if (!string.IsNullOrEmpty(_whenSorting_LastSelectedParam) && _whenSorting_LastSelectedParam.Equals("State") && listCustomers.Count > 1)
                    {
                        if (string.Compare(listCustomers[1].State, listCustomers[0].State, StringComparison.CurrentCulture) > 0)
                            listCustomers.Sort((x, y) => y.State.CompareTo(x.State));
                        else
                            listCustomers.Sort((y, x) => y.State.CompareTo(x.State));
                    }
                    else
                        listCustomers.Sort((x, y) => y.State.CompareTo(x.State));
                    _whenSorting_LastSelectedParam = "State";
                    break;
                case "City":
                    if (!string.IsNullOrEmpty(_whenSorting_LastSelectedParam) && _whenSorting_LastSelectedParam.Equals("City") && listCustomers.Count > 1)
                    {
                        if (string.Compare(listCustomers[1].City, listCustomers[0].City, StringComparison.CurrentCulture) > 0)
                            listCustomers.Sort((x, y) => y.City.CompareTo(x.City));
                        else
                            listCustomers.Sort((y, x) => y.City.CompareTo(x.City));
                    }
                    else
                        listCustomers.Sort((x, y) => y.City.CompareTo(x.City));
                    _whenSorting_LastSelectedParam = "City";
                    break;
                case "Zip":
                    if (!string.IsNullOrEmpty(_whenSorting_LastSelectedParam) && _whenSorting_LastSelectedParam.Equals("Zip") && listCustomers.Count > 1)
                    {
                        if (string.Compare(listCustomers[1].Zip, listCustomers[0].Zip, StringComparison.CurrentCulture) > 0)
                            listCustomers.Sort((x, y) => y.Zip.CompareTo(x.Zip));
                        else
                            listCustomers.Sort((y, x) => y.Zip.CompareTo(x.Zip));
                    }
                    else
                        listCustomers.Sort((x, y) => y.Zip.CompareTo(x.Zip));
                    _whenSorting_LastSelectedParam = "Zip";
                    break;
                case "Phone":
                    if (!string.IsNullOrEmpty(_whenSorting_LastSelectedParam) && _whenSorting_LastSelectedParam.Equals("Phone") && listCustomers.Count > 1)
                    {
                        if (string.Compare(listCustomers[1].Phone, listCustomers[0].Phone, StringComparison.CurrentCulture) > 0)
                            listCustomers.Sort((x, y) => y.Phone.CompareTo(x.Phone));
                        else
                            listCustomers.Sort((y, x) => y.Phone.CompareTo(x.Phone));
                    }
                    else
                        listCustomers.Sort((x, y) => y.Phone.CompareTo(x.Phone));
                    _whenSorting_LastSelectedParam = "Phone";
                    break;
                default:
                    break;
            }

            CustomersList = new PagingCollectionView(listCustomers, CustomersList.ItemsPerPage);

            _isSortingConducted = true;


            e.Handled = true;

        }

        //  Provides ListCollectionView needed for manipulating data between different collections (ListCollectionView && PagingCollectionView)
        private ListCollectionView GetListCollectionView()
        {
            return (ListCollectionView)CollectionViewSource.GetDefaultView(CustomersList);
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
            ListCollectionView listView = (ListCollectionView)GetListCollectionView();
            listView.Cast<Customer>().ToList().RemoveAll(o => o.CustomerId == removeChanges.CustomerId);
            var index = listView.Cast<Customer>().ToList().FindIndex(r => r.CustomerId == removeChanges.CustomerId);

            if (index != -1)
            {
                listView.Cast<Customer>().ToList()[index].FirstName = removeChanges.FirstName;
                listView.Cast<Customer>().ToList()[index].LastName = removeChanges.LastName;
                listView.Cast<Customer>().ToList()[index].Address1 = removeChanges.Address1;
                listView.Cast<Customer>().ToList()[index].City = removeChanges.City;
                listView.Cast<Customer>().ToList()[index].State = removeChanges.State;
                listView.Cast<Customer>().ToList()[index].Zip = removeChanges.Zip;
                listView.Cast<Customer>().ToList()[index].Phone = removeChanges.Phone;
                listView.Cast<Customer>().ToList()[index].Age = removeChanges.Age;
                listView.Cast<Customer>().ToList()[index].Sales = removeChanges.Sales;
            }


            //  Setting current page number to cash will provide returning to the page that was visible
            _currentPagePosition = CustomersList.CurrentPage;
            CustomersList = new PagingCollectionView(listView.Cast<Customer>().ToList(), _numberOfRowsShowing);
            CustomersList.CurrentPage = _currentPagePosition;

        }

        #endregion
    }
}
