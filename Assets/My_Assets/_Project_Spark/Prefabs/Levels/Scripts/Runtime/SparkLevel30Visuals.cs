using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel30Visuals : MonoBehaviour
    {
        [Serializable]
        public sealed class ContactVisual
        {
            public SparkSwitch contact;
            public Transform lever;
            public Renderer indicator;
        }

        public SparkMotor motor;
        public Transform shaft;
        public Transform leadScrew;
        public SparkLED lamp;
        public Renderer bulb;
        public Light bulbLight;
        public ContactVisual[] contacts;
        public Image upButton, offButton, downButton;
        public SparkSwitch upFeed, downFeed;

        private MaterialPropertyBlock block;
        private float glow;
        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int StandardColor = Shader.PropertyToID("_Color");

        private void Awake() => block = new MaterialPropertyBlock();

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (motor != null && motor.IsRunning)
            {
                float degrees = motor.ElectricalState.Voltage / 5f * 900f * dt;
                if (shaft != null) shaft.Rotate(Vector3.right, degrees, Space.Self);
                if (leadScrew != null) leadScrew.Rotate(Vector3.up, degrees, Space.Self);
            }

            bool lit = lamp != null && lamp.IsOn && Mathf.Abs(lamp.ElectricalState.Current) >= .002f;
            glow = Mathf.MoveTowards(glow, lit ? 1f : 0f, dt * 12f);
            if (bulb != null)
                Tint(bulb, Color.Lerp(new Color(.38f,.30f,.17f),new Color(1f,.79f,.34f),glow), glow * 2.5f);
            if (bulbLight != null)
            {
                bulbLight.enabled = glow > .01f;
                bulbLight.intensity = 1.6f * glow;
            }

            if (contacts != null)
                foreach (var visual in contacts)
                {
                    if (visual == null || visual.contact == null) continue;
                    bool closed = visual.contact.IsClosed;
                    if (visual.lever != null)
                        visual.lever.localRotation = Quaternion.Euler(0, 0, closed ? 0f : 28f);
                    if (visual.indicator != null)
                        Tint(visual.indicator, closed ? new Color(.17f,.89f,.48f) : new Color(.36f,.16f,.08f), closed ? 1f : 0f);
                }

            bool up = upFeed != null && upFeed.IsClosed;
            bool down = downFeed != null && downFeed.IsClosed;
            Highlight(upButton, up);
            Highlight(downButton, down);
            Highlight(offButton, !up && !down);
        }

        private void Tint(Renderer target, Color color, float emission)
        {
            target.GetPropertyBlock(block);
            block.SetColor(BaseColor, color);
            block.SetColor(StandardColor, color);
            block.SetColor(Emission, color * emission);
            target.SetPropertyBlock(block);
            block.Clear();
        }

        private static void Highlight(Image image, bool selected)
        {
            if (image != null) image.color = selected ? new Color(.10f,.63f,.81f) : new Color(.13f,.30f,.43f);
        }
    }
}
