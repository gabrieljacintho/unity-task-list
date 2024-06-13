using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class TaskListEditor : EditorWindow
{
    private VisualElement _container;
    private ObjectField _savedTasksObjectField;
    private Button _loadTasksButton;
    private ToolbarSearchField _searchField;
    private TextField _newTaskField;
    private Button _addTaskButton;
    private ScrollView _taskScrollView;
    private Button _saveProgressButton;
    private ProgressBar _taskProgressBar;
    private Notification _notification;

    private TaskListSO _taskListSO;

    public const string Path = "Assets/TaskList/Editor/EditorWindow";


    [MenuItem("Window/Task List")]
    private static void ShowWindow()
    {
        TaskListEditor window = GetWindow<TaskListEditor>();
        window.titleContent = new GUIContent("Task List");
        window.minSize = new Vector2(400f, 450f);
    }

    private void CreateGUI()
    {
        _container = rootVisualElement;

        VisualTreeAsset visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path + "/TaskListEditor.uxml");
        TemplateContainer templateContainer = visualTreeAsset.Instantiate();
        templateContainer.style.flexGrow = 1f;

        _container.Add(templateContainer);

        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path + "/TaskListEditor.uss");
        _container.styleSheets.Add(styleSheet);

        _savedTasksObjectField = _container.Q<ObjectField>("savedTasksObjectField");
        _savedTasksObjectField.objectType = typeof(TaskListSO);

        _loadTasksButton = _container.Q<Button>("loadTasksButton");
        _loadTasksButton.clicked += LoadTasks;

        _searchField = _container.Q<ToolbarSearchField>("searchField");
        _searchField.RegisterValueChangedCallback(OnSearchFieldValueChanged);

        _newTaskField = _container.Q<TextField>("newTaskField");
        _newTaskField.RegisterCallback<KeyDownEvent>(AddTask);

        _addTaskButton = _container.Q<Button>("addTaskButton");
        _addTaskButton.clicked += AddTask;

        _taskScrollView = _container.Q<ScrollView>("taskScrollView");
        _taskScrollView.Clear();

        _saveProgressButton = _container.Q<Button>("saveProgressButton");
        _saveProgressButton.clicked += SaveProgress;

        _taskProgressBar = _container.Q<ProgressBar>("taskProgressBar");
        _taskProgressBar.value = 0f;

        UpdateNotification("Please load a task list to continue.");
    }

    private TaskItem CreateTask(string text)
    {
        TaskItem taskItem = new TaskItem(text);
        taskItem.Toggle.RegisterValueChangedCallback(OnTaskValueChanged);

        return taskItem;
    }

    private void AddTask()
    {
        if (string.IsNullOrEmpty(_newTaskField.value))
        {
            return;
        }

        _taskScrollView.Add(CreateTask(_newTaskField.value));
        SaveTask(_newTaskField.value);

        _newTaskField.value = string.Empty;
        _newTaskField.Focus();

        UpdateProgressBar();
        UpdateNotification("Task added successfully!");
    }

    private void AddTask(KeyDownEvent keyDownEvent)
    {
        if (Event.current.Equals(Event.KeyboardEvent("Return")))
        {
            AddTask();
        }
    }

    private void LoadTasks()
    {
        _taskListSO = _savedTasksObjectField.value as TaskListSO;

        if (_taskListSO == null)
        {
            UpdateNotification("Failed to load task list!");
            return;
        }

        _taskScrollView.Clear();
        List<string> taskList = _taskListSO.GetTasks();

        foreach (string task in taskList)
        {
            _taskScrollView.Add(CreateTask(task));
        }

        UpdateProgressBar();
        UpdateNotification(_taskListSO.name + " successfully loaded!");
    }

    private void SaveTask(string task)
    {
        if (_taskListSO == null)
        {
            return;
        }

        _taskListSO.AddTask(task);

        SaveTaskListSO();
    }

    private void SaveTaskListSO()
    {
        if (_taskListSO == null)
        {
            return;
        }

        EditorUtility.SetDirty(_taskListSO);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void SaveProgress()
    {
        if (_taskListSO == null)
        {
            return;
        }

        List<string> tasks = GetUncompletedTasks();
        _taskListSO.SetTasks(tasks);

        SaveTaskListSO();
        LoadTasks();
        UpdateNotification("Tasks saved successfully!");
    }

    private List<string> GetUncompletedTasks()
    {
        List<string> tasks = new List<string>();

        foreach (TaskItem taskItem in _taskScrollView.Children())
        {
            if (!taskItem.Toggle.value)
            {
                tasks.Add(taskItem.Label.text);
            }
        }

        return tasks;
    }

    private void UpdateProgressBar()
    {
        float value = GetProgress() * 100f;

        _taskProgressBar.value = value;
        _taskProgressBar.title = Mathf.Round(value) + "%";
    }

    private float GetProgress()
    {
        int taskCount = _taskScrollView.childCount;
        int completedTaskCount = 0;

        foreach (TaskItem taskItem in _taskScrollView.Children())
        {
            if (taskItem.Toggle.value)
            {
                completedTaskCount++;
            }
        }

        float value;
        if (taskCount > 0)
        {
            value = (float)completedTaskCount / taskCount;
        }
        else
        {
            value = 1f;
        }

        return value;
    }

    private void UpdateNotification(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            if (_notification != null && _container.Contains(_notification))
            {
                _container.Remove(_notification);
            }
        }
        else if (_notification != null)
        {
            if (!_container.Contains(_notification))
            {
                _container.Add(_notification);
            }

            _notification.Message = message;
        }
        else
        {
            _notification = new Notification(message);
            _container.Add(_notification);
        }
    }

    private void OnSearchFieldValueChanged(ChangeEvent<string> changeEvent)
    {
        foreach (TaskItem taskItem in _taskScrollView.Children())
        {
            string searchValue = _searchField.value.ToLower();
            string taskToLower = taskItem.Label.text.ToLower();

            if (!string.IsNullOrEmpty(searchValue) && taskToLower.Contains(searchValue))
            {
                taskItem.AddToClassList("highlight");
            }
            else
            {
                taskItem.RemoveFromClassList("highlight");
            }
        }
    }

    private void OnTaskValueChanged(ChangeEvent<bool> changeEvent)
    {
        UpdateProgressBar();

        if (_taskScrollView.childCount > 0 && GetProgress() > 0f)
        {
            UpdateNotification("Progress updated. Don't forget to save!");
        }
        else
        {
            UpdateNotification(string.Empty);
        }
    }
}
