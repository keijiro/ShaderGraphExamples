using UnityEngine;
using UnityEditor.ShaderGraph;
using System.Reflection;

[Title("Custom", "AnimatedUV")]
class AnimatedUV : CodeFunctionNode, IMayRequireTime
{
    public AnimatedUV() { name = "Animated UV"; }

    public bool RequiresTime() { return true; }

    public override bool hasPreview { get { return false; } }

    protected override MethodInfo GetFunctionToConvert()
    {
        return GetType().GetMethod("Custom_AnimatedUV", BindingFlags.Static | BindingFlags.NonPublic);
    }

    static string Custom_AnimatedUV(
        [Slot(0, Binding.MeshUV0)] Vector2 UV,
        [Slot(1, Binding.None, 1, 1, 1, 1)] Vector2 Scale,
        [Slot(2, Binding.None, 1, 1, 1, 1)] Vector2 Speed,
        [Slot(3, Binding.None)] out Vector2 Out
    )
    {
        Out = Vector2.zero;
        return @"
{
    Out = UV * Scale + Speed * _Time.y;
}
";
    }
}
