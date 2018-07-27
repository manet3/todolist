using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToDoList.Client.Controls
{
    /// <summary>
    /// Interaction logic for TaskControl.xaml
    /// </summary>
    public partial class ToDoItemControl : UserControl
    {
        public static readonly DependencyProperty ItemChangedCommandProperty;

        public ICommand ItemChangedCommand
        {
            get => (ICommand)GetValue(ItemChangedCommandProperty);
            set => SetValue(ItemChangedCommandProperty, value);
        }

        static ToDoItemControl()
        {
            ItemChangedCommandProperty = DependencyProperty.Register(
                "ItemChangedCommand", typeof(ICommand), typeof(ToDoItemControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(CommandAssignedCallback)));
        }

        public ToDoItemControl()
            => InitializeComponent();

        private static void CommandAssignedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (ToDoItemControl)d;

            obj.TaskCheckBox.Checked += (s, ev) =>
            {
                if (obj.ItemChangedCommand.CanExecute(obj))
                    obj.ItemChangedCommand.Execute(obj.DataContext);
            };
        }
    }
}
