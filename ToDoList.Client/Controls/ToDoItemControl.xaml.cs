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
                "ItemChangedCommand", typeof(ICommand), typeof(ToDoItemControl));

            ItemModelProperty = DependencyProperty.Register(
                "ItemModel", typeof(ToDoItem), typeof(ToDoItemControl));
        }

        public void OnCheckedChanged(object sender, RoutedEventArgs args)
        {
            if (ItemChangedCommand.CanExecute(this))
                ItemChangedCommand.Execute(ItemModel);
        }

        public ToDoItemControl()
            => InitializeComponent();
    }
}
