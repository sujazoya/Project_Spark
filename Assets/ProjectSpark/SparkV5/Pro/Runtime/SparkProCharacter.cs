using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectSpark.SparkV5.Pro
{
    public enum SparkState { Idle, Listening, Thinking, Speaking, Teaching, Diagnosing, Warning, Success, Walking, Examining, Observing, Waiting }
    public enum SparkGesture { None, Explain, Point, Wave, Nod, Shake, Stop, Celebrate, Think, Inspect, Count, Offer, Grasp }
    public enum SparkMouth { Rest, Ah, Ai, Ee, Eh, FV, MBP, Oo, Ooh, RSn, S }

    // A presentation-only rig. No access to game mutation or solver APIs.
    [DisallowMultipleComponent]
    public sealed class SparkProCharacter : MonoBehaviour
    {
        [SerializeField] Transform[] modelRoots;
        [SerializeField] AudioSource voice;
        [SerializeField] NavMeshAgent agent;
        [SerializeField] Transform focus;
        [SerializeField] Material faceMaterial;
        [SerializeField, Range(15,60)] int poseHz = 60;
        [SerializeField] bool animateCrest = true;
        public SparkState State { get; private set; }
        public bool WarningActive { get; private set; }
        public AudioSource Voice => voice;
        public Transform FocusTarget => focus;
        public event Action InterruptRequested;
        public event Action<SparkState> StateChanged;
        public event Action<string> Diagnostic;
        readonly List<Rig> rigs = new List<Rig>();
        readonly float[] audio = new float[256];
        // Unity native objects and shader IDs must be initialized on the main thread.
        MaterialPropertyBlock block;
        int Emission;
        int BaseColor;
        SparkState requested; SparkGesture gesture; SparkMouth mouth;
        bool visemeValid, ready; float gestureEnd, blinkAt, blinkStart = -10, lastPose, amplitude, phase;
        Transform pointTarget; bool pointLeft; Vector3? pointPosition; bool walking;
        static readonly string[] EyeNames={"Eye_L","Eye_R"}, BrowNames={"Brow_L","Brow_R"}, GazeNames={"Gaze_L","Gaze_R"}, ThumbNames={"Thumb_L","Thumb_R"};
        static readonly string[,] Fingers={{"Finger0_L","Finger1_L","Finger2_L","Finger3_L"},{"Finger0_R","Finger1_R","Finger2_R","Finger3_R"}};
        static readonly string[,] FingerTips={{"FingerTip0_L","FingerTip1_L","FingerTip2_L","FingerTip3_L"},{"FingerTip0_R","FingerTip1_R","FingerTip2_R","FingerTip3_R"}};
        static readonly string[] Strands={"Strand_00","Strand_01","Strand_02","Strand_03","Strand_04"};
        static readonly string[] required = {"Body","Head","MouthPivot","StatusCore","Eye_L","Eye_R","Gaze_L","Gaze_R","Brow_L","Brow_R","Shoulder_L","Shoulder_R","Elbow_L","Elbow_R","Wrist_L","Wrist_R","Hip_L","Hip_R","Knee_L","Knee_R","Ankle_L","Ankle_R"};
        sealed class Rig
        {
            public readonly Dictionary<string,Transform> joints = new Dictionary<string,Transform>(StringComparer.Ordinal);
            public readonly Dictionary<Transform,Quaternion> rotations = new Dictionary<Transform,Quaternion>();
            public readonly Dictionary<Transform,Vector3> scales = new Dictionary<Transform,Vector3>();
            public readonly Dictionary<Transform,Vector3> positions = new Dictionary<Transform,Vector3>();
            public Renderer[] core, smile; public LineRenderer line; public Vector3 right, up, forward; public float localMeter;
            public Transform Get(string name) => joints.TryGetValue(name,out var t) ? t : null;
            public void Reset(){foreach(var t in rotations.Keys){if(!t) continue;t.localRotation=rotations[t];t.localScale=scales[t];t.localPosition=positions[t];} if(line) line.forceRenderingOff=true; if(smile!=null) foreach(var r in smile) if(r) r.forceRenderingOff=false;}
        }
        public void Configure(Transform[] roots, Material material, AudioSource source, NavMeshAgent navigation)
        { modelRoots=roots; faceMaterial=material; voice=source; agent=navigation; }
        public void BindVoice(AudioSource source) { voice=source; }
        public void FocusOn(Transform target) { focus=target; }
        public void SetPoseRate(int hz) { poseHz=Mathf.Clamp(hz,15,60); }
        public void SetState(SparkState value) { if(value==SparkState.Warning) { SetWarning(true); return; } requested=value; }
        public void SetWarning(bool active) { if(WarningActive==active)return; WarningActive=active; if(active){StopNavigation();ClearGesture();} }
        public void SetViseme(SparkMouth value) { mouth=value;visemeValid=true; }
        public void ClearViseme() { visemeValid=false;mouth=SparkMouth.Rest; }
        public void PlayGesture(SparkGesture value,float seconds=3) { if(WarningActive)return; ClearGesture();gesture=value;gestureEnd=Time.unscaledTime+Mathf.Clamp(seconds,.1f,30); }
        public void PointAt(Transform target,bool left=false,float seconds=4) { if(!target)return;PlayGesture(SparkGesture.Point,seconds);if(WarningActive)return;pointTarget=target;pointLeft=left;FocusOn(target); }
        public void PointAtPosition(Vector3 target,bool left=false,float seconds=4) { PlayGesture(SparkGesture.Point,seconds);if(WarningActive)return;pointPosition=target;pointLeft=left; }
        public void ClearGesture(){gesture=SparkGesture.None;pointTarget=null;pointPosition=null;}
        public void Interrupt(){ClearViseme();ClearGesture();if(InterruptRequested!=null)InterruptRequested.Invoke();else if(voice)voice.Stop();requested=SparkState.Listening;}
        public bool WalkTo(Vector3 destination)
        { if(WarningActive || !agent || !agent.isActiveAndEnabled || !agent.isOnNavMesh)return false;
          if(!NavMesh.SamplePosition(destination,out NavMeshHit hit, .5f,agent.areaMask))return false;
          var path=new NavMeshPath(); if(!agent.CalculatePath(hit.position,path)||path.status!=NavMeshPathStatus.PathComplete)return false;
          agent.isStopped=false; if(!agent.SetPath(path))return false;walking=true;return true; }
        public void StopNavigation(){walking=false;if(agent && agent.isActiveAndEnabled && agent.isOnNavMesh){agent.isStopped=true;agent.ResetPath();}}
        public Transform GetAnchor(string name){return rigs.Count>0?rigs[0].Get(name):null;}
        void Awake()
        {
            block = new MaterialPropertyBlock();
            Emission = Shader.PropertyToID("_EmissionColor");
            BaseColor = Shader.PropertyToID("_BaseColor");
            if(modelRoots==null||modelRoots.Length==0)modelRoots=new[]{transform};
            foreach(var root in modelRoots){if(!root)continue;var r=new Rig();
                foreach(var t in root.GetComponentsInChildren<Transform>(true)){if(!r.joints.ContainsKey(t.name))r.joints.Add(t.name,t);r.rotations[t]=t.localRotation;r.positions[t]=t.localPosition;r.scales[t]=t.localScale;}
                bool valid=true;foreach(var n in required)if(!r.Get(n)){valid=false;Debug.LogError("[Spark Pro] Missing rig pivot "+n+" in "+root.name,this);}
                if(!valid)continue;
                var mp=r.Get("MouthPivot");var head=r.Get("Head");
                Vector3 front=Vector3.ProjectOnPlane(mp.position-head.position,transform.up).normalized;
                if(front.sqrMagnitude<.5f)front=transform.forward;
                r.forward=mp.InverseTransformDirection(front);r.up=mp.InverseTransformDirection(transform.up);r.right=Vector3.Cross(r.up,r.forward).normalized;
                r.localMeter=1f/Mathf.Max(.001f,mp.lossyScale.magnitude/Mathf.Sqrt(3));
                r.core=r.Get("StatusCore").GetComponentsInChildren<Renderer>(true);
                r.line=mp.GetComponent<LineRenderer>();
                var smiles=new List<Renderer>();foreach(var rr in mp.GetComponentsInChildren<Renderer>(true))if(rr!=r.line)smiles.Add(rr);r.smile=smiles.ToArray();
                if(r.line){r.line.useWorldSpace=false;r.line.loop=true;r.line.positionCount=32;r.line.widthMultiplier=.009f*r.localMeter;r.line.forceRenderingOff=true;if(faceMaterial)r.line.sharedMaterial=faceMaterial;}
                rigs.Add(r);
            }
            if(Application.isMobilePlatform)poseHz=Mathf.Min(poseHz,30);
            ready=rigs.Count>0;blinkAt=Time.unscaledTime+2;lastPose=Time.unscaledTime;
            if(!ready){Diagnostic?.Invoke("No valid V5 rig.");enabled=false;}
        }
        void OnDisable(){foreach(var r in rigs)r.Reset();StopNavigation();ClearGesture();ClearViseme();foreach(var r in rigs)if(r.core!=null)foreach(var rr in r.core)if(rr)rr.SetPropertyBlock(null);}
        void LateUpdate()
        {
            if(!ready)return;float now=Time.unscaledTime;if(now-lastPose<1f/poseHz)return;float dt=Mathf.Min(.1f,now-lastPose);lastPose=now;float k=1-Mathf.Exp(-13*dt);
            if(walking && (!agent||!agent.isOnNavMesh||(!agent.pathPending&&(!agent.hasPath||agent.remainingDistance<=agent.stoppingDistance+.04f))))StopNavigation();
            bool speaking=voice && voice.isPlaying;SparkState next=WarningActive?SparkState.Warning: speaking?SparkState.Speaking:walking?SparkState.Walking:requested;
            if(next!=State){State=next;StateChanged?.Invoke(next);}
            if(now>=gestureEnd)ClearGesture();if(now>=blinkAt){blinkStart=now;blinkAt=now+UnityEngine.Random.Range(2.5f,5.2f);}
            float bp=(now-blinkStart)/.17f;float blink=bp>=0&&bp<=1?1-.94f*Mathf.Sin(bp*Mathf.PI):1;
            float rms=0;if(speaking){voice.GetOutputData(audio,0);for(int i=0;i<audio.Length;i++)rms+=audio[i]*audio[i];rms=Mathf.Clamp01(Mathf.Sqrt(rms/audio.Length)*10);}
            amplitude=Mathf.Lerp(amplitude,rms,k);phase+=dt*(walking?9f:2f);
            foreach(var r in rigs)Animate(r,now,k,blink,speaking);
        }
        void Rotate(Rig r,string name,Vector3 angle,float k){var t=r.Get(name);if(t)t.localRotation=Quaternion.Slerp(t.localRotation,r.rotations[t]*Quaternion.Euler(angle),k);}
        void Scale(Rig r,string name,Vector3 scale,float k){var t=r.Get(name);if(t)t.localScale=Vector3.Lerp(t.localScale,Vector3.Scale(r.scales[t],scale),k);}
        void Animate(Rig r,float now,float k,float blink,bool speaking)
        {
            float breathe=Mathf.Sin(phase);bool warn=State==SparkState.Warning;bool think=State==SparkState.Thinking;bool happy=State==SparkState.Success;
            var head=r.Get("Head");var mp=r.Get("MouthPivot");
            // Aim in the imported model's actual forward direction (derived from the visor), not assumed FBX axes.
            Quaternion old=head.localRotation;head.localRotation=r.rotations[head];Vector3 forward=mp.TransformDirection(r.forward), up=mp.TransformDirection(r.up);Quaternion targetRot=head.rotation;
            if(focus){Vector3 direction=focus.position-head.position;if(direction.sqrMagnitude>.0001f){Quaternion aim=Quaternion.FromToRotation(forward,direction.normalized);targetRot=Quaternion.RotateTowards(head.rotation,aim*head.rotation,32);}}
            float tilt=think?-6:State==SparkState.Listening?5:happy?3:0;targetRot=Quaternion.AngleAxis(tilt,forward)*targetRot;
            if(gesture==SparkGesture.Nod)targetRot=Quaternion.AngleAxis(Mathf.Sin(now*9)*9,Vector3.Cross(up,forward))*targetRot;
            if(gesture==SparkGesture.Shake)targetRot=Quaternion.AngleAxis(Mathf.Sin(now*10)*12,up)*targetRot;
            Quaternion local=Quaternion.Inverse(head.parent.rotation)*targetRot;head.localRotation=Quaternion.Slerp(old,local,k);
            float eyes=warn ? 0.68f : think ? 0.76f : happy ? 0.85f : State==SparkState.Listening ? 1.12f : 1f;
            for(int side=0;side<2;side++){Scale(r,EyeNames[side],new Vector3(1,blink*eyes,1),k);Rotate(r,BrowNames[side],new Vector3(0,0,(side==0?1:-1)*(warn?-16:happy?12:think?7:0)),k);
                var g=r.Get(GazeNames[side]);Vector3 offset=Vector3.zero;if(focus&&g){Vector3 d=(focus.position-head.position).normalized;offset=mp.TransformDirection(r.right)*Mathf.Clamp(Vector3.Dot(d,mp.TransformDirection(r.right))*.02f,-.012f,.012f)+up*Mathf.Clamp(Vector3.Dot(d,up)*.02f,-.01f,.01f);offset=g.parent.InverseTransformVector(offset);}
                if(g)g.localPosition=Vector3.Lerp(g.localPosition,r.positions[g]+offset,k);
            }
            if(animateCrest)for(int i=0;i<5;i++)Rotate(r,Strands[i],new Vector3(Mathf.Sin(phase*.8f+i*.7f)*2.2f,0,0),k);
            Rotate(r,"Body",new Vector3(walking?3:0,0,breathe*.7f),k);
            float swing=walking?Mathf.Sin(phase)*24:0;
            Rotate(r,"Hip_L",new Vector3(swing,0,0),k);Rotate(r,"Hip_R",new Vector3(-swing,0,0),k);
            Rotate(r,"Knee_L",new Vector3(walking?Mathf.Max(0,-swing)*1.5f:0,0,0),k);Rotate(r,"Knee_R",new Vector3(walking?Mathf.Max(0,swing)*1.5f:0,0,0),k);
            Rotate(r,"Ankle_L",new Vector3(-swing*.3f,0,0),k);Rotate(r,"Ankle_R",new Vector3(swing*.3f,0,0),k);
            SparkGesture active=warn?SparkGesture.Stop:gesture==SparkGesture.None?(State==SparkState.Teaching||speaking?SparkGesture.Explain:think?SparkGesture.Think:happy?SparkGesture.Celebrate:SparkGesture.None):gesture;
            float arm=active==SparkGesture.Explain?22+10*Mathf.Sin(now*2.7f):active==SparkGesture.Celebrate?60:active==SparkGesture.Stop?42:active==SparkGesture.Wave?55:active==SparkGesture.Think?26:active==SparkGesture.Offer?34:active==SparkGesture.Inspect?30:active==SparkGesture.Count?40:0;
            Rotate(r,"Shoulder_L",new Vector3(-arm*.75f-swing*.5f,0,-arm*.6f),k);Rotate(r,"Shoulder_R",new Vector3(-arm*.75f+swing*.5f,0,arm*.6f),k);
            Rotate(r,"Elbow_L",new Vector3(-arm*.65f,0,0),k);Rotate(r,"Elbow_R",new Vector3(-arm*.65f,0,0),k);
            Rotate(r,"Wrist_L",new Vector3(0,0,0),k);Rotate(r,"Wrist_R",new Vector3(0,0,active==SparkGesture.Wave?Mathf.Sin(now*9)*25:0),k);
            for(int side=0;side<2;side++){for(int f=0;f<4;f++){bool isPoint=active==SparkGesture.Point&&side==(pointLeft?0:1);float curl=active==SparkGesture.Grasp?48:isPoint&&f!=(side==0?3:0)?55:active==SparkGesture.Count&&f>1?50:5;Rotate(r,Fingers[side,f],new Vector3(-curl,0,0),k);Rotate(r,FingerTips[side,f],new Vector3(-curl*.85f,0,0),k);}Rotate(r,ThumbNames[side],new Vector3(active==SparkGesture.Grasp?-32:0,0,0),k);}
            if(active==SparkGesture.Point&&(pointTarget||pointPosition.HasValue)){Vector3 target=pointTarget?pointTarget.position:pointPosition.Value;AimArm(r,pointLeft?"L":"R",target,k);}
            DrawMouth(r,speaking,think,warn,happy); Color c=warn?new Color(1,.08f,.08f):happy?new Color(.15f,1,.32f):think?new Color(.58f,.23f,1):State==SparkState.Teaching?new Color(1,.65f,.08f):State==SparkState.Diagnosing?new Color(1,.32f,.06f):new Color(.15f,.95f,1);
            float glow=warn ? 1.4f+.6f*Mathf.Sin(now*8) : .8f+.18f*breathe+amplitude*.65f;
            foreach(var rr in r.core){if(!rr)continue;rr.GetPropertyBlock(block);block.SetColor(Emission,c*glow);block.SetColor(BaseColor,c);rr.SetPropertyBlock(block);}
        }
        static void AimArm(Rig r,string side,Vector3 target,float k)
        {
            var a=r.Get("Shoulder_"+side);var b=r.Get("Elbow_"+side);var c=r.Get("Wrist_"+side);if(!a||!b||!c)return;
            Vector3 root=a.position, dir=target-root;float d=dir.magnitude,l1=Vector3.Distance(root,b.position),l2=Vector3.Distance(b.position,c.position);if(d<.001f||l1<.001f||l2<.001f)return;dir/=d;
            float reach=Mathf.Clamp(d-.06f,Mathf.Abs(l1-l2)+.001f,l1+l2-.001f);float along=(l1*l1-l2*l2+reach*reach)/(2*reach);Vector3 pole=Vector3.ProjectOnPlane(b.position-root,dir).normalized;if(pole.sqrMagnitude<.01f)pole=Vector3.ProjectOnPlane(-a.up,dir).normalized;if(pole.sqrMagnitude<.01f)return;
            Vector3 elbow=root+dir*along+pole*Mathf.Sqrt(Mathf.Max(0,l1*l1-along*along)), wrist=root+dir*reach;
            a.rotation=Quaternion.Slerp(a.rotation,Quaternion.FromToRotation(b.position-root,elbow-root)*a.rotation,k);
            b.rotation=Quaternion.Slerp(b.rotation,Quaternion.FromToRotation(c.position-b.position,wrist-b.position)*b.rotation,k);
            // Orient the rigid hand along the pointing direction using its actual finger orientation.
            var finger=r.Get("Finger"+(side=="L"?3:0)+"_"+side);if(finger){Vector3 handDir=finger.position-c.position;if(handDir.sqrMagnitude>.0001f)c.rotation=Quaternion.Slerp(c.rotation,Quaternion.FromToRotation(handDir,target-c.position)*c.rotation,k);}
        }
        void DrawMouth(Rig r,bool speaking,bool think,bool warn,bool happy)
        {
            if(!r.line)return;bool show=speaking||think||warn||happy; r.line.forceRenderingOff=!show;foreach(var rr in r.smile)if(rr)rr.forceRenderingOff=show;if(!show)return;
            float w=.052f,h=.005f;SparkMouth p=visemeValid?mouth:SparkMouth.Ah;
            if(speaking){switch(p){case SparkMouth.Ah:w=.041f;h=.027f;break;case SparkMouth.Ai:w=.054f;h=.019f;break;case SparkMouth.Ee:w=.063f;h=.010f;break;case SparkMouth.Eh:w=.052f;h=.015f;break;case SparkMouth.FV:w=.05f;h=.006f;break;case SparkMouth.MBP:w=.047f;h=.0015f;break;case SparkMouth.Oo:w=.027f;h=.026f;break;case SparkMouth.Ooh:w=.019f;h=.021f;break;case SparkMouth.RSn:w=.035f;h=.012f;break;case SparkMouth.S:w=.057f;h=.004f;break;}if(!visemeValid)h*=Mathf.Lerp(.10f,1,amplitude);}
            if(happy&&!speaking){h=.015f;w=.058f;}if(warn&&!speaking)h=.003f;
            for(int i=0;i<32;i++){float a=i*Mathf.PI*2/32;float x=Mathf.Cos(a)*w,y=Mathf.Sin(a)*h;if(happy&&!speaking)y+=x*x*4;r.line.SetPosition(i,(r.right*x+r.up*y+r.forward*.009f)*r.localMeter);}
        }
    }
}
