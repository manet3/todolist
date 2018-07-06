using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public event PropertyChangedEventHandler PropertyChanged;


        public static DependencyProperty ActiveStateProperty;

        public static DependencyProperty RestartButtonPressedProperty;



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

        public int ParticlesNumber = 5;

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

        private static void OnActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (LoadingVisualiser)d;
            if ((LoadingState)e.NewValue == LoadingState.Started)
                obj.AddParticles();
            else obj.RemoveParticles();
            obj.OnPropertyChanged(nameof(ActiveState));
        }

        public LoadingVisualiser()
        {
            InitializeComponent();
            Particles = new ObservableCollection<Ellipse>();
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
                if (counter == ParticlesNumber)
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
