using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using KontrolluCokus.Interaction;
using KontrolluCokus.Inventory;
using KontrolluCokus.DNA;

namespace KontrolluCokus.Editor
{
    /// <summary>
    /// Builds the temp-sprite prototype on top of the lab scene:
    /// second room + doors, close-up UI, inventory UI, DNA puzzle drugs, scanner and DNA HUD.
    /// Safe to re-run: it removes what it created before rebuilding.
    /// Run from menu: Tools > Build Prototype
    /// </summary>
    public static class PrototypeSceneBuilder
    {
        private const string SpritePath = "Assets/Sprites/Temp/WhiteSquare.png";
        private const string ItemsFolder = "Assets/Items";
        private const float Room2OffsetX = 20f;

        private static Sprite white;
        private static Font font;

        [MenuItem("Tools/Build Prototype")]
        public static void Build()
        {
            white = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (white == null)
            {
                Debug.LogError("[Prototype] WhiteSquare sprite missing, run Tools > Setup Lab Scene first.");
                return;
            }
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            foreach (var n in new[] { "UI_Canvas", "EventSystem", "Room_Storage", "Door_ToStorage", "Spawn_Lab" })
            {
                var old = GameObject.Find(n);
                if (old != null) Object.DestroyImmediate(old);
            }

            // --- DRUGS (DNA puzzle) ---
            // Wife's DNA (DNASystem default): A=Mor, B=Sari, C=Turuncu, D=Mavi
            // One solution: Kirmizi + Mavi (A mor, D mavi), Kizil Damla -> Altin Serum (C turuncu), Konsantre Sari (B sari)
            foreach (var oldItem in new[] { "Serum_A", "Serum_B", "Sedative", "Lab_Note" })
                AssetDatabase.DeleteAsset($"{ItemsFolder}/{oldItem}.asset");

            var red = CreateDrug("Drug_Red", "Kırmızı Serum", "Parlak kırmızı bir sıvı.", new Color(0.9f, 0.2f, 0.2f),
                Fx(0, GeneColor.Kirmizi));
            var blue = CreateDrug("Drug_Blue", "Mavi Serum", "Soğuk, mavi bir sıvı.", new Color(0.2f, 0.4f, 0.95f),
                Fx(0, GeneColor.Mavi), Fx(3, GeneColor.Mavi));
            var crimson = CreateDrug("Drug_Crimson", "Kızıl Damla (Konsantre)", "Koyu kızıl. Etiket: KONSANTRE.", new Color(0.6f, 0.05f, 0.1f),
                Fx(2, GeneColor.Kirmizi, true), Fx(1, GeneColor.Kirmizi, true));
            var gold = CreateDrug("Drug_Gold", "Altın Serum", "Altın sarısı, yoğun bir sıvı.", new Color(0.95f, 0.75f, 0.2f),
                Fx(1, GeneColor.Sari), Fx(2, GeneColor.Sari));
            var yellow = CreateDrug("Drug_Yellow", "Sarı Serum (Konsantre)", "Soluk sarı. Etiket: KONSANTRE.", new Color(1f, 0.95f, 0.5f),
                Fx(1, GeneColor.Sari, true));
            var night = CreateDrug("Drug_Night", "Gece Mavisi", "Neredeyse siyah, koyu mavi.", new Color(0.1f, 0.12f, 0.35f),
                Fx(1, GeneColor.Mavi), Fx(2, GeneColor.Mavi));
            var antidote = CreateDrug("Drug_Antidote", "Panzehir", "Tüm genleri griye döndürür.", new Color(0.9f, 0.9f, 0.9f));
            antidote.resetsDNA = true;
            EditorUtility.SetDirty(antidote);
            AssetDatabase.SaveAssets();

            // --- LAB OBJECTS ---
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            MakeContainer(GameObject.Find("Cabinet"), "İlaç Dolabı", new[] { red, blue }, true);
            MakeContainer(GameObject.Find("Shelf"), "Raf", new[] { yellow }, true);
            MakeContainer(GameObject.Find("Desk"), "Masa", new[] { antidote }, true);
            MakeScanner(GameObject.Find("ChemTable"), "DNA Tarayıcı");

            // --- SECOND ROOM ---
            var room = new GameObject("Room_Storage");
            float x0 = Room2OffsetX;
            Quad("Storage_Floor", room.transform, new Vector2(x0, 0), new Vector2(14, 10), new Color(0.15f, 0.2f, 0.2f), -10, false);
            var wallColor = new Color(0.25f, 0.3f, 0.3f);
            Quad("Storage_Wall_Top", room.transform, new Vector2(x0, 4.5f), new Vector2(14, 0.5f), wallColor, 2, true);
            Quad("Storage_Wall_Bottom", room.transform, new Vector2(x0, -4.5f), new Vector2(14, 0.5f), wallColor, 2, true);
            Quad("Storage_Wall_Left", room.transform, new Vector2(x0 - 6.25f, 0), new Vector2(0.5f, 10), wallColor, 2, true);
            Quad("Storage_Wall_Right", room.transform, new Vector2(x0 + 6.25f, 0), new Vector2(0.5f, 10), wallColor, 2, true);

            var fridge = Quad("Medicine_Fridge", room.transform, new Vector2(x0 + 3, 3), new Vector2(1.2f, 1.8f), new Color(0.85f, 0.9f, 0.95f), 1, true);
            fridge.layer = interactableLayer;
            MakeContainer(fridge, "Soğuk Depo", new[] { crimson, gold, night }, true);

            // --- DOORS ---
            var spawnLab = new GameObject("Spawn_Lab");
            spawnLab.transform.position = new Vector3(4.8f, 0, 0);
            var spawnStorage = new GameObject("Spawn_Storage");
            spawnStorage.transform.SetParent(room.transform);
            spawnStorage.transform.position = new Vector3(x0 - 4.8f, 0, 0);

            var doorColor = new Color(0.55f, 0.4f, 0.25f);
            MakeDoor(Quad("Door_ToStorage", null, new Vector2(5.8f, 0), new Vector2(0.4f, 1.2f), doorColor, 3, true), "Depoya git", spawnStorage.transform);
            MakeDoor(Quad("Door_ToLab", room.transform, new Vector2(x0 - 5.8f, 0), new Vector2(0.4f, 1.2f), doorColor, 3, true), "Laboratuvara dön", spawnLab.transform);

            // --- DNA SYSTEM ---
            var gm = GameObject.Find("GameManager");
            if (gm != null)
            {
                var dna = gm.GetComponent<DNASystem>();
                if (dna == null) dna = gm.AddComponent<DNASystem>();
                var dso = new SerializedObject(dna);
                var pool = dso.FindProperty("drugPool");
                var drugs = new[] { red, blue, crimson, gold, yellow, night };
                pool.arraySize = drugs.Length;
                for (int i = 0; i < drugs.Length; i++) pool.GetArrayElementAtIndex(i).objectReferenceValue = drugs[i];
                dso.ApplyModifiedPropertiesWithoutUndo();
            }

            // --- UI ---
            BuildUI();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Prototype] Build complete.");
        }

