using System.Collections;
using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake instance;
    private CinemachineVirtualCamera vcam;
    private CinemachineBasicMultiChannelPerlin noisePerlin;

    void Awake()
    {
        instance = this;
        vcam = GetComponent<CinemachineVirtualCamera>();
    }

    public void ShakeCamera(float intensity, float time)
    {
        if (noisePerlin == null)
        {
            noisePerlin = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        if (noisePerlin != null)
        {
            noisePerlin.m_AmplitudeGain = intensity;
            StartCoroutine(StopShake(time));
        }
    }

    IEnumerator StopShake(float time)
    {
        yield return new WaitForSeconds(time);
        
        if (noisePerlin != null)
        {
            noisePerlin.m_AmplitudeGain = 0f;
        }
    }
}