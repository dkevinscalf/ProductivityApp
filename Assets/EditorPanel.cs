using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorPanel : MonoBehaviour
{
    public static EditorPanel Instance;
    public string SelectedCategory = "Cat1"; // Category to filter tasks

    public UIDocument uiDocument; // Reference to the UI Document
    public VisualElement container; // The parent element to hold the toggles
    private string categoryName;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowCategory(string category)
    {
        SelectedCategory = category;
        DisplayTasks(category);
    }

    private void DisplayTasks(string selectedCategory)
    {
        // Filter tasks based on the selected category
        List<Tasktivity> filteredTasks = TaskLibrary.Instance.Tasktivities.Tasktivities
            .Where(t => t.CategoryClass == selectedCategory.ToString())
            .ToList();

        var window = uiDocument.rootVisualElement.Q<ScrollView>();
        window.AddToClassList(SelectedCategory.ToString());
        // Get the container for displaying tasks
        container = uiDocument.rootVisualElement.Q<VisualElement>("TaskContainer");
        container.Clear(); // Clear any existing toggles

        var categoryTextField = uiDocument.rootVisualElement.Q<TextField>("CategoryName");
        categoryName = filteredTasks.FirstOrDefault(o => o.CategoryClass == selectedCategory).Category;
        categoryTextField.value = categoryName;
        categoryTextField.RegisterCallback<ChangeEvent<string>>(ChangeCategoryName);

        // Create toggles for each filtered task
        foreach (var task in filteredTasks)
        {
            CreateTask(task);
        }

        uiDocument.rootVisualElement.Q<Button>("BackButton").RegisterCallback<ClickEvent>(ReturnToMenu);
        uiDocument.rootVisualElement.Q<Button>("NewTask").RegisterCallback<ClickEvent>(NewTask);
    }

    private void ChangeCategoryName(ChangeEvent<string> evt)
    {
        foreach(var task in TaskLibrary.Instance.Tasktivities.Tasktivities.Where(o=>o.CategoryClass == SelectedCategory))
        {
            task.Category = evt.newValue;
        }

        foreach (var task in TaskLibrary.Instance.UserProfile.Entries.SelectMany(o=>o.Tasks).Where(o=>o.CategoryClass == SelectedCategory))
        {
            task.Category = evt.newValue;
        }

        TaskLibrary.Instance.SaveUserProfile();
    }

    private void ReturnToMenu(ClickEvent evt)
    {
        MainMenu.ReturnToMenu(this.gameObject);
    }

    private void NewTask(ClickEvent evt)
    {
        var newTask = new Tasktivity
        {
            Points = 1,
            Category = categoryName,
            CategoryClass = SelectedCategory,
            Name = "New Task"
        };

        TaskLibrary.Instance.Tasktivities.Tasktivities.Add(newTask);
        CreateTask(newTask);
    }

    private void CreateTask(Tasktivity task)
    {
        var taskBox = new VisualElement();
        taskBox.AddToClassList("TaskEdit");

        var taskDescription = new TextField();
        taskDescription.value = task.Name;
        taskDescription.RegisterCallback<ChangeEvent<string>, Tasktivity>(SetTaskName, task);
        taskBox.Add(taskDescription);

        var taskPoints = new TextField("Points");
        taskPoints.value = task.PointsString;
        taskPoints.RegisterCallback<ChangeEvent<string>, Tasktivity>(SetTaskPoints, task);

        taskBox.Add(taskPoints);
        container.Add(taskBox);
    }

    private void SetTaskPoints(ChangeEvent<string> evt, Tasktivity task)
    {
        task.PointsString = evt.newValue;
        Tasktivity libraryTask = TaskLibrary.Instance.Tasktivities.Find(task);
        libraryTask.Points = task.Points;
        TaskLibrary.Instance.SaveUserProfile();
    }

    private void SetTaskName(ChangeEvent<string> evt, Tasktivity task)
    {
        task.Name = evt.newValue;
        Tasktivity libraryTask = TaskLibrary.Instance.Tasktivities.Find(task);
        libraryTask.Name = task.Name;
        TaskLibrary.Instance.SaveUserProfile();
    }
}
