using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DeathDissolveRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public string name = "DeathDissolvePass";
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Material material;
    }

    public Settings settings = new Settings();


    DeathDissolvePass m_ScriptablePass;

    public override void Create()
    {
        this.name = settings.name;
        m_ScriptablePass = new DeathDissolvePass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }

    class DeathDissolvePass : ScriptableRenderPass
    {
        // ������Ⱦ Tags
        static readonly string k_RenderTag = "DeathDissolvePass";
        int temp01 = Shader.PropertyToID("dissolvetemp");

        // ����ʹ�ò���
        Material effMat;
        DeathDissolve effVolum;

        RenderTargetIdentifier currentTarget;   // ���õ�ǰ��ȾĿ��

        public void Setup(in RenderTargetIdentifier currentTarget)
        {
            this.currentTarget = currentTarget;
        }

        public DeathDissolvePass(Settings setting)
        {
            renderPassEvent = setting.passEvent;
            if (setting.material == null )
                return;
            effMat = setting.material;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (effMat == null) return;

            if (!renderingData.cameraData.postProcessEnabled) return;


            effVolum = VolumeManager.instance.stack.GetComponent<DeathDissolve>();
            if (effVolum == null) return;
            if (!effVolum.UseThis.value) return;
            if (effVolum.UseSceneCamera.value)
            {

            }
            else
            {
                if (renderingData.cameraData.isSceneViewCamera) return;//������scene�����
            }
            if (!effVolum.active)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
            //������Ⱦ����
            Render(cmd, ref renderingData);

            //ִ�к���
            context.ExecuteCommandBuffer(cmd);
            //�ͷ�
            cmd.ReleaseTemporaryRT(temp01);
            CommandBufferPool.Release(cmd);
        }

        //����һ����Ⱦ����
        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //��ȡ���������
            ref var cameraData = ref renderingData.cameraData;
            //���������������Ϣ
            var desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            cmd.GetTemporaryRT(temp01, desc);
            effMat.SetColor("_OutEdgeColor",effVolum.OutEdgeColor.value);
            effMat.SetColor("_InEdgeColor",effVolum.InEdgeColor.value);
            effMat.SetColor("_BackColor",effVolum.BackColor.value);
            effMat.SetFloat("_Control",effVolum.Control.value);
            effMat.SetFloat("_BigEdgeWidth",effVolum.BigEdgeWidth.value);
            effMat.SetFloat("_SmallEdgeWidth", effVolum.SmallEdgeWidth.value);
            cmd.Blit(currentTarget, temp01,effMat,0);
            cmd.Blit(temp01, currentTarget);
        }

    }
}


