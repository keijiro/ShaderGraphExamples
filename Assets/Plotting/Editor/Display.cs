using System.Reflection;
using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing.Controls;

namespace Plotting
{
    [Title("Utility", "Plotting", "Display")]
    public class DisplayNode : CodeFunctionNode
    {
        public enum DisplayMode { Line, Fill }

        [SerializeField] DisplayMode m_DisplayMode = DisplayMode.Fill;

        public DisplayNode()
        {
            name = "Plot Display";
        }

        protected override MethodInfo GetFunctionToConvert()
        {
            return GetType().GetMethod(
                "Unity_PlotDisplay_" + displayMode,
                BindingFlags.Static | BindingFlags.NonPublic
            );
        }

        [EnumControl("Display Mode")]
        public DisplayMode displayMode
        {
            get { return m_DisplayMode; }
            set
            {
                if (m_DisplayMode == value) return;
                m_DisplayMode = value;
                Dirty(ModificationScope.Graph);
            }
        }

        static string Unity_PlotDisplay_Line(
            [Slot(0, Binding.None, 0, 0, 0, 0)] Vector1 Value,
            [Slot(1, Binding.None, 0, 0, 0, 0)] Vector2 Driver,
            [Slot(2, Binding.None)] out Vector3 Out)
        {
            Out = Vector3.zero;
            return @"
{
    {precision}2 dxy = {precision}2(ddx(Driver.x), ddy(Driver.y));
    {precision} cp = 1 - abs(Value - Driver.y) / max(dxy.y, min(dxy.y * 10, abs(ddx(Value))));
    {precision}2 cg = 1 - abs(round(Driver) - Driver) / dxy;
    {precision}2 cz = 1 - abs(Driver) / dxy;
    Out = max((float3)cp, lerp(float3(max(cz.x, cz.y), 0, max(cg.x, cg.y)), 0, 0.5));
}
";
        }

        static string Unity_PlotDisplay_Fill(
            [Slot(0, Binding.None, 0, 0, 0, 0)] Vector1 Value,
            [Slot(1, Binding.None, 0, 0, 0, 0)] Vector2 Driver,
            [Slot(2, Binding.None)] out Vector3 Out)
        {
            Out = Vector3.zero;
            return @"
{
    {precision}2 dxy = {precision}2(ddx(Driver.x), ddy(Driver.y));
    {precision} cp = (Driver.y < Value) * (0 < Value) * (Driver.y > 0) +
                     (Driver.y > Value) * (0 > Value) * (Driver.y < 0);
    {precision}2 cg = 1 - abs(round(Driver) - Driver) / dxy;
    {precision}2 cz = 1 - abs(Driver) / dxy;
    Out = max((float3)cp, lerp(float3(max(cz.x, cz.y), 0, max(cg.x, cg.y)), 0, 0.5));
}
";
        }
    }
}
