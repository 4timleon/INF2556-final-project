using UnityEngine;
using UnityEditor;
using System.IO;

// danke gemini

public class QuickMaterialGenerator
{
    [MenuItem("Assets/Create/PBR Material From Textures", false, 20)]
    private static void CreateMaterialFromTextures()
    {
        var selectedObjects = Selection.objects;
        if (selectedObjects.Length == 0) return;

        // Nutzt URP Lit falls vorhanden, sonst Standard Shader
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material mat = new Material(shader);

        string targetFolder = "Assets";

        foreach (var obj in selectedObjects)
        {
            if (obj is Texture2D tex)
            {
                string path = AssetDatabase.GetAssetPath(tex);
                targetFolder = Path.GetDirectoryName(path);
                string name = tex.name.ToLower();

                // Albedo / BaseMap
                if (name.Contains("albedo") || name.Contains("basecolor") || name.Contains("diffuse") || name.Contains("_col"))
                {
                    mat.SetTexture("_BaseMap", tex);   // URP
                    mat.SetTexture("_MainTex", tex);   // Built-in
                    mat.name = tex.name.Split('_')[0] + "_Mat";
                }
                // Normal Map
                else if (name.Contains("normal") || name.Contains("_nrm") || name.Contains("_norm"))
                {
                    // Stellt Texture-Type automatisch auf Normal Map um
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null && importer.textureType != TextureImporterType.NormalMap)
                    {
                        importer.textureType = TextureImporterType.NormalMap;
                        importer.SaveAndReimport();
                    }
                    mat.SetTexture("_BumpMap", tex);
                    mat.EnableKeyword("_NORMALMAP");
                }
                // Metallic / Mask
                else if (name.Contains("metallic") || name.Contains("metal") || name.Contains("_met"))
                {
                    mat.SetTexture("_MetallicGlossMap", tex);
                }
                // Occlusion / AO
                else if (name.Contains("occlusion") || name.Contains("ao") || name.Contains("ambientocclusion"))
                {
                    mat.SetTexture("_OcclusionMap", tex);
                }
                // Emission
                else if (name.Contains("emission") || name.Contains("emit"))
                {
                    mat.SetTexture("_EmissionMap", tex);
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }

        string matPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{mat.name ?? "New_PBR_Mat"}.mat");
        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.SaveAssets();

        // Wählt das neu erstellte Material direkt aus
        Selection.activeObject = mat;
    }

    [MenuItem("Assets/Create/PBR Material From Textures", true)]
    private static bool ValidateCreateMaterial() => Selection.objects.Length > 0;
}