using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine.Profiling;

partial class CameraRenderer
{
    partial void DrawUnsupportedShaders();
    partial void PrepareForSceneWindow();
    partial void DrawGizmos();

    partial void PrepareBuffer();

    #if UNITY_EDITOR

    string SampleName { set; get; }

    static ShaderTagId[] legacyShaderTagIds = {
		new ShaderTagId("Always"),
		new ShaderTagId("ForwardBase"),
		new ShaderTagId("PrepassBase"),
		new ShaderTagId("Vertex"),
		new ShaderTagId("VertexLMRGBM"),
		new ShaderTagId("VertexLM")
	};

    static Material errorMaterial;

    partial void DrawUnsupportedShaders()
    {
        if (errorMaterial == null) {
			errorMaterial =
				new Material(Shader.Find("Hidden/InternalErrorShader"));
		}

        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera));
        drawingSettings.overrideMaterial = errorMaterial;
        
        for (int i = 1; i < legacyShaderTagIds.Length; i++) {
			drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
		}

        var filteringSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }

    partial void PrepareForSceneWindow()
    {
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
    }

    partial void PrepareBuffer()
    {
        Profiler.BeginSample("Ediotr Only");
        buffer.name = SampleName = camera.name;
        Profiler.EndSample();
    }

    #else
        // 这里实际上是只读属性，get，是用lamda表达式实现的。
        string SampleName => bufferName;
    #endif
}
