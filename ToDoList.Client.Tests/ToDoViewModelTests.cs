using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Client.ViewModels;
using FluentAssertions;
using ToDoList.Shared;
using System.Linq;
using ToDoList.Client.Test.Mock;
using System.Reflection;

namespace ToDoList.Client.Tests
{
    [TestClass]
    public class ToDoViewModelTests
    {
        private SyncMock _sync;

        private ToDoViewModel _viewModel;

        private string[] _toDoItemsNames = new[] { "new item", "new item 1" };

        [TestInitialize]
        public void ViewModelInit()
        {
            _sync = new SyncMock();
            _viewModel = new ToDoViewModel(_sync);
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

            _viewModel.ToDoLists.Should().HaveCount(0);
            _sync.SyncList.Should().HaveCount(0);
        }

        [TestMethod]
        public void CanSaveAndRestoreSession()
        {
            AddToDoItems();

            _viewModel.ClosingCommand.Execute(this);
            RemoveToDoItems();
            _viewModel.StartCommand.Execute(this);

            _sync.SyncList.Select(x => x.Name).Should().Equal(_toDoItemsNames);
        }

        private void RemoveToDoItems()
        {
            _viewModel.RemoveCommand.Execute(
                _toDoItemsNames.Select(n => new ToDoItem { Name = n })
                .ToList());
        }

        private void AddToDoItems()
        {
            foreach (var itemName in _toDoItemsNames)
            {
                _viewModel.NewItemText = itemName;
                _viewModel.AddCommand.Execute(this);
            }
        }
    }
}
