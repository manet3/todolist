using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Client.ViewModels;
using ToDoList.Client.Tests.Mock;
using System.Collections.Generic;
using FluentAssertions;
using ToDoList.Shared;
using System.Linq;
using System;
using ToDoList.Client.Test.Mock;
using System.Reflection;

namespace ToDoList.Client.Tests
{
    [TestClass]
    public class ToDoViewModelTests
    {
        private SyncMock _sync;

        private ToDoViewModel _tested;

        private string[] _toDoItemsNames = new[] { "new item", "new item 1" };

        [TestInitialize]
        public void ViewModelInit()
        {
            _sync = new SyncMock();
            _tested = new ToDoViewModel(_sync);
        }

        [TestMethod]
        public void CanAddItems()
        {
            AddToDoItems();

            _sync.SyncList.Select(x => x.Name).Should().Equal(_toDoItemsNames);
        }

        [TestMethod]
        public void CanRemoveItems()
        {
            AddToDoItems();

            RemoveToDoItems();

            _sync.SyncList.Should().HaveCount(0);
        }

        [TestMethod]
        public void CanSaveAndRestoreSession()
        {
            AddToDoItems();

            _tested.ClosingCommand.Execute(this);
            RemoveToDoItems();
            typeof(ToDoViewModel).GetMethod("GetSavedSession", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_tested, null);

            _sync.SyncList.Select(x => x.Name).Should().Equal(_toDoItemsNames);
        }

        private void RemoveToDoItems()
        {
            _tested.RemoveCommand.Execute(
                _toDoItemsNames.Select(n => new ToDoItem { Name = n })
                .ToList());
        }

        private void AddToDoItems()
        {
            var toDoItemsNames = _toDoItemsNames.ToList();
            while (toDoItemsNames.Any())
            {
                _tested.NewItemText = toDoItemsNames[0];
                toDoItemsNames.RemoveAt(0);
                _tested.AddCommand.Execute(this);
            }
        }
    }
}
