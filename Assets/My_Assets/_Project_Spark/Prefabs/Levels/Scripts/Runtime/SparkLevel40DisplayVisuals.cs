using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevel40DisplayVisuals : MonoBehaviour
    {
        public SparkLevel40TheLongRun loop;
        public Transform waterPivot, leverPivot;
        public float fullWaterScale, closedLeverAngle, openLeverAngle;
        public TextMesh transmitterReadout, receiverReadout, cableReadout;
        public TextMesh tankReadout, receiverPercentReadout, stateReadout;
        public Renderer openLamp, closedLamp;
        public Color signalColor = new Color(.34f,.90f,1f);
        public Color warningColor = new Color(1f,.63f,.21f);

        private float previousTank = -1f;
        private int previousCable;
        private bool? previousClosed;

        private void LateUpdate()
        {
            if (loop == null) return;

            float tank = Mathf.Clamp01(loop.TankFraction);
            if (!Mathf.Approximately(previousTank,tank) && waterPivot != null)
            {
                previousTank = tank;
                waterPivot.gameObject.SetActive(tank > 0f);
                waterPivot.localScale = new Vector3(1f,Mathf.Max(.001f,fullWaterScale*tank),1f);
            }

            if (loop.lineContact != null)
            {
                bool closed = loop.lineContact.IsClosed;
                if (previousClosed != closed)
                {
                    previousClosed = closed;
                    if (leverPivot != null) leverPivot.localRotation = Quaternion.Euler(0f,0f,closed ? closedLeverAngle : openLeverAngle);
                    if (closedLamp != null) closedLamp.enabled = closed;
                    if (openLamp != null) openLamp.enabled = !closed;
                }
            }

            int cable = loop.cable == null ? 0 : Mathf.RoundToInt(loop.cable.Resistance);
            if (cableReadout != null && previousCable != cable)
            {
                previousCable = cable;
                cableReadout.text = cable > 0 ? $"{cable} Ω" : "—";
            }

            if (transmitterReadout != null)
            {
                transmitterReadout.text = loop.HasLoopReading ? $"{loop.LoopCurrentAmps*1000f:0.00} mA" : "— mA";
                transmitterReadout.color = loop.IsHeadroomLimited ? warningColor : signalColor;
            }
            if (receiverReadout != null)
                receiverReadout.text = loop.HasLoopReading ? $"{loop.ReceiverVoltage:0.00} V" : "— V";
            if (tankReadout != null) tankReadout.text = $"TANK {tank * 100f:0}%";
            if (receiverPercentReadout != null)
                receiverPercentReadout.text = loop.HasLoopReading && !loop.IsLineOpen
                    ? $"RECEIVER {loop.ReceiverFraction * 100f:0}%" : "RECEIVER —";
            if (stateReadout != null)
            {
                stateReadout.text = !loop.HasLoopReading ? "WIRE LOOP" : loop.IsLineOpen ? "LINE OPEN" :
                    loop.IsHeadroomLimited ? "HEADROOM LIMIT" : "REGULATED";
                stateReadout.color = loop.IsLineOpen || loop.IsHeadroomLimited ? warningColor : signalColor;
            }
        }
    }
}
