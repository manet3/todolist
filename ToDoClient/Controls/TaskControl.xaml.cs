using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ToDoList.Client.Controls
{
    /// <summary>
    /// Interaction logic for TaskControl.xaml
    /// </summary>
    public partial class TaskControl : UserControl
    {
        public TaskControl()
        {
            InitializeComponent();
        }

        #region replace by mouse commands binding
        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TaskCheck.IsChecked = !TaskCheck.IsChecked;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = (TaskVM)((UserControl)sender).DataContext;
            vm.IsSelected = !vm.IsSelected;
        }
        #endregion
    }
}
