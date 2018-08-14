using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ToDoList.Client.Views.Attached
{
    static class WindowAttached
    {
        public static readonly DependencyProperty LoadedCommandProperty = DependencyProperty.RegisterAttached(
            "LoadedCommand", typeof(ICommand), typeof(WindowAttached),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(CommandAssignedCallback)));

        public static readonly DependencyProperty ClosingCommandProperty = DependencyProperty.RegisterAttached(
            "ClosingCommand", typeof(ICommand), typeof(WindowAttached),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(CommandAssignedCallback)));

        public static ICommand GetLoadedCommand(DependencyObject obj)
            => (ICommand)obj.GetValue(LoadedCommandProperty);

        public static void SetLoadedCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(LoadedCommandProperty, value);

        public static ICommand GetClosingCommand(DependencyObject obj)
            => (ICommand)obj.GetValue(LoadedCommandProperty);

        public static void SetClosingCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(LoadedCommandProperty, value);

        private static void CommandAssignedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Window window))
                return;

            Action<object, EventArgs> callCommand = (s, ev) =>
            {
                var command = (ICommand)e.NewValue;
                if (command.CanExecute(window))
                    command.Execute(window);
            };

            if (e.Property.Name == ClosingCommandProperty.Name)
                window.Closing += new CancelEventHandler(callCommand);
            else
                window.Loaded += new RoutedEventHandler(callCommand);
        }
    }
}
