using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GenerateTransformMRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Setting
    {
        public string name= "GenerateTransformM";
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingOpaques;
    }
    public Setting setting;


    class CustomRenderPass : ScriptableRenderPass
    {

        // ������Ⱦ Tags
        static readonly string k_RenderTag = "GenerateTransformM";


        public CustomRenderPass(Setting setting)
        {
            renderPassEvent = setting.passEvent;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
            //������Ⱦ����
            Render(cmd, ref renderingData);

            //ִ�к���
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        //����һ����Ⱦ����
        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;
            float height = camera.nearClipPlane * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
            Vector3 up = camera.transform.up * height;
            Vector3 right = camera.transform.right * height * camera.aspect;
            Vector3 forward = camera.transform.forward * camera.nearClipPlane;
           
            Vector3 buttomLeft = forward - right - up;
            float scale = buttomLeft.magnitude / camera.nearClipPlane;
            buttomLeft.Normalize();
            buttomLeft *= scale;
            Vector3 buttomRight = forward + right - up;
            buttomRight.Normalize();
            buttomRight *= scale;
            Vector3 topRight = forward + right + up;
            topRight.Normalize();
            topRight *= scale;
            Vector3 topLeft = forward - right + up;
            topLeft.Normalize();
            topLeft *= scale;

            Matrix4x4 transformM = new Matrix4x4();
            transformM.SetRow(0, buttomLeft);
            transformM.SetRow(1, buttomRight);
            transformM.SetRow(2, topRight);
            transformM.SetRow(3, topLeft);
            cmd.SetGlobalMatrix("_TransformM", transformM);
        }
    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        this.name=setting.name;
        m_ScriptablePass = new CustomRenderPass(setting);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


