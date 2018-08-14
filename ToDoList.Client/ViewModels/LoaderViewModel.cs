using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using ToDoList.Client.Controls;
using ToDoList.Client.ViewModels.Common;


namespace ToDoList.Client.ViewModels
{
    public class LoaderViewModel : ViewModelBase
    {
        private const int PARTICLES_NUMBER = 5;

        private const int RESTART_SEC = 10;

        private const int UNDISLAIED_LOADING_SEC = 1;

        public ObservableCollection<Ellipse> Particles { get; set; }

        public ICommand RestartCommand { get; set; }

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
                RestartCountdown();
            }
        }


        private bool _toBeSetStarted;
        private LoadingState _activeState;
        public LoadingState ActiveState
        {
            get => _activeState;
            set
            {
                if (value == LoadingState.Started)
                {
                    SetStartedAfterDelay();
                    return;
                }
                _toBeSetStarted = false;
                SetActiveState(value);
            }
        }

        public async void SetStartedAfterDelay()
        {
            _toBeSetStarted = true;
            await Task.Delay(TimeSpan.FromSeconds(UNDISLAIED_LOADING_SEC));
            if (_toBeSetStarted)
                SetActiveState(LoadingState.Started);
        }

        public void SetActiveState(LoadingState value)
        {
            SetValue(ref _activeState, value, nameof(ActiveState));
            OnActiveStateChanged();
        }

        public LoaderViewModel()
            => Particles = new ObservableCollection<Ellipse>();

        public void OnActiveStateChanged()
        {
            switch (ActiveState)
            {
                case LoadingState.Started:
                    AutoRestartActive = false;
                    AddParticles();
                    break;
                case LoadingState.None:
                case LoadingState.Failed:
                    AutoRestartActive = false;
                    RemoveParticles();
                    break;
                case LoadingState.Paused:
                    AutoRestartActive = true;
                    RemoveParticles();
                    break;
            }
        }

        private async void RestartCountdown()
        {
            for (int i = RESTART_SEC; i >= 0; i--)
            {
                if (!AutoRestartActive)
                    return;
                RetryAfterSec = i;
                await Task.Delay(new TimeSpan(0, 0, 1));
            }
            Restart();
        }

        public void Restart()
        {
            ActiveState = LoadingState.None;

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
