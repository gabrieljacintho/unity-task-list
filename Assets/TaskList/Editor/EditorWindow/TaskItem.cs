using UnityEditor;
using UnityEngine.UIElements;

public class TaskItem : VisualElement
{
    private Toggle _toggle;
    private Label _label;

    public Toggle Toggle => _toggle;
    public Label Label => _label;


    public TaskItem(string text)
    {
        VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TaskListEditor.Path + "/TaskItem.uxml");
        Add(original.Instantiate());

        _toggle = this.Q<Toggle>("toggle");
        _label = this.Q<Label>("label");
        _label.text = text;
    }
}
