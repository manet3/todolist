using ServiceStack;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ToDoList.Shared;

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

        public static bool AddToDB(ToDoItem item)
        {
            if (item == null) return false;
            using (var dbOpen = _dbFactory.Open())
            {
                dbOpen.CreateTableIfNotExists();
                if (!UpdateDB(item, dbOpen))
                    dbOpen.Insert(item);
                return true;
            }
        }

        public static bool GetDBItems(out IEnumerable<ToDoItem> items)
        {
            using (var dbOpen = _dbFactory.Open())
            {
                try
                {
                    items = dbOpen
                        .Select<ToDoItem>()
                        .ToList()
                        //because items are got in opposite order
                        .OrderByDescending(x => x.Index);
                    return true;
                }
                catch (System.Data.SQLite.SQLiteException)
                {
                    items = new ToDoItem[0];
                    return false;
                }
            }
        }

        private static bool UpdateDB(ToDoItem task, IDbConnection dbOpen)
        {
            var itemFound = dbOpen.Exists<ToDoItem>(new { Name = task.Name });
            if (itemFound)
                dbOpen.Update(task);
            return itemFound;
        }

        public static bool UpdateDB(IEnumerable<ToDoItem> item_set)
        {
            if (item_set == null) return false;
            using (var dbOpen = _dbFactory.Open())
            {
                if (dbOpen.TableExists<ToDoItem>())
                    dbOpen.DeleteAll<ToDoItem>();
                else dbOpen.CreateTable<ToDoItem>();

                foreach (var item in item_set)
                    if (!UpdateDB(item, dbOpen))
                        dbOpen.Insert(item);
                return true;
            }
        }

        public static bool RemoveDBItem(string name)
        {
            using (var dbOpen = _dbFactory.Open())
            {
                dbOpen.Delete<ToDoItem>((x) => x.Name == name);
                return true;
            }
        }
    }
}