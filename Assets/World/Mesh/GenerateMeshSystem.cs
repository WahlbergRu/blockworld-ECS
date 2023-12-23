using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using System;
using Unity.Burst;

namespace BlockGame.BlockWorld
{
	[UpdateAfter(typeof(GenerateChunkSystem))]
	[BurstCompile]
	public partial struct GenerateMeshSystem : ISystem
	{
		private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
		{
			var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
			var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
			return ecb.AsParallelWriter();
		}


		public void OnCreate(ref SystemState state)
		{
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			Dictionary<Entity, GameObject> _goMap = new Dictionary<Entity, GameObject>();
			EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
			AddRenderMeshes(ref state, ref _goMap, ref ecb);
			GenerateMeshData(ref state, ref _goMap, ref ecb);
			CleanGameObjects(ref state, ref _goMap, ref ecb);
		}

		void AddRenderMeshes(ref SystemState state, ref Dictionary<Entity, GameObject> _goMap, ref EntityCommandBuffer.ParallelWriter ecb)
		{
			Debug.Log("AddRenderMeshes");
			var entityInQueryIndex = 0;
			foreach (
				var (chunkHeight, regionIndex, entity) in
					 SystemAPI.Query<ChunkWorldHeight, RegionIndex>()
						.WithAll<GenerateMesh>()
						.WithNone<ChunkMeshVerts>()
						.WithNone<ChunkMeshIndices>()
						.WithNone<ChunkMeshUVs>()
						.WithEntityAccess())  // Relevant in Step 5
			{
				// Debug.Log(entityInQueryIndex);
				float3 p = new float3(
					regionIndex.value.x * Constants.ChunkSizeX,
					chunkHeight,
					regionIndex.value.y * Constants.ChunkSizeZ);

				//var renderMesh = new RenderMesh();
				//buffer.AddSharedComponent(entityInQueryIndex, e, renderMesh);
				//buffer.AddComponent<RenderBounds>(entityInQueryIndex, e);
				//buffer.AddComponent<PerInstanceCullingTag>(entityInQueryIndex, e);
				//buffer.AddComponent<LocalToWorld>(entityInQueryIndex, e);
				//buffer.AddComponent<Translation>(entityInQueryIndex, e, new Translation
				//{
				//	Value = p
				//});

				ecb.AddBuffer<ChunkMeshVerts>(entityInQueryIndex, entity);
				ecb.AddBuffer<ChunkMeshIndices>(entityInQueryIndex, entity);
				ecb.AddBuffer<ChunkMeshUVs>(entityInQueryIndex, entity);

				entityInQueryIndex++;
			}
		}

		void GenerateMeshData(ref SystemState state, ref Dictionary<Entity, GameObject> _goMap, ref EntityCommandBuffer.ParallelWriter ecb)
		{
			var blockUVMap = state.World.GetOrCreateSystemManaged<BlockRegistrySystem>().GetBlockUVMap();
			Debug.Log("GenerateMeshData");

			int entityInQueryIndex = 0;

			foreach (
				var (chunkHeight, regionIndex, entity) in
					 SystemAPI.Query<ChunkWorldHeight, RegionIndex>()
						.WithAll<GenerateMesh>()
						.WithEntityAccess())  // Relevant in Step 5
			{
				// Debug.Log(entityInQueryIndex);
				float3 p = new float3(
					regionIndex.value.x * Constants.ChunkSizeX,
					chunkHeight,
					regionIndex.value.y * Constants.ChunkSizeZ);

				//var renderMesh = new RenderMesh();
				//buffer.AddSharedComponent(entityInQueryIndex, e, renderMesh);
				//buffer.AddComponent<RenderBounds>(entityInQueryIndex, e);
				//buffer.AddComponent<PerInstanceCullingTag>(entityInQueryIndex, e);
				//buffer.AddComponent<LocalToWorld>(entityInQueryIndex, e);
				//buffer.AddComponent<Translation>(entityInQueryIndex, e, new Translation
				//{
				//	Value = p
				//});

				ecb.AddBuffer<ChunkMeshVerts>(entityInQueryIndex, entity);
				ecb.AddBuffer<ChunkMeshIndices>(entityInQueryIndex, entity);
				ecb.AddBuffer<ChunkMeshUVs>(entityInQueryIndex, entity);

				entityInQueryIndex++;
			}

			entityInQueryIndex = 0;
			foreach (
				var (vertsBuffer, indicesBuffer, uvBuffer, blocksBuffer, entity) in
					 SystemAPI.Query<DynamicBuffer<ChunkMeshVerts>, DynamicBuffer<ChunkMeshIndices>, DynamicBuffer<ChunkMeshUVs>, DynamicBuffer<ChunkBlockType>>()
						.WithAll<GenerateMesh>()
						.WithEntityAccess())  // Relevant in Step 5
			{
				Debug.Log("GenerateMeshData");
				var blocks = blocksBuffer.AsNativeArray();

				vertsBuffer.Clear();
				indicesBuffer.Clear();
				uvBuffer.Clear();

				var verts = vertsBuffer.Reinterpret<float3>();
				var indices = indicesBuffer.Reinterpret<int>();
				var uvs = uvBuffer.Reinterpret<float2>();

				for (int i = 0; i < blocks.Length; ++i)
				{
					var curr = blocks[i];
					if (curr == 0)
						continue;

					int3 xyz = GridUtil.Grid3D.IndexToPos(i);

					for (int dirIndex = 0; dirIndex < GridUtil.Grid3D.Directions.Length; ++dirIndex)
					{
						var dir = GridUtil.Grid3D.Directions[dirIndex];
						var adj = GetBlockType(xyz + dir, blocks);

						if (!IsOpaque(adj))
						{
							ref var faceUVs = ref blockUVMap.GetUVs(curr, dirIndex);
							BuildFace(xyz, dir, verts, indices, uvs, ref faceUVs);
						}
					}
				}
			}

			ApplyMeshData(ref state, ref _goMap, ref ecb);
		}

