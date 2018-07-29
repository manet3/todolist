using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Server.Database;
using ToDoList.Server.Database.Models;
using ServiceStack.OrmLite;
using ToDoList.Shared;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using System.Collections.ObjectModel;
using ServiceStack;

namespace ToDoList.Server.Tests.Models
{
    [TestClass]
    public class ToDoItemsLiteRepositoryTests
    {
        private ToDoItemsLiteRepository repository;

        private ReadOnlyCollection<ItemDbModel> _testSet = new List<ItemDbModel>
        {
            new ItemDbModel { Name = "Test item" },
            new ItemDbModel { Name = "Test item 1" },
            new ItemDbModel { Name = "Test item 2", IsChecked = true }
        }.AsReadOnly();

        private static OrmLiteConnectionFactory _dbFactory;

        [TestInitialize]
        public void DbInitialize()
        {
            repository = new ToDoItemsLiteRepository();
            repository.ConnectStorage();

            _dbFactory = new OrmLiteConnectionFactory(
                "~/App_Data/todoDB.sqlite".MapHostAbsolutePath()
                , SqliteDialect.Provider);

            using (var dbConn = _dbFactory.Open())
                dbConn.CreateTableIfNotExists<ItemDbModel>();
        }

        [TestCleanup]
        public void DbFileRemove()
        {
            using (var dbConn = _dbFactory.Open())
                dbConn.DropTable<ItemDbModel>();

            repository.Dispose();
        }

        private ToDoItem[] GetComarableCollection(IEnumerable<ItemDbModel> dbModels)
            => dbModels.Select(m => new ToDoItem { Name = m.Name, IsChecked = m.IsChecked }).ToArray();

        [TestMethod]
        public void CanRewriteItemsTable()
        {
            using (var dbConn = _dbFactory.Open())
            {
                //act
                var res = repository.UpdateAll(_testSet);
                var gotItems = GetComarableCollection(dbConn.Select<ItemDbModel>());
                //assert
                res.IsSuccess.Should().BeTrue();
                GetComarableCollection(_testSet).Should()
                    .Equal(gotItems);
            }
        }

        [TestMethod]
        public void CanGetItems()
        {
            using (var dbConn = _dbFactory.Open())
            {
                //arrange
                dbConn.DeleteAll<ItemDbModel>();
                dbConn.InsertAll(_testSet);
                //act
                var res = repository.List();
                //assert
                res.IsSuccess.Should().BeTrue();
                GetComarableCollection(res.Value).Should()
                    .Equal(GetComarableCollection(_testSet));
            }
        }


        [TestMethod]
        public void CanDeleteItem()
        {
            using (var dbConn = _dbFactory.Open())
            {
                //arrange
                dbConn.DeleteAll<ItemDbModel>();
                dbConn.InsertAll(_testSet);

                var expected = new[] { new ItemDbModel { Name = "Test item" },
                new ItemDbModel { Name = "Test item 2" } };

                //act
                var res = repository.DeleteByName("Test item 1");
                var checkItems = GetComarableCollection(dbConn.Select<ItemDbModel>());

                //assert
                res.IsSuccess.Should().BeTrue();
                checkItems.Should().Equal(GetComarableCollection(expected));
            }
        }

        [TestMethod]
        public void CanUpdateItem()
        {
            using (var dbConn = _dbFactory.Open())
            {
                //arrange
                dbConn.DeleteAll<ItemDbModel>();
                //act
                dbConn.Insert(new ItemDbModel { Name = "Test item" });
                var res = repository.UpdateItem(new ItemDbModel { Name = "Test item", IsChecked = true });
                var checkItems = GetComarableCollection(dbConn.Select<ItemDbModel>());
                //assert
                res.IsFailure.Should().BeFalse();
                checkItems[0].IsChecked.Should().BeTrue();
            }
        }

        [TestMethod]
        public void FailsAddingNonUniqueItem()
        {
            using (var dbConn = _dbFactory.Open())
            {
                //arrange
                var item = new ItemDbModel { Name = "Existing item" };
                dbConn.DeleteAll<ItemDbModel>();
                dbConn.Insert(item);
                //act
                item.IsChecked = true;
                var res = repository.Add(item);
                //assert
                res.IsFailure.Should().BeTrue("should throw {}", res.Error);
            }
        }

    }
}
