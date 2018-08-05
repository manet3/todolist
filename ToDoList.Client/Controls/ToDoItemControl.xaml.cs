using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToDoList.Client.ViewModels;
using ToDoList.Shared;

namespace ToDoList.Client.Controls
{
    /// <summary>
    /// Interaction logic for TaskControl.xaml
    /// </summary>
    public partial class ToDoItemControl : UserControl
    {
        public static readonly DependencyProperty ItemChangedCommandProperty;

        public static readonly DependencyProperty ItemModelProperty;

        public ICommand ItemChangedCommand
        {
            get => (ICommand)GetValue(ItemChangedCommandProperty);
            set => SetValue(ItemChangedCommandProperty, value);
        }

        public ToDoItem ItemModel
        {
            get => (ToDoItem)GetValue(ItemModelProperty);
            set => SetValue(ItemModelProperty, value);
        }

        static ToDoItemControl()
        {
            ItemChangedCommandProperty = DependencyProperty.Register(
                "ItemChangedCommand", typeof(ICommand), typeof(ToDoItemControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(CommandAssignedCallback)));

            ItemModelProperty = DependencyProperty.Register(
                "ItemModel", typeof(ToDoItem), typeof(ToDoItemControl),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(ModelAssignedCallback)));

        }

        private static void ModelAssignedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            var viewModel = new ItemViewModel((ToDoItem)e.NewValue);

            viewModel.UpdateTimestamp();

            ((ToDoItemControl)d).DataContext = viewModel;
        }

        private static void CommandAssignedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (ToDoItemControl)d;

            Action<object, RoutedEventArgs> onChanged
                = (s, ev) => ((ItemViewModel)obj.DataContext)
                .GetOnChangedAction((ICommand)e.NewValue)
                .Invoke();

            obj.TaskCheckBox.Checked += new RoutedEventHandler(onChanged);
            obj.TaskCheckBox.Unchecked += new RoutedEventHandler(onChanged);
        }

        public ToDoItemControl()
            => InitializeComponent();
    }
}
