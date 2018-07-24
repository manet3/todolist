﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Server.Database;
using ToDoList.Server.Database.Models;
using ServiceStack.OrmLite;
using ToDoList.Shared;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using System.Collections.ObjectModel;

namespace ToDoList.Server.Tests.Models
{
    [TestClass]
    public class ItemsDbProviderTests
    {
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
            _dbFactory = new OrmLiteConnectionFactory(ToDoItemsLiteRepository.DbFilePath);
            using (var dbConn = _dbFactory.Open())
                dbConn.CreateTable(modelType: typeof(ItemDbModel), overwrite: true);
        }

        [TestCleanup]
        public void DbFileRemove()
        {
            using (var dbConn = _dbFactory.Open())
                dbConn.DropTable<ItemDbModel>();
        }

        private ToDoItem[] GetComarableCollection(IEnumerable<ItemDbModel> dbModels)
            => dbModels.Select(m => new ToDoItem { Name = m.Name, IsChecked = m.IsChecked }).ToArray();

        [TestMethod]
        public void CanRewriteItemsTable()
        {
            using (var dbConn = _dbFactory.Open())
            {
                //act
                var res = ToDoItemsLiteRepository.DBRewrite(_testSet);
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
                var res = ToDoItemsLiteRepository.GetDBItems();
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
                var res = ToDoItemsLiteRepository.TryRemoveDBItem("Test item 1");
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
                var res = ToDoItemsLiteRepository.DBUpdateItem(new ItemDbModel { Name = "Test item 1", IsChecked = true });
                var checkItems = GetComarableCollection(dbConn.Select<ItemDbModel>());
                //assert
                res.IsFailure.Should().BeFalse();
                checkItems.Should().Equal(new ToDoItem { Name = "Test item", IsChecked = true });
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
                var res = ToDoItemsLiteRepository.AddToDB(item);
                //assert
                res.IsFailure.Should().BeTrue("should throw {}", res.Error);
            }
        }

    }
}
