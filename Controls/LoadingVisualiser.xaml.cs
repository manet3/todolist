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
    /// <summary>
    /// Interaction logic for DownloadVis.xaml
    /// </summary>
    public partial class LoadingVisualiser : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        public static DependencyProperty IsActiveProperty;


        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public ObservableCollection<Ellipse> Particles { get; set; }

        public Visibility LoaderVis
        {
            get => IsActive
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public int ParticlesNumber = 5;

        static LoadingVisualiser()
        {
            IsActiveProperty = DependencyProperty.Register(
                "IsActive", 
                typeof(bool), 
                typeof(LoadingVisualiser),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnActiveChanged)));
        }

        private static void OnActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (LoadingVisualiser)d;
            if ((bool)e.NewValue)
                obj.AddParticles();
            else obj.RemoveParticles();
            obj.OnPropertyChanged(nameof(LoaderVis));
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
