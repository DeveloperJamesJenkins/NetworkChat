using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkChat
{
    /// <summary>
    /// checks the difference between the original task and the edited task
    /// </summary>
    class TasksChecker
    {
        //when user clicks the save then check the source task file for that item
        //if it is diff than what you have in the currentTasks list
        //then warn/load the current document.
        public static Nullable<bool> LocalTaskChanged(TasksLocalShare.Task Task)
        {
            //now check against currenttasks in the last loaded list...
            
            List<TasksLocalShare.Task> sourceTasks = TasksLocalShare.LoadTaskList(); //from source
            //find from list check taskStatus and taskDetails

            TasksLocalShare.Task t = sourceTasks.Find(s => s.TaskId == Task.TaskId);

            if (t == null)
                return null; //been deleted
            
            if (t.TaskDetails == Task.TaskDetails & t.TaskStatus == Task.TaskStatus & t.LastEditBy == Task.LastEditBy)
                return false;
            else
                return true;

        }
    }
}
