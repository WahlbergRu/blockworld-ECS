using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public struct ChunkMesh : IComponentData
	{
	}
	
	public class ChunkMeshAuthoring : MonoBehaviour
	{
	}

	public class ChunkMeshAuthoringBaker : Baker<ChunkMeshAuthoring>
	{
		public override void Bake(ChunkMeshAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new ChunkMesh
			{
			});
			AddBuffer<ChunkMeshIndices>(entity);
			AddBuffer<ChunkMeshUVs>(entity);
			AddBuffer<ChunkMeshVerts>(entity);
		}
	}
}
