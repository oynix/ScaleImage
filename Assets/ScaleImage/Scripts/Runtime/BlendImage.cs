using System;
using UnityEngine.Pool;
using UnityEngine.Sprites;

namespace UnityEngine.UI
{
    [Serializable]
    public class BlendElement
    {
        public Sprite sprite;
        public ScaleType scaleType = ScaleType.CenterCrop;
        public bool roundCorner = false;
        [Range(0, 0.5f)] public float radiusRatio = 0.25f;
        [Range(1, 20)] public int triangleNum = 6;
    }

    [AddComponentMenu("UI/Blend Image", 11)]
    public class BlendImage : Image
    {
        public BlendElement destImage;

        public BlendElement srcImage;

        [Min(0)] public float padding;

        private Sprite destSprite
        {
            get
            {
                if (destImage != null && destImage.sprite != null)
                    return destImage.sprite;

                return null;
            }
        }

        private Sprite srcSprite
        {
            get
            {
                if (srcImage != null && srcImage.sprite != null)
                    return srcImage.sprite;
                return null;
            }
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            // if (activeSprite == null)
            if (destSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }

            sprite = destSprite;
            toFill.Clear();

            var v = GetDrawingDimensionsIgnorePadding();
            if (srcSprite == null)
            {
                DrawScaleSprite(destImage, v, toFill, 0);
                return;
            }

            var w = v.z - v.x;
            var h = v.w - v.y;
            var p = Mathf.Min(Mathf.Min(w / 2, h / 2), padding);

            if (p == 0)
            {
                DrawScaleSprite(srcImage, v, toFill, 0);
                return;
            }
            // GenerateSimpleSprite(toFill, false);
            // GenerateScaleSprite(toFill);

            GenerateBlendSprite(toFill, v, p);
        }

        void GenerateBlendSprite(VertexHelper vh, Vector4 v, float p)
        {
            DrawScaleSprite(destImage, v, vh, 0);

            v.x += p;
            v.y += p;
            v.z -= p;
            v.w -= p;
            DrawScaleSprite(srcImage, v, vh, 0);
        }

