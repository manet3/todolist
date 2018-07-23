using System.Windows;
using System.Windows.Input;

namespace ToDoList.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
            => InitializeComponent();

        private void BtCloseClick(object sender, RoutedEventArgs e)
            => Close();

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => DragMove();

        private void GridFocus(object sender, MouseButtonEventArgs e)
            => ((UIElement)sender).Focus();
    }
}
