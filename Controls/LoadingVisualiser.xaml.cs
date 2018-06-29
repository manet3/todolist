using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class LoadingVisualiser : UserControl
    {
        public ObservableCollection<Ellipse> Particles { get; set; }
        public int ParticlesNumber = 3;

        public static DependencyProperty IsActiveProperty;

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }


        static LoadingVisualiser()
        {
            IsActiveProperty = DependencyProperty.Register(
                "IsActive", 
                typeof(bool), 
                typeof(LoadingVisualiser),
                new FrameworkPropertyMetadata(false));
        }

        public LoadingVisualiser()
        {
            InitializeComponent();
            Particles = new ObservableCollection<Ellipse>();
            AddParticles();
        }

        private void AddParticles()
        {
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            var counter = 0;

            timer.Tick += (s, a) =>
            {
                var ellipse = new Ellipse
                {
                    Height = 5,
                    Width = 5,
                    Fill = new SolidColorBrush(Colors.DimGray)
                };
                Canvas.SetTop(ellipse, 2.5);
                Particles.Add(ellipse);
                counter++;
                if (counter == ParticlesNumber)
                    timer.Stop();
            };

            timer.Start();
        }


    }
}
