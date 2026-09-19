using System;
using UnityEngine;

namespace AAAUI.VFX
{
    [DisallowMultipleComponent]
    public sealed class SignalPath_Manager : MonoBehaviour
    {
        [Header("Wire")]
        [SerializeField]
        private SignalPathMesh meshPrefab;

        [SerializeField]
        private Material positiveMaterial;

        [SerializeField]
        private Material negativeMaterial;

        [Header("Paths")]
        [SerializeField]
        private SignalPath[] paths =
            Array.Empty<SignalPath>();

        private int currentIndex = -1;

        private SignalPath currentPath;
        private SignalPathMesh currentMesh;

        public SignalPath CurrentPath =>
            currentPath;

        public SignalPathMesh CurrentMesh =>
            currentMesh;

        public int CurrentIndex =>
            currentIndex;

        public int Count =>
            paths.Length;

        /// <summary>
        /// Creates the next wire.
        /// Old wires are never cleared.
        /// </summary>
        public SignalPathMesh CreateNextWire(
            WirePolarity polarity)
        {
            currentIndex++;

            SignalPathMesh newMesh;

            if (meshPrefab != null)
            {
                newMesh =
                    Instantiate(
                        meshPrefab,
                        transform
                    );

                newMesh.name =
                    $"SignalWire_{currentIndex}";
            }
            else
            {
                GameObject wireObject =
                    new GameObject(
                        $"SignalWire_{currentIndex}"
                    );

                wireObject.transform.SetParent(
                    transform,
                    false
                );

                wireObject.AddComponent<MeshFilter>();
                wireObject.AddComponent<MeshRenderer>();

                newMesh =
                    wireObject.AddComponent<SignalPathMesh>();
            }

            SignalPath newPath =
                newMesh.gameObject.GetComponent<SignalPath>();

            if (newPath == null)
            {
                newPath =
                    newMesh.gameObject.AddComponent<SignalPath>();
            }

            newMesh.SetPath(
                newPath
            );

            Material material =
                polarity == WirePolarity.Positive
                    ? positiveMaterial
                    : negativeMaterial;

            newMesh.SetMaterial(
                material
            );

            Array.Resize(
                ref paths,
                currentIndex + 1
            );

            paths[currentIndex] =
                newPath;

            currentPath =
                newPath;

            currentMesh =
                newMesh;

            return newMesh;
        }

        /// <summary>
        /// Returns the path currently being edited.
        /// </summary>
        public SignalPath GetCurrentPath()
        {
            return currentPath;
        }
        public SignalPath[] Paths =>
    paths;

        /// <summary>
        /// Returns the mesh belonging to the current path.
        /// </summary>
        public SignalPathMesh GetCurrentMesh()
        {
            return currentMesh;
        }

        /// <summary>
        /// Ends the current wire without deleting it.
        /// </summary>
        public void FinishCurrentPath()
        {
            currentPath = null;
            currentMesh = null;
        }

        /// <summary>
        /// Deletes every created wire.
        /// </summary>
        public void ClearPaths()
        {
            for (int i = 0;
                 i < paths.Length;
                 i++)
            {
                if (paths[i] == null)
                    continue;

                Destroy(
                    paths[i].gameObject
                );
            }

            paths =
                Array.Empty<SignalPath>();

            currentIndex = -1;

            currentPath = null;
            currentMesh = null;
        }

        public SignalPathMesh GetMeshForPath(
    SignalPath path)
{
    if (path == null)
        return null;

    return path.GetComponent<SignalPathMesh>();
}
    }
}