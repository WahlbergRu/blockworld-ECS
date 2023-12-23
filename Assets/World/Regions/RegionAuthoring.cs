using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
	public struct Region : IComponentData
	{
	}

	public class RegionAuthoring : MonoBehaviour
	{
	}

	public class RegionAuthoringBaker : Baker<RegionAuthoring>
	{
		public override void Bake(RegionAuthoring authoring)
		{
			Debug.Log("RegionAuthoringBaker");
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Region
			{
			});
		}
	}
}
