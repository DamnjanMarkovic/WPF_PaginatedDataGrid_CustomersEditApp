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

namespace UILayer
{
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        #region Props
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private readonly IRepositoryHandler _respositoryHandler;
        private NLogLogger _nLogger;


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

        private IPaginatedViewModel _activeViewModel;
        public IPaginatedViewModel ActiveViewModel
        {
            get => _activeViewModel;
            set
            {
                _activeViewModel = value;
                NotifyOfPropertyChange(() => ActiveViewModel);
            }
        }

        #endregion
        public ShellViewModel(IEventAggregator eventAggregator, IRepositoryHandler repositoryHandler, IWindowManager windowManager)
        {
            _eventAggregator = eventAggregator;
            _respositoryHandler = repositoryHandler;
            _windowManager = windowManager;
            _eventAggregator.Subscribe(this);


            //  Logging implemented - Log is in the Bin/Debug
            _nLogger = (NLogLogger)RepositoryLayer.Models.LogManager.GetLog(typeof(NLogLogger));
            _nLogger.Info("App started.");

        }

        public void ShowdataGrid(PaginatedDataGridEnum dataGrid)
        {
            _nLogger.Info($"Selected grid {dataGrid}");

            switch (dataGrid)
            {
                case PaginatedDataGridEnum.FullPaginatedDataGrid:
                    ActiveViewModel = new FullPaginatedViewModel(_eventAggregator, _respositoryHandler, _windowManager);
                    break;
                case PaginatedDataGridEnum.SemiPaginatedDataGrid:
                    ActiveViewModel = new SemiPaginatedViewModel(_eventAggregator, _respositoryHandler, _windowManager);
                    break;

                default:
                    break;
            }

            DataGridVisible = true;
        }
    }
}