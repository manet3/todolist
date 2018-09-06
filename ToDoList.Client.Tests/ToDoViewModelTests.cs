using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Client.ViewModels;
using FluentAssertions;
using ToDoList.Shared;
using System.Linq;
using ToDoList.Client.Test.Mock;
using System.Reflection;
using System.Collections.Generic;

namespace ToDoList.Client.Tests
{
    [TestClass]
    public class ToDoViewModelTests
    {
        private SyncMock _sync;

        private ToDoViewModel _viewModel;

        private string[] _testItemsNames = new[] { "item", "item 1" };

        [TestInitialize]
        public void ViewModelInit()
        {
            _sync = new SyncMock
            {
                SyncList = _testItemsNames.Select(x => new ToDoItem { Name = x }).ToList()
            };
            _viewModel = new ToDoViewModel(_sync)
            {
                ToDoLists = new ObservableHashSet<InteractiveToDoList>{
                    new InteractiveToDoList (new ToDoItemsList{
                        Items = new ObservableHashSet<ToDoItem>(new[]{
                            new ToDoItem { Name = _testItemsNames[0]},
                            new ToDoItem { Name = _testItemsNames[1]} }),
                        Name = "ToDolist 1" }) }
            };
            _viewModel.ActiveToDoList = _viewModel.ToDoLists[0];
        }

        [TestMethod]
        public void CanAddItems()
        {
            AddToDoItems();

            _sync.SyncList.Select(x => x.Name).Should().Equal(_testItemsNames);
        }

        [TestMethod]
        public void FailsAddDuplicateItems()
        {
            AddToDoItems();

            AddToDoItems();

            _sync.SyncList.Select(x => x.Name).Should().Equal(_testItemsNames);
        }

        private void AddToDoItems()
        {
            foreach (var itemName in _testItemsNames)
            {
                _viewModel.NewItemText = itemName;
                _viewModel.AddCommand.Execute(this);
            }
        }

        [TestMethod]
        public void CanRemoveItems()
        {
            RemoveToDoItems();

            _viewModel.ToDoLists[0].Should().HaveCount(0);
        }

        [TestMethod]
        public void CanSaveAndRestoreSession()
        {
            _viewModel.ClosingCommand.Execute(this);
            RemoveToDoItems();
            _viewModel.StartCommand.Execute(this);

            _viewModel.ToDoLists.Should().NotBeNull();
            _sync.SyncList.Should().NotBeNull();
            _viewModel.ToDoLists[0].Select(x => x.Name).Should().Equal(_testItemsNames);
            _sync.SyncList.Select(x => x.Name).Should().Equal(_testItemsNames);
        }

        private void RemoveToDoItems()
        {
            _viewModel.RemoveCommand.Execute(
                _testItemsNames.Select(n => new ToDoItem { Name = n })
                .ToList());
        }

        [TestMethod]
        public void CanAddList()
        {
            _viewModel.ToDoLists.Clear();

            _viewModel.AddListCommand.Execute(this);
            _viewModel.AddListCommand.Execute(this);

            _viewModel.ToDoLists.Select(x => x.Name).Should().Equal( "New list", "New list 1" );
        }
    }
}
