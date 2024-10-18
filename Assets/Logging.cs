using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Logging : MonoBehaviour
{
    public UIDocument uiDocument;
    public static Logging Instance;
    private void Awake()
    {
        Instance = this;
    }
    public void Log(string message, string className = "Log")
    {
        var label = new Label(message); // Pass the message to the label
        label.AddToClassList(className);
        uiDocument.rootVisualElement.Add(label);
    }

    public void LogError(string message) {
        Log(message, "Error");
    }
}
