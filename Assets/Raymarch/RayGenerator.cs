using System;
using System.Collections.Generic;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    [Title("Raymarch", "Ray Generator")]
    public class RayGeneratorNode : AbstractMaterialNode, IGeneratesBodyCode, IMayRequireMeshUV
    {
        public RayGeneratorNode()
        {
            name = "Ray Generator";
            UpdateNodeAfterDeserialization();
        }

        const int UVInputSlotId = 0;
        const int RayOutputSlotId = 1;
        const int PositionOutputSlotId = 2;

        const string kSlotName0 = "UV";
        const string kSlotName1 = "Ray";
        const string kSlotName2 = "Position";

        public override bool hasPreview { get { return false; } }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new UVMaterialSlot     (UVInputSlotId,        kSlotName0, kSlotName0, UVChannel.UV0                ));
            AddSlot(new Vector3MaterialSlot(RayOutputSlotId,      kSlotName1, kSlotName1, SlotType.Output, Vector3.zero));
            AddSlot(new Vector3MaterialSlot(PositionOutputSlotId, kSlotName2, kSlotName2, SlotType.Output, Vector3.zero));

            RemoveSlotsNameNotMatching(new[] { UVInputSlotId, RayOutputSlotId, PositionOutputSlotId });
        }

        public void GenerateNodeCode(ShaderGenerator visitor, GenerationMode generationMode)
        {
            var uvValue = GetSlotValue(UVInputSlotId, generationMode);
            var rayName = GetVariableNameForSlot(RayOutputSlotId);
            var positionName = GetVariableNameForSlot(PositionOutputSlotId);

            visitor.AddShaderChunk(string.Format("float3 rmray = normalize(float3(({1}).xy - 0.5, 1)); float3 {0} = rmray;", rayName, uvValue), false);
            visitor.AddShaderChunk("float3 rmpos = 0;", false);
            visitor.AddShaderChunk("for (int i = 0; i < 32; i++) {", false);
            visitor.AddShaderChunk(string.Format("float3 {0} = rmpos;", positionName), false);
        }

        public bool RequiresMeshUV(UVChannel channel)
        {
            foreach (var slot in this.GetInputSlots<MaterialSlot>())
            {
                if (slot.RequiresMeshUV(channel)) return true;
            }
            return false;
        }
    }
}