        private static void BuildUI()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var canvasGO = new GameObject("UI_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var canvas = canvasGO.transform;

            // Controls hint
            var hint = MakeText("Hint", canvas, "WASD: yürü   E / tıkla: etkileşim   ESC: geri   Slota tıkla: enjekte et", 24, TextAnchor.UpperLeft);
            Place(hint.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(470, -40), new Vector2(900, 40));

            // Inventory bar (bottom center)
            var bar = MakeImage("InventoryBar", canvas, new Color(0, 0, 0, 0.5f));
            bar.rectTransform.pivot = new Vector2(0.5f, 0);
            Place(bar.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(300, 160));
            var slot1 = MakeSlot("Slot1", bar.transform, -70);
            var slot2 = MakeSlot("Slot2", bar.transform, 70);
            var invUI = bar.gameObject.AddComponent<InventoryUI>();
            var so = new SerializedObject(invUI);
            so.FindProperty("slot1Icon").objectReferenceValue = slot1.icon;
            so.FindProperty("slot2Icon").objectReferenceValue = slot2.icon;
            so.FindProperty("slot1Button").objectReferenceValue = slot1.button;
            so.FindProperty("slot2Button").objectReferenceValue = slot2.button;
            so.FindProperty("slot1Label").objectReferenceValue = slot1.label;
            so.FindProperty("slot2Label").objectReferenceValue = slot2.label;
            so.FindProperty("emptySlotSprite").objectReferenceValue = white;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Close-up panel (full screen)
            var panel = MakeImage("CloseUpPanel", canvas, new Color(0.05f, 0.05f, 0.08f, 0.92f));
            Stretch(panel.rectTransform);

            var title = MakeText("ObjectName", panel.transform, "Obje", 48, TextAnchor.MiddleCenter);
            Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(1000, 80));

