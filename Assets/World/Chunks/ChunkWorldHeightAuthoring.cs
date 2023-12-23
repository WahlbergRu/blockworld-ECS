using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    /// <summary>
    /// Represents the world height of the bottom layer of a chunk.
    /// </summary>

    public struct ChunkWorldHeight : IComponentData
    {
        public int value;
        public static implicit operator int(ChunkWorldHeight c) => c.value;
        public static implicit operator ChunkWorldHeight(int v) => new ChunkWorldHeight { value = v };
    }
    
    public class ChunkWorldHeightAuthoring : MonoBehaviour
    {
        public int value;
    }

    public class ChunkWorldHeightAuthoringBaker : Baker<ChunkWorldHeightAuthoring>
    {
        public override void Bake(ChunkWorldHeightAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ChunkWorldHeight
            {
                value = authoring.value
            });
        }
    }
}
