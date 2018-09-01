using ToDoList.Server.Database;
using ToDoList.Server.Database.POCOs;
using ToDoList.Server.Database.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System;
using System.Linq;
using ServiceStack.OrmLite;
using System.Data;

namespace ToDoList.Server.Tests
{
    [TestClass]
    public class ListsRepositoryTests
    {
        private ListsRepository _repository;

        private IDbConnection _dbConn;

        private ListPoco _testList = new ListPoco
        {
            Name = "list",
            Timestamp = DateTime.UtcNow,
            ToDoItems = new[]{
                new ItemPoco { Name = "Item 1", Timestamp = DateTime.UtcNow },
                new ItemPoco { Name = "Item 2", Timestamp = DateTime.UtcNow}}
        };

        [TestInitialize]
        public void DbInitialize()
        {
            _repository = new ListsRepository();
            _repository.ConfigureStorage();

            _dbConn = DbConnectionDistributor.DbConnection;
            _dbConn.DropAndCreateTable<ListPoco>();
            _dbConn.DropAndCreateTable<ItemPoco>();
            _dbConn.Save(_testList, references: true);
        }

        [TestCleanup]
        public void DbCleanup() => DbConnectionDistributor.CloseConnection();

        [TestMethod]
        public void CanAddList()
        {
            _dbConn.DropAndCreateTable<ListPoco>();
            _dbConn.DropAndCreateTable<ItemPoco>();
            _repository.Add(_testList);

            _dbConn.Exists<ListPoco>(x => x.Name == _testList.Name).Should().BeTrue();
            _dbConn.Select<ItemPoco>(x => x.ListPocoId == 1).Should().HaveCount(2);
        }

        [TestMethod]
        public void CanMergeLists()
        {
            var anotherList = new ListPoco
            {
                Name = _testList.Name,
                ToDoItems = new[] {
                    new ItemPoco { Name = "Item 1" },
                    new ItemPoco { Name = "Item 2", IsChecked = true, Timestamp = DateTime.UtcNow },
                    new ItemPoco { Name = "Item 3" } }
            };

            _repository.Add(anotherList);

            var list = _dbConn.Single<ListPoco>(x => x.Name == _testList.Name);
            var listItems = _dbConn.Select<ItemPoco>(item => item.ListPocoId == list.Id);

            listItems.Select(x => x.Name).Should().Equal(new[] { "Item 1", "Item 2", "Item 3" });
            listItems[1].IsChecked.Should().Be(true);
        }

        [TestMethod]
        public void CanGetLists()
        {
            var list = _repository.Get().Value;

            list.First().Name.Should().Be(_testList.Name);
            list.First().ToDoItems.Select(x => x.Name).Should().Equal(new[] { "Item 1", "Item 2" });
            list.First().ToDoItems.Select(x => x.Timestamp).Should().Equal(_testList.ToDoItems.Select(x => x.Timestamp));
        }

        [TestMethod]
        public void CanDeleteList()
        {
            _repository.DeleteById(_testList.Id, _testList.Timestamp);

            _dbConn.Exists<ListPoco>(x => x.Name == _testList.Name).Should().BeFalse();
            _dbConn.Select<ItemPoco>(x => x.ListPocoId == 1).Should().HaveCount(0);
        }
    }
}
