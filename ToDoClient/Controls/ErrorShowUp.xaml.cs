using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ToDoList.Client.Controls
{
    /// <summary>
    /// Interaction logic for ErrorShowUp.xaml
    /// </summary>
    public partial class ErrorShowUp : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty ErrorTextProperty;

        public event PropertyChangedEventHandler PropertyChanged;

        public string ErrorText
        {
            get => (string)GetValue(ErrorTextProperty);
            set => SetValue(ErrorTextProperty, value);
        }

        private bool _gotMessage;
        public bool GotMessage
        {
            get => _gotMessage; set
            {
                _gotMessage = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(GotMessage)));
            }
        }

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
            ((ErrorShowUp)d).GotMessage = newVal != null && newVal != "";
        }

        public ErrorShowUp()
        {
            InitializeComponent();
        }
    }
}
