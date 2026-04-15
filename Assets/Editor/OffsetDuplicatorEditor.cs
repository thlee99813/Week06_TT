using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OffsetDuplicator))]
public class OffsetDuplicatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var tool = (OffsetDuplicator)target;

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(tool.copyCount < 1))
        {
            if (GUILayout.Button("Generate Copies"))
            {
                GenerateCopies(tool);
            }
        }
    }

    private static void GenerateCopies(OffsetDuplicator tool)
    {
        GameObject source = tool.gameObject;

        if (PrefabUtility.IsPartOfPrefabAsset(source))
        {
            Debug.LogWarning("씬 오브젝트에 붙여서 사용");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Offset Duplicate");

        Transform parent = tool.keepSameParent ? source.transform.parent : null;
        Vector3 basePosition = source.transform.position;

        Vector3 step = tool.offsetSpace == Space.Self
            ? source.transform.TransformVector(tool.offset)
            : tool.offset;

        for (int i = 1; i <= tool.copyCount; i++)
        {
            GameObject clone = Object.Instantiate(source);
            Undo.RegisterCreatedObjectUndo(clone, "Create Offset Copy");

            clone.name = $"{source.name}{tool.nameSuffix}_{i:00}";
            clone.transform.SetParent(parent, true);
            clone.transform.position = basePosition + step * i;
        }

        Undo.CollapseUndoOperations(undoGroup);
    }
}
