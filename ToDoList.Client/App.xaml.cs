using System.Diagnostics;
using System.Windows;

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

            InitializeComponent();
        }
    }
}
