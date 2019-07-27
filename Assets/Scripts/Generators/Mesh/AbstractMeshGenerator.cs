using System.Collections.Generic;
using Assets.Scripts.Generators.Density;
using UnityEngine;

namespace Assets.Scripts.Generators.Mesh
{
    public abstract class AbstractMeshGenerator : MonoBehaviour
    {
        [Header("General Settings")]
        public NoiseDensityGenerator densityGenerator;
        public Vector3Int numChunks = Vector3Int.one;
        public ComputeShader shader;
        public bool generateColliders;
        public Material mat;

        [Header("Voxel Settings")]
        public float isoLevel;
        public float boundsSize = 1;
        public Vector3 offset = Vector3.zero;

        [Range(2, 100)]
        public int numPointsPerAxis = 30;

        protected GameObject chunkHolder;
        protected ComputeBuffer triangleBuffer;
        protected ComputeBuffer pointsBuffer;
        protected ComputeBuffer triCountBuffer;
        protected List<Chunk> chunks;
        protected Dictionary<Vector3Int, Chunk> existingChunks;
        protected Queue<Chunk> reusableChunks;

        private const int ThreadGroupSize = 8;

        private const string ChunkHolderName = "Chunks Holder";

        public void UpdateChunkMesh(Chunk chunk)
        {
            var numVoxelsPerAxis = numPointsPerAxis - 1;
            var numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float) ThreadGroupSize);
            var pointSpacing = boundsSize / (numPointsPerAxis - 1);

            var chunkCordinates = chunk.coord;
            var centre = CentreFromCoord(chunkCordinates);

            var worldBounds = new Vector3(numChunks.x, numChunks.y, numChunks.z) * boundsSize;

            densityGenerator.Generate(pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset,
                pointSpacing);

            triangleBuffer.SetCounterValue(0);
            shader.SetBuffer(0, "points", pointsBuffer);
            shader.SetBuffer(0, "triangles", triangleBuffer);
            shader.SetInt("numPointsPerAxis", numPointsPerAxis);
            shader.SetFloat("isoLevel", isoLevel);

            shader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

            // Get number of triangles in the triangle buffer
            ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
            int[] triangleCountArray = {0};
            triCountBuffer.GetData(triangleCountArray);
            var triangleCount = triangleCountArray[0];

            // Get triangle data from shader
            var triangles = new Triangle[triangleCount];
            triangleBuffer.GetData(triangles, 0, 0, triangleCount);

            var mesh = chunk.mesh;
            mesh.Clear();

            var vertices = new Vector3[triangleCount * 3];
            var meshTriangles = new int[triangleCount * 3];

            for (var i = 0; i < triangleCount; i++)
                for (var j = 0; j < 3; j++)
                {
                    meshTriangles[i * 3 + j] = i * 3 + j;
                    vertices[i * 3 + j] = triangles[i][j];
                }

            mesh.vertices = vertices;
            mesh.triangles = meshTriangles;

            mesh.RecalculateNormals();
        }

        protected void CreateBuffers()
        {
            var numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
            var numVoxelsPerAxis = numPointsPerAxis - 1;
            var numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
            var maxTriangleCount = numVoxels * 5;

            // Always create buffers in editor (since buffers are released immediately to prevent memory leak)
            // Otherwise, only create if null or if size has changed
            if (Application.isPlaying && pointsBuffer != null && numPoints == pointsBuffer.count) return;
            if (Application.isPlaying) ReleaseBuffers();

            triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
            pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);
            triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        }

        protected Chunk CreateChunk(Vector3Int coordinates)
        {
            var chunk = new GameObject($"Chunk ({coordinates.x}, {coordinates.y}, {coordinates.z})");
            chunk.transform.parent = chunkHolder.transform;
            var newChunk = chunk.AddComponent<Chunk>();
            newChunk.coord = coordinates;
            return newChunk;
        }

        protected void CreateChunkHolder()
        {
            // Create/find mesh holder object for organizing chunks under in the hierarchy
            if (chunkHolder != null) return;
            chunkHolder = GameObject.Find(ChunkHolderName) ? GameObject.Find(ChunkHolderName) : new GameObject(ChunkHolderName);
        }

        protected void ReleaseBuffers()
        {
            triangleBuffer?.Release();
            pointsBuffer?.Release();
            triCountBuffer?.Release();
        }

        protected abstract Vector3 CentreFromCoord(Vector3Int coord);

        protected struct Triangle
        {
#pragma warning disable 649 // disable unassigned variable warning
            private Vector3 _a;
            private Vector3 _b;
            private Vector3 _c;

            public Vector3 this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0:
                            return _a;
                        case 1:
                            return _b;
                        default:
                            return _c;
                    }
                }
            }
        }
    }
}