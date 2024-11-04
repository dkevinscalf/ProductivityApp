using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DailyTaskList : MonoBehaviour
{
    public static DailyTaskList Instance;
    public string SelectedCategory = "Cat1"; // Category to filter tasks

    public UIDocument uiDocument; // Reference to the UI Document
    public VisualTreeAsset toggleTemplate; // Assign your UXML template in the inspector
    public VisualElement container; // The parent element to hold the toggles

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

        // Get the container for displaying tasks
        container = uiDocument.rootVisualElement.Q<VisualElement>("TaskContainer");
        container.AddToClassList(SelectedCategory.ToString());
        container.Clear(); // Clear any existing toggles

        // Create toggles for each filtered task
        foreach (var task in filteredTasks)
        {
            CreateToggle(task);
        }

        uiDocument.rootVisualElement.Q<Button>("BackButton").RegisterCallback<ClickEvent>(ReturnToMenu);
        uiDocument.rootVisualElement.Q<Button>("EditButton").RegisterCallback<ClickEvent>(EditCategory);
    }

    private void ReturnToMenu(ClickEvent evt)
    {
        MainMenu.ReturnToMenu(this.gameObject);
    }

    private void EditCategory(ClickEvent evt)
    {
        EditorPanel.Instance.gameObject.SetActive(true);
        EditorPanel.Instance.ShowCategory(SelectedCategory);
        this.gameObject.SetActive(false);
    }

    private void CreateToggle(Tasktivity task)
    {
        var display = new Toggle();
        display.label = task.Name;
        container.Add(display);
        display.value = UserProfile.Instance.CurrentDay.IsTaskComplete(task.Name);
        display.RegisterValueChangedCallback(evt =>
        {
            bool isToggled = evt.newValue;  // evt.newValue gives you the current state (true or false)
            
            UserProfile.SetTask(task.Name, isToggled);
        });
    }
}
