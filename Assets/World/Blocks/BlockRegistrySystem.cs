using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using System.Linq;

// TODO: check with ISystem
/// <summary>
/// A struct that can be passed in to jobs to retrieve block UV data from
/// block IDs. This should be retrieved via <see cref="BlockRegistrySystem.GetBlockUVMap"/>
/// </summary>
public struct BlockUVMap
{
    BlobAssetReference<BlockRegistrySystem.BlockUVMapBlob> blob;
    public BlockUVMap(BlobAssetReference<BlockRegistrySystem.BlockUVMapBlob> blob)
    {
        this.blob = blob;
    }
    public ref BlobArray<float2> GetUVs(int blockID, int faceID) => ref blob.Value.uvs[blockID - 1][faceID];
}

/// <summary>
/// A struct that can be passed in to jobs to
/// retrieve block IDs from block names. This should be passed 
/// as read-only. Should be retrieved via <see cref="BlockRegistrySystem.GetBlockIDMap"/>.
/// </summary>
public struct BlockIDMap
{
    NativeHashMap<FixedString32Bytes, int> nameToIDMap;
    public BlockIDMap(NativeHashMap<FixedString32Bytes, int> map)
    {
        nameToIDMap = map;
    }
    public int GetID(FixedString32Bytes blockName) => nameToIDMap[blockName];
}

public partial class BlockRegistrySystem : SystemBase
{

    UnsafeBitArray _blockOpacity = default;
    NativeHashMap<FixedString32Bytes, int> _nameToIDMap = default;

    BlobAssetReference<BlockUVMapBlob> _blockUVMapBlob = default;


    public struct BlockUVs
    {
        NativeList<FixedList64Bytes<float2>> uvMap;
        NativeHashMap<FixedString32Bytes, int> nameToIDMap;
    }

    public int GetBlockID(FixedString32Bytes name) => _nameToIDMap[name];


    public int GetBlockID(string name) => GetBlockID((FixedString32Bytes)name);

    public ref BlobArray<float2> GetBlockFaceUVs(int blockID, int faceID) => ref _blockUVMapBlob.Value.uvs[blockID - 1][faceID];

    /// <summary>
    /// Retrieves a <see cref="BlockIDMap"/> that can be passed in to bursted jobs to retrieve block IDs from
    /// <seealso cref="FixedString32Bytes"/> block names. Should be passed to a job as readonly.
    /// </summary>
    public BlockIDMap GetBlockIDMap() => new BlockIDMap(_nameToIDMap);

    /// <summary>
    /// Retrieves a <see cref="BlockUVMap"/> which can be passed in to bursted jobs to retrieve block UV data
    /// from block ids.
    /// </summary>
    public BlockUVMap GetBlockUVMap() => new BlockUVMap(_blockUVMapBlob);

    protected override void OnCreate()
    {
        base.OnCreate();
        Debug.Log("BlockRegistrySystem OnCreate");

        var blocks = Resources.LoadAll<BlockAsset>("BlockAssets");
        Debug.LogFormat("Blocks length {0}", blocks.Count());

        InitContainers(blocks.Length);

        var builder = new BlobBuilder(Allocator.Temp);

        ref var uvRoot = ref builder.ConstructRoot<BlockUVMapBlob>();
        // Allocate base array-of-arrays
        var baseArray = builder.Allocate(ref uvRoot.uvs, blocks.Length);

        Debug.Log("Registering blocks");
        for (int blockID = 0; blockID < blocks.Length; ++blockID)
        {
            var block = blocks[blockID];
            RegisterBlockID(block, blockID);

            RegisterBlockUVs(block, ref builder, ref baseArray[blockID]);
        }

        _blockUVMapBlob = builder.CreateBlobAssetReference<BlockUVMapBlob>(Allocator.Persistent);
    }

    void RegisterBlockID(BlockAsset block, int id)
    {
        _nameToIDMap[block.name] = id + 1;
    }

    void RegisterBlockUVs(BlockAsset block, ref BlobBuilder builder, ref BlobArray<BlobArray<float2>> blockUVsBaseArray)
    {
        var blockFaces = block._faceSprites;
        var blobFaces = builder.Allocate(ref blockUVsBaseArray, blockFaces.Length);

        for (int faceIndex = 0; faceIndex < blockFaces.Length; ++faceIndex)
        {
            var blockUVs = blockFaces[faceIndex].uv;
            var blobUVs = builder.Allocate(ref blobFaces[faceIndex], blockUVs.Length);

            for (int uvIndex = 0; uvIndex < blockUVs.Length; ++uvIndex)
            {
                blobUVs[uvIndex] = blockUVs[uvIndex];
            }
        }
    }

    protected override void OnUpdate()
    {
    }

    void InitContainers(int len)
    {
        _blockOpacity = new UnsafeBitArray(len, Allocator.Persistent);
        _nameToIDMap = new NativeHashMap<FixedString32Bytes, int>(len, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _blockUVMapBlob.Dispose();
        _nameToIDMap.Dispose();
    }

    public struct BlockUVMapBlob
    {
        // uvs = array[blockIndex][faceIndex]
        public BlobArray<BlobArray<BlobArray<float2>>> uvs;
    }

}