        void DrawScaleSprite(BlendElement be, Vector4 v, VertexHelper vh, float p)
        {
            var uv = be.sprite != null ? DataUtility.GetOuterUV(be.sprite) : Vector4.zero;
            var mBoundSize = new Vector2(v.z - v.x, v.w - v.y);
            var sSize = be.sprite.rect.size;

            switch (be.scaleType)
            {
                case ScaleType.Center:
                    if (mBoundSize.x > sSize.x)
                    {
                        var offsetX = (mBoundSize.x - sSize.x) / 2;
                        v.x += offsetX;
                        v.z -= offsetX;
                    }
                    else if (sSize.x > mBoundSize.x)
                    {
                        var offsetRatio = (sSize.x - mBoundSize.x) / 2 / sSize.x;
                        var offset = offsetRatio * (uv.z - uv.x);
                        uv.x += offset;
                        uv.z -= offset;
                    }

                    if (mBoundSize.y > sSize.y)
                    {
                        var offsetY = (mBoundSize.y - sSize.y) / 2;
                        v.y += offsetY;
                        v.w -= offsetY;
                    }
                    else if (sSize.y > mBoundSize.y)
                    {
                        var offsetRatio = (sSize.y - mBoundSize.y) / 2 / sSize.y;
                        var offset = offsetRatio * (uv.w - uv.y);
                        uv.y += offset;
                        uv.w -= offset;
                    }

                    break;
                case ScaleType.CenterInside:
                {
                    if (sSize.x > mBoundSize.x || sSize.y > mBoundSize.y)
                    {
                        var spriteRatio = sSize.x / sSize.y;
                        var boundRatio = mBoundSize.x / mBoundSize.y;
                        if (boundRatio > spriteRatio)
                        {
                            // w is bigger
                            var oldW = mBoundSize.x;
                            var w = mBoundSize.y * spriteRatio;
                            var offset = (oldW - w) / 2;
                            v.x += offset;
                            v.z -= offset;
                        }
                        else if (boundRatio < spriteRatio)
                        {
                            // h is bigger
                            var oldH = mBoundSize.y;
                            var h = mBoundSize.x / spriteRatio;
                            var offset = (oldH - h) / 2;
                            v.y += offset;
                            v.w -= offset;
                        }
                    }
                    else
                    {
                        var offsetX = (mBoundSize.x - sSize.x) / 2;
                        var offsetY = (mBoundSize.y - sSize.y) / 2;
                        v.x += offsetX;
                        v.y += offsetY;
                        v.z -= offsetX;
                        v.w -= offsetY;
                    }

                    break;
                }
                case ScaleType.CenterCrop:
                {
                    var boundRatio = mBoundSize.x / mBoundSize.y;
                    var spriteRatio = sSize.x / sSize.y;

                    if (spriteRatio > boundRatio)
                    {
                        var oldW = sSize.x;
                        var w = sSize.y * boundRatio;
                        var offsetRatio = (oldW - w) / 2f / oldW;
                        var offset = offsetRatio * (uv.z - uv.x);
                        uv.x += offset;
                        uv.z -= offset;
                    }
                    else
                    {
                        var oldH = sSize.y;
                        var h = sSize.x / boundRatio;
                        var offsetRatio = (oldH - h) / 2f / oldH;
                        var offset = (uv.w - uv.y) * offsetRatio;
                        uv.y += offset;
                        uv.w -= offset;
                    }

                    break;
                }
                case ScaleType.FitXY:
                    break;
                case ScaleType.FitCenter:
                {
                    var spriteRatio = sSize.x / sSize.y;
                    var boundRatio = mBoundSize.x / mBoundSize.y;
                    if (boundRatio > spriteRatio)
                    {
                        // w is bigger
                        var oldW = mBoundSize.x;
                        var w = mBoundSize.y * spriteRatio;
                        var offset = (oldW - w) / 2;
                        v.x += offset;
                        v.z -= offset;
                    }
                    else if (boundRatio < spriteRatio)
                    {
                        // h is bigger
                        var oldH = mBoundSize.y;
                        var h = mBoundSize.x / spriteRatio;
                        var offset = (oldH - h) / 2;
                        v.y += offset;
                        v.w -= offset;
                    }

                    break;
                }
                case ScaleType.FitStart:
                {
                    var spriteRatio = sSize.x / sSize.y;
                    var boundRatio = mBoundSize.x / mBoundSize.y;
                    if (boundRatio > spriteRatio)
                    {
                        // w is bigger
                        var oldW = mBoundSize.x;
                        var w = mBoundSize.y * spriteRatio;
                        var offset = oldW - w;
                        v.z -= offset;
                    }
                    else if (boundRatio < spriteRatio)
                    {
                        // h is bigger
                        var oldH = mBoundSize.y;
                        var h = mBoundSize.x / spriteRatio;
                        var offset = oldH - h;
                        v.y += offset;
                    }

                    break;
                }
                case ScaleType.FitEnd:
                {
                    var spriteRatio = sSize.x / sSize.y;
                    var boundRatio = mBoundSize.x / mBoundSize.y;
                    if (boundRatio > spriteRatio)
                    {
                        // w is bigger
                        var oldW = mBoundSize.x;
                        var w = mBoundSize.y * spriteRatio;
                        var offset = oldW - w;
                        v.x += offset;
                    }
                    else if (boundRatio < spriteRatio)
                    {
                        // h is bigger
                        var oldH = mBoundSize.y;
                        var h = mBoundSize.x / spriteRatio;
                        var offset = oldH - h;
                        v.w -= offset;
                    }

                    break;
                }
            }

            if (!be.roundCorner)
            {
                // if (p > 0)
                //     DrawPaddingRect(vh, v, uv, p);
                // else
                DrawRect(vh, v, uv);
            }
            else
                DrawRoundCornerRect(vh, v, uv, be);
        }

        void DrawRect(VertexHelper vh, Vector4 v, Vector4 uv)
        {
            var color32 = color;
            var offset = vh.currentVertCount;
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

            vh.AddTriangle(0 + offset, 1 + offset, 2 + offset);
            vh.AddTriangle(2 + offset, 3 + offset, 0 + offset);
        }

