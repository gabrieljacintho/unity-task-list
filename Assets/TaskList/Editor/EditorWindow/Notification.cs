using UnityEditor;
using UnityEngine.UIElements;

public class Notification : VisualElement
{
    private Label _label;

    public Label Label => _label;
    public string Message
    {
        get => _label.text;
        set => _label.text = value;
    }


    public Notification(string message)
    {
        VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TaskListEditor.Path + "/Notification.uxml");
        Add(original.Instantiate());

        _label = this.Q<Label>("label");
        _label.text = message;
    }
}
