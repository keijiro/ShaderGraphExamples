using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing.Controls;
using System.Collections.Generic;

namespace Plotting
{
    [Title("Utility", "Plotting", "Driver")]
    public class DriverNode : AbstractMaterialNode, IGeneratesBodyCode, IMayRequireMeshUV
    {
        [SerializeField] Vector2 m_Min = new Vector2(-0.1f, -1.1f);
        [SerializeField] Vector2 m_Max = new Vector2(2.1f, 1.1f);

        public const int kProbeSlotId = 0;
        public const int kDisplaySlotId = 1;

        const string kProbeSlotName = "Probe";
        const string kDisplaySlotName = "Display";

        public DriverNode()
        {
            name = "Plot Driver";
            UpdateNodeAfterDeserialization();
        }

        public override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector1MaterialSlot(kProbeSlotId, kProbeSlotName, kProbeSlotName, SlotType.Output, 0));
            AddSlot(new Vector2MaterialSlot(kDisplaySlotId, kDisplaySlotName, kDisplaySlotName, SlotType.Output, Vector2.zero));
            RemoveSlotsNameNotMatching(new[] { kProbeSlotId, kDisplaySlotId });
        }

        [MultiFloatControl("Min")]
        public Vector2 min
        {
            get { return m_Min; }
            set
            {
                if (m_Min == value) return;
                m_Min = value;
                Dirty(ModificationScope.Node);
            }
        }

        [MultiFloatControl("Max")]
        public Vector2 max
        {
            get { return m_Max; }
            set
            {
                if (m_Max == value) return;
                m_Max = value;
                Dirty(ModificationScope.Node);
            }
        }

        Vector4 uvTransformVector
        {
            get {
                var min = Vector2.Min(m_Min, m_Max - Vector2.one * 1e-4f);
                return new Vector4(m_Max.x - min.x, m_Max.y - min.y, min.x, min.y);
            }
        }

        public void GenerateNodeCode(ShaderGenerator visitor, GenerationMode generationMode)
        {
            var sb = new ShaderStringBuilder();

            var stVarName = GetVariableNameForNode() + "_st";
            var dispVarName = GetVariableNameForSlot(kDisplaySlotId);
            var probeVarName = GetVariableNameForSlot(kProbeSlotId);

            if (!generationMode.IsPreview())
            {
                var st = uvTransformVector;
                sb.AppendLine(
                    "{0}4 {1} = {0}4({2}, {3}, {4}, {5});",
                    precision, dispVarName, st.x, st.y, st.z, st.w
                );
            }

            sb.AppendLine(
                "{0}2 {1} = {2}.xy * IN.{3} + {2}.zw;",
                precision, dispVarName, stVarName, UVChannel.UV0.GetUVName()
            );

            sb.AppendLine(
                "{0} {1} = {2}.x;",
                precision, probeVarName, dispVarName
            );

            visitor.AddShaderChunk(sb.ToString(), false);
        }

        public override void CollectShaderProperties(PropertyCollector properties, GenerationMode generationMode)
        {
            base.CollectShaderProperties(properties, generationMode);

            if (!generationMode.IsPreview()) return;

            properties.AddShaderProperty(new Vector4ShaderProperty() {
                overrideReferenceName = GetVariableNameForNode() + "_st",
                generatePropertyBlock = false,
                value = uvTransformVector
            });
        }

        public override void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
        {
            base.CollectPreviewMaterialProperties(properties);

            properties.Add(new PreviewProperty(PropertyType.Vector4) {
                name = GetVariableNameForNode() + "_st",
                vector4Value = uvTransformVector
            });
        }

        public bool RequiresMeshUV(UVChannel channel)
        {
            return channel == UVChannel.UV0;
        }
    }
}
