using UnityEngine;
using UnityEngine.Rendering;

partial class CameraRenderer
{
    ScriptableRenderContext context;
    Camera camera;

    const string bufferName = "Render Camera";
    CommandBuffer buffer = new CommandBuffer { name = bufferName };

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    CullingResults cullingResults;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        if (!Cull())
        {
            return;
        }

        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        Submit();
    }

    void Setup()
    {
        context.SetupCameraProperties(camera);
        buffer.ClearRenderTarget(true, true, Color.clear);
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
    }

    void DrawVisibleGeometry()
    {
        SortingSettings sortingSettings = new SortingSettings(camera){
            criteria = SortingCriteria.CommonOpaque
        };

        DrawingSettings drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);

        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        context.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }

    void ExecuteBuffer () {
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

    void Submit()
    {
        buffer.EndSample(bufferName);
        ExecuteBuffer();
        context.Submit();
    }
}
