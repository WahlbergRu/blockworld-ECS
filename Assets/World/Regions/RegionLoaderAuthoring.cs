using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public struct RegionLoader : IComponentData
	{
		public int range;
		public int2 regionIndex;
	}


	public class RegionLoaderAuthoring : MonoBehaviour
	{
		public int _range = 15;
	}

	public class RegionLoaderAuthoringBaker : Baker<RegionLoaderAuthoring>
	{
		public override void Bake(RegionLoaderAuthoring authoring)
		{

			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new RegionLoader
			{
				range = authoring._range
			});
		}
	}
}

