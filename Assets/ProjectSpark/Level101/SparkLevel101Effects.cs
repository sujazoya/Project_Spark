using UnityEngine;

namespace ProjectSpark.Gameplay
{
    // These effects present board actions. They never change electrical or objective state.
    public sealed class SparkLevel101Effects : MonoBehaviour
    {
        public Material particlesMaterial;
        private ParticleSystem particles;
        private AudioSource audioSource;
        private AudioClip solder, route, fault, certify;
        private Light inspection;
        private float flash;

        private void Awake()
        {
            var emitter = new GameObject("Solder sparks and diagnostic pulses").transform;
            emitter.SetParent(transform,false);
            particles = emitter.gameObject.AddComponent<ParticleSystem>();
            particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.maxParticles = 190;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = .22f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(.16f,.48f);
            main.startSize = new ParticleSystem.MinMaxCurve(.017f,.043f);
            var emission = particles.emission;
            emission.enabled = false;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            if (particlesMaterial) renderer.sharedMaterial = particlesMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 2.6f;
            renderer.velocityScale = .18f;
            var lightObject = new GameObject("Inspection light pulse");
            lightObject.transform.SetParent(transform,false);
            inspection = lightObject.AddComponent<Light>();
            inspection.type = LightType.Point;
            inspection.range = .85f;
            inspection.intensity = 0;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = .25f;
            audioSource.volume = .40f;
            solder = CreateCue("Board Foundry solder", .20f, 690, 290, .34f);
            route = CreateCue("Board Foundry copper", .14f, 420, 740, .05f);
            fault = CreateCue("Board Foundry fault", .32f, 270, 110, .08f);
            certify = CreateCue("Board Foundry certified", .55f, 510, 1050, .01f);
        }

        public void Emit(Vector3 contact, int action)
        {
            if (!particles) return;
            Color color = action switch
            {
                0 => new Color(1,.52f,.13f),
                2 => new Color(1,.2f,.12f),
                3 => new Color(.30f,1,.63f),
                _ => new Color(.20f,.86f,1)
            };
            particles.transform.position = contact;
            int count = action == 0 ? 21 : action == 3 ? 32 : 12;
            for (int i=0;i<count;i++)
            {
                float angle = i*2.399963f;
                float speed = .16f + .47f*((i*17)%count)/count;
                var emit = new ParticleSystem.EmitParams
                {
                    position = contact, velocity = new Vector3(Mathf.Cos(angle)*speed,.28f+speed,Mathf.Sin(angle)*speed),
                    startColor = color
                };
                particles.Emit(emit,1);
            }
            inspection.transform.position = contact+Vector3.up*.13f;
            inspection.color = color;
            flash = action == 3 ? 2.2f : 1.4f;
            audioSource.PlayOneShot(action == 0 ? solder : action == 2 ? fault : action == 3 ? certify : route);
        }

        private void Update()
        {
            if (!inspection) return;
            flash = Mathf.MoveTowards(flash,0,Time.unscaledDeltaTime*7);
            inspection.intensity = flash;
        }

        private static AudioClip CreateCue(string name, float length, float start, float end, float noise)
        {
            const int sampleRate = 24000;
            var samples = new float[Mathf.CeilToInt(length*sampleRate)];
            var random = new System.Random(113);
            float phase = 0;
            for (int i=0;i<samples.Length;i++)
            {
                float t = i/(float)sampleRate;
                float fraction = t/length;
                phase += Mathf.Lerp(start,end,fraction)*Mathf.PI*2/sampleRate;
                float envelope = Mathf.Min(1,t*160)*Mathf.Pow(1-fraction,2.6f);
                samples[i] = envelope*(Mathf.Sin(phase)*.38f+Mathf.Sin(phase*1.5f)*.13f+
                    ((float)random.NextDouble()*2-1)*noise);
            }
            var clip = AudioClip.Create(name,samples.Length,1,sampleRate,false);
            clip.SetData(samples,0);
            return clip;
        }

        private void OnDestroy()
        {
            foreach(var clip in new[]{solder,route,fault,certify}) if (clip) Destroy(clip);
        }
    }
}
