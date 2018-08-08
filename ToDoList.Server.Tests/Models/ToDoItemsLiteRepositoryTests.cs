using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System;
using System.Linq;
using ServiceStack.OrmLite;
using ServiceStack;
using ToDoList.Server.Database;
using ToDoList.Server.Database.Models;
using System.Data;
using System.Collections.Generic;

namespace ToDoList.Server.Tests.Models
{
    [TestClass]
    public class ToDoItemsLiteRepositoryTests
    {
        private ToDoItemsLiteRepository _repository;

        private ItemDbModel[] _testSet = new[]
        {
            new ItemDbModel { Name = "Test item"},
            new ItemDbModel { Name = "Test item 10"},
            new ItemDbModel { Name = "Test item 2", IsChecked = true}
        };

        private static OrmLiteConnectionFactory _dbFactory;

        [TestInitialize]
        public void DbInitialize()
        {
            _repository = new ToDoItemsLiteRepository();
            _repository.ConnectStorage();

            _dbFactory = new OrmLiteConnectionFactory(
                "~/App_Data/todoDB.sqlite".MapHostAbsolutePath()
                , SqliteDialect.Provider);

            using (var dbConn = _dbFactory.Open())
                dbConn.CreateTableIfNotExists<ItemDbModel>();
        }

        [TestCleanup]
        public void DbCleanup()
        {
            using (var dbConn = _dbFactory.Open())
                dbConn.DropTable<ItemDbModel>();

            _repository.Dispose();
        }

        [TestMethod]
        public void CanGetItems()
        {
            using (var dbConn = _dbFactory.Open())
                RewriteTable(dbConn, _testSet);

            var res = _repository.List();

            res.IsSuccess.Should().BeTrue();
            CollectionAssert(res.Value, _testSet);
        }

        [TestMethod]
        public void CanDeleteItem()
        {
            using (var dbConn = _dbFactory.Open())
            {
                RewriteTable(dbConn, _testSet);
                var deletedItem = dbConn.Single<ItemDbModel>(x => x.Name == _testSet[1].Name);

                var res = _repository.DeleteById(deletedItem.Id, DateTime.UtcNow);

                res.IsSuccess.Should().BeTrue();
                CollectionAssert(
                    dbConn.Select<ItemDbModel>(), 
                    new[] { _testSet[0], _testSet[2] });
            }
        }

        [TestMethod]
        public void CanUpdateItem()
        {
            using (var dbConn = _dbFactory.Open())
            {
                RewriteTable(dbConn, _testSet[0]);

                var res = _repository.UpdateItem(new ItemDbModel
                {
                    Name = _testSet[0].Name,
                    IsChecked = true,
                    Timestamp = DateTime.UtcNow
                });
                var checkItems = dbConn.Select<ItemDbModel>();

                res.IsFailure.Should().BeFalse();
                checkItems[0].IsChecked.Should().BeTrue();
            }
        }

        [TestMethod]
        public void FailsAddingNonUniqueItem()
        {
            using (var dbConn = _dbFactory.Open())
            {
                var item = new ItemDbModel { Name = "Existing item" };
                RewriteTable(dbConn, item);

                item.IsChecked = true;
                var res = _repository.Add(item);

                res.IsFailure.Should().BeTrue("should throw {}", res.Error);
            }
        }

        private void CollectionAssert(IEnumerable<ItemDbModel> gotItems, IEnumerable<ItemDbModel> expectedItems)
        {
            foreach (var test in gotItems.Zip(expectedItems, (x, y) => new { Got = x, Expected = y }))
            {
                test.Got.Name.Should().Be(test.Expected.Name);
                test.Got.IsChecked.Should().Be(test.Expected.IsChecked);
                test.Got.Timestamp.Should().Be(test.Expected.Timestamp);
            }
        }

        private void RewriteTable(IDbConnection dbConn, params ItemDbModel[] items)
        {
            dbConn.DeleteAll<ItemDbModel>();
            var saveTime = DateTime.UtcNow;
            foreach (var item in items)
            {
                item.Timestamp = saveTime;
                item.Id = (ulong)dbConn.Insert(
                    new ItemDbModel
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
