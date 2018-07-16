using ServiceStack;
using ServiceStack.OrmLite;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ToDoList.Server.Database.Models;

namespace ToDoList.Server.Database
{
    public static class ItemsDbProvider
    {
        private static readonly string _dbFilePath;

        private static readonly OrmLiteConnectionFactory _dbFactory;

        static ItemsDbProvider()
        {
            _dbFilePath = "~/App_Data/todoDB.sqlite".MapHostAbsolutePath();
            _dbFactory = new OrmLiteConnectionFactory(_dbFilePath, SqliteDialect.Provider);
        }

        public static void AddToDB(ItemDbModel item)
        {
            using (var dbOpen = _dbFactory.Open())
            {
                dbOpen.CreateTableIfNotExists<ItemDbModel>();
                if (!TryUpdateDBItem(item, dbOpen))
                    dbOpen.Insert(item);
                return;
            }
        }

        public static bool TryGetDBItems(out IEnumerable<ItemDbModel> items)
        {
            using (var dbOpen = _dbFactory.Open())
            {
                try
                {
                    items = dbOpen
                        .Select<ItemDbModel>()
                        .ToList();
                    //because items are got in opposite order
                    items.Reverse();
                    return true;
                }
                catch (System.Data.SQLite.SQLiteException)
                {
                    items = new ItemDbModel[0];
                    return false;
                }
            }
        }

        public static bool TryUpdateDBItem(ItemDbModel task, IDbConnection dbOpen)
        {
            var itemFound = dbOpen.Exists<ItemDbModel>(new { Name = task.Name });
            if (itemFound)
                dbOpen.Update(task);
            return itemFound;
        }

        public static void UpdateDB(IEnumerable<ItemDbModel> item_set)
        {
            using (var dbOpen = _dbFactory.Open())
            {
                if (dbOpen.TableExists<ItemDbModel>())
                    dbOpen.DeleteAll<ItemDbModel>();
                else dbOpen.CreateTable<ItemDbModel>();

                foreach (var item in item_set)
                    if (!TryUpdateDBItem(item, dbOpen))
                        dbOpen.Insert(item);
                return;
            }
        }

        public static bool TryRemoveDBItem(string name)
        {
            using (var dbOpen = _dbFactory.Open())
            {
                if (!dbOpen.Exists<ItemDbModel>(new { Name = name }))
                    return false;
                dbOpen.Delete<ItemDbModel>((x) => x.Name == name);
                return true;
            }
        }
    }
}