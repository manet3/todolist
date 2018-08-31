using ToDoList.Server.Database.POCOs;
using ToDoList.Server.Database.Services;
using ServiceStack.OrmLite;
using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace ToDoList.Server.Database
{
    public class ListsRepository : RepositoryBase, IListsRepository
    {
        public override void ConfigureStorage()
            => StorageConfiguration = CreateTablesIfNot(typeof(ToDoListPoco), typeof(ToDoItemPoco));

        public Result<IEnumerable<ToDoListPoco>> Get()
        {
            throw new NotImplementedException();
        }

        public Result Add(ToDoListPoco list)
        {
            throw new NotImplementedException();
        }

        public Result DeleteById(ulong id, DateTime timestamp)
        {
            throw new NotImplementedException();
        }
    }
}