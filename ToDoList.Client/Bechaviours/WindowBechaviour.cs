using System.Windows;
using System.Windows.Input;

namespace ToDoList.Client.Bechaviours
{
    static class WindowBechaviour
    {
        public static readonly DependencyProperty ClosingCommandProperty = DependencyProperty.RegisterAttached(
        "ClosingCommand", typeof(ICommand), typeof(WindowBechaviour),
        new FrameworkPropertyMetadata(new PropertyChangedCallback(CommandAssignedCallback)));

        public static ICommand GetClosingCommand(DependencyObject obj)
            => (ICommand)obj.GetValue(ClosingCommandProperty);

        public static void SetClosingCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(ClosingCommandProperty, value);

        private static void CommandAssignedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                window.Closing += (s, ev) =>
                {
                    var command = GetClosingCommand(window);
                    if (command.CanExecute(window))
                        command.Execute(window);
                };
            }
        }

    }
}
