using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    internal sealed class SparkNativeCableVisualCapture : IDisposable
    {
        private const string Folder = "Logs/SparkMasterAudit/NativeWireVisualRepair";
        private readonly string run;
        private readonly SignalWireBuilder builder;
        private readonly SparkNativeWireCinematic performer;
        private readonly SparkV5FlightController flight;
        private readonly SparkNativeWireDirector director;
        private readonly SparkAINativeSignalWirePort port;
        private readonly Action<string> log;
        private readonly StreamWriter samples;
        private SparkTerminal source, destination;
        private float nextSample;
        private SignalPath lastPath;

        internal SparkNativeCableVisualCapture(string run, SignalWireBuilder builder,
            SparkNativeWireCinematic performer, SparkV5FlightController flight,
            SparkNativeWireDirector director, SparkTerminal a, SparkTerminal b, Action<string> log)
        {
            this.run = run;
            this.builder = builder;
            this.performer = performer;
            this.flight = flight;
            this.director = director;
            port = director.GetComponent<SparkAINativeSignalWirePort>();
            this.log = log;
            Track(a, b);
            samples = new StreamWriter(Path.Combine(Folder, run + "-geometry.tsv"));
            samples.WriteLine("seconds\tframe\tphase\trouting\tdrag\troot\tsocket A\tsocket B\tgrip\tpreview end\troute A\troute B\tpoints\tvertices\tlength\tspan\tmax turn deg\tcorridor\tnonfinite\tactive previews\tmesh");
            EditorApplication.update += ReadPreview;
            builder.TerminalWireConnected += ReadCompleted;
        }

        internal void Track(SparkTerminal a, SparkTerminal b) { source = a; destination = b; }

        public void Dispose()
        {
            EditorApplication.update -= ReadPreview;
            if (builder) builder.TerminalWireConnected -= ReadCompleted;
            samples.Dispose();
        }

        private void ReadPreview()
        {
            if (!builder || !performer) return;
            var path = builder.CurrentPath;
            if (!path || (path == lastPath && Time.unscaledTime < nextSample)) return;
            string phase = path != lastPath ? "source" : performer.Stage.ToString();
            if (flight.IsFlying) phase += flight.IsMoving ? "-flying" : "-hover";
            lastPath = path;
            nextSample = Time.unscaledTime + .08f;
            Record(path, phase, builder.StartTerminal ? builder.StartTerminal : source,
                port.ActiveDestination ? port.ActiveDestination : destination);
        }

        private void ReadCompleted(SparkTerminal a, SparkTerminal b, SignalPath path)
        {
            Record(path, "attached", a, b);
            samples.Flush();
        }

        private void Record(SignalPath path, string phase, SparkTerminal a, SparkTerminal b)
        {
            var geometry = path.GetComponent<SignalPathMesh>();
            var points = geometry.GetEffectiveRoutePoints();
            if (points.Length < 2) return;
            bool holding = performer.TryGetHeldCableGrip(out var grip);
            var mesh = geometry.GeneratedMesh;
            var (length, turn, corridor, invalid) = Measure(points);
            int previews = Object.FindObjectsByType<SignalPath>(FindObjectsSortMode.None).Count(p => p.HasPreview);
            Vector3 start = points[0], end = points[^1];
            static string Position(Vector3 p) => p.ToString("F4", CultureInfo.InvariantCulture);
            samples.WriteLine(FormattableString.Invariant(
                $"{Time.unscaledTime:F3}\t{Time.frameCount}\t{phase}\t{builder.CableRouting}\t{builder.DragRoutePointCount}\t{Position(director.transform.position)}\t{(a ? Position(a.transform.position) : "")}\t{(b ? Position(b.transform.position) : "")}\t{(holding ? Position(grip) : "")}\t{(path.HasPreview ? Position(path.PreviewPoint) : "")}\t{Position(start)}\t{Position(end)}\t{points.Length}\t{(mesh ? mesh.vertexCount : 0)}\t{length:F4}\t{Vector3.Distance(start, end):F4}\t{turn:F2}\t{corridor:F4}\t{invalid}\t{previews}\t{geometry.name}"));
        }

        internal static (float length, float turn, float corridor, int invalid) Measure(Vector3[] route)
        {
            float length = 0f, turn = 0f, corridor = 0f;
            int invalid = 0;
            Vector3 span = route[^1] - route[0];
            float spanSquared = Mathf.Max(span.sqrMagnitude, .000001f);
            for (int i = 0; i < route.Length; i++)
            {
                Vector3 p = route[i];
                if (!float.IsFinite(p.x) || !float.IsFinite(p.y) || !float.IsFinite(p.z)) { invalid++; continue; }
                Vector3 offset = p - route[0];
                corridor = Mathf.Max(corridor,
                    (offset - span * Mathf.Clamp01(Vector3.Dot(offset, span) / spanSquared)).magnitude);
                if (i > 0) length += Vector3.Distance(route[i - 1], p);
                if (i > 0 && i + 1 < route.Length)
                    turn = Mathf.Max(turn, Vector3.Angle(p - route[i - 1], route[i + 1] - p));
            }
            return (length, turn, corridor, invalid);
        }

        internal async Task IsolateRenderer(SignalPath path, string phase)
        {
            var geometry = path.GetComponent<SignalPathMesh>();
            var meshRenderer = geometry.MeshRenderer;
            string material = string.Join(", ", meshRenderer.sharedMaterials
                .Select(m => m ? m.name + " [" + m.shader.name + "]" : "None"));
            log($"RENDERER {phase} {Hierarchy(meshRenderer.transform)} ID={meshRenderer.GetInstanceID()} " +
                $"scale={meshRenderer.transform.lossyScale:F4}, world-to-mesh={meshRenderer.transform.worldToLocalMatrix}, materials={material}");
            foreach (var renderer in path.GetComponentsInChildren<Renderer>(true))
                log($"  Native render subtree {renderer.GetType().Name} {Hierarchy(renderer.transform)} enabled={renderer.enabled}, forcedOff={renderer.forceRenderingOff}, material={renderer.sharedMaterial?.name}, shares native mesh={renderer.GetComponent<MeshFilter>()?.sharedMesh == geometry.GeneratedMesh}.");
            foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                .Where(r => r is LineRenderer || r is TrailRenderer || r.GetComponent<SignalPathMesh>()))
                log($"  Scene renderer {renderer.GetType().Name} {Hierarchy(renderer.transform)} enabled={renderer.enabled}, " +
                    $"forcedOff={renderer.forceRenderingOff}, material={(renderer.sharedMaterial ? renderer.sharedMaterial.name : "")}" +
                    (renderer is LineRenderer line ? $", worldSpace={line.useWorldSpace}, points={line.positionCount}" : ""));
            ScreenCapture.CaptureScreenshot(Path.GetFullPath(Path.Combine(Folder, run + "-" + phase + ".png")));
            await Task.Delay(180);
            foreach (var renderer in path.GetComponentsInChildren<MeshRenderer>())
            {
                var before = renderer.forceRenderingOff;
                try
                {
                    renderer.forceRenderingOff = true;
                    await Task.Delay(180);
                    string suffix = renderer == meshRenderer ? "-native-hidden" : "-glow-hidden";
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath(Path.Combine(Folder, run + "-" + phase + suffix + ".png")));
                    await Task.Delay(180);
                }
                finally { if (renderer) renderer.forceRenderingOff = before; }
            }
        }

        internal async Task ReplayHistoricalRoute(SparkAINativeSignalWirePort port, SparkTerminal a,
            SparkTerminal b, SparkCircuitSystem circuit, bool legacy = true)
        {
            if (!port.Begin(a, b, out var error))
                throw new InvalidOperationException("Historical route fixture could not reserve the native tool. " + error);
            try
            {
                if (legacy)
                {
                    builder.SetTerminalCableRouting(TerminalCableRouting.FreehandDrag);
                    builder.SetTerminalInputStyle(TerminalWireInputStyle.DragAndRelease);
                }
                if (!port.AttachSource(a, out error)) throw new InvalidOperationException(error);
                // Controlled replay of the original world-space hand writer. This
                // deliberately produces an exaggerated detour on the real Level 20 tool.
                var path = builder.CurrentPath;
                var center = director.transform.position + Vector3.up * .55f;
                for (int i = 0; i <= 38; i++)
                {
                    float angle = i / 38f * Mathf.PI * 3f;
                    Vector3 point = center + new Vector3(Mathf.Cos(angle), i * .012f, Mathf.Sin(angle)) * .8f;
                    if (legacy) builder.UpdateWire(point);
                    else if (!port.MoveLooseEnd(point, out error)) throw new InvalidOperationException(error);
                }
                string label = legacy ? "legacy-detour" : "endpoint-replay";
                Record(path, label + "-preview", a, b);
                if (!legacy && (path.PointCount != 1 || builder.DragRoutePointCount != 0))
                    throw new InvalidOperationException("Moving the loose cable end left historical points.");
                await IsolateRenderer(path, label + "-preview");
                if (!port.AttachDestination(b, out error)) throw new InvalidOperationException(error);
                Record(path, label + "-attached", a, b);
                await IsolateRenderer(path, label + "-attached");
                if (!circuit.HasConnectionBetween(a, b) || !port.UndoLastCompletedWire(out error))
                    throw new InvalidOperationException("Historical test did not cleanly undo. " + error);
            }
            finally { if (port.IsWiring) port.Abort(); }
        }

        private static string Hierarchy(Transform current) =>
            !current.parent ? current.name : Hierarchy(current.parent) + "/" + current.name;
    }
}
