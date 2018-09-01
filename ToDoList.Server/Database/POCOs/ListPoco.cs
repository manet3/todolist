using ServiceStack.DataAnnotations;

namespace ToDoList.Server.Database.POCOs
{
    public class ListPoco : PocoCommon
    {
        [Reference]
        public ItemPoco[] ToDoItems { get; set; }
    }
}