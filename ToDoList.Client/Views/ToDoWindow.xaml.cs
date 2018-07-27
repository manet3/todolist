using System.Windows;
using System.Windows.Input;

namespace ToDoList.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ToDoWindow : Window
    {
        public static readonly DependencyProperty ClosingCommandProperty = DependencyProperty.Register(
                "ClosingCommand", typeof(ICommand), typeof(ToDoWindow),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(CommandAssignedCallback)));

        public ICommand ClosingCommand
        {
            get => (ICommand)GetValue(ClosingCommandProperty);
            set => SetValue(ClosingCommandProperty, value);
        }

        private static void CommandAssignedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (ToDoWindow)d;

            obj.Closing += (s, ev) =>
            {
                if (obj.ClosingCommand.CanExecute(obj))
                    obj.ClosingCommand.Execute(obj);
            };
        }

        public ToDoWindow()
            => InitializeComponent();

        private void BtCloseClick(object sender, RoutedEventArgs e)
            => Close();

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => DragMove();

        private void GridFocus(object sender, MouseButtonEventArgs e)
            => ((UIElement)sender).Focus();
    }
}
