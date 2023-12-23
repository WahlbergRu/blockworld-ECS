// using Unity.Collections;
// using Unity.Entities;
// using Unity.Transforms;
// using Unity.Burst;
// using Unity.Mathematics;
// using UnityEngine;


// // TODO: add namespace
// [BurstCompile]
// [UpdateBefore(typeof(TransformSystemGroup))]
// public partial struct OptimizedBlockSpawnerSystem : ISystem
// {
//     public Config config;
//     public Entity block;

//     [BurstCompile]
//     public void OnCreate(ref SystemState state)
//     {
//         // RequireForUpdate<T> causes the system to skip updates
//         // as long as no instances of component T exist in the world.

//         // Normally a system will start updating before the main scene is loaded. By using RequireForUpdate,
//         // we can make a system skip updating until certain components are loaded from the scene.

//         // This system needs to access the singleton component Config, which
//         // won't exist until the scene has loaded.
//         state.RequireForUpdate<Config>();
//         Debug.Log("BlockSwapn");


//     }

//     public void OnDestroy(ref SystemState state) { }

//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//         state.Enabled = false;

//         config = SystemAPI.GetSingleton<Config>();

//         EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
//         // TODO: rework to 1d array 
//         // Spawn the obstacles in a grid.
//         for (int column = 0; column < config.NumColumns; column++)
//         {
//             for (int row = 0; row < config.NumColumns; row++)
//             {
//                 block = state.EntityManager.Instantiate(config.BlockPrefab);
//                 // Instantiate copies an entity: a new entity is created with all the same component types
//                 // and component values as the BlockPrefab entity.

//                 // Position the new obstacle by setting its LocalTransform component.
//                 state.EntityManager.SetComponentData(block, new LocalTransform
//                 {
//                     Position = new float3
//                     {
//                         x = (column * config.BlockGridCellSize),
//                         y = 0,
//                         z = (row * config.BlockGridCellSize),
//                     },
//                     Scale = 1,
//                     Rotation = quaternion.identity
//                 });
//             }
//         }

        // Creates a new instance of the job, assigns the necessary data, and schedules the job in parallel.
        // new ProcessBlockSpawnerJob
        // {
        //     ElapsedTime = SystemAPI.Time.ElapsedTime,
        //     Ecb = ecb
        // }.ScheduleParallel();
//     }

//     private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
//     {
//         var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
//         var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
//         return ecb.AsParallelWriter();
//     }

// }

// [BurstCompile]
// public partial struct ProcessBlockSpawnerJob : IJobEntity
// {
//     public EntityCommandBuffer.ParallelWriter Ecb;
//     public double ElapsedTime;

//     // IJobEntity generates a component data query based on the parameters of its `Execute` method.
//     // This example queries for all Spawner components and uses `ref` to specify that the operation
//     // requires read and write access. Unity processes `Execute` for each entity that matches the
//     // component data query.
//     private void Execute([ChunkIndexInQuery] int chunkIndex, ref BlockSpawner blockSpawner)
//     {
//         // If the next spawn time has passed.
//         if (blockSpawner.NextSpawnTime < ElapsedTime)
//         {
//             // Spawns a new entity and positions it at the blockSpawner.
//             Entity newEntity = Ecb.Instantiate(chunkIndex, blockSpawner.Prefab);
//             Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPosition(new float3(chunkIndex, 0, 0)));

//             // Resets the next spawn time.
//             blockSpawner.NextSpawnTime = (float)ElapsedTime + blockSpawner.SpawnRate;
//         }
//     }
// }
