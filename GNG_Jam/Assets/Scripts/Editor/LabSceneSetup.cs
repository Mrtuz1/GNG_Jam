using UnityEngine;
using UnityEditor;

namespace KontrolluCokus.Editor
{
    /// <summary>
    /// Editor utility to set up placeholder sprites for all scene objects.
    /// Run from menu: Tools > Setup Lab Scene
    /// </summary>
    public static class LabSceneSetup
    {
        [MenuItem("Tools/Setup Lab Scene")]
        public static void SetupScene()
        {
            // Create a white square texture to use as base sprite
            Texture2D whiteTex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            whiteTex.SetPixels(pixels);
            whiteTex.Apply();

            // Save the texture as an asset
            string texPath = "Assets/Sprites/Temp";
            if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
                AssetDatabase.CreateFolder("Assets", "Sprites");
            if (!AssetDatabase.IsValidFolder(texPath))
                AssetDatabase.CreateFolder("Assets/Sprites", "Temp");

            byte[] pngData = whiteTex.EncodeToPNG();
            string filePath = texPath + "/WhiteSquare.png";
            System.IO.File.WriteAllBytes(Application.dataPath + "/../" + filePath, pngData);
            AssetDatabase.Refresh();

            // Configure the texture as a sprite
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 64;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }

            Sprite whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
            if (whiteSprite == null)
            {
                Debug.LogError("[LabSetup] Failed to load sprite!");
                return;
            }

            // --- PLAYER ---
            SetupObject("Player", whiteSprite, new Color(0.2f, 0.8f, 0.3f), // green scientist
                new Vector3(0.6f, 0.8f, 1f), new Vector2(0.5f, 0.7f), 5);

            // --- LAB FLOOR ---
            SetupObject("Lab_Floor", whiteSprite, new Color(0.18f, 0.18f, 0.22f), // dark floor
                new Vector3(14f, 10f, 1f), Vector2.zero, -10, false);

            // --- INTERACTABLE OBJECTS ---
            SetupObject("Cabinet", whiteSprite, new Color(0.45f, 0.3f, 0.2f), // brown cabinet
                new Vector3(1.2f, 1.8f, 1f), new Vector2(1f, 1.6f), 1);

            SetupObject("Desk", whiteSprite, new Color(0.5f, 0.35f, 0.2f), // wood desk
                new Vector3(2f, 1f, 1f), new Vector2(1.8f, 0.8f), 1);

            SetupObject("Shelf", whiteSprite, new Color(0.35f, 0.35f, 0.45f), // metal shelf
                new Vector3(0.8f, 1.8f, 1f), new Vector2(0.6f, 1.6f), 1);

            SetupObject("ChemTable", whiteSprite, new Color(0.6f, 0.6f, 0.65f), // steel table
                new Vector3(2f, 1f, 1f), new Vector2(1.8f, 0.8f), 1);

            // --- WALLS ---
            SetupObject("Wall_Top", whiteSprite, new Color(0.3f, 0.28f, 0.35f),
                new Vector3(14f, 0.5f, 1f), new Vector2(14f, 0.5f), 2, false);

            SetupObject("Wall_Bottom", whiteSprite, new Color(0.3f, 0.28f, 0.35f),
                new Vector3(14f, 0.5f, 1f), new Vector2(14f, 0.5f), 2, false);

            SetupObject("Wall_Left", whiteSprite, new Color(0.3f, 0.28f, 0.35f),
                new Vector3(0.5f, 10f, 1f), new Vector2(0.5f, 10f), 2, false);

            SetupObject("Wall_Right", whiteSprite, new Color(0.3f, 0.28f, 0.35f),
                new Vector3(0.5f, 10f, 1f), new Vector2(0.5f, 10f), 2, false);

            Debug.Log("[LabSetup] Scene setup complete! All sprites and colliders configured.");
        }

        private static void SetupObject(string objectName, Sprite sprite, Color color,
            Vector3 scale, Vector2 colliderSize, int sortingOrder, bool hasCollider = true)
        {
            GameObject obj = GameObject.Find(objectName);
            if (obj == null)
            {
                Debug.LogWarning($"[LabSetup] Could not find '{objectName}'");
                return;
            }

            // Set scale
            obj.transform.localScale = scale;

            // Set sprite and color
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = sprite;
                sr.color = color;
                sr.sortingOrder = sortingOrder;
            }

            // Set collider size (independent of scale)
            if (hasCollider)
            {
                BoxCollider2D col = obj.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    // Collider size is in local space, so divide by scale
                    col.size = new Vector2(
                        colliderSize.x / scale.x,
                        colliderSize.y / scale.y
                    );
                    col.offset = Vector2.zero;
                }
            }

            Debug.Log($"[LabSetup] Configured '{objectName}': scale={scale}, color={color}");
        }
    }
}