        void DrawPaddingRect(VertexHelper vh, Vector4 v, Vector4 uv, float paddingPixels)
        {
            var w = v.z - v.x;
            var h = v.w - v.y;

            var p = paddingPixels;

            var pUV = new Vector2(p / w * (uv.z - uv.x), p / h * (uv.w - uv.y));

            var color32 = color;
            var offset = vh.currentVertCount;
            // 0 1
            vh.AddVert(new Vector3(v.x, v.w), color32,
                new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(v.x, v.y), color32,
                new Vector2(uv.x, uv.y));
            // 2 3 4 5
            vh.AddVert(new Vector3(v.x + p, v.w), color32,
                new Vector2(uv.x + pUV.x, uv.w));
            vh.AddVert(new Vector3(v.x + p, v.w - p), color32,
                new Vector2(uv.x + pUV.x, uv.w - pUV.y));
            vh.AddVert(new Vector3(v.x + p, v.y + p), color32,
                new Vector2(uv.x + pUV.x, uv.y + pUV.y));
            vh.AddVert(new Vector3(v.x + p, v.y), color32,
                new Vector2(uv.x + pUV.x, uv.y));
            // 6 7 8 9
            vh.AddVert(new Vector3(v.z - p, v.w), color32,
                new Vector2(uv.z - pUV.x, uv.w));
            vh.AddVert(new Vector3(v.z - p, v.w - p), color32,
                new Vector2(uv.z - pUV.x, uv.w - pUV.y));
            vh.AddVert(new Vector3(v.z - p, v.y + p), color32,
                new Vector2(uv.z - pUV.x, uv.y + pUV.y));
            vh.AddVert(new Vector3(v.z - p, v.y), color32,
                new Vector2(uv.z - pUV.x, uv.y));
            // 10 11
            vh.AddVert(new Vector3(v.z, v.w), color32,
                new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(v.z, v.y), color32,
                new Vector2(uv.z, uv.y));

            vh.AddTriangle(0 + offset, 1 + offset, 2 + offset);
            vh.AddTriangle(1 + offset, 2 + offset, 5 + offset);

            vh.AddTriangle(2 + offset, 3 + offset, 6 + offset);
            vh.AddTriangle(3 + offset, 6 + offset, 7 + offset);

            vh.AddTriangle(4 + offset, 5 + offset, 8 + offset);
            vh.AddTriangle(9 + offset, 5 + offset, 8 + offset);

            vh.AddTriangle(9 + offset, 10 + offset, 6 + offset);
            vh.AddTriangle(9 + offset, 10 + offset, 11 + offset);
        }

