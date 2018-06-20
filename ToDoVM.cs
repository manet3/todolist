using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToDoList
{
    class ToDoVM: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region generic_fields
        #endregion

        #region ICommands
        public ICommand AddCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand SelectedCommand { get; set; }
        public ICommand CheckedCommand { get; set; }
        #endregion

        public string ToDoItem { get; set; } = "Make the ToDo list";
        public Visibility ButtonRemoveVis
        {
            get => Selected.Count == 0
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public ObservableCollection<CheckBox> ToDo { get; set; } 
            = new ObservableCollection<CheckBox>();
        private List<CheckBox> Selected = new List<CheckBox>();
        private List<CheckBox> Checked = new List<CheckBox>();

        public ToDoVM()
        {
            AddCommand = new Command(ToDoAdd, ()=>ToDoItem != null && !ToDoItem.Equals(""));
            RemoveCommand = new Command(ToDoRemove);
            SelectedCommand = new Command(SelectedChanged);
            CheckedCommand = new Command(CheckedAdd);
        }

        private void CheckedAdd(object obj)
        {
            Checked.Add((CheckBox)obj);
        }

        private void SelectedChanged(object obj)
        {
            //a crutchy way
            var selectedItems = ((ListView)obj).SelectedItems;
            var buffer = new CheckBox[selectedItems.Count];
            selectedItems.CopyTo(buffer, 0);
            Selected = buffer.ToList();
            OnPropertyChanged(nameof(ButtonRemoveVis));
        }

        private void ToDoAdd(object obj)
        {
            var box = new CheckBox
            {
                Content = ToDoItem,
                Command = CheckedCommand
            };
            box.CommandParameter = box;
            ToDo.Add(box);
        }

        private void ToDoRemove(object obj)
        {
            foreach (var item in Selected)
                ToDo.Remove(item);
            Selected.Clear();
        }

        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
