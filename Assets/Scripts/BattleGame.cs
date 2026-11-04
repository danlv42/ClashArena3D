using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleGame : MonoBehaviour
{
    private Camera cam; private Canvas canvas;
    private Text elixirText, timerText, statusText; private Slider elixirBar;
    private readonly Button[] cards=new Button[4];
    private readonly List<BattleUnit> units=new List<BattleUnit>();
    private readonly List<Tower> towers=new List<Tower>();
    private readonly string[] names={"KNIGHT","ARCHER","GIANT","GOBLIN"};
    private readonly int[] costs={3,3,5,2};
    private readonly float[] hitPoints={150,80,420,55}, damages={22,15,35,10}, speeds={2f,1.7f,1f,3.1f};
    private Material blue,red,gold; private float elixir=5,matchTime=180,enemyTimer=4,uiTimer; private int selected;
    private bool ended;

    public void Initialize(Camera c,Canvas cv)
    { cam=c;canvas=cv;blue=ArenaBootstrap.Mat(new Color(.17f,.46f,.94f));red=ArenaBootstrap.Mat(new Color(.9f,.21f,.18f));gold=ArenaBootstrap.Mat(new Color(1f,.72f,.12f));CreateTowers();CreateUI(); }

    private void Update()
    {
        if(cam==null||ended)return; float dt=Time.deltaTime;
        elixir=Mathf.Min(10,elixir+dt*.42f); matchTime-=dt; enemyTimer-=dt;
        if(enemyTimer<=0){Spawn(Random.Range(0,4),false,new Vector3(Random.Range(-4f,4f),0,11.5f));enemyTimer=Random.Range(7f,11f);}
        if(matchTime<=0)Finish("TIME UP");
        if(Input.GetKeyDown(KeyCode.R))UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        if(Input.GetMouseButtonDown(0)&&(EventSystem.current==null||!EventSystem.current.IsPointerOverGameObject()))Deploy();
        uiTimer+=dt;if(uiTimer>=.15f){uiTimer=0;RefreshUI();}
    }

    private void CreateTowers()
    {
        MakeTower("Blue King",new Vector3(0,0,-12),true,true);MakeTower("Blue Left",new Vector3(-6,0,-7),true,false);MakeTower("Blue Right",new Vector3(6,0,-7),true,false);
        MakeTower("Red King",new Vector3(0,0,12),false,true);MakeTower("Red Left",new Vector3(-6,0,7),false,false);MakeTower("Red Right",new Vector3(6,0,7),false,false);
    }
    private void MakeTower(string n,Vector3 p,bool friendly,bool king)
    {
        GameObject root=new GameObject(n);root.transform.position=p;
        GameObject body=GameObject.CreatePrimitive(PrimitiveType.Cube);body.transform.SetParent(root.transform,false);body.transform.localPosition=new Vector3(0,king?1.2f:.85f,0);body.transform.localScale=king?new Vector3(2.1f,2.4f,2.1f):new Vector3(1.8f,1.7f,1.8f);body.GetComponent<Renderer>().material=friendly?blue:red;
        GameObject roof=GameObject.CreatePrimitive(PrimitiveType.Cylinder);roof.transform.SetParent(root.transform,false);roof.transform.localPosition=new Vector3(0,king?2.6f:1.8f,0);roof.transform.localScale=new Vector3(1.4f,.35f,1.4f);roof.GetComponent<Renderer>().material=gold;
        Tower t=root.AddComponent<Tower>();t.Initialize(friendly,king,king?1500:950,king?22:16,this);towers.Add(t);
    }

    private void CreateUI()
    {
        GameObject top=Panel("Top HUD",canvas.transform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-12),new Vector2(430,82),new Color(.06f,.1f,.16f,.9f));
        timerText=Label(top.transform,"03:00",25,TextAnchor.MiddleCenter,new Vector2(0,17),new Vector2(150,38));statusText=Label(top.transform,"BLUE TEAM  VS  RED TEAM",15,TextAnchor.MiddleCenter,new Vector2(0,-20),new Vector2(390,28));
        GameObject bottom=Panel("Cards",canvas.transform,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,12),new Vector2(680,142),new Color(.06f,.09f,.15f,.95f));
        elixirText=Label(bottom.transform,"ELIXIR 5.0",17,TextAnchor.MiddleLeft,new Vector2(-260,48),new Vector2(170,28));
        elixirBar=bottom.AddComponent<Slider>();elixirBar.minValue=0;elixirBar.maxValue=10;elixirBar.value=elixir;
        RectTransform br=elixirBar.GetComponent<RectTransform>();br.anchorMin=br.anchorMax=new Vector2(.5f,.5f);br.anchoredPosition=new Vector2(20,48);br.sizeDelta=new Vector2(300,14);
        GameObject bg=new GameObject("Background",typeof(RectTransform),typeof(Image));bg.transform.SetParent(elixirBar.transform,false);bg.GetComponent<Image>().color=new Color(.23f,.16f,.31f);
        GameObject area=new GameObject("Fill Area",typeof(RectTransform));area.transform.SetParent(elixirBar.transform,false);
        GameObject fill=new GameObject("Fill",typeof(RectTransform),typeof(Image));fill.transform.SetParent(area.transform,false);fill.GetComponent<Image>().color=new Color(.72f,.28f,.95f);elixirBar.fillRect=fill.GetComponent<RectTransform>();
        GameObject row=new GameObject("Card Row",typeof(RectTransform),typeof(HorizontalLayoutGroup));row.transform.SetParent(bottom.transform,false);RectTransform rr=row.GetComponent<RectTransform>();rr.anchorMin=rr.anchorMax=new Vector2(.5f,.5f);rr.anchoredPosition=new Vector2(0,-24);rr.sizeDelta=new Vector2(620,78);
        HorizontalLayoutGroup hl=row.GetComponent<HorizontalLayoutGroup>();hl.spacing=10;hl.childAlignment=TextAnchor.MiddleCenter;
        for(int i=0;i<4;i++){int index=i;GameObject card=new GameObject("Card "+names[i],typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));card.transform.SetParent(row.transform,false);card.GetComponent<Image>().color=i==0?new Color(.36f,.55f,.88f):new Color(.24f,.3f,.41f);LayoutElement le=card.GetComponent<LayoutElement>();le.preferredWidth=145;le.preferredHeight=74;Text label=Label(card.transform,names[i]+"\n"+costs[i]+" ELIXIR",15,TextAnchor.MiddleCenter,Vector2.zero,new Vector2(140,70));label.raycastTarget=false;cards[i]=card.GetComponent<Button>();cards[i].onClick.AddListener(()=>Select(index));}
        Label(canvas.transform,"SELECT A CARD • CLICK YOUR HALF OF THE ARENA TO DEPLOY",14,TextAnchor.MiddleCenter,new Vector2(0,165),new Vector2(800,30),new Vector2(.5f,0));
    }
    private GameObject Panel(string n,Transform parent,Vector2 amin,Vector2 amax,Vector2 pos,Vector2 size,Color color)
    {GameObject o=new GameObject(n,typeof(RectTransform),typeof(Image));o.transform.SetParent(parent,false);RectTransform r=o.GetComponent<RectTransform>();r.anchorMin=amin;r.anchorMax=amax;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;o.GetComponent<Image>().color=color;return o;}
    private Text Label(Transform parent,string value,int size,TextAnchor align,Vector2 pos,Vector2 dims,Vector2? anchor=null)
    {GameObject o=new GameObject("Text",typeof(RectTransform),typeof(Text));o.transform.SetParent(parent,false);RectTransform r=o.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor??new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=dims;Text t=o.GetComponent<Text>();t.text=value;t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");t.fontSize=size;t.alignment=align;t.color=Color.white;return t;}
    private void Select(int i){selected=i;for(int k=0;k<4;k++)cards[k].GetComponent<Image>().color=k==selected?new Color(.36f,.55f,.88f):new Color(.24f,.3f,.41f);}
    private void Deploy()
    {if(elixir<costs[selected])return;Ray ray=cam.ScreenPointToRay(Input.mousePosition);if(Physics.Raycast(ray,out RaycastHit hit,100)&&hit.point.z< -1.7f&&Mathf.Abs(hit.point.x)<7.5f){Vector3 p=hit.point;p.y=0;elixir-=costs[selected];Spawn(selected,true,p);}}
    private void Spawn(int type,bool friendly,Vector3 pos)
    {GameObject root=new GameObject((friendly?"Blue ":"Red ")+names[type]);root.transform.position=pos;GameObject body=GameObject.CreatePrimitive(type==2?PrimitiveType.Capsule:PrimitiveType.Sphere);body.transform.SetParent(root.transform,false);body.transform.localPosition=new Vector3(0,.55f,0);body.transform.localScale=type==2?new Vector3(1.05f,1.15f,1.05f):new Vector3(.78f,.85f,.78f);body.GetComponent<Renderer>().material=friendly?blue:red;GameObject head=GameObject.CreatePrimitive(PrimitiveType.Sphere);head.transform.SetParent(root.transform,false);head.transform.localPosition=new Vector3(0,1.28f,0);head.transform.localScale=Vector3.one*.48f;head.GetComponent<Renderer>().material=gold;BattleUnit u=root.AddComponent<BattleUnit>();u.Initialize(friendly,hitPoints[type],damages[type],speeds[type],this);units.Add(u);}
    public Transform FindTarget(BattleUnit unit,float range,out Tower foundTower)
    {foundTower=null;Transform best=null;float bestD=range;foreach(BattleUnit other in units){if(other==null||other.IsFriendly==unit.IsFriendly||other.Health<=0)continue;float d=Vector3.Distance(unit.transform.position,other.transform.position);if(d<bestD){bestD=d;best=other.transform;}}if(best!=null)return best;foreach(Tower tower in towers){if(tower==null||tower.IsFriendly==unit.IsFriendly||tower.Health<=0)continue;float d=Vector3.Distance(unit.transform.position,tower.transform.position);if(d<bestD){bestD=d;best=tower.transform;foundTower=tower;}}return best;}
    public void Remove(BattleUnit unit){units.Remove(unit);} public void TowerDown(Tower tower){if(tower.IsKing)Finish(tower.IsFriendly?"RED TEAM WINS":"BLUE TEAM WINS");}
    private void Finish(string result){ended=true;if(statusText!=null)statusText.text=result+" • PRESS R TO RESTART";}
    private void RefreshUI(){if(elixirText!=null)elixirText.text="ELIXIR "+elixir.ToString("0.0");if(elixirBar!=null)elixirBar.value=elixir;if(timerText!=null)timerText.text=Mathf.Max(0,Mathf.FloorToInt(matchTime/60)).ToString("00")+":"+Mathf.Max(0,Mathf.FloorToInt(matchTime%60)).ToString("00");}
}

