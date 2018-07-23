using ServiceStack.DataAnnotations;
namespace ToDoList.Server.Database.Models
{
    public class ItemDbModel
    {
        [AutoIncrement]
        public ulong Id { get; set; }

        [Required, Unique]
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }
}