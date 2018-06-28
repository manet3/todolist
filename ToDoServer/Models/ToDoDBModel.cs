using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack;
using System.Data;

namespace ToDoList.Server.Models
{
    public static class ToDoDBModel
    {
        private static readonly string _dbFilePath;

        private static readonly OrmLiteConnectionFactory _dbFactory;

        private static IDbConnection dbOpen;

        static ToDoDBModel()
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

        public static bool AddToDB(TaskModel item)
        {
            dbOpen.CreateTableIfNotExists<TaskModel>();
            if (!UpdateDB(item))
                dbOpen.Insert(item);
            return true;
        }

        public static bool GetDBItems(ref IEnumerable<TaskModel> items)
        {
            try
            {
                items = dbOpen
                    .Select<TaskModel>()
                    .ToList()
                    .OrderBy(x => x.Index);
                return true;
            }
            catch (System.Data.SQLite.SQLiteException)
            {
                items = new TaskModel[0];
                return false;
            }
        }

        public static bool GetDbItem(int id, ref TaskModel item)
        {
            item = dbOpen.Single<TaskModel>(x=> x.Id == id);
            return dbOpen.Exists<TaskModel>(new { Id = id });
            
        }

        public static bool UpdateDB( TaskModel task)
        {
            var itemFound = dbOpen.Exists<TaskModel>(new { Id = task.Id });
            if (itemFound)
                dbOpen.Update(task);
            return itemFound;
        }

        public static bool RemoveDBItem(int id)
        {
            dbOpen.DeleteById<TaskModel>(id);
            return true;
        }
    }
}