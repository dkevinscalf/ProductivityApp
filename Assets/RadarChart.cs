using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class RadarChart : MonoBehaviour
{
    public static RadarChart Instance;
    public Rect ChartBounds;
    public Transform GraphContainer;
    public GameObject IconPrefab;
    public GameObject pointPrefab; // Prefab for the points (UI Image, for example)
    public GameObject linePrefab; // Prefab for the line (using a LineRenderer component)
    public float maxScore = 5; // The maximum value for a category
    public List<RadarCategory> Categories; // List of category values (0 to maxScore)
    public UIDocument uiDocument; // Reference to the UI Document
    public int NumberOfDays = 30;

    private List<float> categoryValues;
    private int numberOfCategories = 7; // We have 7 categories
    public float chartRadius = 1f; // Radius of the radar chart
    public float IconRadius = 1f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    { 
        gameObject.SetActive(false);
    }

    private void ReturnToMenu(ClickEvent evt)
    {
        MainMenu.ReturnToMenu(this.gameObject);
    }

    public void CreateChart()
    {
        var root = uiDocument.rootVisualElement;
        var backbutton = root.Q<Button>();
        uiDocument.rootVisualElement.Q<Button>("BackButton").RegisterCallback<ClickEvent>(ReturnToMenu);
        CalculateCategoryValues();
        categoryValues = Categories.Select(o => o.Value + 1).ToList();
        numberOfCategories = categoryValues.Count;
        maxScore = categoryValues.Max();
        CreateRadarChart(categoryValues);
    }

    private void CalculateCategoryValues()
    {
        foreach(var category in Categories)
        {
            var totalPoints = TaskLibrary.Instance.UserProfile.Entries
                .Where(j => j.EntryDate >= DateTime.Today.AddDays(-NumberOfDays)) // Filter entries from the last 30 days
                .SelectMany(j => j.Tasks) // Flatten the task lists from each entry
                .Where(t => t.CategoryClass == category.Name && t.Complete) // Filter tasks by category and completion status
                .Sum(t => t.Points);
            category.Value = totalPoints;
        }
    }

    // Method to create radar chart based on given values
    void CreateRadarChart(List<float> values)
    {
        foreach(Transform child in GraphContainer)
        {
            Destroy(child.gameObject);
        }
        // Angle between each point (category)
        float angleStep = 360f / numberOfCategories;

        // Array to store points (positions of each category)
        Vector2[] points = new Vector2[numberOfCategories];

        // Iterate over each category and create a point
        for (int i = 0; i < numberOfCategories; i++)
        {
            // Calculate the angle in radians
            float angle = i * angleStep * Mathf.Deg2Rad;

            // Calculate normalized value (0 to 1 based on maxScore)
            float normalizedValue = Mathf.Clamp(values[i], 0, maxScore) / maxScore;

            // Determine the point's position based on the radius
            float pointRadius = chartRadius * normalizedValue;
            float xPosition = Mathf.Cos(angle) * pointRadius;
            float yPosition = Mathf.Sin(angle) * pointRadius;

            float iconX = Mathf.Cos(angle)* IconRadius;
            float iconY = Mathf.Sin(angle) * IconRadius;
            var icon = Instantiate(IconPrefab, new Vector3(iconX, iconY), Quaternion.identity, GraphContainer);
            icon.GetComponent<SpriteRenderer>().sprite = Categories[i].Icon;

            // Store the calculated position in the points array
            points[i] = new Vector2(xPosition, yPosition);

            // Instantiate the point prefab and set its position in the chart
            GameObject point = Instantiate(pointPrefab, GraphContainer);
            point.transform.position = points[i];
        }

        // After placing the points, draw lines to connect them
        DrawRadarLines(points);
    }

    // Method to draw lines between the points
    void DrawRadarLines(Vector2[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            // Get the start and end points for the line
            Vector2 startPoint = points[i];
            Vector2 endPoint = points[(i + 1) % points.Length]; // Connect last point to the first

            // Instantiate a line prefab and set its start/end points
            GameObject line = Instantiate(linePrefab, GraphContainer);
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

            // Set the positions for the LineRenderer
            Vector3[] positions = new Vector3[2];
            positions[0] = new Vector3(startPoint.x, startPoint.y, 0);
            positions[1] = new Vector3(endPoint.x, endPoint.y, 0);

            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(positions);
        }
    }
}

[Serializable]
public class RadarCategory
{
    public string Name;
    public Sprite Icon;
    public float Value;
}