using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToDoList.Client.ViewModels;

namespace ToDoList.Client.Controls
{
    public enum LoadingState
    {
        None,
        Started,
        Failed,
        Paused
    }
    /// <summary>
    /// Interaction logic for DownloadVis.xaml
    /// </summary>
    public partial class Loader : UserControl
    {
        public static readonly DependencyProperty ActiveStateProperty;

        public static readonly DependencyProperty RestartButtonPressedProperty;

        public LoadingState ActiveState
        {
            get => (LoadingState)GetValue(ActiveStateProperty);
            set => SetValue(ActiveStateProperty, value);
        }

        public ICommand RestartCommand
        {
            get => (ICommand)GetValue(RestartButtonPressedProperty);
            set => SetValue(RestartButtonPressedProperty, value);
        }

        public LoaderViewModel ViewModel => (LoaderViewModel)MainGrid.DataContext;

        static Loader()
        {
            ActiveStateProperty = DependencyProperty.Register(
                "ActiveState",
                typeof(LoadingState),
                typeof(Loader),
                new FrameworkPropertyMetadata(LoadingState.None,
                new PropertyChangedCallback(OnActiveChanged)));

            RestartButtonPressedProperty = DependencyProperty.Register(
                "RestartCommand",
                typeof(ICommand),
                typeof(Loader),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(RestartCommandChanged)));
        }

        private static void RestartCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((Loader)d).ViewModel.RestartCommand = (ICommand)e.NewValue;

        private static void OnActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((Loader)d).ViewModel.ActiveState = (LoadingState)e.NewValue;

        public Loader()
            => InitializeComponent();
    }
}
