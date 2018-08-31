using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System;
using System.Linq;
using ServiceStack.OrmLite;
using ServiceStack;
using ToDoList.Server.Database;
using ToDoList.Server.Database.POCOs;
using System.Data;
using System.Collections.Generic;

namespace ToDoList.Server.Tests.Models
{
    [TestClass]
    public class ToDoItemsLiteRepositoryTests
    {
        private ItemsRepository _repository;

        private ToDoItemPoco[] _testSet = new[]
        {
            new ToDoItemPoco { Name = "Test item"},
            new ToDoItemPoco { Name = "Test item 10"},
            new ToDoItemPoco { Name = "Test item 2", IsChecked = true}
        };

        private IDbConnection _dbConn;

        [TestInitialize]
        public void DbInitialize()
        {
            _repository = new ItemsRepository();
            _repository.ConnectStorage();

            _dbConn = new OrmLiteConnectionFactory(
                "~/App_Data/todoDB.sqlite".MapHostAbsolutePath()
                , SqliteDialect.Provider).Open();

            _dbConn.DropAndCreateTable<ToDoItemPoco>();
        }

        [TestCleanup]
        public void DbCleanup()
        {
            _repository.Dispose();
            _dbConn.Dispose();
        }

        [TestMethod]
        public void CanGetItems()
        {
            RewriteTable(_dbConn, _testSet);

            var res = _repository.List();

            res.IsSuccess.Should().BeTrue();
            CollectionAssert(res.Value, _testSet);
        }

        [TestMethod]
        public void CanDeleteItem()
        {
            RewriteTable(_dbConn, _testSet);
            var deletedItem = _dbConn.Single<ToDoItemPoco>(x => x.Name == _testSet[1].Name);

            //var res = _repository.DeleteById(deletedItem.Id, DateTime.UtcNow);

            //res.IsSuccess.Should().BeTrue();
            //CollectionAssert(
            //    _dbConn.Select<ToDoItemPOCO>(),
            //    new[] { _testSet[0], _testSet[2] });
        }

        [TestMethod]
        public void CanUpdateItem()
        {
            RewriteTable(_dbConn, _testSet[0]);

            var res = _repository.UpdateItem(new ToDoItemPoco
            {
                Name = _testSet[0].Name,
                IsChecked = true,
                Timestamp = DateTime.UtcNow
            });
            var checkItems = _dbConn.Select<ToDoItemPoco>();

            res.IsFailure.Should().BeFalse();
            checkItems[0].IsChecked.Should().BeTrue();
        }

        [TestMethod]
        public void FailsAddingNonUniqueItem()
        {
            var item = new ToDoItemPoco { Name = "Existing item" };
            RewriteTable(_dbConn, item);

            item.IsChecked = true;
            var res = _repository.Add(item);

            res.IsFailure.Should().BeTrue("should throw {}", res.Error);
        }

        private void CollectionAssert(IEnumerable<ToDoItemPoco> gotItems, IEnumerable<ToDoItemPoco> expectedItems)
        {
            foreach (var test in gotItems.Zip(expectedItems, (x, y) => new { Got = x, Expected = y }))
            {
                test.Got.Name.Should().Be(test.Expected.Name);
                test.Got.IsChecked.Should().Be(test.Expected.IsChecked);
                test.Got.Timestamp.Should().Be(test.Expected.Timestamp);
            }
        }

        private void RewriteTable(IDbConnection dbConn, params ToDoItemPoco[] items)
        {
            dbConn.DeleteAll<ToDoItemPoco>();
            var saveTime = DateTime.UtcNow;
            foreach (var item in items)
            {
                item.Timestamp = saveTime;
                item.Id = (ulong)dbConn.Insert(
                    new ToDoItemPOCO
                    {
                        Name = item.Name,
                        IsChecked = item.IsChecked,
                        //only in tests two hours are added durring reading )))
                        Timestamp = item.Timestamp.AddHours(-2)
                    },
                    selectIdentity: true);
            }
        }

    }
}
