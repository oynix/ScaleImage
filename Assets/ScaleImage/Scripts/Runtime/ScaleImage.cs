using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Solitaire.UI
{
    public enum ScaleType
    {
        /**
         * 不缩放
         * 1. 目标尺寸能完全显示图片，居中，完全显示
         * 2. 目标尺寸不能完全显示图片，居中，超出部分裁剪
         */
        Center,

        /**
         * 原图比例、完全显示
         * 1. 目标尺寸能完全显示，居中，完全显示
         * 2. 目标尺寸不能完全显示，居中，按照原图比例缩放至完全显示
         */
        CenterInside,

        /**
         * 原图比例、填满目标尺寸
         * 1. 不能覆盖目标尺寸时，居中，按原图比例放大至完全覆盖
         * 2. 超出目标尺寸时，居中，按原图比例缩小至最多一个方向超出原图，超出部分裁剪
         */
        CenterCrop,

        /**
         * 完全显示，填满目标尺寸
         * 1. X方向缩小/放大至填满目标尺寸，居中
         * 2. Y方向缩小/放大至填满目标尺寸，居中
         */
        FitXY,

        /**
         * 原图比例，完全显示
         * 1. 按原图比例放大/缩小至最少有一个方向填满目标尺寸
         * 2. 与CenterInside区别：当不能填满目标尺寸时会放大
         */
        FitCenter,
        
        /**
         * 原图比例，完全显示
         * 1. 同FitCenter，区别是不居中，向左/向上对齐
         */
        FitStart,
        
        /**
         * 原图比例，完全显示
         * 1. 同FitCenter，区别是不居中，向右/向下对齐
         */
        FitEnd,
    }

    public class ScaleImage : Image
    {
        public ScaleType scaleType = ScaleType.CenterCrop;
        public bool roundCorner = false;
        [Range(0, 0.5f)] public float radiusRatio = 0.25f;
        [Range(1, 20)] public int triangleNum = 6;

        private Sprite activeSprite => overrideSprite != null ? overrideSprite : sprite;

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (activeSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }

            // GenerateSimpleSprite(toFill, false);
            GenerateScaleSprite(toFill);
        }

        void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
        {
            var v = GetDrawingDimensions(lPreserveAspect);
            var uv = (activeSprite != null) ? DataUtility.GetOuterUV(activeSprite) : Vector4.zero;

            var color32 = color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        void GenerateScaleSprite(VertexHelper vh)
        {
            var uv = (activeSprite != null) ? DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
            var v = GetDrawingDimensionsIgnorePadding(false);
            var r = rectTransform.rect;
            var mBoundSize = new Vector2(r.width, r.height);

            var sSize = activeSprite.rect.size;

            switch (scaleType)
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


            if (!roundCorner)
                DrawRect(vh, v, uv);
            else
                DrawRoundCornerRect(vh, v, uv);
        }

        private void DrawRect(VertexHelper vh, Vector4 v, Vector4 uv)
        {
            var color32 = color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        private void DrawRoundCornerRect(VertexHelper vh, Vector4 v, Vector4 uv)
        {
            var color32 = color;
            vh.Clear();
            // 对radius的值做限制，必须在0-较小的边的1/2的范围内
            var radius = radiusRatio * Mathf.Min(v.z - v.x, v.w - v.y);
            if (radius < 0) radius = 0;
            // 计算出uv中对应的半径值坐标轴的半径
            var uvRadiusX = radius / (v.z - v.x) * (uv.z - uv.x);
            var uvRadiusY = radius / (v.w - v.y) * (uv.w - uv.y);

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
            vh.AddTriangle(1, 0, 3);
            vh.AddTriangle(1, 3, 4);
            // 中间的矩形
            vh.AddTriangle(5, 2, 6);
            vh.AddTriangle(5, 6, 9);
            // 右边的矩形
            vh.AddTriangle(8, 7, 10);
            vh.AddTriangle(8, 10, 11);

            // 开始构造四个角
            var vCenterList = new List<Vector2>();
            var uvCenterList = new List<Vector2>();
            var vCenterVertList = new List<int>();

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
            var degreeDelta = Mathf.PI / 2 / triangleNum;
            //当前的角度
            float curDegree = 0;

            for (var i = 0; i < vCenterVertList.Count; i++)
            {
                var preVertNum = vh.currentVertCount;
                for (var j = 0; j <= triangleNum; j++)
                {
                    var cosA = Mathf.Cos(curDegree);
                    var sinA = Mathf.Sin(curDegree);
                    Vector3 vPosition = new Vector3(vCenterList[i].x + cosA * radius, vCenterList[i].y + sinA * radius);
                    Vector3 uvPosition = new Vector2(uvCenterList[i].x + cosA * uvRadiusX,
                        uvCenterList[i].y + sinA * uvRadiusY);
                    vh.AddVert(vPosition, color32, uvPosition);
                    curDegree += degreeDelta;
                }

                curDegree -= degreeDelta;
                for (var j = 0; j <= triangleNum - 1; j++)
                {
                    vh.AddTriangle(vCenterVertList[i], preVertNum + j + 1, preVertNum + j);
                }
            }
        }

        /// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
        private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
        {
            var padding = activeSprite == null
                ? Vector4.zero
                : DataUtility.GetPadding(activeSprite);
            var size = activeSprite == null
                ? Vector2.zero
                : new Vector2(activeSprite.rect.width, activeSprite.rect.height);

            var r = GetPixelAdjustedRect();

            var spriteW = Mathf.RoundToInt(size.x);
            var spriteH = Mathf.RoundToInt(size.y);

            var v = new Vector4(
                padding.x / spriteW,
                padding.y / spriteH,
                (spriteW - padding.z) / spriteW,
                (spriteH - padding.w) / spriteH);

            if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
            {
                PreserveSpriteAspectRatio(ref r, size);
            }

            v = new Vector4(
                r.x + r.width * v.x,
                r.y + r.height * v.y,
                r.x + r.width * v.z,
                r.y + r.height * v.w
            );

            return v;
        }

        private Vector4 GetDrawingDimensionsIgnorePadding(bool shouldPreserveAspect)
        {
            var size = activeSprite == null
                ? Vector2.zero
                : new Vector2(activeSprite.rect.width, activeSprite.rect.height);

            var r = GetPixelAdjustedRect();
            var v = new Vector4(0, 0, 1, 1);

            if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
            {
                PreserveSpriteAspectRatio(ref r, size);
            }

            v = new Vector4(
                r.x + r.width * v.x,
                r.y + r.height * v.y,
                r.x + r.width * v.z,
                r.y + r.height * v.w
            );

            return v;
        }

        private void PreserveSpriteAspectRatio(ref Rect rect, Vector2 spriteSize)
        {
            var spriteRatio = spriteSize.x / spriteSize.y;
            var rectRatio = rect.width / rect.height;

            if (spriteRatio > rectRatio)
            {
                var oldHeight = rect.height;
                rect.height = rect.width * (1.0f / spriteRatio);
                rect.y += (oldHeight - rect.height) * rectTransform.pivot.y;
            }
            else
            {
                var oldWidth = rect.width;
                rect.width = rect.height * spriteRatio;
                rect.x += (oldWidth - rect.width) * rectTransform.pivot.x;
            }
        }

        #region insprctor

        public void ReviseSprite()
        {
            SetAllDirty();
        }

        #endregion
    }
}