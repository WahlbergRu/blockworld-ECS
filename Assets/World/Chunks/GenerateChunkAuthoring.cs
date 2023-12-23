using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{

	public struct GenerateChunk : IComponentData
	{
	}

	public class GenerateChunkAuthoring : MonoBehaviour
	{

	}


	public class GenerateChunkAuthoringBaker : Baker<GenerateChunkAuthoring>
	{
		public override void Bake(GenerateChunkAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new GenerateChunk
			{
			});
		}
	}
}
