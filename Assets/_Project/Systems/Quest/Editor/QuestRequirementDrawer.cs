using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(QuestRequirement))]
public class QuestRequirementDrawer : PropertyDrawer
{
    private const float VerticalSpacing = 2f;

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty typeProperty =
            property.FindPropertyRelative("type");

        SerializedProperty itemProperty =
            property.FindPropertyRelative("item");

        SerializedProperty noteProperty =
            property.FindPropertyRelative("note");

        SerializedProperty amountProperty =
            property.FindPropertyRelative("amount");

        Rect typeRect = new(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(typeRect, typeProperty, label);

        Rect valueRect = new(
            position.x,
            typeRect.yMax + VerticalSpacing,
            position.width,
            EditorGUIUtility.singleLineHeight);

        QuestRequirementType requirementType =
            (QuestRequirementType)typeProperty.enumValueIndex;

        if (requirementType == QuestRequirementType.Item)
        {
            const float amountWidth = 68f;
            const float gap = 6f;

            Rect itemRect = new(
                valueRect.x,
                valueRect.y,
                valueRect.width - amountWidth - gap,
                valueRect.height);

            Rect amountRect = new(
                itemRect.xMax + gap,
                valueRect.y,
                amountWidth,
                valueRect.height);

            EditorGUI.PropertyField(itemRect, itemProperty, GUIContent.none);
            EditorGUI.PropertyField(amountRect, amountProperty, GUIContent.none);
        }
        else
        {
            EditorGUI.PropertyField(valueRect, noteProperty, GUIContent.none);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(
        SerializedProperty property,
        GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2f + VerticalSpacing;
    }
}
