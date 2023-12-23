using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    [Serializable]
    public struct GenerateRegionHeightMapSettings : IComponentData
    {
        public int iterations;
        public int maxHeight;
        public int minHeight;
        public int range;
        public float scale;
        public float persistence;

        public static GenerateRegionHeightMapSettings Default => new GenerateRegionHeightMapSettings
        {
            iterations = 16,
            persistence = .5f,
            range = 15,
            scale = 0.01f,
            minHeight = 0,
            maxHeight = 15,
        };
    }

    public class GenerateRegionHeightMapSettingsAuthoring : MonoBehaviour
    {
        public int iterations;
        public int maxHeight;
        public int minHeight;
        public int range;
        public float scale;
        public float persistence;
    }

    public class GenerateRegionHeightMapSettingsAuthoringBaker : Baker<GenerateRegionHeightMapSettingsAuthoring>
    {
        public override void Bake(GenerateRegionHeightMapSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GenerateRegionHeightMapSettings
            {
                iterations = authoring.iterations,
                persistence = authoring.persistence,
                range = authoring.range,
                scale = authoring.scale,
                minHeight = authoring.minHeight,
                maxHeight = authoring.maxHeight,
            });
        }
    }
}