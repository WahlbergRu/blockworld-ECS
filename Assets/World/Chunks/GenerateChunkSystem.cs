using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using Unity.Profiling;

namespace BlockGame.BlockWorld
{
    [BurstCompile]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial struct GenerateChunkSystem : ISystem
    {

        struct BlockIDs
        {
            public int sand;
            public int dirt;
            public int grass;
            public int stone;
        }

        BlockIDs GetBlockIDs(ref SystemState state)
        {
            Debug.Log("GetBlockIDs");

            var ids = new BlockIDs();

            var registry = state.World.GetOrCreateSystemManaged<BlockRegistrySystem>();
            ids.dirt = registry.GetBlockID("Dirt");
            ids.sand = registry.GetBlockID("Sand");
            ids.grass = registry.GetBlockID("Grass");
            ids.stone = registry.GetBlockID("Stone");
            return ids;
        }

        static ChunkBlockType SelectBlock(int height, int maxHeight, ref BlockIDs ids)
        {
            ChunkBlockType block = default;

            int sandHeight = 4;
            int dirtHeight = 7;
            int grassHeight = 10;
            int stonHeight = int.MaxValue;

            // Sand
            if (height <= sandHeight)
                block = ids.sand;
            // Dirt
            else if (height <= dirtHeight)
                block = ids.dirt;
            // Grass
            else if (height <= grassHeight)
            {
                if (height == maxHeight)
                    block = ids.grass;
                else
                    block = ids.dirt;
            }
            // Stone
            else if (height < stonHeight)
                block = ids.stone;

            return block;
        }

        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Debug.Log("GenerateChunkSystem OnUpdate");
            EntityCommandBuffer.ParallelWriter commandBuffer = GetEntityCommandBuffer(ref state);

            // Creates a new instance of the job, assigns the necessary data, and schedules the job in parallel.
            // new ProcessSpawnerJob
            // {
            //     ElapsedTime = SystemAPI.Time.ElapsedTime,
            //     Ecb = ecb
            // }.ScheduleParallel();


            var entityInQueryIndex = 0;
            foreach (
                var (chunk, entity) in
                     SystemAPI.Query<RefRO<GenerateChunk>>()
                         .WithEntityAccess())  // Relevant in Step 5
            {
                commandBuffer.AddBuffer<ChunkBlockType>(entityInQueryIndex, entity);
                // Debug.Log(entityInQueryIndex);
                entityInQueryIndex++;
            }

            // var blockIDs = GetBlockIDs();

            // foreach (var (entityInQueryIndex, e) in
            //          SystemAPI.Query<RefRW<entityInQueryIndex>, RefRW<Entity>>()
            //             .WithName("InitializeChunkBlocks")
            //             .WithAll<GenerateChunk>())  // Relevant in Step 5
            // {
            //     commandBuffer.AddBuffer<ChunkBlockType>(entityInQueryIndex, e);
            // }

            // ProfilerMarker marker = new ProfilerMarker("InitChunkBlocks");
            // Entities
            //     .WithName("InitializeChunkBlocks")
            //     .WithAll<GenerateChunk>()
            //     .ForEach((int entityInQueryIndex, Entity e, ref DynamicBuffer<ChunkBlockType> blocksBuffer,
            //     in ChunkWorldHeight chunkWorldHeight, in RegionHeightMap heightMapBlob) =>
            //     {
            //         blocksBuffer.ResizeUninitialized(Constants.ChunkVolume);

            //         var blocks = blocksBuffer.AsNativeArray();

            //         ref var heightMap = ref heightMapBlob.Array;

            //         for (int i = 0; i < blocks.Length; ++i)
            //         {
            //             int3 xyz = GridUtil.Grid3D.IndexToPos(i);
            //             int x = xyz.x;
            //             int y = xyz.y;
            //             int z = xyz.z;

            //             int xzIndex = GridUtil.Grid2D.PosToIndex(x, z);

            //             int maxHeight = heightMap[xzIndex];
            //             int height = y + chunkWorldHeight;

            //             if (height <= maxHeight)
            //                 blocks[i] = SelectBlock(height, maxHeight, ref blockIDs);
            //             else
            //                 blocks[i] = default;
            //         }

            //         commandBuffer.RemoveComponent<GenerateChunk>(entityInQueryIndex, e);
            //         commandBuffer.AddComponent<GenerateMesh>(entityInQueryIndex, e);
            //     }).ScheduleParallel();

        }

        // TODO: extract to static
        private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            return ecb.AsParallelWriter();
        }

    }

    // [BurstCompile]
    // public partial struct ProcessSpawnerJob : IJobEntity
    // {
    //     public EntityCommandBuffer.ParallelWriter Ecb;
    //     public double ElapsedTime;

    //     // IJobEntity generates a component data query based on the parameters of its `Execute` method.
    //     // This example queries for all Spawner components and uses `ref` to specify that the operation
    //     // requires read and write access. Unity processes `Execute` for each entity that matches the
    //     // component data query.
    //     private void Execute([ChunkIndexInQuery] int chunkIndex, ref Spawner spawner)
    //     {
    //         // If the next spawn time has passed.
    //         if (spawner.NextSpawnTime < ElapsedTime)
    //         {
    //             // Spawns a new entity and positions it at the spawner.
    //             Entity newEntity = Ecb.Instantiate(chunkIndex, spawner.Prefab);
    //             Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPosition(spawner.SpawnPosition));

    //             // Resets the next spawn time.
    //             spawner.NextSpawnTime = (float)ElapsedTime + spawner.SpawnRate;
    //         }
    //     }
    // }
}