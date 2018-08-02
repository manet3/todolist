using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using ToDoList.Client.Controls;

namespace ToDoList.Client.ViewModels
{
    class LoaderViewModel : ViewModelBase
    {
        private const int PARTICLES_NUMBER = 5;

        public ObservableCollection<Ellipse> Particles { get; set; }

        public ICommand RestartCommand;

        int _retryAfterSec;
        public int RetryAfterSec
        {
            get => _retryAfterSec;
            set => SetValue(ref _retryAfterSec, value);
        }

        bool _autoRestart;
        public bool AutoRestartActive
        {
            get => _autoRestart;
            set
            {
                SetValue(ref _autoRestart, value);
                if (_autoRestart)
                    RestartActivate();
            }
        }

        public LoaderViewModel()
            => Particles = new ObservableCollection<Ellipse>();

        public void ChangeLoadingState(LoadingState newLoaderState)
        {
            switch (newLoaderState)
            {
                case LoadingState.Started:
                    AddParticles();
                    AutoRestartActive = false;
                    break;
                case LoadingState.Failed:
                    RemoveParticles();
                    AutoRestartActive = true;
                    break;
                case LoadingState.None:
                    RemoveParticles();
                    AutoRestartActive = false;
                    break;
            }
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

            if (RestartCommand != null && RestartCommand.CanExecute(this))
                RestartCommand.Execute(this);
        }

        public void AddParticles()
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

        public void RemoveParticles()
            => Particles.Clear();
    }
}
