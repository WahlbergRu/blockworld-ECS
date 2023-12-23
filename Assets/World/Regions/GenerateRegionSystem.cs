using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public partial struct GenerateRegionSystem : ISystem
    {
        private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            return ecb.AsParallelWriter();
        }

        public void OnCreate(ref SystemState state)
        {
            Debug.Log("GenerateRegionSystem OnCreate");
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            Debug.Log("RegionLoaderSystem OnUpdate");
            InitializeRegions(ref state);

            GenerateHeightMapBlobs(ref state);

            GenerateChunks(ref state);

            AssignChunkSharedIndices(ref state);
        }

        void InitializeRegions(ref SystemState state)
        {
            Debug.Log("InitializeRegions");
            EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);

            // Apply default generation settings if none exist
            // Entities
            //     .WithName("ApplyDefaultRegionGenerationSettings")
            //     .WithAll<GenerateRegion>()
            //     .WithNone<GenerateRegionHeightMapSettings>()
            //     .ForEach((int entityInQueryIndex, Entity e) =>
            //     {
            //         Debug.Log(entityInQueryIndex);
            //         concurrentBuffer.AddComponent(entityInQueryIndex, e,
            //             GenerateRegionHeightMapSettings.Default);
            //     }).ScheduleParallel();
            // _barrier.AddJobHandleForProducer(Dependency);

            int entityInQueryIndex = 0;
            // Assign region loader indices 
            foreach (
                var (generateRegion, entity) in
                SystemAPI
                    .Query<GenerateRegion>()
                    .WithEntityAccess())
            {
                ecb.AddComponent<GenerateRegion>(entityInQueryIndex, entity);
                entityInQueryIndex++;
            }

            Debug.LogFormat("entityInQueryLength {0}", entityInQueryIndex);

            //             // Assign region indices from "GenerateRegion" components
            //             // and add "LinkedEntityGroup" buffers for chunks
            //             EntityCommandBuffer buffer = _barrier.CreateCommandBuffer();
            //             Entities
            //                 .WithName("AssignRegionIndices")
            //                 .WithAll<GenerateRegionHeightMapSettings>()
            //                 .WithNone<RegionIndex>()
            //                 .WithoutBurst()
            //                 .ForEach((int entityInQueryIndex, Entity e, in GenerateRegion generate) =>
            //                 {
            //                     int2 i = generate.regionIndex;

            //                     buffer.AddComponent<RegionIndex>(e, i);
            //                     buffer.AddSharedComponent<SharedRegionIndex>(e, i);
            //                     buffer.AddBuffer<LinkedEntityGroup>(e);
            //                     buffer.AddComponent<Region>(e);
            // #if UNITY_EDITOR
            //                     // Note this causes an exception if you generate a lot of regions.
            //                     //EntityManager.SetName(e, $"Region ({i.x}, {i.y})");
            // #endif
            //                 }).Run();


            entityInQueryIndex = 0;

            foreach (
                var (generate, entity) in
                SystemAPI
                    .Query<GenerateRegion>()
                    .WithNone<RegionIndex>()
                    .WithEntityAccess())
            {

                int2 i = generate.regionIndex;

                ecb.AddComponent<RegionIndex>(entityInQueryIndex, entity, i);
                ecb.AddSharedComponent<SharedRegionIndex>(entityInQueryIndex, entity, i);
                ecb.AddBuffer<LinkedEntityGroup>(entityInQueryIndex, entity);
                ecb.AddComponent<Region>(entityInQueryIndex, entity);

                entityInQueryIndex++;
            }

            // _barrier.AddJobHandleForProducer(Dependency);
        }

        void GenerateHeightMapBlobs(ref SystemState state)
        {
            Debug.Log("GenerateHeightMapBlobs");
            EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
            // var commandBuffer = _barrier.CreateCommandBuffer().AsParallelWriter();
            // Entities
            //     .WithNone<RegionHeightMap>()
            //     .WithAll<GenerateRegion>()
            //     .ForEach((int entityInQueryIndex, Entity e,
            //     in GenerateRegionHeightMapSettings settings,
            //     in RegionIndex regionIndex) =>
            //     {
            //         // Debug.LogFormat("regionIndex {0}", regionIndex.value.x);
            //         var heightMapBlob = HeightMapBuilder.BuildHeightMap(
            //             regionIndex.value * Constants.ChunkSize.xz, Constants.ChunkSurfaceSize,
            //             settings, Allocator.Persistent);

            //         var blobComponent = new RegionHeightMap { heightMapBlob = heightMapBlob };

            //         commandBuffer.AddComponent(entityInQueryIndex, e, blobComponent);

            //         commandBuffer.RemoveComponent<GenerateRegionHeightMapSettings>(entityInQueryIndex, e);
            //         commandBuffer.RemoveComponent<GenerateRegion>(entityInQueryIndex, e);

            //         commandBuffer.AddComponent<GenerateRegionChunks>(entityInQueryIndex, e);
            //     }).ScheduleParallel();

            // _barrier.AddJobHandleForProducer(Dependency);


            int entityInQueryIndex = 0;

            foreach (
                var (settings, regionIndex, entity) in
                SystemAPI
                    .Query<GenerateRegionHeightMapSettings, RegionIndex>()
                    .WithAll<GenerateRegion>()
                    .WithNone<RegionHeightMap>()
                    .WithEntityAccess())
            {
                var heightMapBlob = HeightMapBuilder.BuildHeightMap(
                    regionIndex.value * Constants.ChunkSize.xz, Constants.ChunkSurfaceSize,
                    settings, Allocator.Persistent);

                var blobComponent = new RegionHeightMap { heightMapBlob = heightMapBlob };

                ecb.AddComponent(entityInQueryIndex, entity, blobComponent);

                ecb.RemoveComponent<GenerateRegionHeightMapSettings>(entityInQueryIndex, entity);
                ecb.RemoveComponent<GenerateRegion>(entityInQueryIndex, entity);

                ecb.AddComponent<GenerateRegionChunks>(entityInQueryIndex, entity);

                entityInQueryIndex++;
            }

        }

        void GenerateChunks(ref SystemState state)
        {
            Debug.Log("GenerateChunks");
            EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);

            var chunkArchetype = state.EntityManager.CreateArchetype(
                typeof(ChunkBlockType)
            );
            // Entities
            //     .WithName("GenerateChunksInRegion")
            //     .WithStoreEntityQueryInField(ref _regionQuery)
            //     .WithAll<GenerateRegionChunks>()
            //     .ForEach((Entity e, int entityInQueryIndex,
            //     ref DynamicBuffer<LinkedEntityGroup> linkedGroup,
            //     in RegionHeightMap heightMap, in RegionIndex regionIndex
            //     ) =>
            //     {
            //         ref var hmArray = ref heightMap.Array;
            //         int maxHeight = int.MinValue;
            //         int maxHeightIndex = -1;

            //         for (int i = 0; i < hmArray.Length; ++i)
            //         {
            //             int height = hmArray[i];
            //             if (height > maxHeight)
            //             {
            //                 maxHeight = height;
            //                 maxHeightIndex = i;
            //             }
            //         }

            //         var playbackBuffer = commandBuffer.SetBuffer<LinkedEntityGroup>(entityInQueryIndex, e);
            //         if (linkedGroup.Length > 0)
            //             playbackBuffer.AddRange(linkedGroup.AsNativeArray());

            //         for (int y = 0; y < maxHeight; y += Constants.ChunkHeight)
            //         {
            //             var chunkEntity = commandBuffer.CreateEntity(entityInQueryIndex, chunkArchetype);
            //             commandBuffer.AddComponent<ChunkWorldHeight>(entityInQueryIndex, chunkEntity, y);
            //             commandBuffer.AddComponent<RegionHeightMap>(entityInQueryIndex, chunkEntity, heightMap);
            //             commandBuffer.AddComponent<RegionIndex>(entityInQueryIndex, chunkEntity, regionIndex);
            //             commandBuffer.AddComponent<GameChunk>(entityInQueryIndex, chunkEntity);
            //             commandBuffer.AddComponent<GenerateChunk>(entityInQueryIndex, chunkEntity);

            //             playbackBuffer.Add(chunkEntity);
            //         }

            //         commandBuffer.RemoveComponent<GenerateRegionChunks>(entityInQueryIndex, e);
            //     }).ScheduleParallel();

            // _barrier.AddJobHandleForProducer(Dependency);

            // (Entity e, int entityInQueryIndex,
            // ref DynamicBuffer<LinkedEntityGroup> linkedGroup,
            // in RegionHeightMap heightMap, in RegionIndex regionIndex

            // .WithName("GenerateChunksInRegion")
            // .WithStoreEntityQueryInField(ref _regionQuery)
            // .WithAll<GenerateRegionChunks>()
            int entityInQueryIndex = 0;
            foreach (
                var (settings, regionIndex, heightMap, entity) in
                SystemAPI
                    .Query<GenerateRegionHeightMapSettings, RegionIndex, RegionHeightMap>()
                    .WithAll<GenerateRegionChunks>()
                    .WithEntityAccess())
            {
                ref var hmArray = ref heightMap.Array;
                int maxHeight = int.MinValue;
                int maxHeightIndex = -1;

                for (int i = 0; i < hmArray.Length; ++i)
                {
                    int height = hmArray[i];
                    if (height > maxHeight)
                    {
                        maxHeight = height;
                        maxHeightIndex = i;
                    }
                }


                var query = SystemAPI.QueryBuilder().WithAll<GenerateRegionChunks>().Build();
                EntityQueryMask queryMask = query.GetEntityQueryMask();

                // var playbackBuffer = ecb.SetComponentForLinkedEntityGroup<GenerateRegionChunks>(entityInQueryIndex, entity, queryMask);
                // if (linkedGroup.Length > 0)
                //     playbackBuffer.AddRange(linkedGroup.AsNativeArray());

                for (int y = 0; y < maxHeight; y += Constants.ChunkHeight)
                {
                    var chunkEntity = ecb.CreateEntity(entityInQueryIndex, chunkArchetype);
                    ecb.AddComponent<ChunkWorldHeight>(entityInQueryIndex, chunkEntity, y);
                    ecb.AddComponent<RegionHeightMap>(entityInQueryIndex, chunkEntity, heightMap);
                    ecb.AddComponent<RegionIndex>(entityInQueryIndex, chunkEntity, regionIndex);
                    ecb.AddComponent<GameChunk>(entityInQueryIndex, chunkEntity);
                    ecb.AddComponent<GenerateChunk>(entityInQueryIndex, chunkEntity);

                    // playbackBuffer.Add(chunkEntity);
                }

                ecb.RemoveComponent<GenerateRegionChunks>(entityInQueryIndex, entity);
                // ecb.Playback(state.EntityManager);
                entityInQueryIndex++;
            }

        }

        void AssignChunkSharedIndices(ref SystemState state)
        {
            Debug.Log("AssignChunkSharedIndices");
            EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
            // var buffer = _barrier.CreateCommandBuffer();


            //             Entities
            //                 .WithName("SetChunkSharedIndices")
            //                 .WithoutBurst()
            //                 .WithAll<GameChunk>()
            //                 .WithNone<SharedRegionIndex>()
            //                 .ForEach((Entity e, in RegionIndex regionIndex, in ChunkWorldHeight chunkHeight) =>
            //                 {
            //                     int2 index = regionIndex.value;
            //                     buffer.AddSharedComponent<SharedRegionIndex>(e, index);

            // #if UNITY_EDITOR
            //                     // Note this causes an exception if you generate a lot of regions.
            //                     //EntityManager.SetName(e, $"Chunk ({index.x}, {index.y}): {chunkHeight.value / Constants.ChunkHeight}");
            // #endif
            //                 }).Run();
            int entityInQueryIndex = 0;

            foreach (
                var (regionIndex, entity) in
                SystemAPI
                    .Query<RegionIndex>()
                    .WithAll<GameChunk>()
                    .WithNone<SharedRegionIndex>()
                    .WithEntityAccess())
            {
                int2 index = regionIndex.value;
                ecb.AddSharedComponent<SharedRegionIndex>(entityInQueryIndex, entity, index);
                entityInQueryIndex++;

            }
        }

        // Unusued, switched to using blob assets for height maps
        void GenerateHeightMapBuffer(ref SystemState state)
        {
            Debug.Log("GenerateHeightMapBuffer");

            // TODO: check DynamicBuffer
            foreach (
                var (heightMapBuffer, settings, regionIndex, entity) in
                SystemAPI
                    .Query<DynamicBuffer<GenerateRegionHeightMap>, GenerateRegionHeightMapSettings, RegionIndex>()
                    .WithEntityAccess())
            {
                heightMapBuffer.ResizeUninitialized(Constants.ChunkSurfaceVolume);
                var heightMap = heightMapBuffer.Reinterpret<int>().AsNativeArray();

                int iterations = settings.iterations;
                int minHeight = settings.minHeight;
                int maxHeight = settings.maxHeight;
                float persistence = settings.persistence;
                float scale = settings.scale;

                int2 regionWorldPos = regionIndex.value * Constants.ChunkSize.xz;
                for (int x = 0; x < Constants.ChunkSizeX; ++x)
                    for (int z = 0; z < Constants.ChunkSizeZ; ++z)
                    {
                        int2 localPos = new int2(x, z);
                        int2 worldPos = regionWorldPos + localPos;
                        float h = NoiseUtil.SumOctave(
                            regionIndex.value.x + x, regionIndex.value.y + z,
                            iterations, persistence, scale, minHeight, maxHeight);

                        int i = GridUtil.Grid2D.PosToIndex(localPos);
                        heightMap[i] = (int)math.floor(h);
                    }
            }
        }
    }
}