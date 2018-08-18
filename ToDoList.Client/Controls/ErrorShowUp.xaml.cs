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

        public bool GotMessage
        {
            get => !string.IsNullOrEmpty(ErrorText);
        }

        public static DependencyProperty ErrorTextProperty;

        static ErrorShowUp()
        {
            ErrorTextProperty = DependencyProperty.Register(
                "ErrorText",
                typeof(string),
                typeof(ErrorShowUp),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnMessageGot)));
        }

        private static void OnMessageGot(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((ErrorShowUp)d).PropertyChange(nameof(GotMessage));

        private void PropertyChange(string property)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        public ErrorShowUp()
            => InitializeComponent();
    }
}
