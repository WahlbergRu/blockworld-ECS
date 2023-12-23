using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public struct GenerateHeightMap : IComponentData
    {
        public GenerateRegionHeightMapSettings _settings;
    }

    public class GenerateHeightMapAuthoring : MonoBehaviour
    {
        public GenerateRegionHeightMapSettings _settings = GenerateRegionHeightMapSettings.Default;
    }

    public class GenerateHeightMapAuthoringBaker : Baker<GenerateHeightMapAuthoring>
    {
        public override void Bake(GenerateHeightMapAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new GenerateHeightMap
            {
            });
        }
    }
}
