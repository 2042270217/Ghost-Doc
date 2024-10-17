using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEffectUtils
{
    static int bloodDirtyId = Shader.PropertyToID("_BloodDirty");
    static int moodLightColorId = Shader.PropertyToID("_MoodLightColor");

    float transitionStartTime = 0;

    float transitionDuration = 0;

    float currentDirty = 0;

    float targetDirty;

    SceneEffectSetting setting;

    void SetDirty(float dirty)
    {
        Shader.SetGlobalFloat(bloodDirtyId, dirty);
    }

    public void Setup(SceneEffectSetting setting)
    {
        this.setting = setting;
    }

    /// <summary>
    /// ���������������ʹ���˱任ʱ��Ĳ���������Ҫÿ֡����Update
    /// </summary>
    public void Update()
    {
        if(transitionDuration > 0)
        {
            float factor = (Time.time - transitionStartTime)/transitionDuration;
            float current = Mathf.Lerp(currentDirty, targetDirty, factor);
            SetDirty(current);
            if (factor > 1)
            {
                transitionDuration = 0;
                currentDirty = current;
            }
        }
    }
    /// <summary>
    /// ����Player��Ѫ�������õ�����tʱ����ҪUpdate����
    /// </summary>
    /// <param name="target">��Ҫ���õ�Ѫ����[0,1]</param>
    /// <param name="t">�ӵ�ǰ״̬�任��Ŀ������ʱ��</param>
    public void SetPlayerBloodDirty(float target,float t)
    {
        if (transitionDuration != 0)
        {
            float factor = (Time.time - transitionStartTime) / transitionDuration;
            float current = Mathf.Lerp(currentDirty, targetDirty, factor);
            currentDirty = current;
            targetDirty = Mathf.Clamp01(target);
            transitionDuration = t;
            transitionStartTime = Time.time;
        }
        else
        {
            targetDirty = Mathf.Clamp01(target);
            transitionDuration = t;
            transitionStartTime = Time.time;
        }
    }

    /// <summary>
    /// ֱ������Ѫ����������ҪUpdate
    /// </summary>
    /// <param name="target">��Ҫ���õ�Ѫ����[0,1]</param>
    public void SetPlayerBloodDirty(float target)
    {
        currentDirty = Mathf.Clamp01(target);
        SetDirty(currentDirty);
    }

    /// <summary>
    /// ���ó�����Χ�ƣ�mood��Χ[0,1]������û�����ù��ɣ���Χֵ����Ӧ����������
    /// </summary>
    /// <param name="mood">��Χֵ</param>
    public void SetMoodLight(float mood)
    {
        Color c = setting.SceneEffectKey.Evaluate(mood);
        Shader.SetGlobalColor(moodLightColorId, c.linear);
    }

}
