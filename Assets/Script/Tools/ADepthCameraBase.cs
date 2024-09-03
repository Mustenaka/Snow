using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 深度摄像机控制基类
/// </summary>
public abstract class ADepthCameraBase : MonoBehaviour
{
    [Header("渲染层级")]
    public LayerMask renderLayer;
    [Header("视口宽高比")]
    [Range(0, 5f)]
    public float cameraAspect = 1f;
    [Header("摄像机深度")]
    public int cameraDepth = -2;

    protected Camera depthCamera;

    private LayerMask lastLayer;
    private float lastAspect;
    private int lastDepth;

    protected virtual void Awake()
    {
        depthCamera = GetComponent<Camera>();
        if (depthCamera == null)
        {
#if UNITY_EDITOR
            Debug.LogErrorFormat("depthCamera is null");
#endif
            return;
        }
        depthCamera.clearFlags = CameraClearFlags.Depth;
        UpdateCullMask(renderLayer);
        UpdateAspect(cameraAspect);
        UpdateDepth(cameraDepth);
        depthCamera.useOcclusionCulling = false;
        depthCamera.allowHDR = false;
        depthCamera.allowMSAA = false;
        depthCamera.allowDynamicResolution = false;
        depthCamera.depthTextureMode = DepthTextureMode.Depth;
    }

    protected virtual void Start()
    {
        int layer = LayerMask.NameToLayer("Player");
    }

    void Update()
    {
        UpdateCullMask(renderLayer);
        UpdateAspect(cameraAspect);
        UpdateDepth(cameraDepth);
    }

    protected virtual void UpdateCullMask(LayerMask layermask)
    {
        if (lastLayer != layermask)
        {
            List<int> layers = GetLayerValuesFromLayerMask(layermask);
            for (int i = 0; i < layers.Count; i++)
            {
                depthCamera.cullingMask |= (1 << layers[i]);
            }
            lastLayer = layermask;
        }
    }

    protected virtual List<int> GetLayerValuesFromLayerMask(LayerMask layermask)
    {
        List<int> vallist = new List<int>();
        for (int i = 0; i < 32; i++)
        {
            if (((layermask.value >> i) & 1) == 1)
            {
                vallist.Add(i);
            }
        }
        return vallist;
    }

    protected virtual void UpdateAspect(float aspect)
    {
        if (lastAspect != aspect)
        {
            depthCamera.aspect = aspect;
            lastAspect = aspect;
        }
    }

    protected virtual void UpdateDepth(int depth)
    {

        if (lastDepth != depth)
        {
            depthCamera.depth = depth;
            lastDepth = depth;
        }
    }

    protected virtual void OnDestroy()
    {

    }
}