            var closeUp = MakeImage("CloseUpImage", panel.transform, Color.white);
            closeUp.sprite = white;
            Place(closeUp.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-250, 0), new Vector2(560, 700));

            var list = new GameObject("ItemList", typeof(RectTransform), typeof(VerticalLayoutGroup));
            list.transform.SetParent(panel.transform, false);
            Place((RectTransform)list.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(380, 100), new Vector2(500, 400));
            var vlg = list.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 16;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            var template = MakeButton("ItemButtonTemplate", list.transform, "Al: Eşya", new Color(0.25f, 0.45f, 0.3f));
            ((RectTransform)template.transform).sizeDelta = new Vector2(500, 70);

            var info = MakeText("InfoText", panel.transform, "", 30, TextAnchor.UpperCenter);
            Place(info.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(380, -120), new Vector2(620, 340));

            var back = MakeButton("BackButton", panel.transform, "Geri (ESC)", new Color(0.5f, 0.2f, 0.2f));
            Place((RectTransform)back.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(320, 80));

            template.gameObject.SetActive(false);
            panel.gameObject.SetActive(false);

            BuildDNAUI(canvas);

            var manager = Object.FindAnyObjectByType<InteractionManager>();
            if (manager == null)
            {
                Debug.LogError("[Prototype] No InteractionManager in scene.");
                return;
            }
            var mso = new SerializedObject(manager);
            mso.FindProperty("closeUpPanel").objectReferenceValue = panel.gameObject;
            mso.FindProperty("closeUpImage").objectReferenceValue = closeUp;
            mso.FindProperty("backButton").objectReferenceValue = back;
            mso.FindProperty("objectNameText").objectReferenceValue = title;
            mso.FindProperty("itemListParent").objectReferenceValue = list.transform;
            mso.FindProperty("itemButtonTemplate").objectReferenceValue = template;
            mso.FindProperty("infoText").objectReferenceValue = info;
            mso.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildDNAUI(Transform canvas)
        {
            // Distortion overlay sits under everything else so UI stays readable
            var overlay = MakeImage("CollapseOverlay", canvas, new Color(0.5f, 0f, 0.15f, 0f));
            overlay.raycastTarget = false;
            Stretch(overlay.rectTransform);
            overlay.transform.SetAsFirstSibling();

            // DNA HUD (top right)
            var hud = MakeImage("DNA_HUD", canvas, new Color(0, 0, 0, 0.5f));
            hud.rectTransform.pivot = new Vector2(1, 1);
            Place(hud.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(360, 150));
            var hudTitle = MakeText("Title", hud.transform, "DNA'n", 24, TextAnchor.MiddleLeft);
            Place(hudTitle.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(90, -22), new Vector2(160, 30));
            var dose = MakeText("DoseText", hud.transform, "Doz: 0", 24, TextAnchor.MiddleRight);
            Place(dose.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-90, -22), new Vector2(160, 30));

            var genes = new Image[DNASystem.GENE_COUNT];
            for (int i = 0; i < DNASystem.GENE_COUNT; i++)
            {
                float x = -127.5f + i * 85f;
                var g = MakeImage("Gene_" + DNASystem.GeneNames[i], hud.transform, GeneColors.ToColor(GeneColor.Gri));
                g.sprite = white;
                Place(g.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, -5), new Vector2(65, 65));
                var l = MakeText("Label", g.transform, DNASystem.GeneNames[i], 20, TextAnchor.MiddleCenter);
                Place(l.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -16), new Vector2(60, 26));
                genes[i] = g;
            }

            // Last injection result, under the HUD
            var effect = MakeText("EffectText", canvas, "", 24, TextAnchor.UpperRight);
            effect.rectTransform.pivot = new Vector2(1, 1);
            Place(effect.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -180), new Vector2(900, 70));

            // End screen (win / collapse)
            var end = MakeImage("EndPanel", canvas, new Color(0, 0, 0, 0.9f));
            Stretch(end.rectTransform);
            var endTitle = MakeText("EndTitle", end.transform, "", 64, TextAnchor.MiddleCenter);
            Place(endTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(1400, 100));
            var endBody = MakeText("EndBody", end.transform, "", 32, TextAnchor.MiddleCenter);
            Place(endBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(1400, 80));
            var restart = MakeButton("RestartButton", end.transform, "Tekrar Dene", new Color(0.3f, 0.3f, 0.4f));
            Place((RectTransform)restart.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(320, 80));
            end.gameObject.SetActive(false);

            var ui = hud.gameObject.AddComponent<DNAUI>();
            var so = new SerializedObject(ui);
            var arr = so.FindProperty("geneImages");
            arr.arraySize = genes.Length;
            for (int i = 0; i < genes.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = genes[i];
            so.FindProperty("doseText").objectReferenceValue = dose;
            so.FindProperty("effectText").objectReferenceValue = effect;
            so.FindProperty("distortionOverlay").objectReferenceValue = overlay;
            so.FindProperty("endPanel").objectReferenceValue = end.gameObject;
            so.FindProperty("endTitle").objectReferenceValue = endTitle;
            so.FindProperty("endBody").objectReferenceValue = endBody;
            so.FindProperty("restartButton").objectReferenceValue = restart;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- helpers ----------

        private static GeneEffect Fx(int gene, GeneColor color, bool overwrite = false)
        {
            return new GeneEffect { gene = gene, color = color, overwrite = overwrite };
        }

        private static ItemData CreateDrug(string file, string name, string desc, Color color, params GeneEffect[] effects)
        {
            var item = CreateItem(file, name, desc, ItemType.Medicine, true, file, color);
            item.geneEffects = effects;
            item.resetsDNA = false;
            EditorUtility.SetDirty(item);
            return item;
        }

        private static void MakeScanner(GameObject go, string displayName)
        {
            if (go == null) return;
            if (go.GetComponent<ScannerInteractable>() == null)
            {
                var plain = go.GetComponent<Interactable>();
                if (plain != null) Object.DestroyImmediate(plain);
                go.AddComponent<ScannerInteractable>();
            }
            SetName(go, displayName);
        }

        private static ItemData CreateItem(string file, string name, string desc, ItemType type, bool consumable, string effect, Color color)
        {
            if (!AssetDatabase.IsValidFolder(ItemsFolder))
                AssetDatabase.CreateFolder("Assets", "Items");
            string path = $"{ItemsFolder}/{file}.asset";
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemData>();
                AssetDatabase.CreateAsset(item, path);
            }
            item.itemName = name;
            item.description = desc;
            item.itemType = type;
            item.isConsumable = consumable;
            item.effectId = effect;
            item.placeholderColor = color;
            EditorUtility.SetDirty(item);
            AssetDatabase.SaveAssets();
            return item;
        }

        private static void SetName(GameObject go, string displayName)
        {
            if (go == null) return;
            var inter = go.GetComponent<Interactable>();
            if (inter == null) return;
            var so = new SerializedObject(inter);
            so.FindProperty("interactableName").stringValue = displayName;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void MakeContainer(GameObject go, string displayName, ItemData[] items, bool infinite)
        {
            if (go == null) return;
            var container = go.GetComponent<ContainerInteractable>();
            if (container == null)
            {
                var plain = go.GetComponent<Interactable>();
                if (plain != null) Object.DestroyImmediate(plain);
                container = go.AddComponent<ContainerInteractable>();
            }
            var so = new SerializedObject(container);
            so.FindProperty("interactableName").stringValue = displayName;
            so.FindProperty("infiniteSupply").boolValue = infinite;
            var arr = so.FindProperty("containedItems");
            arr.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void MakeDoor(GameObject go, string displayName, Transform target)
        {
            go.layer = LayerMask.NameToLayer("Interactable");
            var door = go.AddComponent<DoorInteractable>();
            var so = new SerializedObject(door);
            so.FindProperty("interactableName").stringValue = displayName;
            so.FindProperty("opensCloseUp").boolValue = false;
            so.FindProperty("targetSpawnPoint").objectReferenceValue = target;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject Quad(string name, Transform parent, Vector2 pos, Vector2 size, Color color, int order, bool collider)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = white;
            sr.color = color;
            sr.sortingOrder = order;
            if (collider) go.AddComponent<BoxCollider2D>();
            return go;
        }

        private struct Slot { public Button button; public Image icon; public Text label; }

        private static Slot MakeSlot(string name, Transform parent, float x)
        {
            var bg = MakeImage(name, parent, new Color(1, 1, 1, 0.15f));
            Place(bg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 15), new Vector2(110, 110));
            var button = bg.gameObject.AddComponent<Button>();
            var icon = MakeImage("Icon", bg.transform, Color.white);
            icon.sprite = white;
            icon.raycastTarget = false;
            Place(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80, 80));
            var label = MakeText("Label", bg.transform, "Boş", 18, TextAnchor.MiddleCenter);
            Place(label.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -22), new Vector2(140, 30));
            return new Slot { button = button, icon = icon, label = label };
        }

        private static Image MakeImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        private static Text MakeText(string name, Transform parent, string text, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = Color.white;
            t.raycastTarget = false;
            return t;
        }

        private static Button MakeButton(string name, Transform parent, string label, Color color)
        {
            var img = MakeImage(name, parent, color);
            var button = img.gameObject.AddComponent<Button>();
            var text = MakeText("Label", img.transform, label, 30, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
