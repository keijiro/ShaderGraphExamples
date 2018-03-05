using System;
using System.Collections.Generic;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    [Title("Raymarch", "Ray Collector")]
    public class RayCollectorNode : AbstractMaterialNode, IGeneratesBodyCode
    {
        public RayCollectorNode()
        {
            name = "Ray Collector";
            UpdateNodeAfterDeserialization();
        }

        const int DistanceInputSlotId = 0;
        const int RayInputSlotId = 1;
        const int PositionInputSlotId = 2;
        const int PositionOutputSlotId = 3;
        const int NormalOutputSlotId = 4;

        const string kSlotName0 = "Distance";
        const string kSlotName1 = "Ray";
        const string kSlotName2 = "Position";
        const string kSlotName3 = "Position";
        const string kSlotName4 = "Normal";

        public override bool hasPreview { get { return false; } }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector1MaterialSlot(DistanceInputSlotId,  kSlotName0, kSlotName0, SlotType.Input,  0));
            AddSlot(new Vector3MaterialSlot(RayInputSlotId,       kSlotName1, kSlotName1, SlotType.Input,  Vector3.zero));
            AddSlot(new Vector3MaterialSlot(PositionInputSlotId,  kSlotName2, kSlotName2, SlotType.Input,  Vector3.zero));
            AddSlot(new Vector3MaterialSlot(PositionOutputSlotId, kSlotName3, kSlotName3, SlotType.Output, Vector3.zero));
            AddSlot(new Vector3MaterialSlot(NormalOutputSlotId,   kSlotName4, kSlotName4, SlotType.Output, Vector3.zero));

            RemoveSlotsNameNotMatching(new[] { DistanceInputSlotId, RayInputSlotId, PositionInputSlotId, PositionOutputSlotId, NormalOutputSlotId });
        }

        public void GenerateNodeCode(ShaderGenerator visitor, GenerationMode generationMode)
        {
            var distanceInput = GetSlotValue(DistanceInputSlotId, generationMode);
            var rayInput = GetSlotValue(RayInputSlotId, generationMode);
            var positionInput = GetSlotValue(PositionInputSlotId, generationMode);
            var positionOutput = GetVariableNameForSlot(PositionOutputSlotId);
            var normalOutput = GetVariableNameForSlot(NormalOutputSlotId);

            visitor.AddShaderChunk(string.Format("float rmdist = {0};", distanceInput), false);
            visitor.AddShaderChunk("if (rmdist < 0.01) break; rmpos += rmray * rmdist * 1.0; if (i==31) rmpos = 0;}", false);
            visitor.AddShaderChunk(string.Format("float3 {0} = rmpos;", positionOutput), false);
            visitor.AddShaderChunk(string.Format("float3 {0} = rmpos;", normalOutput), false);
        }
    }
}
