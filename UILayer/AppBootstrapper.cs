namespace UILayer
{
    using System;
    using System.Collections.Generic;
    using Caliburn.Micro;
    using RepositoryLayer.Implementation;
    using RepositoryLayer.Interfaces;
    using RepositoryLayer.Models;
    using UILayer.Models;
    using UILayer.ViewModels;

    public class AppBootstrapper : BootstrapperBase
    {
        SimpleContainer container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();

            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();

            RegisterViewModels();
            RegisterServices();
            RegisterLogger();
        }

        private void RegisterLogger()
        {
            RepositoryLayer.Models.LogManager.GetLog = type => new NLogLogger(type);
        }

        public void RegisterViewModels()
        {
            container.PerRequest<IShell, ShellViewModel>();
            container.PerRequest<IFullPaginatedViewModel, FullPaginatedViewModel>();
            container.PerRequest<ISemiPaginatedViewModel, SemiPaginatedViewModel>();
        }
        public void RegisterServices()
        {
            container.PerRequest<IRepositoryHandler, RepositoryHandler>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
        }
    }
}