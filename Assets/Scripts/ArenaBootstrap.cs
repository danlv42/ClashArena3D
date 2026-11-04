using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArenaBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureGame()
    {
        if (Object.FindObjectOfType<BattleGame>() != null) return;
        new GameObject("ClashArena3D").AddComponent<BattleGame>();
    }

    private void Start() { BuildWorld(); }

    private void BuildWorld()
    {
        RenderSettings.ambientLight = new Color(.72f, .76f, .82f);
        Material grass = Mat(new Color(.32f, .58f, .25f));
        Material bank = Mat(new Color(.19f, .39f, .19f));
        Material water = Mat(new Color(.10f, .43f, .70f));
        Material wood = Mat(new Color(.48f, .29f, .16f));
        Cube("Arena", new Vector3(0,-.3f,0), new Vector3(18,.5f,30), grass);
        Cube("River", new Vector3(0,-.01f,0), new Vector3(18,.16f,3.2f), water);
        Cube("North edge", new Vector3(0,0,15), new Vector3(18,.3f,.45f), bank);
        Cube("South edge", new Vector3(0,0,-15), new Vector3(18,.3f,.45f), bank);
        Cube("West edge", new Vector3(-9,0,0), new Vector3(.45f,.3f,30), bank);
        Cube("East edge", new Vector3(9,0,0), new Vector3(.45f,.3f,30), bank);
        foreach (int side in new int[]{-1,1})
        {
            float x = side * 4.1f;
            Cube("Bridge", new Vector3(x,.14f,0), new Vector3(2.5f,.32f,4.2f), wood);
            for (int z=-1; z<=1; z++) Cube("Plank", new Vector3(x,.32f,z*1.25f), new Vector3(2.45f,.05f,.08f), Mat(new Color(.68f,.46f,.26f)));
        }
        for (int i=0;i<14;i++)
        {
            float x = (i%2==0?-1:1)*(6.8f + (i%3)*.55f);
            float z = -13 + i*1.95f;
            if (Mathf.Abs(z)>2.8f) Sphere("Bush",new Vector3(x,.35f,z),Vector3.one*.85f,Mat(new Color(.12f,.34f,.12f)));
        }
        GameObject cameraObject = new GameObject("Arena Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.transform.position = new Vector3(0,23,-27);
        camera.transform.rotation = Quaternion.Euler(38,0,0);
        camera.fieldOfView = 48;
        camera.backgroundColor = new Color(.48f,.68f,.78f);
        GameObject sun = new GameObject("Sun");
        Light light = sun.AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.25f;
        light.transform.rotation=Quaternion.Euler(48,-32,0);
        if (Object.FindObjectOfType<EventSystem>()==null)
        { GameObject es=new GameObject("EventSystem"); es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>(); }
        GameObject ui=new GameObject("Game UI");
        Canvas canvas=ui.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler=ui.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
        ui.AddComponent<GraphicRaycaster>();
        GetComponent<BattleGame>().Initialize(camera,canvas);
    }

    public static Material Mat(Color color)
    {
        Shader shader=Shader.Find("Universal Render Pipeline/Lit"); if(shader==null) shader=Shader.Find("Standard");
        Material m=new Material(shader); m.color=color; return m;
    }
    public static GameObject Cube(string n,Vector3 p,Vector3 s,Material m)
    { GameObject o=GameObject.CreatePrimitive(PrimitiveType.Cube); o.name=n;o.transform.position=p;o.transform.localScale=s;o.GetComponent<Renderer>().material=m;return o; }
    public static GameObject Sphere(string n,Vector3 p,Vector3 s,Material m)
    { GameObject o=GameObject.CreatePrimitive(PrimitiveType.Sphere); o.name=n;o.transform.position=p;o.transform.localScale=s;o.GetComponent<Renderer>().material=m;return o; }
}
