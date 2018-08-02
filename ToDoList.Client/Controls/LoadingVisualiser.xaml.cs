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
        Failed
    }
    /// <summary>
    /// Interaction logic for DownloadVis.xaml
    /// </summary>
    public partial class LoadingVisualiser : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty ActiveStateProperty;

        public static readonly DependencyProperty RestartButtonPressedProperty;

        public LoadingState ActiveState
        {
            get => (LoadingState)GetValue(ActiveStateProperty);
            set => SetValue(ActiveStateProperty, value);
        }

        public ICommand RestartButtonPressed
        {
            get => (ICommand)GetValue(RestartButtonPressedProperty);
            set => SetValue(RestartButtonPressedProperty, value);
        }

        static LoadingVisualiser()
        {
            ActiveStateProperty = DependencyProperty.Register(
                "ActiveState",
                typeof(LoadingState),
                typeof(LoadingVisualiser),
                new FrameworkPropertyMetadata(LoadingState.None,
                new PropertyChangedCallback(OnActiveChanged)));

            RestartButtonPressedProperty = DependencyProperty.Register(
                "RestartButtonPressed",
                typeof(ICommand),
                typeof(LoadingVisualiser),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(RestartCommandChanged)));
        }

        private static void RestartCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (LoaderViewModel)((LoadingVisualiser)d).MainGrid.DataContext;
            viewModel.RestartCommand = (ICommand)e.NewValue;
        }

        //in case of setting Datacontext of the controll 
        //this method will not be called ))
        private static void OnActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newState = (LoadingState)e.NewValue;

            ((LoaderViewModel)((LoadingVisualiser)d).MainGrid.DataContext)
                .ChangeLoadingState(newState);

            var obj = (LoadingVisualiser)d;
            obj.PropertyChanged?.Invoke(obj,
                new PropertyChangedEventArgs(nameof(obj.ActiveState)));
        }

        public LoadingVisualiser()
            => InitializeComponent();
    }
}