public class BattleUnit:MonoBehaviour
{
    public bool IsFriendly{get;private set;} public float Health{get;private set;}
    private float damage,speed,cooldown;private BattleGame game;
    public void Initialize(bool friendly,float hp,float attack,float move,BattleGame owner){IsFriendly=friendly;Health=hp;damage=attack;speed=move;game=owner;}
    private void Update()
    {if(Health<=0||game==null)return;cooldown-=Time.deltaTime;Tower tower;Transform target=game.FindTarget(this,1.65f,out tower);if(target!=null){if(cooldown<=0){BattleUnit enemy=target.GetComponent<BattleUnit>();if(enemy!=null)enemy.TakeDamage(damage);else if(tower!=null)tower.TakeDamage(damage);cooldown=.85f;}return;}float dir=IsFriendly?1:-1;Vector3 p=transform.position;p+=Vector3.forward*dir*speed*Time.deltaTime;p.x=Mathf.Lerp(p.x,0,.012f);transform.position=p;transform.rotation=Quaternion.LookRotation(Vector3.forward*dir);}
    public void TakeDamage(float amount){Health-=amount;if(Health<=0){game.Remove(this);Destroy(gameObject);}}
}
public class Tower:MonoBehaviour
{
    public bool IsFriendly{get;private set;} public bool IsKing{get;private set;} public float Health{get;private set;}
    private float damage,cooldown;private BattleGame game;
    public void Initialize(bool friendly,bool king,float hp,float attack,BattleGame owner){IsFriendly=friendly;IsKing=king;Health=hp;damage=attack;game=owner;}
    private void Update(){cooldown-=Time.deltaTime;if(cooldown>0)return;BattleUnit closest=null;float distance=5.8f;foreach(BattleUnit unit in FindObjectsOfType<BattleUnit>()){if(unit.IsFriendly==IsFriendly||unit.Health<=0)continue;float d=Vector3.Distance(transform.position,unit.transform.position);if(d<distance){distance=d;closest=unit;}}if(closest!=null){closest.TakeDamage(damage);cooldown=1f;}}
    public void TakeDamage(float amount){Health-=amount;if(Health<=0){game.TowerDown(this);Destroy(gameObject);}}
}
