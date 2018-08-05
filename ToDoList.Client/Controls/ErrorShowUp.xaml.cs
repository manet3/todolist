using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace ToDoList.Client.Controls
{
    /// <summary>
    /// Interaction logic for ErrorShowUp.xaml
    /// </summary>
    public partial class ErrorShowUp : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string ErrorText
        {
            get => (string)GetValue(ErrorTextProperty);
            set => SetValue(ErrorTextProperty, value);
        }

        public string LastError
        {
            get => ErrorStack.Any() 
                ? ErrorStack.Peek()
                : string.Empty;
        }

        public bool GotMessage
        {
            get => ErrorStack.Any();
        }

        public static DependencyProperty ErrorTextProperty;

        public Stack<string> ErrorStack = new Stack<string>();

        static ErrorShowUp()
        {
            ErrorTextProperty = DependencyProperty.Register(
                "ErrorText",
                typeof(string),
                typeof(ErrorShowUp),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnMessageGot)));
        }

        private static void OnMessageGot(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newVal = (string)e.NewValue;

            var obj = (ErrorShowUp)d;

            if (string.IsNullOrEmpty(newVal))
            {
                //using empty message parameter to delete the showed one
                if (obj.ErrorStack.Any())
                    obj.ErrorStack.Pop();
            }
            else obj.ErrorStack.Push(newVal);
            obj.PropertyChange(nameof(GotMessage));
            obj.PropertyChange(nameof(LastError));
        }

        private void PropertyChange(string property)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        public ErrorShowUp()
        {
            InitializeComponent();
        }
    }
}
