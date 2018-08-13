using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Client.ViewModels;
using ToDoList.Client.Tests.Mock;
using System.Collections.Generic;
using FluentAssertions;
using ToDoList.Shared;
using System.Linq;
using System;
using ToDoList.Client.Test.Mock;

namespace ToDoList.Client.Tests
{
    [TestClass]
    public class ToDoViewModelTests
    {
        private SyncMock _sync;

        private ToDoViewModel _tested;

        [TestInitialize]
        public void ViewModelInit()
        {
            _sync = new SyncMock();
            _tested = new ToDoViewModel(_sync);
        }

        [TestMethod]
        public void CanAddItem()
        {
            var testItem = new ToDoItem { Name = "new item" };

            _tested.NewItemText = testItem.Name;
            _tested.AddCommand.Execute(this);

            _tested.ToDoItems.Should().Contain(
                item => item.Name == testItem.Name
                && item.IsChecked == testItem.IsChecked);
        }
    }
}
