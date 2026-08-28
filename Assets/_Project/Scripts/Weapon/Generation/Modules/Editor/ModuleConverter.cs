#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

// danke gemini

public static class ModuleConverter
{
    [MenuItem("CONTEXT/MonoBehaviour/Convert Module")]
    private static void OpenConvertMenu(MenuCommand command)
    {
        MonoBehaviour sourceComp = command.context as MonoBehaviour;
        if (sourceComp == null) return;

        Type sourceType = sourceComp.GetType();
        Type baseType = GetCustomBaseType(sourceType);
        var derivedTypes = TypeCache.GetTypesDerivedFrom(baseType);

        Rect mouseRect = new Rect(Event.current != null ? Event.current.mousePosition : Vector2.zero, Vector2.zero);

        EditorApplication.delayCall += () =>
        {
            if (sourceComp == null) return;

            GenericMenu menu = new GenericMenu();

            foreach (var targetType in derivedTypes)
            {
                if (targetType.IsAbstract || targetType == sourceType) continue;

                menu.AddItem(new GUIContent($"{baseType.Name}/{targetType.Name}"), false, () =>
                {
                    ConvertComponent(sourceComp, targetType);
                });
            }

            if (menu.GetItemCount() == 0)
            {
                EditorUtility.DisplayDialog("No Modules",
                    $"No classes for '{baseType.Name}' found.", "OK");
                return;
            }

            menu.DropDown(mouseRect);
        };
    }

    private static void ConvertComponent(MonoBehaviour source, Type targetType)
    {
        GameObject go = source.gameObject;

        Undo.RegisterCompleteObjectUndo(go, $"Convert {source.GetType().Name} to {targetType.Name}");

        string json = EditorJsonUtility.ToJson(source);
        Component newComp = Undo.AddComponent(go, targetType);
        EditorJsonUtility.FromJsonOverwrite(json, newComp);
        Undo.DestroyObjectImmediate(source);

        EditorUtility.SetDirty(go);
    }

    private static Type GetCustomBaseType(Type type)
    {
        Type current = type;
        while (current.BaseType != null &&
               current.BaseType != typeof(MonoBehaviour) &&
               current.BaseType != typeof(Behaviour) &&
               current.BaseType != typeof(Component))
        {
            current = current.BaseType;
        }
        return current;
    }
}
#endif