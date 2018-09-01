using ToDoList.Server.Database;
using ToDoList.Server.Database.POCOs;
using ToDoList.Server.Database.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System;
using System.Linq;
using ServiceStack.OrmLite;
using System.Data;
using System.Collections.Generic;

namespace ToDoList.Server.Tests.Repositories
{
    [TestClass]
    public class ItemsRepositoryTests
    {
        private ItemsRepository _repository;

        private IDbConnection _dbConn;

        private ItemPoco[] _testSet = new[]
        {
            new ItemPoco { Name = "Test item"},
            new ItemPoco { Name = "Test item 1"},
            new ItemPoco { Name = "Test item 2", IsChecked = true}
        };

        [TestInitialize]
        public void DbInitialize()
        {
            _repository = new ItemsRepository();
            _repository.ConfigureStorage();

            _dbConn = DbConnectionDistributor.DbConnection;

            _dbConn.DropAndCreateTable<ItemPoco>();
        }

        [TestCleanup]
        public void DbCleanup() => DbConnectionDistributor.CloseConnection();

        [TestMethod]
        public void CanDeleteItem()
        {
            WriteTable(_dbConn, _testSet);
            var deletedItem = _dbConn.Single<ItemPoco>(x => x.Name == _testSet[1].Name);

            var res = _repository.DeleteById(deletedItem.Id, DateTime.UtcNow);

            res.IsSuccess.Should().BeTrue();
            CollectionAssert( _dbConn.Select<ItemPoco>(), new[] { _testSet[0], _testSet[2] });
        }

        [TestMethod]
        public void CanUpdateItem()
        {
            WriteTable(_dbConn, _testSet[0]);

            var res = _repository.Update(new ItemPoco
            {
                Id = 1,
                IsChecked = true,
                Timestamp = DateTime.UtcNow
            });
            var checkItems = _dbConn.Select<ItemPoco>();

            res.IsFailure.Should().BeFalse();
            checkItems[0].IsChecked.Should().BeTrue();
        }

        [TestMethod]
        public void FailsAddingNonUniqueItem()
        {
            var item = new ItemPoco { Name = "Existing item" };
            WriteTable(_dbConn, item);

            item.IsChecked = true;
            var res = _repository.Add(item);

            res.IsFailure.Should().BeTrue("should throw {}", res.Error);
        }

        private void CollectionAssert(IEnumerable<ItemPoco> gotItems, IEnumerable<ItemPoco> expectedItems)
        {
            foreach (var test in gotItems.Zip(expectedItems, (x, y) => new { Got = x, Expected = y }))
            {
                test.Got.Name.Should().Be(test.Expected.Name);
                test.Got.IsChecked.Should().Be(test.Expected.IsChecked);
                test.Got.Timestamp.Should().Be(test.Expected.Timestamp);
            }
        }

        private void WriteTable(IDbConnection dbConn, params ItemPoco[] items)
        {
            var saveTime = DateTime.UtcNow;
            foreach (var item in items)
            {
                item.Timestamp = saveTime;
                item.Id = (ulong)dbConn.Insert(
                    new ItemPoco
                    {
                        Name = item.Name,
                        IsChecked = item.IsChecked,
                        Timestamp = item.Timestamp
                    },
                    selectIdentity: true);
            }
        }

    }
}
