using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public struct GenerateRegion : IComponentData
	{
		public int2 regionIndex;
	}

	public class GenerateRegionAuthoring : MonoBehaviour
	{
		public int2 regionIndex;
	}

	public class GenerateRegionAuthoringBaker : Baker<GenerateRegionAuthoring>
	{
		public override void Bake(GenerateRegionAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			float3 chunkPos = math.floor(new float3(authoring.transform.position) / Constants.ChunkSize);
			int2 pos = (int2)chunkPos.xz;
			AddComponent(entity, new GenerateRegion
			{
				regionIndex = pos
			});
		}
	}
}
