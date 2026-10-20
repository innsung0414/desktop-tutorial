using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// 기존 낚시 장면에 환경 아트를 저장합니다. Play 중에 지형을 생성하지 않습니다.
/// Deep Blue > Upgrade Lake Environment에서 같은 구성으로 다시 만들 수 있습니다.
/// </summary>
public static class DeepBlueEnvironmentBuilder
{
    private const string RootName = "Deep Blue Environment";
    private const string Art = "Assets/Art/Environment/";
    private static Transform root;
    private static Material wood, woodLight, bark, stone, ground, pine, pineTip, grass, metal, rope, warm;
    private static System.Random random;
    private static readonly List<CombineInstance> trunks = new List<CombineInstance>();
    private static readonly List<CombineInstance> crowns = new List<CombineInstance>();
    private static readonly List<CombineInstance> grasses = new List<CombineInstance>();
    private static readonly List<CombineInstance> rocks = new List<CombineInstance>();

    [MenuItem("Deep Blue/Upgrade Lake Environment")]
    public static void UpgradeEnvironment()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/DeepBlueFishing.unity")
            throw new InvalidOperationException("Open DeepBlueFishing.unity first.");
        EnsureFolder("Assets/Scenes/Backups");
        string backup = "Assets/Scenes/Backups/DeepBlueFishing_BeforeEnvironment_20261008.unity";
        EditorSceneManager.SaveScene(scene);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(backup) == null) AssetDatabase.CopyAsset(scene.path, backup);
        EnsureFolder(Art + "Materials"); EnsureFolder(Art + "Meshes");
        foreach (string name in new[] { "Cedar_Planks", "Forest_Shore", "Granite_Rock", "Dawn_Sky", "Pine_Branch", "Moss_Floor" })
        {
            string path = Art + "Textures/" + name + ".png";
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new InvalidOperationException("Missing generated texture: " + path);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true; importer.mipmapEnabled = true;
            importer.alphaIsTransparency = name == "Pine_Branch";
            if (name == "Pine_Branch")
            {
                importer.mipMapsPreserveCoverage = true;
                importer.alphaTestReferenceValue = 0.22f;
            }
            importer.wrapMode = TextureWrapMode.Repeat; importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 8; importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
        Undo.IncrementCurrentGroup(); int undo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Upgrade Deep Blue lake environment");
        GameObject old = GameObject.Find(RootName);
        if (old != null) Undo.DestroyObjectImmediate(old);
        root = new GameObject(RootName).transform;
        Undo.RegisterCreatedObjectUndo(root.gameObject, "Create environment");
        random = new System.Random(241008);
        trunks.Clear(); crowns.Clear(); grasses.Clear(); rocks.Clear();
        BuildMaterials();
        BuildLandscape();
        BuildDock();
        BuildCabin();
        BuildShoreDetails();
        BuildForest();
        BuildForestBoundary();
        ConfigureLakeAndLighting();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        Undo.CollapseUndoOperations(undo);
        Debug.Log("Deep Blue environment saved: sculpted lake, forest, mountains, dock, cabin and dawn lighting.");
    }

    private static void EnsureFolder(string path)
    {
        path = path.TrimEnd('/');
        if (AssetDatabase.IsValidFolder(path)) return;
        int cut = path.LastIndexOf('/');
        EnsureFolder(path.Substring(0, cut));
        AssetDatabase.CreateFolder(path.Substring(0, cut), path.Substring(cut + 1));
    }

    private static Texture2D Tex(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>(Art + "Textures/" + name + ".png");
    private static Material Mat(string name, Color color, Texture texture = null, float smooth = 0.12f)
    {
        string path = Art + "Materials/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
        mat.shader = Shader.Find("Universal Render Pipeline/Lit");
        mat.SetColor("_BaseColor", color); mat.SetTexture("_BaseMap", texture);
        mat.SetFloat("_Smoothness", smooth); mat.enableInstancing = true;
        EditorUtility.SetDirty(mat); return mat;
    }

    private static void BuildMaterials()
    {
        wood = Mat("Dock Cedar", new Color(0.67f, 0.57f, 0.43f), Tex("Cedar_Planks"), 0.25f);
        woodLight = Mat("Cedar Silvered", new Color(0.8f, 0.75f, 0.63f), Tex("Cedar_Planks"), 0.2f);
        bark = Mat("Pine Bark", new Color(0.3f, 0.24f, 0.18f), Tex("Cedar_Planks"));
        stone = Mat("Lake Granite", new Color(0.7f, 0.76f, 0.75f), Tex("Granite_Rock"), 0.18f);
        ground = Mat("Forest Shore", new Color(0.78f, 0.77f, 0.64f), Tex("Forest_Shore"));
        ground.shader = Shader.Find("Deep Blue/Forest Ground");
        ground.SetTexture("_Soil", Tex("Forest_Shore")); ground.SetTexture("_Moss", Tex("Moss_Floor"));
        pine = Mat("Pine Needles", new Color(0.9f, 0.94f, 0.85f), Tex("Pine_Branch"));
        pine.SetFloat("_AlphaClip", 1); pine.SetFloat("_Cutoff", 0.22f); pine.SetFloat("_Cull", 0);
        pine.EnableKeyword("_ALPHATEST_ON"); pine.renderQueue = (int)RenderQueue.AlphaTest; pine.doubleSidedGI = true;
        pineTip = Mat("Pine New Growth", new Color(0.24f, 0.34f, 0.19f));
        grass = Mat("Shore Grasses", new Color(0.36f, 0.40f, 0.19f)); grass.SetFloat("_Cull", 0);
        metal = Mat("Aged Iron", new Color(0.12f, 0.17f, 0.18f), null, 0.45f); metal.SetFloat("_Metallic", 0.6f);
        rope = Mat("Hemp Rope", new Color(0.49f, 0.39f, 0.24f));
        warm = Mat("Lantern Glass", new Color(1f, 0.65f, 0.24f), null, 0.45f);
        warm.EnableKeyword("_EMISSION"); warm.SetColor("_EmissionColor", new Color(1f, 0.49f, 0.12f) * 2.3f);
    }

    private static float R(float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());
    private static float Noise(float x, float z) => Mathf.PerlinNoise(x * 0.025f + 19f, z * 0.025f + 47f);
    private static float Height(float x, float z)
    {
        float distance = Mathf.Sqrt(x * x / (58f * 58f) + (z - 68f) * (z - 68f) / (58f * 58f));
        float edge = (distance - 1f) * 45f;
        float rise = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-1f, 11f, edge));
        float h = -0.38f + rise * (2.4f + Noise(x, z) * 11f);
        float core = (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(31, 39, Mathf.Abs(x))))
            * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(9.5f, 10.8f, z)))
            * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-16, -10, z));
        return Mathf.Lerp(h, 0.045f, core);
    }

    private static void BuildLandscape()
    {
        HideOriginal("Shore Ground");
        Mesh mesh = Grid("Sculpted Shore", 150, 150, new Vector2(-150, -80), new Vector2(150, 220), Height, 0.25f);
        GameObject terrain = MeshObject("Sculpted shoreline and forest floor", mesh, ground, root);
        terrain.AddComponent<MeshCollider>().sharedMesh = mesh;
        Material mountainMat = Mat("Distant Granite", new Color(0.35f, 0.43f, 0.44f), Tex("Granite_Rock"));
        Mesh mountain = Grid("Mountain Range", 180, 80, new Vector2(-290, 155), new Vector2(290, 340), (x,z) =>
        {
            float h = -5f;
            float[] peaksX = { -235, -142, -65, 22, 110, 202, 273 };
            float[] peaksZ = { 270, 238, 295, 250, 286, 244, 310 };
            for (int i = 0; i < peaksX.Length; i++)
            {
                float dx=(x-peaksX[i])/68f, dz=(z-peaksZ[i])/76f;
                h=Mathf.Max(h, (66f+(i%3)*19f)*Mathf.Exp(-(dx*dx+dz*dz)*1.8f));
            }
            float crags=(Noise(x*6.5f,z*6.5f)-0.5f)*18f;
            // Sink the front and side edges below the shore so no sky shows beneath the range.
            float edge = Mathf.Min(z - 155f, 290f - Mathf.Abs(x));
            float skirt = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, 28f, edge));
            return Mathf.Lerp(-20f, h + Noise(x*2.8f,z*2.8f)*12f + crags + 6f, skirt);
        }, 0.06f);
        var rockFaces = new List<int>(); var snowFaces = new List<int>();
        Vector3[] mountainVertices = mountain.vertices; int[] faces = mountain.triangles;
        for (int i = 0; i < faces.Length; i += 3)
        {
            Vector3 p = (mountainVertices[faces[i]] + mountainVertices[faces[i+1]] + mountainVertices[faces[i+2]]) / 3;
            Vector3 normal = Vector3.Cross(mountainVertices[faces[i+1]]-mountainVertices[faces[i]],mountainVertices[faces[i+2]]-mountainVertices[faces[i]]).normalized;
            var target = p.y > 67f + Noise(p.x*4f,p.z*4f)*14f && normal.y > 0.58f ? snowFaces : rockFaces;
            target.Add(faces[i]); target.Add(faces[i+1]); target.Add(faces[i+2]);
        }
        mountain.subMeshCount = 2; mountain.SetTriangles(rockFaces,0); mountain.SetTriangles(snowFaces,1); EditorUtility.SetDirty(mountain);
        MeshObject("Distant mountain ridges", mountain, new[]{mountainMat,Mat("Mountain Snow",new Color(0.72f,0.79f,0.81f))}, root,Vector3.zero,Vector3.one);
    }

    private static Mesh Grid(string name, int nx, int nz, Vector2 min, Vector2 max, Func<float,float,float> height, float uvScale)
    {
        var v=new Vector3[(nx+1)*(nz+1)]; var uv=new Vector2[v.Length]; var triangles=new int[nx*nz*6];
        for(int z=0;z<=nz;z++) for(int x=0;x<=nx;x++)
        {
            int i=z*(nx+1)+x; float px=Mathf.Lerp(min.x,max.x,x/(float)nx), pz=Mathf.Lerp(min.y,max.y,z/(float)nz);
            v[i]=new Vector3(px,height(px,pz),pz); uv[i]=new Vector2(px,pz)*uvScale;
        }
        int t=0;
        for(int z=0;z<nz;z++) for(int x=0;x<nx;x++)
        {
            int i=z*(nx+1)+x;
            triangles[t++]=i; triangles[t++]=i+nx+1; triangles[t++]=i+1;
            triangles[t++]=i+1; triangles[t++]=i+nx+1; triangles[t++]=i+nx+2;
        }
        var mesh=new Mesh { name=name }; mesh.vertices=v; mesh.uv=uv; mesh.triangles=triangles; mesh.RecalculateNormals();
        return SaveMesh(mesh,name);
    }

    private static void HideOriginal(string name)
    {
        var obj=GameObject.Find(name); if(obj==null) return;
        var renderer=obj.GetComponent<Renderer>();
        if(renderer!=null){Undo.RecordObject(renderer,"Replace blockout visuals");renderer.enabled=false;}
    }

    private static Transform Group(string name) { var go=new GameObject(name); go.transform.SetParent(root,false); return go.transform; }
    private static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, Material material, bool collision=false)
        => Primitive(name, PrimitiveType.Cube, parent, pos, size, material, collision);
    private static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 pos, Vector3 size, Material material, bool collision=false)
    {
        GameObject go=GameObject.CreatePrimitive(type); go.name=name; go.transform.SetParent(parent,false);
        go.transform.localPosition=pos; go.transform.localScale=size; go.GetComponent<Renderer>().sharedMaterial=material;
        if(!collision)Object.DestroyImmediate(go.GetComponent<Collider>());
        go.isStatic=true; return go;
    }
    private static void Beam(string name,Transform parent,Vector3 a,Vector3 b,float diameter,Material material)
    {
        GameObject go=Primitive(name,PrimitiveType.Cylinder,parent,(a+b)*0.5f,new Vector3(diameter,Vector3.Distance(a,b)*0.5f,diameter),material);
        go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
    }

    private static void BuildDock()
    {
        HideOriginal("Small Fishing Dock");
        Transform dock=Group("Cedar fishing pier");
        for(int i=0;i<32;i++)
        {
            float z=4.125f+i*0.25f;
            var board=Box("Individual cedar plank "+(i+1),dock,new Vector3(0,0.13f,z),new Vector3(3.79f,0.145f,0.237f),i%4==0?woodLight:wood);
            board.transform.localRotation=Quaternion.Euler(0,R(-0.15f,0.15f),0);
            for(int side=-1;side<=1;side+=2)
                Primitive("Iron fastener",PrimitiveType.Cylinder,dock,new Vector3(side*1.55f,0.206f,z),new Vector3(0.03f,0.002f,0.03f),metal);
        }
        foreach(float x in new[]{-1.52f,1.52f}) Box("Under deck bearer",dock,new Vector3(x,-0.09f,8),new Vector3(0.19f,0.24f,8.25f),bark);
        foreach(float x in new[]{-1.98f,1.98f})
        {
            foreach(float z in new[]{4.7f,8f,11.7f})
            {
                Primitive("Cedar mooring post",PrimitiveType.Cylinder,dock,new Vector3(x,-0.1f,z),new Vector3(0.26f,0.91f,0.26f),wood,true);
                Primitive("Post cap",PrimitiveType.Cylinder,dock,new Vector3(x,0.825f,z),new Vector3(0.29f,0.035f,0.29f),woodLight);
                for(int j=0;j<3;j++) MeshObject("Rope binding",TorusMesh,new Material[]{rope},dock,new Vector3(x,0.56f+j*0.045f,z),new Vector3(0.17f,0.17f,0.17f));
            }
            // Side ropes guide the eye; the front stays open for casting and water entry.
            foreach(var span in new[]{new Vector2(4.7f,8f),new Vector2(8f,11.7f)})
            for(int i=0;i<10;i++)
            {
                float a=i/10f,b=(i+1)/10f;
                Beam("Sagging rope",dock,new Vector3(x,0.64f-Mathf.Sin(a*Mathf.PI)*0.14f,Mathf.Lerp(span.x,span.y,a)),
                    new Vector3(x,0.64f-Mathf.Sin(b*Mathf.PI)*0.14f,Mathf.Lerp(span.x,span.y,b)),0.035f,rope);
            }
        }
        Lantern(dock,new Vector3(-1.98f,1.05f,4.7f));
        // Gear remains beside the walking route.
        Box("Fishing tackle box",dock,new Vector3(1.25f,0.40f,6.0f),new Vector3(0.65f,0.38f,0.42f),metal,true);
        Box("Tackle box lid",dock,new Vector3(1.25f,0.60f,6),new Vector3(0.69f,0.06f,0.46f),woodLight);
        Beam("Tackle handle",dock,new Vector3(1.1f,0.69f,6),new Vector3(1.4f,0.69f,6),0.035f,metal);
    }

    private static void Lantern(Transform parent,Vector3 position)
    {
        Transform lamp=new GameObject("Warm pier lantern").transform; lamp.SetParent(parent,false); lamp.localPosition=position;
        Box("Lantern glow",lamp,Vector3.zero,new Vector3(0.15f,0.22f,0.15f),warm);
        Box("Lantern base",lamp,new Vector3(0,-0.14f,0),new Vector3(0.23f,0.055f,0.23f),metal);
        Box("Lantern roof",lamp,new Vector3(0,0.14f,0),new Vector3(0.25f,0.06f,0.25f),metal);
        foreach(float x in new[]{-0.095f,0.095f}) foreach(float z in new[]{-0.095f,0.095f})
            Box("Lantern frame",lamp,new Vector3(x,0,z),new Vector3(0.018f,0.27f,0.018f),metal);
        Light light=lamp.gameObject.AddComponent<Light>(); light.type=LightType.Point; light.color=new Color(1,0.64f,0.31f);
        light.intensity=1.4f; light.range=3.5f; light.shadows=LightShadows.None;
    }

    private static void BuildCabin()
    {
        Transform cabin=Group("Lakeside timber shelter"); cabin.localPosition=new Vector3(-10,0.1f,0.5f); cabin.localRotation=Quaternion.Euler(0,18,0);
        Box("Cabin foundation",cabin,new Vector3(0,0.07f,0),new Vector3(4.8f,0.18f,3.6f),stone,true);
        Box("Closed cabin collision",cabin,new Vector3(0,1.4f,-0.3f),new Vector3(4.15f,2.6f,2.85f),wood,true);
        for(int i=0;i<20;i++)
        {
            Box("Front cedar siding",cabin,new Vector3(-2+i*0.21f,1.38f,1.15f),new Vector3(0.19f,2.55f,0.09f),i%3==0?woodLight:wood);
            Box("Side cedar siding",cabin,new Vector3(2.12f,1.38f,-1.7f+i*0.15f),new Vector3(0.09f,2.55f,0.13f),wood);
        }
        for(int side=-1;side<=1;side+=2)
        {
            var roof=Box("Sloped metal roof",cabin,new Vector3(side*1.13f,3.0f,-0.3f),new Vector3(2.65f,0.12f,3.65f),metal);
            roof.transform.localRotation=Quaternion.Euler(0,0,side*-21f);
            for(int i=0;i<8;i++)
            {
                var seam=Box("Roof standing seam",cabin,new Vector3(side*1.13f,3.08f,-1.9f+i*0.46f),new Vector3(2.66f,0.06f,0.035f),metal);
                seam.transform.localRotation=roof.transform.localRotation;
            }
        }
        Box("Door",cabin,new Vector3(-0.65f,1.1f,1.23f),new Vector3(0.94f,1.98f,0.10f),bark);
        Primitive("Door knob",PrimitiveType.Sphere,cabin,new Vector3(-0.32f,1.08f,1.31f),Vector3.one*0.075f,metal);
        Box("Window frame",cabin,new Vector3(1,1.65f,1.26f),new Vector3(1.12f,0.89f,0.12f),woodLight);
        Box("Window glass",cabin,new Vector3(1,1.65f,1.34f),new Vector3(0.94f,0.70f,0.04f),Mat("Cabin Window",new Color(0.13f,0.29f,0.32f),null,0.8f));
        Box("Window mullion",cabin,new Vector3(1,1.65f,1.38f),new Vector3(0.045f,0.74f,0.04f),woodLight);
        Box("Window rail",cabin,new Vector3(1,1.65f,1.38f),new Vector3(0.98f,0.045f,0.04f),woodLight);
        Lantern(cabin,new Vector3(-1.5f,2.15f,1.42f));
        var chimney=Box("Stove chimney",cabin,new Vector3(-1.4f,3.55f,-0.9f),new Vector3(0.18f,1.5f,0.18f),metal);
        Box("Chimney cap",cabin,chimney.transform.localPosition+Vector3.up*0.79f,new Vector3(0.32f,0.08f,0.32f),metal);
        Transform bench=Group("Lakeside bench"); bench.localPosition=new Vector3(6,0.04f,3.7f); bench.localRotation=Quaternion.Euler(0,-18,0);
        foreach(float x in new[]{-0.8f,0.8f}) Box("Bench leg",bench,new Vector3(x,0.25f,0),new Vector3(0.16f,0.5f,0.60f),metal,true);
        for(int i=0;i<3;i++)Box("Bench seat board",bench,new Vector3(0,0.52f,-0.21f+i*0.21f),new Vector3(2.1f,0.09f,0.19f),woodLight,true);
        for(int i=0;i<2;i++)Box("Bench back board",bench,new Vector3(0,0.87f+i*0.22f,-0.32f),new Vector3(2.1f,0.18f,0.07f),wood,true);
    }

    private static void BuildShoreDetails()
    {
        Mesh boulder=RockMesh();
        for(int i=0;i<100;i++)
        {
            float angle=R(0,Mathf.PI*2), radius=R(0.94f,1.035f);
            float x=Mathf.Cos(angle)*58*radius,z=68+Mathf.Sin(angle)*58*radius;
            if(Mathf.Abs(x)<4 && z<16)continue;
            Vector3 size=new Vector3(R(0.45f,2.3f),R(0.3f,1.1f),R(0.5f,1.8f));
            rocks.Add(new CombineInstance{mesh=boulder,transform=Matrix4x4.TRS(new Vector3(x,Mathf.Max(-0.15f,Height(x,z)),z),Quaternion.Euler(R(-15,15),R(0,360),R(-10,10)),size)});
        }
        foreach(Vector3 p in new[]{new Vector3(-4,0.05f,9),new Vector3(4.4f,0,10),new Vector3(8,0.1f,10),new Vector3(-7,0,11)})
        {
            var rock=MeshObject("Shore boulder",boulder,stone,root); rock.transform.localPosition=p;
            rock.transform.localScale=new Vector3(R(1.2f,2.2f),R(0.5f,0.95f),R(1,1.8f));
            rock.AddComponent<SphereCollider>().radius=0.72f;
        }
        Combine("Shore rock clusters",rocks,stone);
        Mesh tuft=GrassMesh();
        for(int i=0;i<440;i++)
        {
            float x=R(-42,42),z=R(-18,26),y=Height(x,z);
            if(y<0.02f || (Mathf.Abs(x)<3.2f && z>-9) || (x>-14&&x<-6&&z>-3&&z<4) || (x>4&&x<8&&z>2&&z<5))continue;
            grasses.Add(new CombineInstance{mesh=tuft,transform=Matrix4x4.TRS(new Vector3(x,y,z),Quaternion.Euler(0,R(0,360),0),Vector3.one*R(0.55f,1.45f))});
        }
        Combine("Shore grass and reed tufts",grasses,grass);
        // A small row of stepping stones identifies the approach to the pier.
        for(int i=0;i<7;i++)
        {
            var rock=MeshObject("Approach stepping stone",boulder,stone,root);
            rock.transform.localPosition=new Vector3(Mathf.Sin(i)*0.18f,0.02f,2.9f-i*0.9f);
            rock.transform.localScale=new Vector3(0.8f,0.05f,0.56f);
        }
    }

    private static void BuildForest()
    {
        Mesh crown=PineMesh();
        var temporary=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Mesh cylinder=temporary.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(temporary);
        Action<float,float,float> add=(x,z,h)=>
        {
            float y=Height(x,z); Quaternion rotation=Quaternion.Euler(0,R(0,360),0);
            Matrix4x4 world=Matrix4x4.TRS(new Vector3(x,y,z),rotation,new Vector3(h/8f,h/8f,h/8f));
            crowns.Add(new CombineInstance{mesh=crown,transform=world});
            trunks.Add(new CombineInstance{mesh=cylinder,transform=world*Matrix4x4.TRS(new Vector3(0,3.5f,0),Quaternion.identity,new Vector3(0.28f,3.5f,0.28f))});
            if(z<24 && Mathf.Abs(x)<35)
            {
                var collider=new GameObject("Pine trunk collision");collider.transform.SetParent(root,false);collider.transform.position=new Vector3(x,y+h*0.45f,z);
                var capsule=collider.AddComponent<CapsuleCollider>();capsule.height=h*0.9f;capsule.radius=0.2f;collider.isStatic=true;
            }
        };
        // Foreground pines frame the open view from the pier.
        add(-8.5f,7.4f,10.5f);add(10.4f,8.4f,12);add(-18,10,11);add(22,9,13);add(-23,-4,12);add(18,-7,10);
        for(int i=0;i<245;i++)
        {
            float x=R(-112,112),z=R(-35,189);
            if(Height(x,z)<0.55f || (Mathf.Abs(x)<8 && z<12) || (x>-18&&x<-4&&z>-7&&z<9))continue;
            add(x,z,R(7,15));
        }
        Combine("Pine forest crowns",crowns,pine);
        Combine("Pine forest trunks",trunks,bark);
    }

    private static Mesh PineMesh()
    {
        var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
        for(int tier=0;tier<10;tier++)
        {
            float y=1.15f+tier*0.67f,r=2.55f*(1-tier/11f);
            for(int branch=0;branch<7;branch++)
            {
                float angle=branch*Mathf.PI*2/7+tier*0.71f+R(-0.16f,0.16f);
                Vector3 direction=new Vector3(Mathf.Cos(angle),R(-0.08f,0.21f),Mathf.Sin(angle));
                Vector3 start=new Vector3(0,y+R(-0.2f,0.2f),0),end=start+direction*r*R(0.86f,1.15f);
                for(int card=0;card<2;card++)
                {
                    Vector3 up=card==0?Vector3.up:(Vector3.up+Vector3.Cross(direction,Vector3.up)*0.8f).normalized;
                    up*=r*0.38f;
                    int i=vertices.Count;
                    vertices.Add(start-up);vertices.Add(end-up);vertices.Add(end+up);vertices.Add(start+up);
                    uv.Add(Vector2.zero);uv.Add(Vector2.right);uv.Add(Vector2.one);uv.Add(Vector2.up);
                    triangles.AddRange(new[]{i,i+2,i+1,i,i+3,i+2});
                }
            }
        }
        var mesh=new Mesh{name="Layered Pine Crown"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();
        return SaveMesh(mesh,"Layered Pine Crown");
    }

    private static Mesh GrassMesh()
    {
        var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
        for(int b=0;b<12;b++)
        {
            float angle=R(0,Mathf.PI*2),h=R(0.18f,0.6f),w=R(0.018f,0.036f);
            Vector3 p=new Vector3(R(-0.16f,0.16f),0,R(-0.16f,0.16f));
            Vector3 side=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*w;
            Vector3 bend=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle))*h*0.35f;
            int s=v.Count;v.Add(p-side);v.Add(p+side);v.Add(p+Vector3.up*h*0.58f+bend*0.3f-side*0.6f);
            v.Add(p+Vector3.up*h*0.58f+bend*0.3f+side*0.6f);v.Add(p+Vector3.up*h+bend);
            uv.Add(Vector2.zero);uv.Add(Vector2.right);uv.Add(new Vector2(0,0.6f));uv.Add(new Vector2(1,0.6f));uv.Add(Vector2.up);
            t.AddRange(new[]{s,s+2,s+1,s+1,s+2,s+3,s+2,s+4,s+3});
        }
        var mesh=new Mesh{name="Shore Grass Tuft"};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateNormals();
        return SaveMesh(mesh,"Shore Grass Tuft");
    }

    private static Mesh RockMesh()
    {
        var temp=GameObject.CreatePrimitive(PrimitiveType.Sphere);Mesh source=temp.GetComponent<MeshFilter>().sharedMesh;
        Mesh mesh=Object.Instantiate(source);Object.DestroyImmediate(temp);mesh.name="Weathered Shore Boulder";
        Vector3[] vertices=mesh.vertices;
        for(int i=0;i<vertices.Length;i++)
        {
            Vector3 p=vertices[i]*2;
            float n=Mathf.PerlinNoise(p.x*2.8f+8,p.z*2.8f+p.y+4);
            vertices[i]=p*(0.78f+n*0.38f);
        }
        mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();return SaveMesh(mesh,"Weathered Shore Boulder");
    }

    private static Mesh torus;
    private static Mesh TorusMesh
    {
        get
        {
            if(torus!=null)return torus;
            var v=new Vector3[33*7];var uv=new Vector2[v.Length];var t=new List<int>();
            for(int a=0;a<=32;a++)for(int b=0;b<=6;b++)
            {
                float theta=a*Mathf.PI*2/32,phi=b*Mathf.PI*2/6,r=1+Mathf.Cos(phi)*0.10f;
                int i=a*7+b;v[i]=new Vector3(Mathf.Cos(theta)*r,Mathf.Sin(phi)*0.1f,Mathf.Sin(theta)*r);uv[i]=new Vector2(a/32f,b/6f);
                if(a<32&&b<6)t.AddRange(new[]{i,i+7,i+1,i+1,i+7,i+8});
            }
            var mesh=new Mesh{name="Rope Coil"};mesh.vertices=v;mesh.uv=uv;mesh.SetTriangles(t,0);mesh.RecalculateNormals();
            torus=SaveMesh(mesh,"Rope Coil");return torus;
        }
    }

    private static Mesh SaveMesh(Mesh mesh,string name)
    {
        string path=Art+"Meshes/"+name.Replace(' ','_')+".asset";
        Mesh existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(existing==null){AssetDatabase.CreateAsset(mesh,path);return mesh;}
        // Assign geometry explicitly so rerunning also refreshes native/GPU mesh buffers.
        existing.Clear();existing.indexFormat=mesh.indexFormat;existing.name=mesh.name;
        existing.vertices=mesh.vertices;existing.normals=mesh.normals;existing.uv=mesh.uv;
        existing.tangents=mesh.tangents;existing.colors=mesh.colors;existing.subMeshCount=mesh.subMeshCount;
        for(int i=0;i<mesh.subMeshCount;i++)existing.SetTriangles(mesh.GetTriangles(i),i);
        existing.RecalculateBounds();Object.DestroyImmediate(mesh);EditorUtility.SetDirty(existing);return existing;
    }
    private static GameObject MeshObject(string name,Mesh mesh,Material material,Transform parent)
        => MeshObject(name,mesh,new[]{material},parent,Vector3.zero,Vector3.one);
    private static GameObject MeshObject(string name,Mesh mesh,Material[] materials,Transform parent,Vector3 pos,Vector3 scale)
    {
        var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);
        go.transform.localPosition=pos;go.transform.localScale=scale;go.GetComponent<MeshFilter>().sharedMesh=mesh;
        go.GetComponent<MeshRenderer>().sharedMaterials=materials;go.isStatic=true;return go;
    }
    private static void Combine(string name,List<CombineInstance> sources,Material material)
    {
        if(sources.Count==0)return;
        var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(sources.ToArray(),true,true);mesh.RecalculateBounds();
        MeshObject(name,SaveMesh(mesh,name),material,root);
    }

    [MenuItem("Deep Blue/Update Forest Boundary")]
    public static void UpdateForestBoundary()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/DeepBlueFishing.unity")
            throw new InvalidOperationException("Open DeepBlueFishing.unity first.");
        GameObject environment = GameObject.Find(RootName);
        if (environment == null) throw new InvalidOperationException("Create the lake environment first.");
        EditorSceneManager.SaveScene(scene);
        string backup = "Assets/Scenes/Backups/DeepBlueFishing_BeforeBoundary_20261008.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(backup) == null) AssetDatabase.CopyAsset(scene.path, backup);
        root = environment.transform;
        wood = AssetDatabase.LoadAssetAtPath<Material>(Art + "Materials/Dock Cedar.mat");
        BuildForestBoundary();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static void BuildForestBoundary()
    {
        Transform terrain = root.Find("Sculpted shoreline and forest floor");
        if (terrain == null || wood == null) throw new InvalidOperationException("Missing shore mesh or cedar material.");
        MeshCollider shore = terrain.GetComponent<MeshCollider>();
        if (shore == null || shore.sharedMesh == null) throw new InvalidOperationException("Missing shore collision mesh.");
        Transform previous = root.Find("Forest perimeter fence");
        if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);
        Transform fence = Group("Forest perimeter fence");
        Undo.RegisterCreatedObjectUndo(fence.gameObject, "Create forest boundary");
        Physics.SyncTransforms();

        // Keep the fence eight metres inside the actual terrain edge.
        Bounds bounds = shore.bounds;
        float left = bounds.min.x + 8f, right = bounds.max.x - 8f;
        float near = bounds.min.z + 8f, far = bounds.max.z - 8f;
        // The mountain range is scenery, so keep walking in front of its open back faces.
        Transform mountains = root.Find("Distant mountain ridges");
        if (mountains != null)
            far = Mathf.Min(far, mountains.GetComponent<Renderer>().bounds.min.z - 5f);
        Vector2[] corners = { new Vector2(left, near), new Vector2(right, near),
            new Vector2(right, far), new Vector2(left, far) };
        var source = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cube = source.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(source);
        var pieces = new List<CombineInstance>();
        for (int side = 0; side < corners.Length; side++)
        {
            Vector2 start = corners[side], end = corners[(side + 1) % corners.Length];
            int spans = Mathf.CeilToInt(Vector2.Distance(start, end) / 4f);
            for (int i = 0; i < spans; i++)
            {
                Vector2 p = Vector2.Lerp(start, end, i / (float)spans);
                Vector2 q = Vector2.Lerp(start, end, (i + 1) / (float)spans);
                Vector3 a = BoundaryGround(shore, p), b = BoundaryGround(shore, q);
                AddFencePiece(pieces, cube, a + Vector3.up * 1.05f, Quaternion.identity, new Vector3(0.22f, 2.4f, 0.22f));
                Vector3 direction = b - a;
                foreach (float height in new[] { 0.8f, 1.7f })
                    AddFencePiece(pieces, cube, (a + b) * 0.5f + Vector3.up * height,
                        Quaternion.LookRotation(direction), new Vector3(0.14f, 0.18f, direction.magnitude + 0.16f));

                // Solid, overlapping spans prevent stepping between or over the visible rails.
                var barrier = new GameObject("Fence collision " + side + "-" + i);
                barrier.transform.SetParent(fence, false);
                float bottom = Mathf.Min(a.y, b.y) - 0.5f;
                float top = Mathf.Max(a.y, b.y) + 2.2f;
                barrier.transform.position = new Vector3((a.x + b.x) * 0.5f, (bottom + top) * 0.5f, (a.z + b.z) * 0.5f);
                barrier.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                var collision = barrier.AddComponent<BoxCollider>();
                collision.size = new Vector3(0.28f, top - bottom, Vector2.Distance(p, q) + 0.2f);
                barrier.isStatic = true;
            }
        }
        var mesh = new Mesh { name = "Forest_Perimeter_Fence", indexFormat = IndexFormat.UInt32 };
        mesh.CombineMeshes(pieces.ToArray(), true, true);
        mesh.RecalculateBounds();
        MeshObject("Cedar perimeter posts and rails", SaveMesh(mesh, mesh.name), wood, fence);
    }

    private static Vector3 BoundaryGround(MeshCollider shore, Vector2 point)
    {
        var ray = new Ray(new Vector3(point.x, shore.bounds.max.y + 10f, point.y), Vector3.down);
        if (!shore.Raycast(ray, out RaycastHit hit, shore.bounds.size.y + 20f))
            throw new InvalidOperationException("Forest boundary must stay on the shore collision mesh.");
        return hit.point;
    }

    private static void AddFencePiece(List<CombineInstance> pieces, Mesh cube, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        pieces.Add(new CombineInstance { mesh = cube,
            transform = root.worldToLocalMatrix * Matrix4x4.TRS(position, rotation, scale) });
    }

    private static void ConfigureLakeAndLighting()
    {
        var water=GameObject.Find("Lake Water");Undo.RecordObject(water.transform,"Extend scenic lake");
        water.transform.position=new Vector3(0,0.025f,78);water.transform.localScale=new Vector3(13,1,13.6f);
        Undo.RecordObject(water,"Water layer");water.layer=4;
        var mat=AssetDatabase.LoadAssetAtPath<Material>(Art+"Materials/Dawn Lake.mat");
        if(mat==null){mat=new Material(Shader.Find("Deep Blue/Lake Surface"));AssetDatabase.CreateAsset(mat,Art+"Materials/Dawn Lake.mat");}
        mat.SetTexture("_SkyTex",Tex("Dawn_Sky"));mat.SetFloat("_ReflectionStrength",0.55f);EditorUtility.SetDirty(mat);
        var renderer=water.GetComponent<Renderer>();Undo.RecordObject(renderer,"Apply lake surface");renderer.sharedMaterial=mat;
        renderer.reflectionProbeUsage=ReflectionProbeUsage.Simple;
        foreach(string name in new[]{"Water Death Zone","Lake Bed (Collision)"})
        {
            var obj=GameObject.Find(name);Undo.RecordObject(obj.transform,"Match lake collision extent");
            Vector3 p=obj.transform.position;p.z=78;obj.transform.position=p;
            var collider=obj.GetComponent<BoxCollider>();Undo.RecordObject(collider,"Match lake collision extent");
            Vector3 size=collider.size;size.x=130;size.z=136;collider.size=size;
        }
        var sky=AssetDatabase.LoadAssetAtPath<Material>(Art+"Materials/Dawn Sky.mat");
        if(sky==null){sky=new Material(Shader.Find("Skybox/Panoramic"));AssetDatabase.CreateAsset(sky,Art+"Materials/Dawn Sky.mat");}
        sky.SetTexture("_MainTex",Tex("Dawn_Sky"));sky.SetFloat("_Exposure",0.85f);sky.SetFloat("_Rotation",75);EditorUtility.SetDirty(sky);
        RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(0.56f,0.63f,0.68f);RenderSettings.ambientEquatorColor=new Color(0.43f,0.49f,0.46f);
        RenderSettings.ambientGroundColor=new Color(0.24f,0.27f,0.23f);
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;
        RenderSettings.fogColor=new Color(0.40f,0.53f,0.57f);RenderSettings.fogDensity=0.0036f;
        Light sun=GameObject.Find("Sun").GetComponent<Light>();Undo.RecordObject(sun,"Dawn lighting");Undo.RecordObject(sun.transform,"Dawn lighting");
        sun.transform.rotation=Quaternion.Euler(24,-34,0);sun.color=new Color(1,0.87f,0.70f);sun.intensity=1.45f;
        sun.shadows=LightShadows.Soft;sun.shadowStrength=0.8f;RenderSettings.sun=sun;
        Camera camera=Camera.main;Undo.RecordObject(camera,"Environment visibility");camera.farClipPlane=520;camera.clearFlags=CameraClearFlags.Skybox;
        var data=camera.GetComponent<UniversalAdditionalCameraData>();if(data==null)data=Undo.AddComponent<UniversalAdditionalCameraData>(camera.gameObject);
        Undo.RecordObject(data,"Enable environment grading");data.renderPostProcessing=true;data.volumeLayerMask=1;
        data.antialiasing=AntialiasingMode.FastApproximateAntialiasing;
        var urp=UniversalRenderPipeline.asset;Undo.RecordObject(urp,"Environment shadows");urp.supportsHDR=true;urp.shadowDistance=100;EditorUtility.SetDirty(urp);
        var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Art+"Dawn Grade.asset");
        if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Art+"Dawn Grade.asset");}
        var tone=Override<Tonemapping>(profile);tone.mode.Override(TonemappingMode.ACES);
        var color=Override<ColorAdjustments>(profile);color.postExposure.Override(0.3f);color.contrast.Override(7);color.saturation.Override(-5);
        var bloom=Override<Bloom>(profile);bloom.intensity.Override(0.15f);bloom.threshold.Override(1.1f);bloom.scatter.Override(0.5f);
        var vignette=Override<Vignette>(profile);vignette.intensity.Override(0.12f);vignette.smoothness.Override(0.4f);
        EditorUtility.SetDirty(profile);
        var volume=Group("Dawn atmosphere volume").gameObject.AddComponent<Volume>();volume.isGlobal=true;volume.priority=10;volume.sharedProfile=profile;
        var probe=Group("Lake reflection probe").gameObject.AddComponent<ReflectionProbe>();probe.transform.position=new Vector3(0,3,20);
        probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;
        probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.AllFacesAtOnce;probe.resolution=128;probe.hdr=true;
        probe.size=new Vector3(220,120,280);probe.center=new Vector3(0,0,55);probe.nearClipPlane=0.3f;probe.farClipPlane=450;
        probe.cullingMask=~(1<<4);probe.boxProjection=false;probe.RenderProbe();
        DynamicGI.UpdateEnvironment();
    }

    private static T Override<T>(VolumeProfile profile) where T : VolumeComponent
    {
        if(profile.TryGet<T>(out var existing))return existing;
        T value=profile.Add<T>(true);AssetDatabase.AddObjectToAsset(value,profile);return value;
    }
}
