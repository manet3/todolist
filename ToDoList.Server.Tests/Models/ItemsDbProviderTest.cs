using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Server.Database;
using ToDoList.Server.Database.Models;
using ToDoList.Shared;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace ToDoList.Server.Tests.Models
{
    [TestClass]
    public class ItemsDbProviderTests
    {
        [ClassInitialize]
        public static void MapDbModel(TestContext context)
            => Mapper.Initialize(m => m.CreateMap<ItemDbModel, ToDoItem>());

        private ItemDbModel[] TestSet = new[] {
            new ItemDbModel{Name = "Test item" },
            new ItemDbModel{Name = "Test item 1" },
            new ItemDbModel{ Name = "Test item 2", IsChecked = true } };

        private ToDoItem[] GetComapableCollection(IEnumerable<ItemDbModel> dbModels)
            => Mapper.Map<IEnumerable<ToDoItem>>(dbModels).ToArray();

        [TestMethod]
        public void DB_Update()
        {
            var expected = new[] {
            new ItemDbModel{Name = "Test item 2", IsChecked = true },
            new ItemDbModel{ Name = "Test item" } };

            ItemsDbProvider.UpdateDB(TestSet);
            ItemsDbProvider.UpdateDB(expected);
            ItemsDbProvider.TryGetDBItems(out IEnumerable<ItemDbModel> gotItems);

            //for correct equals comparation
            CollectionAssert.AreEqual(
                GetComapableCollection(expected)
                ,GetComapableCollection(gotItems));

        }

        [TestMethod]
        public void DB_Get()
        {
            //Arrange
            ItemsDbProvider.UpdateDB(TestSet);
            //Act
            ItemsDbProvider.TryGetDBItems(out IEnumerable<ItemDbModel> gotItems);
            //Assert
            Assert.IsNotNull(gotItems);
            CollectionAssert.AreEqual(
                GetComapableCollection(TestSet),
                GetComapableCollection(gotItems));
        }


        [TestMethod]
        public void DB_Delete()
        {
            ItemsDbProvider.UpdateDB(TestSet);
            var expected = new[] { new ItemDbModel { Name = "Test item" },
                new ItemDbModel { Name = "Test item 2" } };

            var succeed = ItemsDbProvider.TryRemoveDBItem("Test item 1");

            ItemsDbProvider.TryGetDBItems(out IEnumerable<ItemDbModel> gotItems);

            Assert.IsTrue(succeed);
            CollectionAssert.AreEqual(
                GetComapableCollection(expected), 
                GetComapableCollection(gotItems));
        }

        [TestMethod]
        public void DB_Add()
        {
            ItemsDbProvider.UpdateDB(new ItemDbModel[0]);
            var expected = new[] { new ItemDbModel { Name = "Test item" },
                new ItemDbModel { Name = "Test item 2" } };

            ItemsDbProvider.AddToDB(new ItemDbModel { Name = "Test item" });

            ItemsDbProvider.AddToDB(new ItemDbModel { Name = "Test item 2" });

            ItemsDbProvider.TryGetDBItems(out IEnumerable<ItemDbModel> gotItems);

            CollectionAssert.AreEqual(
                GetComapableCollection(expected),
                GetComapableCollection(gotItems));
        }

    }
}
