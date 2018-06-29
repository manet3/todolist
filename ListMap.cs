using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.Client
{
    /// <summary>
    /// 1:M datatable for easily 
    /// replacing list items (instead of Index)
    /// </summary>
    class ListMap
    {
        public IEnumerable<TaskModel> Map { get; private set; }

        public static ListMap Instance { get; private set; }

        private ListMap(IEnumerable<TaskModel> map)
        {
            Map = map;
        }

        public static ListMap MakeInstance(IEnumerable<TaskModel> map)
        {
            if (Instance == null)
                Instance = new ListMap(map);
            return Instance;
        }
    }
}
