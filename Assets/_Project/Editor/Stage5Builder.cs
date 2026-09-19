using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using ButchersGames;
using Object = UnityEngine.Object;

public static class Stage5Builder
{
    const string MeshRoot = "Assets/ThirdParty/RunRich/Visual/Mesh/";
    const string Out = "Assets/_Project/Art/Stage5/";
    const string Prefabs = "Assets/_Project/Prefabs/";

    static Material atlas, doorMaterial, gold, orange, yellow, green, blue, white, dark, water;
    static readonly string[] GateNames = {"Gate_x2_Poor", "Gate_x3_Descent", "Gate_x4_Rich", "Gate_x5_Millionaire"};

    public static void Build()
    {
        Directory.CreateDirectory("Documentation/Stage5");
        Directory.CreateDirectory(Out);
        Directory.CreateDirectory(Prefabs + "Environment");
        Directory.CreateDirectory(Prefabs + "Obstacles");
        AssetDatabase.Refresh();
        atlas = Material("Atlas", Color.white, "atlas.png");
        doorMaterial = Material("Door", Color.white, "door_texture.png");
        gold = Material("Gold", new Color(1f, .68f, .055f));
        orange = Material("Orange", new Color(1f, .31f, .025f));
        yellow = Material("Yellow", new Color(1f, .85f, .035f));
        green = Material("Green", new Color(.19f, .8f, .16f));
        blue = Material("Blue", new Color(.13f, .3f, .75f));
        white = Material("White", new Color(.89f, .97f, 1f));
        dark = Material("Dark", new Color(.035f, .045f, .075f));
        water = Material("Water", new Color(.015f, .57f, .86f));
        BuildPickups();
        BuildChoice();
        BuildFlags();
        BuildObstacle();
        BuildGuard();
        for (int i = 0; i < 4; i++) BuildGate(i);
        DressLevel("Level_01");
        DressLevel("Level_02");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Validate();
        RenderResults();
        Debug.Log("STAGE5_BUILD_OK");
    }

