using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace JinYanMaoYing
{
    public sealed class TaiHeFinale : MonoBehaviour
    {
        private const float RunFrameDuration = 0.12f;
        private const float AirborneTiltDegrees = 12f;
        // Serialized fields and callbacks remain for the existing scene and ending.
        public Button[] parts;
        public Text instruction, feedback, progress;
        public GameObject completionPanel;
        public LevelIntroVideo introVideo;

        private readonly string[] names = { "前庭 · 丹陛台基", "檐下 · 修缮木架", "正殿 · 御座龙纹" };
        private readonly string[] piecesNames = { "台基石雕", "斗拱彩绘", "御座龙纹" };
        private readonly Vector2[] pickups = {
            new Vector2(-690f,-285f), new Vector2(-125f,9f), new Vector2(-620f,-25f)
        };
        private readonly Vector2[] sockets = {
            new Vector2(635f,45f), new Vector2(330f,290f), new Vector2(70f,-100f)
        };
        private readonly int[] requiredTurns = { 1, 3, 2 };
        private readonly bool[] taken = new bool[3], solved = new bool[3];
        private readonly Texture2D[] scenes = new Texture2D[3], runs = new Texture2D[3];
        private readonly Material[] materials = new Material[3];
        private Texture2D items;
        private RectTransform root, cat;
        private RawImage backdrop, catArt, collectible, silhouette, carriedArt, rotatingArt, targetArt;
        private Text title, map, score, objective, hint, assemblyHint;
        private CanvasGroup fade;
        private GameObject assembly;
        private int room, carrying = -1, jumps, turns, facing = 1;
        private float x = -820f, feet = -365f, velocity, runFrameAccumulator, hintEnd;
        private int runFrameIndex;
        private bool transitioning, inAssembly, finished;
        private AudioSource runAudioSource;
        private AudioSource effectAudioSource;
        private AudioClip jumpClip, landingClip, interactionClip;
        private Quaternion catRestRotation;
        private const float Gravity = 1450f;
        private const float HalfCat = 40f;

        private struct Deck {
            public float a,b,y;
            public Deck(float left,float right,float height) { a=left; b=right; y=height; }
        }
        // Only the visible, supported scaffold decks are walkable.
        private static readonly Deck[] eaves = {
            new Deck(-620f,405f,-65f), new Deck(420f,815f,10f),
            new Deck(-465f,760f,215f)
        };
        private static readonly Deck[] hall = {
            new Deck(-805f,-445f,-100f), new Deck(445f,805f,-100f)
        };

        private void Start()
        {
            if (completionPanel != null) completionPanel.SetActive(false);
            if (parts != null) foreach (Button button in parts) if (button != null) button.gameObject.SetActive(false);
            if (instruction != null) instruction.gameObject.SetActive(false);
            if (feedback != null) feedback.gameObject.SetActive(false);
            if (progress != null) progress.gameObject.SetActive(false);
            Canvas canvas = FindSceneCanvas();
            if (canvas == null) { Debug.LogError("太和殿缺少画布"); enabled=false; return; }
            string[] sceneFiles = { "前庭台基", "檐下修缮架", "正殿内景" };
            for (int i=0;i<3;i++) {
                scenes[i]=Resources.Load<Texture2D>("TaiHeFinale/"+sceneFiles[i]);
                runs[i]=Resources.Load<Texture2D>("TaiHeFinale/猫跑"+(i+1));
            }
            items=Resources.Load<Texture2D>("TaiHeFinale/复色构件");
            Shader shader=Resources.Load<Shader>("TaiHeReveal");
            if (shader == null || items == null || scenes[0] == null || scenes[1] == null || scenes[2] == null)
                Debug.LogError("太和殿场景或复色素材缺失");
            if (shader != null) for(int i=0;i<3;i++) {
                materials[i]=new Material(shader);
                materials[i].SetFloat("_RevealX",(sockets[i].x+960f)/1920f);
                materials[i].SetFloat("_RevealY",(sockets[i].y+540f)/1080f);
                materials[i].SetFloat("_RevealRadius",0f);
            }
            Build(canvas.transform);
            Enter(0,true);
            Tell("A/D 移动 · 空格二段跳 · 梯口 ↑/↓ 攀爬 · 靠近构件按 E",8f);
        }
        private Canvas FindSceneCanvas()
        {
            foreach (GameObject sceneRoot in gameObject.scene.GetRootGameObjects())
            {
                Canvas sceneCanvas = sceneRoot.GetComponent<Canvas>();
                if (sceneCanvas != null) return sceneCanvas;
            }
            return null;
        }
        private void OnDestroy() {
            if (root != null) Destroy(root.gameObject);
            foreach(Material material in materials) if(material!=null) Destroy(material);
        }
        private void LateUpdate()
        {
            // Both behaviours start on the same object; keep the CG above the runtime level.
            if (introVideo == null || !introVideo.IsPlaying || root == null) return;
            Transform video = root.parent.Find("关卡CG");
            if (video != null) video.SetAsLastSibling();
        }

        private void Build(Transform canvas)
        {
            root=Rect(canvas,"太和殿三景复色关卡",new Vector2(1920,1080),Vector2.zero);
            root.anchorMin=Vector2.zero;
            root.anchorMax=Vector2.one;
            root.offsetMin=Vector2.zero;
            root.offsetMax=Vector2.zero;
            backdrop=Raw(root,"建筑底图",Vector2.zero,new Vector2(1920,1080),scenes[0]);
            backdrop.rectTransform.anchorMin=Vector2.zero;
            backdrop.rectTransform.anchorMax=Vector2.one;
            backdrop.rectTransform.offsetMin=Vector2.zero;
            backdrop.rectTransform.offsetMax=Vector2.zero;
            backdrop.raycastTarget=false;
            Panel(root,"顶部墨色",new Vector2(1920,144),new Vector2(0,466),
                new Color(.13f,.10f,.08f,.78f));
            title=Text(root,"场景标题","",46,new Vector2(760,70),new Vector2(0,488),Color.white);
            map=Text(root,"场景位置","",31,new Vector2(970,55),new Vector2(-340,412),Color.white);
            score=Text(root,"复色进度","",33,new Vector2(400,55),new Vector2(590,412),Color.white);
            Button(root,"返回地图",new Vector2(220,69),new Vector2(830,482),OpenMap);
            silhouette=Raw(root,"建筑缺口",Vector2.zero,new Vector2(184,162),items);
            silhouette.color=new Color(.18f,.16f,.14f,.84f);
            silhouette.raycastTarget=false;
            Outline rim=silhouette.gameObject.AddComponent<Outline>();
            rim.effectColor=new Color(1f,.92f,.72f,.95f);
            collectible=Raw(root,"带色构件",Vector2.zero,new Vector2(175,155),items);
            collectible.raycastTarget=false;
            collectible.gameObject.AddComponent<CollectibleGlow>();
            catArt=Raw(root,"墨宝",Vector2.zero,new Vector2(164,88),runs[0]);
            catArt.raycastTarget=false;
            cat=catArt.rectTransform;
            catRestRotation=cat.localRotation;
            AudioClip runClip=Resources.Load<AudioClip>("Audio/Cat_Run");
            if(runClip!=null) {
                runAudioSource=gameObject.AddComponent<AudioSource>();
                runAudioSource.clip=runClip;
                runAudioSource.loop=true;
                runAudioSource.playOnAwake=false;
                runAudioSource.spatialBlend=0f;
                runAudioSource.pitch=1f;
                runAudioSource.volume=.65f;
            }
            jumpClip=Resources.Load<AudioClip>("Audio/Jump_Takeoff");
            landingClip=Resources.Load<AudioClip>("Audio/Jump_Land");
            interactionClip=Resources.Load<AudioClip>("Audio/UI_Click");
            if(jumpClip!=null || landingClip!=null || interactionClip!=null) {
                effectAudioSource=gameObject.AddComponent<AudioSource>();
                effectAudioSource.playOnAwake=false;
                effectAudioSource.spatialBlend=0f;
                effectAudioSource.volume=.82f;
            }
            carriedArt=Raw(cat,"背上构件",new Vector2(0,93),new Vector2(76,70),items);
            carriedArt.gameObject.SetActive(false);
            Panel(root,"操作宣纸",new Vector2(1660,120),new Vector2(0,-465),
                new Color(.97f,.91f,.78f,.92f));
            objective=Text(root,"当前行动","",31,new Vector2(1590,48),
                new Vector2(0,-440),new Color(.22f,.14f,.10f));
            hint=Text(root,"提示","",28,new Vector2(1590,48),
                new Vector2(0,-491),new Color(.37f,.20f,.14f));
            BuildAssembly();
            Image curtain=Panel(root,"切换渐隐",new Vector2(1920,1080),Vector2.zero,Color.black);
            curtain.rectTransform.anchorMin=Vector2.zero;
            curtain.rectTransform.anchorMax=Vector2.one;
            curtain.rectTransform.offsetMin=Vector2.zero;
            curtain.rectTransform.offsetMax=Vector2.zero;
            curtain.raycastTarget=false;
            fade=curtain.gameObject.AddComponent<CanvasGroup>();
            fade.alpha=0; fade.blocksRaycasts=false;
        }
        private void BuildAssembly()
        {
            RectTransform panel=Rect(root,"亲手拼合",new Vector2(1920,1080),Vector2.zero);
            assembly=panel.gameObject;
            Panel(panel,"暗幕",new Vector2(1920,1080),Vector2.zero,new Color(.06f,.05f,.04f,.76f));
            Image paper=Panel(panel,"宣纸框",new Vector2(1050,650),Vector2.zero,
                new Color(.97f,.93f,.82f,.99f));
            paper.gameObject.AddComponent<Outline>().effectColor=new Color(.32f,.20f,.15f);
            Text(panel,"标题","转正彩色构件，嵌入失色轮廓",45,new Vector2(910,78),
                new Vector2(0,240),new Color(.20f,.13f,.10f));
            targetArt=Raw(panel,"缺口剪影",new Vector2(215,42),new Vector2(230,210),items);
            targetArt.color=new Color(.20f,.18f,.15f,.55f);
            rotatingArt=Raw(panel,"可转构件",new Vector2(-215,42),new Vector2(230,210),items);
            Button(panel,"左转 A",new Vector2(220,72),new Vector2(-275,-190),()=>Rotate(-1));
            Button(panel,"嵌合 空格",new Vector2(240,72),new Vector2(0,-190),Assemble);
            Button(panel,"右转 D",new Vector2(220,72),new Vector2(275,-190),()=>Rotate(1));
            Button(panel,"返回地图",new Vector2(220,69),new Vector2(830,482),OpenMap);
            assemblyHint=Text(panel,"提示","",30,new Vector2(900,65),
                new Vector2(0,-100),new Color(.42f,.22f,.16f));
            assembly.SetActive(false);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape)) { OpenMap(); return; }
            if(finished || transitioning || introVideo!=null && introVideo.IsPlaying) {
                SetRunAudio(false); return;
            }
            if(inAssembly) {
                SetRunAudio(false);
                if(Input.GetKeyDown(KeyCode.A)||Input.GetKeyDown(KeyCode.LeftArrow)) {
                    PlayEffectSound(interactionClip,.72f); Rotate(-1);
                }
                if(Input.GetKeyDown(KeyCode.D)||Input.GetKeyDown(KeyCode.RightArrow)) {
                    PlayEffectSound(interactionClip,.72f); Rotate(1);
                }
                if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Return)) {
                    PlayEffectSound(interactionClip,.78f); Assemble();
                }
                return;
            }
            float verticalInput=Input.GetAxisRaw("Vertical");
            float dt=Mathf.Min(Time.deltaTime,.04f), oldX=x, oldFeet=feet;
            bool wasAirborne=jumps>0 || velocity>0f;
            float move=Input.GetAxisRaw("Horizontal");
            if(Mathf.Abs(move)>.1f) facing=move<0?-1:1;
            x=Mathf.Clamp(x+move*390f*dt,-970f,970f);
            bool climbing=Climb(dt);
            if(!climbing) {
                if((Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.UpArrow))&&jumps<2) {
                    velocity=jumps==0?685f:630f; jumps++;
                    PlayEffectSound(jumpClip,.80f);
                }
                if(jumps==0 && Mathf.Abs(oldFeet-Ground(room,oldX))<15f && velocity<=0)
                    feet=Ground(room,x);
                else {
                    velocity-=Gravity*dt; feet+=velocity*dt; Land(oldFeet);
                }
            }
            if(wasAirborne && jumps==0 && !climbing) PlayEffectSound(landingClip,.88f);
            cat.anchoredPosition=new Vector2(x,feet+HalfCat);
            cat.localScale=new Vector3(facing,1,1);
            bool airborne=jumps>0 || velocity>0f;
            bool movingOnGround=!airborne && Mathf.Abs(move)>.1f && Mathf.Abs(verticalInput)<.1f;
            if(airborne) cat.localRotation=catRestRotation*Quaternion.Euler(0f,0f,AirborneTiltDegrees);
            else {
                cat.localRotation=catRestRotation;
                if(movingOnGround&&runs[0]!=null) {
                    runFrameAccumulator+=dt;
                    while(runFrameAccumulator>=RunFrameDuration) {
                        runFrameAccumulator-=RunFrameDuration;
                        runFrameIndex=(runFrameIndex+1)%runs.Length;
                    }
                    catArt.texture=runs[runFrameIndex];
                }
            }
            SetRunAudio(movingOnGround);
            if(Input.GetKeyDown(KeyCode.E)) Interact();
            if(x>943f||x< -943f) {
                int next=x>0?room+1:room-1;
                if(carrying>=0) { x=x>0?905f:-905f; Tell("先把背上的构件送回本景缺口。",3f); }
                else if(next>=0&&next<3) StartCoroutine(Change(next,x>0));
                else x=x>0?905f:-905f;
            }
            if(Time.time>hintEnd) hint.text="A/D 移动 · 空格二段跳 · 梯口 ↑/↓ 攀爬 · E 交互";
        }
        private void SetRunAudio(bool shouldPlay)
        {
            if(runAudioSource==null) return;
            runAudioSource.pitch=room==1?1.04f:room==2?1.01f:.97f;
            if(shouldPlay) { if(!runAudioSource.isPlaying) runAudioSource.Play(); }
            else if(runAudioSource.isPlaying) runAudioSource.Stop();
        }
        private void PlayEffectSound(AudioClip clip,float volume)
        {
            if(effectAudioSource!=null && clip!=null) effectAudioSource.PlayOneShot(clip,volume);
        }
        private bool Climb(float dt)
        {
            float v=Input.GetAxisRaw("Vertical");
            if(Mathf.Abs(v)<.1f) return false;
            float lx=0,bottom=0,top=0;
            bool active=false;
            if(room==1) {
                if(Mathf.Abs(x+590f)<85f && feet>=-355f && feet<=-45f)
                { lx=-590; bottom=-340; top=-65; active=true; }
                else if(Mathf.Abs(x-675f)<90f && feet>=-85f && feet<=225f)
                { lx=675; bottom=10; top=215; active=true; }
            } else if(room==2 && Mathf.Abs(x+630f)<85f && feet>=-405f && feet<=-80f)
            { lx=-630; bottom=-390; top=-100; active=true; }
            if(!active) return false;
            x=Mathf.MoveTowards(x,lx,350f*dt);
            feet=Mathf.Clamp(feet+v*390f*dt,bottom,top);
            velocity=0; jumps=0;
            return true;
        }
        private void Land(float oldFeet)
        {
            if(velocity>0) return;
            float floor=Ground(room,x);
            Deck[] decks=room==1?eaves:room==2?hall:null;
            if(decks!=null) foreach(Deck d in decks)
                if(x>=d.a&&x<=d.b&&oldFeet>=d.y-5f&&feet<=d.y)
                    floor=Mathf.Max(floor,d.y);
            if(feet<=floor && oldFeet>=floor-18f)
            { feet=floor; velocity=0; jumps=0; }
        }
        private static float Ground(int scene,float px)
        {
            if(scene==0) return Mathf.Lerp(-365f,20f,Mathf.InverseLerp(-460f,470f,px));
            if(scene==1) return -340f;
            if(px< -385f) return -390f;
            if(px< -165f) return Mathf.Lerp(-390f,-185f,Mathf.InverseLerp(-385f,-165f,px));
            if(px<225f) return -185f;
            if(px<440f) return Mathf.Lerp(-185f,-390f,Mathf.InverseLerp(225f,440f,px));
            return -390f;
        }
        private void Interact()
        {
            Vector2 paws=new Vector2(x,feet+56f);
            if(!taken[room]&&carrying<0&&Vector2.Distance(paws,pickups[room])<155f) {
                PlayEffectSound(interactionClip,.78f);
                taken[room]=true; carrying=room;
                collectible.gameObject.SetActive(false);
                carriedArt.gameObject.SetActive(true);
                carriedArt.uvRect=UV(room);
                Hud(); Tell("带色构件已背起。穿过台阶或修缮架，送到对应轮廓。",5f);
            } else if(carrying==room&&Vector2.Distance(paws,sockets[room])<165f) {
                PlayEffectSound(interactionClip,.78f); OpenAssembly();
            }
            else Tell(carrying==room?"缺口还在前方，继续沿可走路线前进。":
                solved[room]?"本景已复色，从画面边缘前往相邻场景。":
                "循着彩色构件靠近，按 E 拾起。",3f);
        }
        private void OpenAssembly()
        {
            inAssembly=true; turns=0;
            rotatingArt.uvRect=targetArt.uvRect=UV(room);
            rotatingArt.rectTransform.localEulerAngles=Vector3.zero;
            targetArt.rectTransform.localEulerAngles=new Vector3(0,0,-90f*requiredTurns[room]);
            assemblyHint.text="A / D 转动构件；角度对准后按空格嵌合。";
            assembly.SetActive(true);
        }
        private void Rotate(int direction)
        {
            turns=(turns+direction+4)%4;
            rotatingArt.rectTransform.localEulerAngles=new Vector3(0,0,-90f*turns);
        }
        private void Assemble()
        {
            if(!inAssembly) return;
            if(turns!=requiredTurns[room]) {
                assemblyHint.text="角度尚未对齐缺口，请再转动构件。"; return;
            }
            inAssembly=false; assembly.SetActive(false);
            carrying=-1; carriedArt.gameObject.SetActive(false);
            solved[room]=true; silhouette.color=Color.white;
            StartCoroutine(Reveal(room));
        }
        private IEnumerator Reveal(int index)
        {
            transitioning=true;
            Tell("「"+piecesNames[index]+"」归位，颜色正从缺口蔓延至建筑。",5f);
            if(materials[index]!=null) for(float t=0;t<2.5f;t+=Time.deltaTime) {
                materials[index].SetFloat("_RevealRadius",
                    Mathf.Lerp(0,1.7f,Mathf.SmoothStep(0,1,t/2.5f)));
                yield return null;
            }
            if(materials[index]!=null) materials[index].SetFloat("_RevealRadius",1.7f);
            Hud();
            if(solved[0]&&solved[1]&&solved[2]) {
                finished=true;
                GameNavigator.TaiHeDianCleared=true;
                if(completionPanel!=null) {
                    completionPanel.SetActive(true);
                    completionPanel.transform.SetAsLastSibling();
                }
            }
            transitioning=false;
        }
        private IEnumerator Change(int next,bool forward)
        {
            transitioning=true; fade.blocksRaycasts=true;
            for(float t=0;t<.45f;t+=Time.deltaTime) {
                fade.alpha=Mathf.Clamp01(t/.45f); yield return null;
            }
            Enter(next,forward);
            for(float t=0;t<.45f;t+=Time.deltaTime) {
                fade.alpha=1f-Mathf.Clamp01(t/.45f); yield return null;
            }
            fade.alpha=0; fade.blocksRaycasts=false; transitioning=false;
        }
        private void Enter(int next,bool fromLeft)
        {
            room=next; backdrop.texture=scenes[room]; backdrop.material=materials[room];
            collectible.uvRect=silhouette.uvRect=UV(room);
            collectible.rectTransform.anchoredPosition=pickups[room];
            silhouette.rectTransform.anchoredPosition=sockets[room];
            collectible.gameObject.SetActive(!taken[room]);
            silhouette.color=solved[room]?Color.white:new Color(.18f,.16f,.14f,.84f);
            x=fromLeft?-845f:845f; feet=Ground(room,x); velocity=0; jumps=0;
            cat.anchoredPosition=new Vector2(x,feet+HalfCat);
            Hud();
            Tell(room==0?"沿画中的丹陛石阶登上月台，寻找台基缺口。":
                room==1?"左侧梯口通向低架，右侧梯口通向檐下高架。":
                "左侧修缮梯通向高架，中央石阶通向殿内台座。",7f);
        }
        private void Hud()
        {
            title.text=names[room];
            map.text=(room==0?"●":solved[0]?"◆":"○")+" 前庭    "+
                (room==1?"●":solved[1]?"◆":"○")+" 檐下    "+
                (room==2?"●":solved[2]?"◆":"○")+" 正殿";
            int count=(solved[0]?1:0)+(solved[1]?1:0)+(solved[2]?1:0);
            score.text="建筑复色 "+count+"/3";
            objective.text=solved[room]?"此处已复色，走到边缘前往相邻场景。":
                taken[room]?"将「"+piecesNames[room]+"」背到失色轮廓，按 E 拼合。":
                "寻找「"+piecesNames[room]+"」；靠近后按 E 背起。";
        }
        private void Tell(string message,float seconds) {
            hint.text=message; hintEnd=Time.time+seconds;
        }
        // Crop out the narrow transparent boundaries between the generated icons.
        private static Rect UV(int index) {
            if(index==0) return new Rect(0f,0f,.345f,1f);
            if(index==1) return new Rect(.345f,0f,.340f,1f);
            return new Rect(.705f,0f,.295f,1f);
        }
        private static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 pos) {
            GameObject go=new GameObject(name,typeof(RectTransform));
            go.transform.SetParent(parent,false);
            RectTransform r=go.GetComponent<RectTransform>();
            r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);
            r.sizeDelta=size; r.anchoredPosition=pos; return r;
        }
        private static RawImage Raw(Transform parent,string name,Vector2 pos,Vector2 size,Texture texture) {
            RectTransform r=Rect(parent,name,size,pos);
            RawImage image=r.gameObject.AddComponent<RawImage>(); image.texture=texture; return image;
        }
        private static Image Panel(Transform parent,string name,Vector2 size,Vector2 pos,Color color) {
            RectTransform r=Rect(parent,name,size,pos);
            Image image=r.gameObject.AddComponent<Image>(); image.color=color; return image;
        }
        private Text Text(Transform parent,string name,string value,int size,Vector2 box,Vector2 pos,Color color) {
            RectTransform r=Rect(parent,name,box,pos);
            Text t=r.gameObject.AddComponent<Text>();
            t.font=instruction!=null&&instruction.font!=null?instruction.font:
                Font.CreateDynamicFontFromOSFont("Microsoft YaHei",size);
            t.fontSize=size; t.text=value; t.color=color;
            t.alignment=TextAnchor.MiddleCenter; t.raycastTarget=false; return t;
        }
        private void Button(Transform parent,string label,Vector2 size,Vector2 pos,UnityAction callback) {
            Image image=Panel(parent,label,size,pos,new Color(.20f,.14f,.11f,.93f));
            UnityEngine.UI.Button button=image.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(callback);
            image.gameObject.AddComponent<InkHoverButton>();
            Text(image.transform,"按钮文字",label,31,size,Vector2.zero,new Color(1f,.94f,.80f));
        }
        public void SelectPart(int index) { if(hint!=null) Tell("请移动墨宝，亲自寻找彩色构件。",3f); }
        public void OpenEnding() { SceneTransition.Load(GameNavigator.Ending); }
        public void OpenMap() { SceneTransition.Load(GameNavigator.Map); }
    }
}
