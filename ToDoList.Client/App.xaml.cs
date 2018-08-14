using System.Diagnostics;
using System.Windows;
using ToDoList.Client.DataServices;
using ToDoList.Client.ViewModels;
using Unity;

namespace ToDoList.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //var currentProc = Process.GetCurrentProcess();

            //if (Process.GetProcessesByName(currentProc.ProcessName).Any())
            //    currentProc.Kill();

            InjectionConfig.RegisterDataServices();

            InitializeComponent();
        }
    }
}