    static Material Material(string name, Color color, string texture = null)
    {
        string path = Out + "M_" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", .12f);
        mat.SetFloat("_Metallic", 0);
        mat.SetFloat("_Cull", 0);
        mat.SetTexture("_BaseMap", texture == null ? null : AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/RunRich/Visual/Texture2D/" + texture));
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static GameObject Node(string name, Transform parent = null, Vector3 position = default)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        return go;
    }

    static GameObject Cube(string name, Transform parent, Vector3 pos, Vector3 size, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = size;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static Mesh Mesh(string name) => AssetDatabase.LoadAssetAtPath<Mesh>(MeshRoot + name + ".asset") ?? throw new Exception("Missing mesh " + name);

    static GameObject RawMesh(string name, Mesh mesh, Transform parent, Material mat)
    {
        var go = Node(name, parent);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterials = Enumerable.Repeat(mat, mesh.subMeshCount).ToArray();
        return go;
    }

    static GameObject Prop(string meshName, Transform parent, Vector3 pos, float height, Material mat = null, float yaw = 0)
    {
        var root = Node(meshName, parent, pos);
        var mesh = Mesh(meshName);
        var visual = RawMesh("Model", mesh, root.transform, mat ?? atlas);
        float scale = height / Mathf.Max(mesh.bounds.size.y, .001f);
        visual.transform.localScale = Vector3.one * scale;
        visual.transform.localPosition = new Vector3(-mesh.bounds.center.x, -mesh.bounds.min.y, -mesh.bounds.center.z) * scale;
        root.transform.localRotation = Quaternion.Euler(0, yaw, 0);
        return root;
    }

    static void Set(Object component, string field, Object value)
    {
        var so = new SerializedObject(component);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    static void SetInt(Object component, string field, int value)
    {
        var so = new SerializedObject(component);
        so.FindProperty(field).intValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    static void SetFloat(Object component, string field, float value)
    {
        var so = new SerializedObject(component);
        so.FindProperty(field).floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    static GameObject Save(GameObject go, string path)
    {
        var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return saved;
    }
    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--) Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }
    static void Label(Transform parent, string text, Vector3 position, Color color, float size = 5)
    {
        var go = Node("Label_" + text, parent, position);
        var label = go.AddComponent<TextMeshPro>();
        label.text = text;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.rectTransform.sizeDelta = new Vector2(4, 1);
        label.textWrappingMode = TextWrappingModes.NoWrap;
    }

    static void BuildPickups()
    {
        foreach (string name in new[] {"MoneyPickup", "AlcoholPickup"})
        {
            string path = Prefabs + "Pickups/" + name + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            ClearChildren(root.transform);
            string fbx = name == "MoneyPickup" ? "bills" : "bottle";
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(MeshRoot + "LowPoly/" + fbx + ".fbx");
            var visual = Object.Instantiate(source, root.transform);
            visual.name = "Visual";
            foreach (var r in visual.GetComponentsInChildren<Renderer>()) r.sharedMaterials = Enumerable.Repeat(atlas, r.sharedMaterials.Length).ToArray();
            var renderers = visual.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            float scale = .65f / Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            visual.transform.localScale *= scale;
            visual.transform.localPosition -= bounds.center * scale;
            var collider = root.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = .48f;
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void BuildChoice()
    {
        var root = Node("ChoiceGate_SchoolParty");
        var group = root.AddComponent<WealthChangeZoneGroup>();
        for (int side = 0; side < 2; side++)
        {
            var half = Node(side == 0 ? "Party" : "School", root.transform, new Vector3(side == 0 ? -1.25f : 1.25f, 0, 0));
            var mat = side == 0 ? orange : green;
            Cube("LeftPost", half.transform, new Vector3(-1.14f, 1.5f, 0), new Vector3(.16f, 3, .2f), mat);
            Cube("RightPost", half.transform, new Vector3(1.14f, 1.5f, 0), new Vector3(.16f, 3, .2f), mat);
            Cube("Sign", half.transform, new Vector3(0, 2.8f, 0), new Vector3(2.15f, .5f, .18f), dark);
            Label(half.transform, side == 0 ? "ВЕЧЕРИНКА" : "ШКОЛА", new Vector3(0, 2.8f, -.12f), Color.white, 2.5f);
            var zone = half.AddComponent<WealthChangeZone>();
            var collider = half.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0, 1.1f, 0);
            collider.size = new Vector3(2.3f, 2.2f, .5f);
            SetInt(zone, "amount", side == 0 ? -20 : 20);
            Set(zone, "group", group);
            if (side == 0)
            {
                Prop("Bottle.001", half.transform, new Vector3(-.45f, 1.65f, -.15f), .6f);
                Prop("Bottle.001", half.transform, new Vector3(.45f, 1.65f, -.15f), .6f, atlas, -20);
            }
            else
            {
                Prop("Hat_School", half.transform, new Vector3(.25f, 1.7f, -.15f), .32f);
                Prop("Diplome", half.transform, new Vector3(-.35f, 1.55f, -.15f), .17f);
            }
        }
        Save(root, Prefabs + "Gates/ChoiceGate_SchoolParty.prefab");
    }

    static void BuildObstacle()
    {
        var root = Node("TrashObstacle");
        Prop("Trash.000", root.transform, Vector3.zero, 1.05f);
        var col = root.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, .55f, 0);
        col.size = new Vector3(.85f, 1.1f, .85f);
        var zone = root.AddComponent<WealthChangeZone>();
        SetInt(zone, "amount", -10);
        Save(root, Prefabs + "Obstacles/TrashObstacle.prefab");
    }

    static void BuildFlags()
    {
        string path = Prefabs + "Checkpoints/FlagCheckpoint.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        var flags = root.GetComponent<FlagCheckpoint>();
        ClearChildren(root.transform);
        Cube("GoldStrip", root.transform, new Vector3(0,.012f,0), new Vector3(5,.024f,1.2f), yellow);
        Mesh original = Mesh("Flag");
        // Split the supplied mesh into static post and moving triangular cloth.
        Vector3[] vertices = original.vertices;
        var clothIndices = new System.Collections.Generic.List<int>();
        var postIndices = new System.Collections.Generic.List<int>();
        int[] triangles = original.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 center = (vertices[triangles[i]] + vertices[triangles[i+1]] + vertices[triangles[i+2]]) / 3f;
            var list = center.x < -.22f && center.y > 2.5f ? clothIndices : postIndices;
            list.Add(triangles[i]); list.Add(triangles[i+1]); list.Add(triangles[i+2]);
        }
        Mesh part(string name, System.Collections.Generic.List<int> indices)
        {
            var mesh = Object.Instantiate(original);
            mesh.name = name;
            mesh.subMeshCount = 1;
            mesh.SetTriangles(indices, 0);
            string assetPath = Out + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existing) { EditorUtility.CopySerialized(mesh, existing); Object.DestroyImmediate(mesh); return existing; }
            AssetDatabase.CreateAsset(mesh, assetPath);
            return mesh;
        }
        var postMesh = part("FlagPost", postIndices);
        var clothMesh = part("FlagCloth", clothIndices);
        var so = new SerializedObject(flags);
        var array = so.FindProperty("flags"); array.arraySize = 2;
        for (int i = 0; i < 2; i++)
        {
            var assembly = Node(i == 0 ? "LeftFlag" : "RightFlag", root.transform, new Vector3(i == 0 ? -2.05f : 2.05f, 0, 0));
            assembly.transform.localScale = new Vector3(i == 0 ? -.45f : .45f, .45f, .45f);
            RawMesh("Post", postMesh, assembly.transform, gold);
            var cloth = RawMesh("Cloth", clothMesh, assembly.transform, orange);
            cloth.transform.localPosition = new Vector3(0, -2.1f, 0);
            array.GetArrayElementAtIndex(i).objectReferenceValue = cloth.transform;
        }
        so.FindProperty("raiseHeight").floatValue = 2.1f;
        so.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void BuildGate(int stage)
    {
        var root = Node(GateNames[stage]);
        Material color = new[] {orange, yellow, green, gold}[stage];
        string frame = new[] {"Door_Poor_000", "Door_Descent_02", "Door_Rich_00", "Door_Descent_02"}[stage];
        Prop(frame, root.transform, Vector3.zero, stage == 2 ? 4.4f : 3.25f, color);
        string[] leaves = stage == 0 ? new[] {"Door_Poor_002", "Door_Poor_001"} : stage == 1 ? new[] {"Door_Descent_00", "Door_Descent_01"} : stage == 2 ? new[] {"Door_Rich_01", "Door_Rich_02"} : new[] {"Door_Million_00", "Door_Million_00"};
        var hinges = new Transform[2];
        for (int i = 0; i < 2; i++)
        {
            hinges[i] = Node(i == 0 ? "LeftHinge" : "RightHinge", root.transform, new Vector3(i == 0 ? -2.25f : 2.25f, 0, 0)).transform;
            var mesh = Mesh(leaves[i]);
            var door = RawMesh("Door", mesh, hinges[i], stage == 3 ? gold : doorMaterial);
            float sx = 2.25f / mesh.bounds.size.x;
            float sy = (stage == 3 ? 3.5f : 2.9f) / mesh.bounds.size.y;
            if (stage == 3 && i == 1) sx = -sx;
            door.transform.localScale = new Vector3(sx, sy, 2.3f);
            door.transform.localPosition = new Vector3((i == 0 ? 1.125f : -1.125f) - mesh.bounds.center.x * sx, -mesh.bounds.min.y * sy, -mesh.bounds.center.z * 2.3f);
        }
        float top = stage == 2 ? 4.4f : stage == 3 ? 4.1f : 3.7f;
        Cube("MultiplierPlaque", root.transform, new Vector3(0, top, -.12f), new Vector3(1.35f,.85f,.14f), color);
        Label(root.transform, "×" + (stage+2), new Vector3(0,top,-.22f), Color.white, 7);
        var collider = root.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = new Vector3(0,1.5f,-2);
        collider.size = new Vector3(5,3,4);
        var gate = root.AddComponent<FinishGate>();
        SetInt(gate,"requiredWealth",20*(stage+1));
        SetInt(gate,"rewardMultiplier",stage+2);
        Set(gate,"leftHinge",hinges[0]); Set(gate,"rightHinge",hinges[1]);
        Save(root, Prefabs + "Gates/" + GateNames[stage] + ".prefab");
    }

    static void BuildGuard()
    {
        var root = Node("SecurityGuard");
        var model = Node("Model", root.transform);
        model.transform.localPosition = new Vector3(0, 1.05f, 0);
        model.transform.localScale = Vector3.one * 1.15f;
        foreach (string part in new[] {"Head_Male", "shirt_Vigile", "pants_Vigile"})
            RawMesh(part, PoseGuardMesh(part), model.transform, atlas);
        root.transform.localRotation = Quaternion.Euler(0,180,0);
        Save(root, Prefabs + "Environment/SecurityGuard.prefab");
    }

    static Mesh PoseGuardMesh(string name)
    {
        Mesh source = Mesh(name);
        var posed = Object.Instantiate(source);
        posed.name = name + "_Standing";
        Matrix4x4 rootBind = source.bindposes[0];
        var positions = source.vertices;
        var normals = source.normals;
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 v = rootBind.MultiplyPoint3x4(positions[i]);
            Vector3 n = rootBind.MultiplyVector(normals[i]).normalized;
            // Bake a relaxed static pose in common hip coordinates. The original
            // meshes have distinct bind transforms; raw coordinates cannot be mixed.
            float blend = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.16f, .30f, Mathf.Abs(v.x)));
            if (v.y > .18f && blend > 0f)
            {
                float side = Mathf.Sign(v.x);
                Vector3 shoulder = new Vector3(.18f * side, .43f, 0);
                Quaternion rotation = Quaternion.Euler(0, 0, -side * 72f * blend);
                v = shoulder + rotation * (v - shoulder);
                n = rotation * n;
            }
            positions[i] = v;
            normals[i] = n;
        }
        posed.vertices = positions; posed.normals = normals;
        posed.RecalculateBounds();
        string path = Out + posed.name + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(existing) { EditorUtility.CopySerialized(posed,existing); Object.DestroyImmediate(posed); return existing; }
        AssetDatabase.CreateAsset(posed,path);
        return posed;
    }

    static GameObject Instance(string path, Transform parent, Vector3 pos)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path), parent);
        go.transform.localPosition = pos;
        return go;
    }

    static void DressLevel(string name)
    {
        string path = Prefabs + "Levels/" + name + ".prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        foreach (var gate in root.GetComponentsInChildren<FinishGate>(true)) Object.DestroyImmediate(gate.gameObject);
        foreach (var gate in root.GetComponentsInChildren<WealthChangeZoneGroup>(true)) Object.DestroyImmediate(gate.gameObject);
        foreach (string old in new[] {"Stage5Environment", "Stage5Obstacles", "Stage5Pickups"})
        {
            var t = root.transform.Find(old); if(t) Object.DestroyImmediate(t.gameObject);
        }
        var ground = root.transform.Find("Ground");
        ground.localPosition = new Vector3(0,-.25f,50);
        ground.localScale = new Vector3(5,.5f,110);
        ground.GetComponent<Renderer>().sharedMaterial = white;
        foreach (var zone in root.GetComponentsInChildren<GameOutcomeZone>(true))
        {
            var so = new SerializedObject(zone);
            if(so.FindProperty("isFinish").boolValue) zone.transform.localPosition = new Vector3(0,1.5f,96);
        }
        var marker = root.transform.Find("FinishMarker");
        if(marker) marker.gameObject.SetActive(false);
        var decor = Node("Stage5Environment",root.transform);
        Cube("Ocean",decor.transform,new Vector3(0,-.65f,50),new Vector3(250,.1f,300),water);
        for(int i=0;i<10;i++) Cube("FinishTile",decor.transform,new Vector3(-2.25f+i*.5f,.016f,38),new Vector3(.5f,.032f,.5f),i%2==0?white:dark);
        for(int i=0;i<10;i++) Cube("FinishTile",decor.transform,new Vector3(-2.25f+i*.5f,.016f,38.5f),new Vector3(.5f,.032f,.5f),i%2==0?dark:white);
        Instance(Prefabs+"Gates/ChoiceGate_SchoolParty.prefab",root.transform,new Vector3(0,0,27));
        for(int i=0;i<4;i++)
        {
            float z=45+i*15;
            Instance(Prefabs+"Gates/"+GateNames[i]+".prefab",root.transform,new Vector3(0,0,z));
            if(i==0)
            {
                Prop("Trash.000",decor.transform,new Vector3(-2.1f,0,z-2),1.1f);
                Prop("Trash.001",decor.transform,new Vector3(2.1f,0,z-2),1.1f);
                Prop("TirePile",decor.transform,new Vector3(-2.05f,0,z-4),.7f);
            }
            if(i==1)
            {
                Prop("Bench",decor.transform,new Vector3(-2.1f,0,z-2),.85f,atlas,90);
                Prop("Bench",decor.transform,new Vector3(2.1f,0,z-2),.85f,atlas,-90);
            }
            if(i>=2)
            {
                if(i!=3) Prop("Plant_01",decor.transform,new Vector3(-2.05f,0,z-2),1.6f);
                Prop("Plant_01",decor.transform,new Vector3(2.05f,0,z-2),1.6f);
            }
            Cube("Carpet",decor.transform,new Vector3(0,.012f,z-5),new Vector3(1.4f,.025f,8),i==3?Material("RedCarpet",new Color(.7f,.035f,.08f)):i==2?blue:i==1?dark:orange);
        }
        Instance(Prefabs+"Environment/SecurityGuard.prefab",decor.transform,new Vector3(-1.8f,0,88));
        var obstacles = Node("Stage5Obstacles",root.transform);
        Instance(Prefabs+"Obstacles/TrashObstacle.prefab",obstacles.transform,new Vector3(-1.55f,0,12));
        Instance(Prefabs+"Obstacles/TrashObstacle.prefab",obstacles.transform,new Vector3(1.55f,0,32));
        var pickups = Node("Stage5Pickups",root.transform);
        if(root.GetComponentsInChildren<WealthPickup>().Length==0)
            for(int i=0;i<7;i++) Instance(Prefabs+"Pickups/MoneyPickup.prefab",pickups.transform,new Vector3(0,.7f,4+i*3));
        foreach(float z in new[]{33f,36f,49f,52f,64f,67f,79f,82f})
            Instance(Prefabs+"Pickups/MoneyPickup.prefab",pickups.transform,new Vector3(0,.7f,z));
        foreach(float z in new[]{35f,51f,66f,81f})
            Instance(Prefabs+"Pickups/AlcoholPickup.prefab",pickups.transform,new Vector3(1.4f,.7f,z));
        PrefabUtility.SaveAsPrefabAsset(root,path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    public static void Validate()
    {
        foreach(string name in new[]{"Level_01","Level_02"})
        {
            var root=PrefabUtility.LoadPrefabContents(Prefabs+"Levels/"+name+".prefab");
            if(root.GetComponentsInChildren<FinishGate>().Length!=4) throw new Exception("Gate count " + name);
            if(root.GetComponentsInChildren<FlagCheckpoint>().Length!=1) throw new Exception("Flag count " + name);
            if(root.GetComponentsInChildren<WealthChangeZoneGroup>().Length!=1) throw new Exception("Choice count " + name);
            foreach(var t in root.GetComponentsInChildren<Transform>(true))
                if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0) throw new Exception("Missing script " + t.name);
            foreach(var r in root.GetComponentsInChildren<Renderer>(true))
                foreach(var m in r.sharedMaterials)
                    if(m==null || m.shader==null) throw new Exception("Missing material/shader " + r.name);
            PrefabUtility.UnloadPrefabContents(root);
        }
        File.WriteAllText("Documentation/Stage5/Validation.txt","Both levels: 4 finish gates, 1 choice group, 1 flag checkpoint. No missing scripts or renderer materials.\n");
        Debug.Log("STAGE5_VALIDATION_OK");
    }

    public static void RenderResults()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var root=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+"Levels/Level_01.prefab"));
        var light=Node("PreviewLight").AddComponent<Light>();
        light.type=LightType.Directional; light.intensity=1.2f;
        light.transform.rotation=Quaternion.Euler(45,-35,0);
        RenderSettings.ambientMode=AmbientMode.Flat;
        RenderSettings.ambientLight=new Color(.65f,.7f,.8f);
        var camera=Node("PreviewCamera").AddComponent<Camera>();
        camera.fieldOfView=50; camera.backgroundColor=new Color(.05f,.8f,.95f); camera.clearFlags=CameraClearFlags.SolidColor;
        foreach(int z in new[]{18,39,54,69,84})
        {
            camera.transform.position=new Vector3(0,4,z-5);
            camera.transform.rotation=Quaternion.Euler(22,0,0);
            Capture(camera,"Documentation/Stage5/View_"+z+".png");
        }
    }

    static void Capture(Camera camera,string path)
    {
        var rt=new RenderTexture(540,960,24);
        camera.targetTexture=rt;
        camera.Render();
        var previous=RenderTexture.active; RenderTexture.active=rt;
        var image=new Texture2D(540,960,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,540,960),0,0); image.Apply();
        File.WriteAllBytes(path,image.EncodeToPNG());
        RenderTexture.active=previous; camera.targetTexture=null;
        Object.DestroyImmediate(image); Object.DestroyImmediate(rt);
    }

    public static void Inspect()
    {
        Directory.CreateDirectory("Documentation/Stage5");
        var report = new StringBuilder();
        foreach (string path in Directory.GetFiles(MeshRoot, "*.asset"))
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (!(name.StartsWith("Door") || new[] {"Flag", "Bench", "Trash.000", "Trash.001", "Plante_Descent", "Plante_Rich", "Plante_Poor", "Head_Male", "shirt_Vigile", "pants_Vigile", "hair_vigile", "body_base", "Hat_School", "Diplome", "Bottle.001", "Bag_Billet"}.Contains(name))) continue;
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (!mesh) continue;
            report.AppendLine($"{name}: bounds={mesh.bounds} vertices={mesh.vertexCount} bones={mesh.bindposes.Length} submeshes={mesh.subMeshCount}");
            try { RenderMesh(mesh, name); } catch (Exception e) { report.AppendLine("Preview: " + e.Message); }
        }
        foreach (string path in Directory.GetFiles(MeshRoot + "LowPoly", "*.fbx"))
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            report.AppendLine("FBX " + path);
            foreach (var t in model.GetComponentsInChildren<Transform>(true)) report.AppendLine("  " + t.name);
            foreach (var r in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                report.AppendLine($"RENDERER {r.name} mesh={r.sharedMesh.name} bones={r.bones.Length} scale={r.transform.lossyScale} bounds={r.bounds}");
        }
        File.WriteAllText("Documentation/Stage5/AssetInspection.txt", report.ToString());
        Debug.Log("STAGE5_INSPECTION_OK");
    }

    static void RenderMesh(Mesh mesh, string name)
    {
        var preview = new PreviewRenderUtility();
        var go = new GameObject(name);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(.7f, .75f, .8f);
        go.AddComponent<MeshRenderer>().sharedMaterials = Enumerable.Repeat(mat, mesh.subMeshCount).ToArray();
        preview.AddSingleGO(go);
        float radius = Mathf.Max(mesh.bounds.extents.magnitude, .1f);
        preview.camera.transform.position = mesh.bounds.center + new Vector3(0.3f, .15f, -1).normalized * radius * 3.2f;
        preview.camera.transform.LookAt(mesh.bounds.center);
        preview.camera.nearClipPlane = radius * .01f;
        preview.camera.fieldOfView = 60;
        preview.camera.farClipPlane = radius * 10;
        preview.camera.backgroundColor = new Color(.08f, .11f, .15f);
        preview.camera.clearFlags = CameraClearFlags.SolidColor;
        preview.lights[0].intensity = 1.2f;
        preview.lights[0].transform.rotation = Quaternion.Euler(40, -30, 0);
        preview.BeginStaticPreview(new Rect(0, 0, 384, 384));
        preview.Render(true);
        var image = preview.EndStaticPreview();
        File.WriteAllBytes("Documentation/Stage5/" + name + ".png", image.EncodeToPNG());
        Object.DestroyImmediate(image);
        preview.Cleanup();
        Object.DestroyImmediate(mat);
    }
}

