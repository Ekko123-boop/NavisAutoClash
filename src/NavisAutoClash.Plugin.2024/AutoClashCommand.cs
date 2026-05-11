using System;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using Autodesk.Navisworks.Api.Plugins;
using NavisAutoClash.Core.Application.Contracts;
using NavisAutoClash.Core.Application.UseCases;
using NavisAutoClash.Infrastructure.Repositories;
using NavisAutoClash.Infrastructure.Threading;
using NavisAutoClash.UI.ViewModels;
using NavisAutoClash.UI.Views;
using NavisApp = Autodesk.Navisworks.Api.Application;

namespace NavisAutoClash.Plugin
{
    [PluginAttribute("NavisAutoClash", "BIMDev", DisplayName = "Auto Clash", ToolTip = "Automated Clash Generation")]
    [AddInPluginAttribute(AddInLocation.AddIn)]
    public class AutoClashCommand : AddInPlugin
    {
        private IServiceProvider? _serviceProvider;

        public override int Execute(params string[] parameters)
        {
            try
            {
                if (_serviceProvider == null)
                {
                    ConfigureServices();
                }

                var viewModel = _serviceProvider!.GetRequiredService<MainViewModel>();
                var window = new MainWindow(viewModel);

                var helper = new WindowInteropHelper(window);
                helper.Owner = NavisApp.Gui.MainWindow.Handle;

                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Startup Error: " + ex.Message);
            }
            return 0;
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Infrastructure
            services.AddSingleton<INavisDispatcher, NavisDispatcher>();
            services.AddSingleton<IClashTestRepository, NavisClashTestRepository>();

            // UseCases
            services.AddTransient<RunClashTestsUseCase>();

            // UI
            services.AddTransient<MainViewModel>();

            _serviceProvider = services.BuildServiceProvider();
        }
    }
}
