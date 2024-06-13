using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Toolkit/Task List", fileName = "New Task List")]
public class TaskListSO : ScriptableObject
{
    [SerializeField] private List<string> _tasks = new List<string>();


    public List<string> GetTasks()
    {
        return _tasks;
    }

    public void SetTasks(List<string> value)
    {
        _tasks.Clear();
        _tasks = value;
    }

    public void AddTask(string task)
    {
        _tasks.Add(task);
    }
}
