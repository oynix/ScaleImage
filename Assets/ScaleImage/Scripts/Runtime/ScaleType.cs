namespace UnityEngine.UI
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
}