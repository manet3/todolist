using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ToDoList.Server.Models
{
    public class ItemsDbProvider
    {
        private static readonly string _dbFilePath;

        private static readonly OrmLiteConnectionFactory _dbFactory;

        private static IDbConnection dbOpen;

        static ItemsDbProvider()
        {
            _dbFilePath = "~/App_Data/todoDB.sqlite".MapHostAbsolutePath();
            _dbFactory = new OrmLiteConnectionFactory(_dbFilePath, SqliteDialect.Provider);
        }

        public static void UsingDb(Func<bool> exec)
        {
            using (dbOpen = _dbFactory.Open())
            {
                exec();
            }
        }

        public static bool AddToDB(object item)
        {
            dbOpen.CreateTableIfNotExists();
            if (!UpdateDB(item))
                dbOpen.Insert(item);
            return true;
        }

        public static bool GetDBItems(ref IEnumerable items)
        {
            try
            {
                items = dbOpen
                    .Select()
                    .ToList()
                    .OrderBy(x => x.Index);
                return true;
            }
            catch (System.Data.SQLite.SQLiteException)
            {
                items = new object[0];
                return false;
            }
        }

        public static bool GetDbItem(int id, ref object item)
        {
            item = dbOpen.Single(x => x.Id == id);
            return dbOpen.Exists(new { Id = id });

        }

        public static bool UpdateDB(object task)
        {
            var itemFound = dbOpen.Exists(new { Id = task.Id });
            if (itemFound)
                dbOpen.Update(task);
            return itemFound;
        }

        public static bool UpdateDB(IEnumerable<object> item_set)
        {
            if (dbOpen.TableExists())
                dbOpen.DeleteAll();
            else dbOpen.CreateTable();

            foreach (var item in item_set)
                if (!UpdateDB(item))
                    dbOpen.Insert(item);
            return true;
        }

        public static bool RemoveDBItem(int id)
        {
            dbOpen.DeleteById(id);
            return true;
        }
    }
}