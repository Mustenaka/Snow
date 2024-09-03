using UnityEngine;

namespace Script.Tools
{
    public class BrushPainter : MonoBehaviour
    {
        public Texture2D sourceTexture; // 原始灰度图
        public Color brushColor = Color.red; // 笔刷颜色
        public float brushRadius = 10f; // 笔刷半径
        public RenderTexture targetTexture; // 用于绘制的目标纹理
        public float restoreDuration = 5.0f; // 恢复原始贴图的时间，默认5秒

        private Texture2D targetTexture2D;
        private Texture2D originalTexture2D;
        private Camera mainCamera;
        private bool isRestoring = false;
        private float restoreStartTime;

        void Start()
        {
            mainCamera = Camera.main;
            if (sourceTexture != null)
            {
                // 创建一个新的 Texture2D 用于绘制
                targetTexture2D = new Texture2D(sourceTexture.width, sourceTexture.height);
                Graphics.Blit(sourceTexture, targetTexture); // 将灰度图复制到目标纹理
                RenderTexture.active = targetTexture;
                targetTexture2D.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
                targetTexture2D.Apply();

                // 保存原始灰度图状态
                originalTexture2D = new Texture2D(targetTexture2D.width, targetTexture2D.height, targetTexture2D.format, false);
                Graphics.CopyTexture(targetTexture2D, originalTexture2D);
            }
        }

        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 pixelUV;
                if (GetMouseUV(out pixelUV))
                {
                    // 将鼠标点击位置转换为纹理上的坐标
                    Paint(pixelUV);
                }
            }

            if (isRestoring)
            {
                RestoreOriginalTexture();
            }
        }

        bool GetMouseUV(out Vector2 pixelUV)
        {
            pixelUV = Vector2.zero;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // 将命中位置转换为纹理坐标
                Renderer rend = hit.transform.GetComponent<Renderer>();
                if (rend != null && rend.material.mainTexture != null)
                {
                    var texture2rd = rend.material.GetTexture("_HeightTex");
                    Texture2D tex = texture2rd as Texture2D;
                    pixelUV = hit.textureCoord;
                    pixelUV.x *= tex.width;
                    pixelUV.y *= tex.height;
                    return true;
                }
            }

            return false;
        }

        void Paint(Vector2 pixelUV)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogError("Renderer not found on the GameObject. Please ensure the GameObject has a Renderer component.");
                return;
            }

            int startX = Mathf.Clamp((int)(pixelUV.x - brushRadius), 0, targetTexture2D.width);
            int startY = Mathf.Clamp((int)(pixelUV.y - brushRadius), 0, targetTexture2D.height);
            int endX = Mathf.Clamp((int)(pixelUV.x + brushRadius), 0, targetTexture2D.width);
            int endY = Mathf.Clamp((int)(pixelUV.y + brushRadius), 0, targetTexture2D.height);

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), pixelUV);
                    if (dist <= brushRadius)
                    {
                        targetTexture2D.SetPixel(x, y, brushColor);
                    }
                }
            }

            targetTexture2D.Apply();

            // 更新材质的 _HeightTex 属性
            renderer.material.SetTexture("_HeightTex", targetTexture2D);

            // 开始恢复原始灰度图
            StartRestore();
        }

        void StartRestore()
        {
            isRestoring = true;
            restoreStartTime = Time.time;
        }

        void RestoreOriginalTexture()
        {
            float elapsed = Time.time - restoreStartTime;
            float t = Mathf.Clamp01(elapsed / restoreDuration);

            for (int x = 0; x < targetTexture2D.width; x++)
            {
                for (int y = 0; y < targetTexture2D.height; y++)
                {
                    Color originalColor = originalTexture2D.GetPixel(x, y);
                    Color currentColor = targetTexture2D.GetPixel(x, y);
                    Color blendedColor = Color.Lerp(currentColor, originalColor, t);
                    targetTexture2D.SetPixel(x, y, blendedColor);
                }
            }

            targetTexture2D.Apply();
            GetComponent<Renderer>().material.SetTexture("_HeightTex", targetTexture2D);

            if (t >= 1.0f)
            {
                isRestoring = false;
            }
        }
    }
}