		/// <summary>
		/// Create the mesh from our mesh data.
		/// </summary>
		void ApplyMeshData(ref SystemState state, ref Dictionary<Entity, GameObject> _goMap, ref EntityCommandBuffer.ParallelWriter ecb)
		{
			Debug.Log("ApplyMeshData");

			Material _sharedMat = Resources.Load<Material>("Materials/BlocksMat");
			int entityInQueryIndex = 0;

			foreach (
				var (vertBuffer, indicesBuffer, uvBuffer, regionIndex, height, entity) in
					 SystemAPI.Query<DynamicBuffer<ChunkMeshVerts>, DynamicBuffer<ChunkMeshIndices>, DynamicBuffer<ChunkMeshUVs>, RegionIndex, ChunkWorldHeight>()
						.WithAll<GenerateMesh>()
						.WithEntityAccess())  // Relevant in Step 5
			{

				var mesh = new Mesh();

				mesh.SetVertices(vertBuffer.Reinterpret<float3>().AsNativeArray());
				mesh.SetIndices(indicesBuffer.Reinterpret<int>().AsNativeArray(), MeshTopology.Triangles, 0);
				mesh.SetUVs(0, uvBuffer.Reinterpret<float2>().AsNativeArray());

				mesh.RecalculateBounds();
				mesh.RecalculateNormals();
				//mesh.RecalculateTangents();

				int2 index = regionIndex.value;

				var go = new GameObject($"Chunk ({index.x}, {index.y}): {height.value / Constants.ChunkHeight}");
				var filter = go.AddComponent<MeshFilter>();
				var renderer = go.AddComponent<MeshRenderer>();
				filter.sharedMesh = mesh;
				renderer.sharedMaterial = _sharedMat;

				float3 p = new float3(index.x, height, index.y);
				p.xz *= Constants.ChunkSize.xz;
				go.transform.position = p;

				go.isStatic = true;

				ecb.AddComponent<ChunkMeshGameObjectState>(entityInQueryIndex, entity);
				_goMap[entity] = go;

				ecb.RemoveComponent<GenerateMesh>(entityInQueryIndex, entity);
				entityInQueryIndex++;
			}
		}

		/// <summary>
		/// Destroy gameobjects attached to chunks that no longer exist
		/// </summary>
		void CleanGameObjects(ref SystemState state, ref Dictionary<Entity, GameObject> _goMap, ref EntityCommandBuffer.ParallelWriter ecb)
		{
			int entityInQueryIndex = 0;

			foreach (
				var (chunkMeshGameObjectState, entity) in
					 SystemAPI.Query<ChunkMeshGameObjectState>()
						.WithNone<ChunkMeshVerts>()
						.WithEntityAccess())  // Relevant in Step 5
			{
				GameObject.Destroy(_goMap[entity]);
				_goMap.Remove(entity);
				ecb.RemoveComponent<ChunkMeshGameObjectState>(entityInQueryIndex, entity);
				entityInQueryIndex++;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void BuildFace(float3 p, int3 dir,
			DynamicBuffer<float3> verts, DynamicBuffer<int> indices, DynamicBuffer<float2> uvs,
			ref BlobArray<float2> faceUVs)
		{
			float3 center = p + new float3(.5f);
			float3 normal = dir;

			float3 up = new float3(0, 1, 0);
			if (normal.y != 0)
				up = new float3(-1, 0, 0);

			float3 front = center + normal * .5f;

			float3 perp1 = math.cross(normal, up);
			float3 perp2 = math.cross(perp1, normal);

			int start = verts.Length;

			verts.Add(front + (-perp1 + perp2) * .5f);
			verts.Add(front + (perp1 + perp2) * .5f);
			verts.Add(front + (-perp1 + -perp2) * .5f);
			verts.Add(front + (perp1 + -perp2) * .5f);

			// For a normal going in the negative Z direction (Quad visible to a forward facing camera):
			// 0---1
			// | / |
			// 2---3
			indices.Add(start + 0);
			indices.Add(start + 1);
			indices.Add(start + 2);
			indices.Add(start + 3);
			indices.Add(start + 2);
			indices.Add(start + 1);

			// Uv order set to match the default order of Unity's sprite UVs.
			uvs.Add(faceUVs[0]);
			uvs.Add(faceUVs[2]);
			uvs.Add(faceUVs[3]);
			uvs.Add(faceUVs[1]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool IsOpaque(ChunkBlockType block) => block.blockType != 0;

		static ChunkBlockType GetBlockType(int3 p, NativeArray<ChunkBlockType> blocks)
		{
			if (p.x >= 0 && p.x < Constants.ChunkSizeX &&
				p.y >= 0 && p.y < Constants.ChunkSizeY &&
				p.z >= 0 && p.z < Constants.ChunkSizeZ)
			{
				int i = GridUtil.Grid3D.PosToIndex(p);
				return blocks[i];
			}

			return default;
		}
	}
}
