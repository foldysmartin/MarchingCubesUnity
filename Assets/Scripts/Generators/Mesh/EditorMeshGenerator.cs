using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Generators.Mesh
{
    [ExecuteInEditMode]
    public class EditorMeshGenerator : AbstractMeshGenerator
    {

        [Header("Update Settings")]
        public bool autoUpdateInEditor = true;
        public bool autoUpdateInGame = true;
        
        [Header("Gizmos")]
        public bool showBoundsGizmo = true;
        public Color boundsGizmoCol = Color.white;
    
        private bool _settingsUpdated;
        
        void Update()
        {
            if (!_settingsUpdated) return;
            RequestMeshUpdate();
            _settingsUpdated = false;
        }

        public void Run()
        {
            CreateBuffers();

            InitChunks();
            UpdateAllChunks();

            ReleaseBuffers();
        }

        //Not sure this is good practice
        public void RequestMeshUpdate()
        {
            if ((Application.isPlaying && autoUpdateInGame) || (!Application.isPlaying && autoUpdateInEditor))
            {
                Run();
            }
        }
   
        public void UpdateAllChunks()
        {

            // Create mesh for each chunk
            foreach (Chunk chunk in chunks)
            {
                UpdateChunkMesh(chunk);
            }

        }

        protected override Vector3 CentreFromCoord(Vector3Int coord)
        {
            Vector3 totalBounds = (Vector3)numChunks * boundsSize;
            return -totalBounds / 2 + (Vector3)coord * boundsSize + Vector3.one * boundsSize / 2;
        }
    

        // Create/get references to all chunks
        void InitChunks()
        {
            CreateChunkHolder();
            chunks = new List<Chunk>();
            List<Chunk> oldChunks = new List<Chunk>(FindObjectsOfType<Chunk>());

            // Go through all coords and create a chunk there if one doesn't already exist
            for (int x = 0; x < numChunks.x; x++)
            {
                for (int y = 0; y < numChunks.y; y++)
                {
                    for (int z = 0; z < numChunks.z; z++)
                    {
                        Vector3Int coord = new Vector3Int(x, y, z);
                        bool chunkAlreadyExists = false;

                        // If chunk already exists, add it to the chunks list, and remove from the old list.
                        for (int i = 0; i < oldChunks.Count; i++)
                        {
                            if (oldChunks[i].coord != coord) continue;
                            chunks.Add(oldChunks[i]);
                            oldChunks.RemoveAt(i);
                            chunkAlreadyExists = true;
                            break;
                        }

                        // Create new chunk
                        if (!chunkAlreadyExists)
                        {
                            var newChunk = CreateChunk(coord);
                            chunks.Add(newChunk);
                        }

                        chunks[chunks.Count - 1].SetUp(mat, generateColliders);
                    }
                }
            }

            // Delete all unused chunks
            foreach (var t in oldChunks)
            {
                t.DestroyOrDisable();
            }
        }

        void OnValidate()
        {
            _settingsUpdated = true;
        }

        void OnDrawGizmos()
        {
            if (!showBoundsGizmo) return;
            Gizmos.color = boundsGizmoCol;

            List<Chunk> chunks = (this.chunks == null) ? new List<Chunk>(FindObjectsOfType<Chunk>()) : this.chunks;
            foreach (var chunk in chunks)
            {
                Bounds bounds = new Bounds(CentreFromCoord(chunk.coord), Vector3.one * boundsSize);
                Gizmos.color = boundsGizmoCol;
                Gizmos.DrawWireCube(CentreFromCoord(chunk.coord), Vector3.one * boundsSize);
            }
        }

    }
    
}