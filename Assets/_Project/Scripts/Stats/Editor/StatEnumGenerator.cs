using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class StatEnumGenerator
{
    [MenuItem("Tools/Generate Stat Enum")]
    public static void GenerateEnum()
    {
        string[] guids = AssetDatabase.FindAssets("t:StatTypeSO");

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// AUTO-GENERATED. DO NOT EDIT MANUALLY.");
        sb.AppendLine("public enum StatType");
        sb.AppendLine("{");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            StatTypeSO statSO = AssetDatabase.LoadAssetAtPath<StatTypeSO>(path);

            string safeName = statSO.statName.Replace(" ", "");
            sb.AppendLine($"    {safeName},");
        }

        sb.AppendLine("}");

        string filePath = Application.dataPath + "/_Project/Scripts/Stats/StatType.cs";
        File.WriteAllText(filePath, sb.ToString());

        AssetDatabase.Refresh();
        Debug.Log("StatType Enum erfolgreich generiert!");
    }
}