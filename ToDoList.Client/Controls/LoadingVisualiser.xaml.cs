using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

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
        private const int PARTICLES_NUMBER = 5;

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

        public ObservableCollection<Ellipse> Particles { get; set; }

        int _retryAfterSec;
        public int RetryAfterSec
        {
            get => _retryAfterSec;
            set
            {
                _retryAfterSec = value;
                OnPropertyChanged(nameof(RetryAfterSec));
            }
        }

        bool _autoRestart;
        public bool AutoRestartActive
        {
            get => _autoRestart;
            set
            {
                _autoRestart = value;
                if (_autoRestart)
                    RestartActivate();
                OnPropertyChanged(nameof(AutoRestartActive));
            }
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
                typeof(LoadingVisualiser));
        }

        public LoadingVisualiser()
        {
            InitializeComponent();
            Particles = new ObservableCollection<Ellipse>();
        }

        private static void OnActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (LoadingVisualiser)d;

            var newState = (LoadingState)e.NewValue;

            if (newState == (LoadingState)e.OldValue)
                return;

            switch (newState)
            {
                case LoadingState.Started:
                    obj.AddParticles();
                    obj.AutoRestartActive = false;
                    break;
                case LoadingState.Failed:
                    obj.RemoveParticles();
                    obj.AutoRestartActive = true;
                    break;
                case LoadingState.None:
                    obj.RemoveParticles();
                    obj.AutoRestartActive = false;
                    break;
            }

            obj.OnPropertyChanged(nameof(ActiveState));
        }

        private async void RestartActivate()
        {
            for (int i = 10; i >= 0; i--)
            {
                RetryAfterSec = i;
                await Task.Delay(new TimeSpan(0, 0, 1));

                //when canceled
                if (!AutoRestartActive) return;
            }
            RestartButtonPressed.Execute(this);
        }

        private void AddParticles()
        {
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };

            var counter = 0;

            timer.Tick += (s, a) =>
            {
                Particles.Add(new Ellipse());
                counter++;
                if (counter == PARTICLES_NUMBER)
                    timer.Stop();
            };

            timer.Start();
        }

        private void RemoveParticles()
        {
            Particles.Clear();
        }

        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
