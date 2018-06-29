﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToDoList.Client
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
        #endregion

        public string ToDoItem { get; set; } = "Make the ToDo list";

        public bool IsDownloading { get; set; }

        public Visibility ButtonRemoveVis
        {
            get => Selected.Count == 0
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public ObservableCollection<TaskVM> ToDo { get; private set; } 
            = new ObservableCollection<TaskVM>();

        private List<TaskVM> Selected = new List<TaskVM>();

        private ToDoModel model;

        public ToDoVM()
        {
            AddCommand = new Command(ToDoAdd, ()=>ToDoItem != null && !ToDoItem.Equals(""));
            RemoveCommand = new Command(ToDoRemove);
            SelectedCommand = new Command(SelectedChanged);
            model = new ToDoModel();
            TaskVM.CheckedChanged += CheckedAdd;
            Synchronisator.SynchChanged += SyncHandler;
            model.GotItems += ToDoItemsGet;
            model.ReadData();
        }

        private void SyncHandler(SynchState state)
        {
            IsDownloading = state == SynchState.Started;
        }

        private void ToDoItemsGet()
        {
            if (model.Tasks == null)
                return;

            var items = model.Tasks.Select(x=> new TaskVM(x));   
            
            ToDo = new ObservableCollection<TaskVM>(items);
            OnPropertyChanged(nameof(ToDo));
        }

        private void CheckedAdd(object sender, bool isChecked)
        {
            model.AddItem(((TaskVM)sender).Model);
        }

        private void SelectedChanged(object obj)
        {
            //a crutchy way
            var selectedItems = ((ListView)obj).SelectedItems;
            var buffer = new TaskVM[selectedItems.Count];
            selectedItems.CopyTo(buffer, 0);
            Selected = buffer.ToList();
            OnPropertyChanged(nameof(ButtonRemoveVis));
        }

        private void ToDoAdd(object obj)
        {
            var itemIndex = ToDo.Count != 0
                ? ToDo.Max(x => x.Model.Index) + 1
                : 0;
            var newItem = new TaskVM(ToDoItem, false, itemIndex);
            model.AddItem(newItem.Model);
            ToDo.Add(newItem);
        }

        private void ToDoRemove(object obj)
        {
            foreach (var item in Selected)
            {
                ToDo.Remove(item);
                model.DeleteItem(item.Model);
            }
            Selected.Clear();
        }

        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
