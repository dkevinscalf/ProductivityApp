using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TaskLibrary : MonoBehaviour
{
    public string TaskFilePath = "TaskList"; // Path to the JSON file without the extension
    public TaskLibraryObject Tasktivities; // Holds the list of tasks
    public UserProfile UserProfile;
    public static TaskLibrary Instance;

    private void Awake()
    {
        Instance = this;// Load tasks when the game starts
        StartCoroutine(LoadCR());
    }

    private IEnumerator LoadCR()
    {
        while (Instance == null)
        {
            yield return new WaitForEndOfFrame();
        }
        LoadUserProfile();
    }

    private void LoadUserProfile()
    {
        var json = PlayerPrefs.GetString("save");
        if(string.IsNullOrEmpty(json))
        {
            UserProfile = new UserProfile();
            LoadTasksFromFile(TaskFilePath);
            UserProfile.TasktivityLibrary = Tasktivities;
            UserProfile.LoadCurrentDay();
        }
        else
        {
            UserProfile = JsonUtility.FromJson<UserProfile>(json);
            UserProfile.LoadCurrentDay();
            Tasktivities = UserProfile.TasktivityLibrary;
        }
    }

    public void SaveUserProfile()
    {
        UserProfile.TasktivityLibrary = Tasktivities;
        var json = JsonUtility.ToJson(UserProfile);
        PlayerPrefs.SetString("save", json);
    }

    public void LoadTasksFromFile(string filePath)
    {
        // Load the JSON file from the Resources folder
        TextAsset jsonFile = Resources.Load<TextAsset>(filePath);

        if (jsonFile != null)
        {
            // Deserialize the JSON data into the TaskLibraryObject
            Tasktivities = JsonUtility.FromJson<TaskLibraryObject>(jsonFile.text);
        }
        else
        {
            Debug.LogError("Could not find JSON file: " + filePath);
        }
    }
}

[Serializable]
public class TaskLibraryObject
{
    public List<Tasktivity> Tasktivities; // Array to hold tasks

    public List<Tasktivity> DeepCopy()
    {
        return Tasktivities.Select(o => new Tasktivity
        {
            Category = o.Category,
            CategoryClass = o.CategoryClass,
            Name = o.Name,
            Points = o.Points,
            Description = o.Description,
            Complete = false
        }).ToList();
    }

    public Tasktivity Find(Tasktivity task)
    {
        return Tasktivities.FirstOrDefault(o => o.CategoryClass == task.CategoryClass && o.Name == task.Name);
    }
}

[Serializable]
public class Tasktivity
{
    public string Name; // Name of the task
    public string Category; // Category of the task
    public string CategoryClass;
    public int Points; // Points associated with the task
    public string PointsString { get { return Points.ToString(); } set { Points = int.Parse(value); } }
    public string Description; // Description of the task
    public bool Complete;
}

[Serializable]
public class UserProfile
{
    public List<JournalEntry> Entries;

    public TaskLibraryObject TasktivityLibrary;

    public static UserProfile Instance;

    public JournalEntry CurrentDay;

    public static void SetTask(string Name, bool complete)
    {
        Instance.LoadCurrentDay();
        var task = Instance.CurrentDay.Tasks.FirstOrDefault(o => o.Name == Name);
        if (task != null)
            task.Complete = complete;
        TaskLibrary.Instance.SaveUserProfile();
    }

    public UserProfile()
    {
        Entries = new List<JournalEntry>();
        Instance = this;
    }

    internal void LoadCurrentDay()
    {
        if(CurrentDay == null)
            CurrentDay = Entries.FirstOrDefault(o => o.EntryDate == DateTime.Today);
        if (CurrentDay == null)
        {
            //Logging.Instance.Log("NEW DAY");
            var newDay = new JournalEntry
            {
                EntryDate = DateTime.Today,
                Tasks = TaskLibrary.Instance.Tasktivities.DeepCopy()
            };
            Entries.Add(newDay);
            CurrentDay = newDay;
        }
        else if(CurrentDay.Tasks == null || !CurrentDay.Tasks.Any())
        {
            CurrentDay.Tasks = TaskLibrary.Instance.Tasktivities.DeepCopy();
        }
        LoadMissingTasks();
    }

    private void LoadMissingTasks()
    {
        foreach(var task in TaskLibrary.Instance.Tasktivities.DeepCopy())
        {
            var matchingTask = CurrentDay.Tasks.FirstOrDefault(o => o.Category == task.Category && o.Name == task.Name);
            if (matchingTask == null)
                CurrentDay.Tasks.Add(task);
        }
    }
}

[Serializable]
public class JournalEntry
{
    public JournalEntry() { Tasks = new List<Tasktivity>(); }
    public long DateTimeTicks;
    public DateTime EntryDate
    {
        get
        {
            return new DateTime(DateTimeTicks);
        }
        set
        {
            DateTimeTicks = value.Ticks;
        }
    }
    public List<Tasktivity> Tasks;

    public bool IsTaskComplete(string name)
    {
        var task = Tasks.FirstOrDefault(o => o.Name == name);
        if (task == null)
            return false;
        return task.Complete;
    }
}