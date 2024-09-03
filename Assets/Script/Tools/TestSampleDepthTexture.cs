using UnityEngine;

namespace Script.Tools
{
    public class TestSampleDepthTexture: MonoBehaviour
    {
        public Material depthMat;

        private Camera depthCamera;

        void Start()
        {
            depthCamera = GetComponent<Camera>();
            depthCamera.depthTextureMode = DepthTextureMode.Depth;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, depthMat);
        }
    }
}