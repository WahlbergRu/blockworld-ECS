using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;

namespace BlockGame.BlockWorld
{

	// [BurstCompile]
	[UpdateAfter(typeof(GenerateChunkSystem))]
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	public partial struct RegionLoaderSystem : ISystem
	{
		EntityCommandBuffer.ParallelWriter ecb;
		EntityArchetype _regionArchetype;

		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			Debug.Log("RegionLoaderSystem OnCreate");
			state.RequireForUpdate<GenerateRegionHeightMapSettings>();
			// _barrier = World.GetOrCreateSystemManaged<EndInitializationEntityCommandBufferSystem>();

			// _regionArchetype = state.EntityManager.CreateArchetype(typeof(Region), typeof(GenerateRegion));
		}

		static void GetPointsInRange(int2 start, NativeList<int2> points, int range)
		{
			// Debug.Log(range);
			for (int x = -range; x < range; ++x)
				for (int z = -range; z < range; ++z)
				{
					int2 p = start + new int2(x, z);
					//if (GridUtil.Grid2D.TaxicabDistance(start, p) > range)
					//	continue;
					if (!points.Contains(p))
					{
						points.Add(p);
						// Debug.Log(p);
					}
				}
		}

		private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
		{
			var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
			var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
			return ecb.AsParallelWriter();
		}


		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			// state.Enabled = false;
			Debug.Log("RegionLoaderSystem OnUpdate");
			EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
			var config = SystemAPI.GetSingleton<GenerateRegionHeightMapSettings>();
			// Debug.Log(config.minHeight);
			// GenerateRegionHeightMapSettings settings = GenerateRegionHeightMapSettings.Default;
			// var settingsUI = GameObject.FindObjectOfType<MapGenerationSettingsUI>();
			// if (settingsUI != null)
			// 	settings = settingsUI.Settings;


			var regionArchetype = _regionArchetype;

			NativeList<int2> loadedPoints = new NativeList<int2>(100, Allocator.Persistent);

			// Debug.Log(range);
			// Debug.Log(loadedPoints.Capacity);
			// Debug.Log(regionArchetype);
			// Debug.Log(state.World);

			int entityInQueryIndex = 0;

			// Assign region loader indices 
			foreach (
				var (loader, translation) in SystemAPI.Query<RefRW<RegionLoader>, LocalTransform>())
			{
				loader.ValueRW.regionIndex = (int2)math.floor(translation.Position.xz / Constants.ChunkSize.xz);
				GetPointsInRange(loader.ValueRW.regionIndex, loadedPoints, config.range);
				entityInQueryIndex++;
			}
			Debug.Log(entityInQueryIndex);


			// Assign region loader indices 
			foreach (
				var (region, regionIndexComp, entity) in SystemAPI.Query<RefRW<Region>, RefRO<RegionIndex>>()
						 .WithEntityAccess())
			{

				int2 regionIndex = regionIndexComp.ValueRO;
				for (int i = loadedPoints.Length - 1; i >= 0; --i)
				{
					int2 p = loadedPoints[i];
					if (regionIndex.x == p.x && regionIndex.y == p.y)
					{
						loadedPoints.RemoveAtSwapBack(i);
						return;
					}
				}




				var myBufferElementQuery = SystemAPI.QueryBuilder().WithAll<LinkedEntityGroup>().Build();
				// LinkedEntityGroup
				// Any regions not in our loaded list can be unloaded/destroyed
				// Note that "LinkedEntityGroup" only destroys linked entities automatically if 
				// the root entity is destroyed via an entity query, so we do it manually here.
				// for (int i = 0; i < entity.linkedGroup.Length; ++i)
				// 	ecb.DestroyEntity(entityInQueryIndex, linkedGroup[i].Value);
				// ecb.DestroyEntity(entityInQueryIndex, entity);
				// }

			}


			// // Load any remaining unloaded regions
			// Dependency = new LoadUnloadedRegionsJob
			// {
			// 	genSettings = settings,
			// 	chunkArchetype = regionArchetype,
			// 	loadedPoints = loadedPoints.AsDeferredJobArray(),
			// 	ecb = ecb
			// }.Schedule(loadedPoints, 64, Dependency);

			// Dependency.Complete();

			// // var arr = loadedPoints.AsArray();
			// // Debug.Log("GET LENGTH: " + loadedPoints.Length);
			// // for (var i = 0; i < arr.Length; i++)
			// // {
			// // 	Debug.Log(arr[i]);
			// // }

			// loadedPoints.Dispose(Dependency);
		}

		struct LoadUnloadedRegionsJob : IJobParallelForDefer
		{
			[NativeSetThreadIndex]
#pragma warning disable 0649
			int m_ThreadIndex;
#pragma warning restore 0649

			[ReadOnly]
			public NativeArray<int2> loadedPoints;

			public EntityCommandBuffer.ParallelWriter ecb;
			public EntityArchetype chunkArchetype;
			public GenerateRegionHeightMapSettings genSettings;

			public void Execute(int index)
			{
				var regionEntity = ecb.CreateEntity(m_ThreadIndex, chunkArchetype);
				ecb.SetComponent(m_ThreadIndex, regionEntity, new GenerateRegion
				{
					regionIndex = loadedPoints[index]
				});
				ecb.AddComponent(m_ThreadIndex, regionEntity, genSettings);
			}
		}
	}
}