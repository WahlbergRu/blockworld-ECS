using Unity.Entities;
using UnityEngine;


public struct Config : IComponentData
{
    public int NumRows; // Blocks and players spawns in a grid, one Block and player per cell
    public int NumColumns;
    public float BlockGridCellSize;
    public float BlockRadius;
    public float BlockOffset;
    public float PlayerOffset;
    public float PlayerSpeed; // meters per second
    public float BallStartVelocity;
    public float BallVelocityDecay;
    public float BallKickingRangeSQ; // square distance of how close a player must be to a ball to kick it
    public float BallKickForce;
    public Entity BlockPrefab;
    public Entity PlayerPrefab;
}


public class ConfigAuthoring : MonoBehaviour
{
    // Most of these fields are unused in Step 1, but they will be used in later steps.
    public int BlocksNumRows;
    public int BlocksNumColumns;
    public float BlockGridCellSize;
    public float BlockRadius;
    public float BlockOffset;
    public float PlayerOffset;
    public float PlayerSpeed;
    public float BallStartVelocity;
    public float BallVelocityDecay;
    public float BallKickingRange;
    public float BallKickForce;
    public GameObject BlockPrefab;
    public GameObject PlayerPrefab;
}

class Baker : Baker<ConfigAuthoring>
{
    public override void Bake(ConfigAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        // Each authoring field corresponds to a component field of the same name.
        AddComponent(entity, new Config
        {
            NumRows = authoring.BlocksNumRows,
            NumColumns = authoring.BlocksNumColumns,
            BlockGridCellSize = authoring.BlockGridCellSize,
            BlockRadius = authoring.BlockRadius,
            BlockOffset = authoring.BlockOffset,
            PlayerOffset = authoring.PlayerOffset,
            PlayerSpeed = authoring.PlayerSpeed,
            BallStartVelocity = authoring.BallStartVelocity,
            BallVelocityDecay = authoring.BallVelocityDecay,
            BallKickingRangeSQ = authoring.BallKickingRange * authoring.BallKickingRange,
            BallKickForce = authoring.BallKickForce,
            // GetEntity() bakes a GameObject prefab into its entity equivalent.
            BlockPrefab = GetEntity(authoring.BlockPrefab, TransformUsageFlags.Dynamic),
            PlayerPrefab = GetEntity(authoring.PlayerPrefab, TransformUsageFlags.Dynamic),
        });
    }
}