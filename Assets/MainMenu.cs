using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    public UIDocument uiDocument; // Reference to the UI Document
    public VisualElement container; // The parent element to hold the toggles
    public static MainMenu Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        BuildMenu();
    }

    private void BuildMenu()
    {
        uiDocument.rootVisualElement.Q<Button>().RegisterCallback<ClickEvent>(OpenStats);
        var root = uiDocument.rootVisualElement.Q<VisualElement>("MainMenu");
        root.Clear();
        for(var i = 1; i<8;i++)
        {
            var category = "Cat" + i;
            var categoryByName = TaskLibrary.Instance.Tasktivities.Tasktivities.FirstOrDefault(o => o.CategoryClass == category);
            var tile = new VisualElement();
            var icon = new VisualElement();

            icon.AddToClassList("ChakraIcon");
            icon.AddToClassList(category.ToString());

            var label = new Label();
            label.text = categoryByName.Category;

            tile.Add(icon);
            tile.Add(label);

            tile.AddToClassList("BGTile");
            tile.AddToClassList(category.ToString());

            tile.style.backgroundImage = null;

            tile.RegisterCallback<ClickEvent, string>(SelectCategory, category.ToString());

            root.Add(tile);
        }
    }

    private void OpenStats(ClickEvent evt)
    {
        RadarChart.Instance.gameObject.SetActive(true);
        RadarChart.Instance.CreateChart();
        this.gameObject.SetActive(false);
    }

    private void SelectCategory(ClickEvent evt, string category)
    {
        DailyTaskList.Instance.gameObject.SetActive(true);
        DailyTaskList.Instance.ShowCategory(category);
        this.gameObject.SetActive(false);
    }

    public static void ReturnToMenu(GameObject gameObject)
    {
        gameObject.SetActive(false);
        Instance.gameObject.SetActive(true);
        Instance.BuildMenu();
    }
}
