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
            get => _errorStack.Any() 
                ? _errorStack.Peek()
                : string.Empty;
        }

        public bool GotMessage
        {
            get => _errorStack.Any();
        }

        public static DependencyProperty ErrorTextProperty;

        private static Stack<string> _errorStack = new Stack<string>();

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

            if (string.IsNullOrEmpty(newVal))
            {
                //using empty message parameter to delete the showed one
                if (_errorStack.Any())
                    _errorStack.Pop();
            }
            else _errorStack.Push(newVal);

            var obj = (ErrorShowUp)d;
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
