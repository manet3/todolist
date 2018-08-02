using System.Collections.Generic;

namespace ToDoList.Client.DataServices
{
    public class SyncMemento
    {
        public Queue<ItemSendAction> Actions { get; }

        public SyncMemento(Queue<ItemSendAction> actions)
            => Actions = actions;
    }
}