        void DrawRoundCornerRect(VertexHelper vh, Vector4 v, Vector4 uv, BlendElement be)
        {
            var color32 = color;
            // 对radius的值做限制，必须在0-较小的边的1/2的范围内
            var radius = be.radiusRatio * Mathf.Min(v.z - v.x, v.w - v.y);
            if (radius < 0) radius = 0;
            // 计算出uv中对应的半径值坐标轴的半径
            var uvRadiusX = radius / (v.z - v.x) * (uv.z - uv.x);
            var uvRadiusY = radius / (v.w - v.y) * (uv.w - uv.y);

            var offset = vh.currentVertCount;
            // 0，1
            vh.AddVert(new Vector3(v.x, v.w - radius), color32,
                new Vector2(uv.x, uv.w - uvRadiusY));
            vh.AddVert(new Vector3(v.x, v.y + radius), color32,
                new Vector2(uv.x, uv.y + uvRadiusY));

            // 2，3，4，5
            vh.AddVert(new Vector3(v.x + radius, v.w), color32,
                new Vector2(uv.x + uvRadiusX, uv.w));
            vh.AddVert(new Vector3(v.x + radius, v.w - radius), color32,
                new Vector2(uv.x + uvRadiusX, uv.w - uvRadiusY));
            vh.AddVert(new Vector3(v.x + radius, v.y + radius), color32,
                new Vector2(uv.x + uvRadiusX, uv.y + uvRadiusY));
            vh.AddVert(new Vector3(v.x + radius, v.y), color32,
                new Vector2(uv.x + uvRadiusX, uv.y));

            // 6，7，8，9
            vh.AddVert(new Vector3(v.z - radius, v.w), color32,
                new Vector2(uv.z - uvRadiusX, uv.w));
            vh.AddVert(new Vector3(v.z - radius, v.w - radius), color32,
                new Vector2(uv.z - uvRadiusX, uv.w - uvRadiusY));
            vh.AddVert(new Vector3(v.z - radius, v.y + radius), color32,
                new Vector2(uv.z - uvRadiusX, uv.y + uvRadiusY));
            vh.AddVert(new Vector3(v.z - radius, v.y), color32,
                new Vector2(uv.z - uvRadiusX, uv.y));

            // 10，11
            vh.AddVert(new Vector3(v.z, v.w - radius), color32,
                new Vector2(uv.z, uv.w - uvRadiusY));
            vh.AddVert(new Vector3(v.z, v.y + radius), color32,
                new Vector2(uv.z, uv.y + uvRadiusY));

            // 左边的矩形
            // vh.AddTriangle(1 + offset, 0 + offset, 3 + offset);
            // vh.AddTriangle(1 + offset, 3 + offset, 4 + offset);
            AddTriangle(vh, offset, 1, 3, 0);
            AddTriangle(vh, offset, 1, 3, 4);
            // 中间的矩形
            // vh.AddTriangle(5 + offset, 2 + offset, 6 + offset);
            // vh.AddTriangle(5 + offset, 6 + offset, 9 + offset);
            AddTriangle(vh, offset, 5, 6, 2);
            AddTriangle(vh, offset, 5, 6, 9);
            // 右边的矩形
            // vh.AddTriangle(8 + offset, 7 + offset, 10 + offset);
            // vh.AddTriangle(8 + offset, 10 + offset, 11 + offset);
            AddTriangle(vh, offset, 8, 7, 10);
            AddTriangle(vh, offset, 8, 11, 10);

            // 开始构造四个角
            // var vCenterList = new List<Vector2>();
            var vCenterList = ListPool<Vector2>.Get();
            // var uvCenterList = new List<Vector2>();
            var uvCenterList = ListPool<Vector2>.Get();
            // var vCenterVertList = new List<int>();
            var vCenterVertList = ListPool<int>.Get();

            // 右上角的圆心
            vCenterList.Add(new Vector2(v.z - radius, v.w - radius));
            uvCenterList.Add(new Vector2(uv.z - uvRadiusX, uv.w - uvRadiusY));
            vCenterVertList.Add(7);

            // 左上角的圆心
            vCenterList.Add(new Vector2(v.x + radius, v.w - radius));
            uvCenterList.Add(new Vector2(uv.x + uvRadiusX, uv.w - uvRadiusY));
            vCenterVertList.Add(3);

            // 左下角的圆心
            vCenterList.Add(new Vector2(v.x + radius, v.y + radius));
            uvCenterList.Add(new Vector2(uv.x + uvRadiusX, uv.y + uvRadiusY));
            vCenterVertList.Add(4);

            // 右下角的圆心
            vCenterList.Add(new Vector2(v.z - radius, v.y + radius));
            uvCenterList.Add(new Vector2(uv.z - uvRadiusX, uv.y + uvRadiusY));
            vCenterVertList.Add(8);

            //每个三角形的顶角
            var degreeDelta = Mathf.PI / 2 / be.triangleNum;
            //当前的角度
            float curDegree = 0;

            for (var i = 0; i < vCenterVertList.Count; i++)
            {
                var preVertNum = vh.currentVertCount;
                for (var j = 0; j <= be.triangleNum; j++)
                {
                    var cosA = Mathf.Cos(curDegree);
                    var sinA = Mathf.Sin(curDegree);
                    var vPosition = new Vector3(vCenterList[i].x + cosA * radius, vCenterList[i].y + sinA * radius);
                    var uvPosition = new Vector2(uvCenterList[i].x + cosA * uvRadiusX,
                        uvCenterList[i].y + sinA * uvRadiusY);
                    vh.AddVert(vPosition, color32, uvPosition);
                    curDegree += degreeDelta;
                }

                curDegree -= degreeDelta;
                for (var j = 0; j <= be.triangleNum - 1; j++)
                {
                    vh.AddTriangle(offset + vCenterVertList[i], preVertNum + j + 1, preVertNum + j);
                }
            }

            ListPool<Vector2>.Release(vCenterList);
            ListPool<Vector2>.Release(uvCenterList);
            ListPool<int>.Release(vCenterVertList);
        }

        void AddTriangle(VertexHelper vh, int offset, int a, int b, int c)
        {
            vh.AddTriangle(a + offset, b + offset, c + offset);
        }

        private Vector4 GetDrawingDimensionsIgnorePadding()
        {
            var r = GetPixelAdjustedRect();
            var v = new Vector4(0, 0, 1, 1);

            v = new Vector4(
                r.x,
                r.y,
                r.x + r.width * v.z,
                r.y + r.height * v.w
            );

            return v;
        }
    }
}