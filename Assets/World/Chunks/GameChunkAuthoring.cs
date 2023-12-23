using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public struct GameChunk : IComponentData
	{
	}

	public class GameChunkAuthoring : MonoBehaviour
	{
	}

	public class GameChunkAuthoringBaker : Baker<GameChunkAuthoring>
	{
		public override void Bake(GameChunkAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new GameChunk
			{
			});
		}
	}
}
