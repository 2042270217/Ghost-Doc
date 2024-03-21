using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SphereBlurRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public string name = "SphereBlurRenderFeature";
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader blurShader;
        public Shader radialDistortShader;
        public Shader sphereMaskShader;
    }

    public Settings settings = new Settings();


    SphereBlurPass m_ScriptablePass;

    public override void Create()
    {
        this.name = settings.name;
        m_ScriptablePass = new SphereBlurPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }

    class SphereBlurPass : ScriptableRenderPass
    {
        // ������Ⱦ Tags
        static readonly string k_RenderTag = "SphereBlurPass";
        int temp01 = Shader.PropertyToID("blurTemp01");
        int temp02 = Shader.PropertyToID("blurTemp02");
        int temp03 = Shader.PropertyToID("temp03");
        int blurRT = Shader.PropertyToID("_BlurTex");
        // ����ʹ�ò���
        Material sphereMaskMaterial;
        SphereBlur effVolum;
        Material blurMaterial;
        Material radialDistortMaterial;

        RenderTargetIdentifier currentTarget;   // ���õ�ǰ��ȾĿ��

        //������
        int width;
        int height;

        public void Setup(in RenderTargetIdentifier currentTarget)
        {
            this.currentTarget = currentTarget;
        }

        public SphereBlurPass(Settings setting)
        {
            renderPassEvent = setting.passEvent;
            Shader blurShader = setting.blurShader;
            Shader radialDistortShader = setting.radialDistortShader;
            Shader sphereMaskShader = setting.sphereMaskShader;
            if (blurShader == null || radialDistortShader==null || sphereMaskShader==null)
                return;
            blurMaterial = CoreUtils.CreateEngineMaterial(blurShader);
            radialDistortMaterial = CoreUtils.CreateEngineMaterial(radialDistortShader);
            sphereMaskMaterial = CoreUtils.CreateEngineMaterial(sphereMaskShader);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (blurMaterial == null) return;
            
            if (!renderingData.cameraData.postProcessEnabled) return;


            effVolum = VolumeManager.instance.stack.GetComponent<SphereBlur>();
            if (!effVolum.UseThis.value) return;
            if (effVolum == null) return;
            if (effVolum.UseSceneCamera.value)
            {

            }
            else
            {
                if (renderingData.cameraData.isSceneViewCamera) return;//������scene�����
            }
            if (!effVolum.active) return;

            CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
            //������Ⱦ����
            Render(cmd, ref renderingData);

            //ִ�к���
            context.ExecuteCommandBuffer(cmd);
            //�ͷ�
            cmd.ReleaseTemporaryRT(temp01);
            cmd.ReleaseTemporaryRT(temp02);
            cmd.ReleaseTemporaryRT(temp03);
            cmd.ReleaseTemporaryRT(blurRT);
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
            cmd.GetTemporaryRT(temp03, desc);
            cmd.GetTemporaryRT(blurRT, desc);
            desc.width = desc.width / effVolum.DownSample.value;
            desc.height = desc.height / effVolum.DownSample.value;

            var source = currentTarget;
            cmd.GetTemporaryRT(temp01, desc);
            cmd.GetTemporaryRT(temp02, desc);

            cmd.Blit(source, temp01);
            float size = 0;
            int iteration = effVolum.Loop.value;
            blurMaterial.SetFloat("_Size", size);
            for(int i = 0; i < iteration; ++i)
            {
                size = i * effVolum.Radius.value;
                blurMaterial.SetFloat("_Size", size);
                cmd.Blit(temp01, temp02, blurMaterial);
                var temp = temp02;
                temp02 = temp01;
                temp01 = temp;
            }
            //cmd.Blit(temp01, source);
            cmd.Blit(temp01, temp03);
            radialDistortMaterial.SetVector("_PolarCenter", new Vector4(effVolum.Center.value.x, effVolum.Center.value.y, 0, 0));
            radialDistortMaterial.SetFloat("_Density", effVolum.DistortPara.value.x);
            radialDistortMaterial.SetFloat("_DistortIntensity", effVolum.DistortPara.value.y);
            radialDistortMaterial.SetFloat("_DistortStart", effVolum.DistortPara.value.z);
            radialDistortMaterial.SetFloat("_DistortEnd", effVolum.DistortPara.value.w);
            radialDistortMaterial.SetFloat("_FlowSpeed", effVolum.FlowSpeed.value);

            cmd.Blit(temp03, blurRT,radialDistortMaterial,0);

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

            sphereMaskMaterial.SetMatrix("_TransformM", transformM);
            sphereMaskMaterial.SetVector("_TargetPos", effVolum.TargetPos.value);
            sphereMaskMaterial.SetFloat("_MaskStart", effVolum.MaskPara.value.x);
            sphereMaskMaterial.SetFloat("_MaskEnd", effVolum.MaskPara.value.y);
#if UNITY_EDITOR
            if (effVolum.OutputMask.value)
            {
                sphereMaskMaterial.EnableKeyword("OUTMASK_ON");
            }
            else
            {
                sphereMaskMaterial.DisableKeyword("OUTMASK_ON");
            }
#endif
            cmd.Blit(source, temp03, sphereMaskMaterial, 0);
            cmd.Blit(temp03,source);
        }

    }
}


