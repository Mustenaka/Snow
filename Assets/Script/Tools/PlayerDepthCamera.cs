using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 高度图生成控制类
/// 雪面depth-角色depth
/// </summary>
public class PlayerDepthCamera : ADepthCameraBase
{
    public Material heightMat;              //处理高度图
    public Material snowFieldMat;           //处理雪地
    public ComputeShader compCS;            //合成高度图

    public RawImage rawImgHeightTex;

    private RenderTexture heiRt;

    private RenderTexture compositeRT;

    protected override void Start()
    {
        base.Start();
        compositeRT = RenderTexture.GetTemporary(512, 512);
        compositeRT.enableRandomWrite = true;
        compositeRT.Create();
        heiRt = RenderTexture.GetTemporary(512, 512);
        heiRt.enableRandomWrite = true;
        heiRt.Create();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, heiRt, heightMat);
        int kernel = compCS.FindKernel("CSMain");
        compCS.SetTexture(kernel, "HeightTex", heiRt);
        compCS.SetTexture(kernel, "ResultTex", compositeRT);
        compCS.Dispatch(kernel, 512 / 8, 512 / 8, 1);
        rawImgHeightTex.texture = compositeRT;
        snowFieldMat.SetTexture("_HeightTex", compositeRT);
    }



    protected override void OnDestroy()
    {
        base.OnDestroy();
        RenderTexture.ReleaseTemporary(heiRt);
        RenderTexture.ReleaseTemporary(compositeRT);
    }
}