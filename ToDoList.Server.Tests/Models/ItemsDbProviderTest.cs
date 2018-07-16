using ToDoList.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoList.Server.Models;
using System.Collections.Generic;
using System.Linq;

namespace ToDoList.Server.Tests.Models
{
    [TestClass]
    public class ItemsDbProviderTests
    {
        private ToDoItem[] TestSet = new[] { new ToDoItem("Test item", index:1),
            new ToDoItem("Test item 1"), new ToDoItem("Test item 2", true)};

        [TestMethod]
        public void DB_Update()
        {
            var expected = new[] { new ToDoItem("Test item"),
                new ToDoItem("Test item 1", true)};

            ItemsDbProvider.UpdateDB(TestSet);
            ItemsDbProvider.UpdateDB(expected);
            ItemsDbProvider.GetDBItems(out IEnumerable<ToDoItem> gotItems);

            CollectionAssert.AreEqual(expected, gotItems.Select((x) => x).ToArray());

        }

        [TestMethod]
        public void DB_Get()
        {
            //Arrange
            ItemsDbProvider.UpdateDB(TestSet);
            var expected = new[] { new ToDoItem("Test item 1"), new ToDoItem("Test item") } ;
            //Act
            ItemsDbProvider.GetDBItems(out IEnumerable<ToDoItem> gotItems);
            //Assert
            Assert.IsNotNull(gotItems);
            CollectionAssert.AreEqual(TestSet, gotItems.Select((x)=>x).ToArray());
        }


        [TestMethod]
        public void DB_Delete()
        {
            ItemsDbProvider.UpdateDB(TestSet);
            var expected = new[] {new ToDoItem("Test item"), new ToDoItem("Test item 2") };

            var succeed = ItemsDbProvider.RemoveDBItem("Test item 1");

            ItemsDbProvider.GetDBItems(out IEnumerable<ToDoItem> gotItems);

            Assert.IsTrue(succeed);
            CollectionAssert.AreEqual(expected, gotItems.Select((x) => x).ToArray());
        }
    }
}
