using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            var currentProc = Process.GetCurrentProcess();

            if (Process.GetProcessesByName(currentProc.ProcessName).Length > 1)
                currentProc.Kill();

            InitializeComponent();
        }
    }
}
