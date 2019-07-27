using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Generators.Mesh
{
    public class ContinuesMeshGenerator : AbstractMeshGenerator
    {
        [Header("Camera Settings")]
        public Transform viewer;
        public float viewDistance = 30;

        private void Awake()
        {
            if (!Application.isPlaying) return;
            InitiateVariableChunkStructures();

            var oldChunks = FindObjectsOfType<Chunk>();
            for (var i = oldChunks.Length - 1; i >= 0; i--) Destroy(oldChunks[i].gameObject);
        }

        private void Update()
        {
            Run();
        }

        private void OnDestroy()
        {
            ReleaseBuffers();
        }

        private void InitiateVariableChunkStructures()
        {
            reusableChunks = new Queue<Chunk>();
            chunks = new List<Chunk>();
            existingChunks = new Dictionary<Vector3Int, Chunk>();
        }

        public void Run()
        {
            CreateBuffers();
            InitiateVisibleChunks();
        }

        private void InitiateVisibleChunks()
        {
            if (chunks == null) return;
            CreateChunkHolder();

            var p = viewer.position;
            var ps = p / boundsSize;
            var viewerCoordinates = new Vector3Int(Mathf.RoundToInt(ps.x), Mathf.RoundToInt(ps.y), Mathf.RoundToInt(ps.z));

            var maxChunksInView = Mathf.CeilToInt(viewDistance / boundsSize);
            var sqrViewDistance = viewDistance * viewDistance;

            // Go through all existing chunks and flag for recyling if outside of max view dst
            for (var i = chunks.Count - 1; i >= 0; i--)
            {
                var chunk = chunks[i];
                var centre = CentreFromCoord(chunk.coord);
                var viewerOffset = p - centre;
                var o = new Vector3(Mathf.Abs(viewerOffset.x), Mathf.Abs(viewerOffset.y), Mathf.Abs(viewerOffset.z)) -
                        Vector3.one * boundsSize / 2;
                var sqrDst = new Vector3(Mathf.Max(o.x, 0), Mathf.Max(o.y, 0), Mathf.Max(o.z, 0)).sqrMagnitude;

                if (!(sqrDst > sqrViewDistance)) continue;

                existingChunks.Remove(chunk.coord);
                reusableChunks.Enqueue(chunk);
                chunks.RemoveAt(i);
            }

            for (var x = -maxChunksInView; x <= maxChunksInView; x++)
            for (var y = -maxChunksInView; y <= maxChunksInView; y++)
            for (var z = -maxChunksInView; z <= maxChunksInView; z++)
            {
                var coord = new Vector3Int(x, y, z) + viewerCoordinates;

                if (existingChunks.ContainsKey(coord)) continue;

                var centre = CentreFromCoord(coord);
                var viewerOffset = p - centre;
                var o = new Vector3(Mathf.Abs(viewerOffset.x), Mathf.Abs(viewerOffset.y), Mathf.Abs(viewerOffset.z)) -
                        Vector3.one * boundsSize / 2;
                var sqrDst = new Vector3(Mathf.Max(o.x, 0), Mathf.Max(o.y, 0), Mathf.Max(o.z, 0)).sqrMagnitude;

                // Chunk is within view distance and should be created (if it doesn't already exist)
                if (!(sqrDst <= sqrViewDistance)) continue;

                var bounds = new Bounds(CentreFromCoord(coord), Vector3.one * boundsSize);

                if (!IsVisibleFrom(bounds, Camera.main)) continue;

                if (reusableChunks.Count > 0)
                {
                    var chunk = reusableChunks.Dequeue();
                    chunk.coord = coord;
                    existingChunks.Add(coord, chunk);
                    chunks.Add(chunk);
                    UpdateChunkMesh(chunk);
                }
                else
                {
                    var chunk = CreateChunk(coord);
                    chunk.coord = coord;
                    chunk.SetUp(mat, generateColliders);
                    existingChunks.Add(coord, chunk);
                    chunks.Add(chunk);
                    UpdateChunkMesh(chunk);
                }
            }
        }


        protected override Vector3 CentreFromCoord(Vector3Int coord)
        {
            return new Vector3(coord.x, coord.y, coord.z) * boundsSize;
        }

        public bool IsVisibleFrom(Bounds bounds, Camera camera)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }
    }
}