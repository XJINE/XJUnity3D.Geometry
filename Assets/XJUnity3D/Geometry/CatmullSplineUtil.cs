using UnityEngine;

namespace XJUnity3D.Geometry
{
    /// <summary>
    /// Catmull 曲線に関するユーティリティ。
    /// </summary>
    public static class CatmullSplineUtil
    {
        /// <summary>
        /// Catmull Spline 上のある位置での座標を算出します。
        /// </summary>
        /// <param name="t">
        /// 位置を示す係数 t.
        /// </param>
        /// <param name="ctrlPointFunc">
        /// Catmull Spline 上の点を取得する関数。
        /// </param>
        /// <returns>
        /// CatmullSpline 上のある位置での座標。
        /// </returns>
        public static Vector3 Position(float t, System.Func<int, ControlPoint> ctrlPointFunc)
        {
            var i = Mathf.FloorToInt(t);
            t -= i;

            return Position(t,
                            ctrlPointFunc(i - 1).position,
                            ctrlPointFunc(i).position,
                            ctrlPointFunc(i + 1).position,
                            ctrlPointFunc(i + 2).position);
        }

        /// <summary>
        /// Catumull Spline 上のある位置での速度を算出します。
        /// </summary>
        /// <param name="t">
        /// 位置を示す係数 t.
        /// </param>
        /// <param name="ctrlPointFunc">
        /// Catmull Spline 上の点を取得する関数。
        /// </param>
        /// <returns>
        /// Catmull Spline 上のある位置での速度。
        /// </returns>
        public static Vector3 Velocity(float t, System.Func<int, ControlPoint> ctrlPointFunc)
        {
            var i = Mathf.FloorToInt(t);
            t -= i;

            return Velocity(t,
                            ctrlPointFunc(i - 1).position,
                            ctrlPointFunc(i).position,
                            ctrlPointFunc(i + 1).position,
                            ctrlPointFunc(i + 2).position);
        }

        /// <summary>
        /// Catmull 曲線上のある位置での回転を算出します。
        /// </summary>
        /// <param name="t">
        /// 位置を示す係数 t.
        /// </param>
        /// <param name="ctrlPointFunc">
        /// Catmull Spline 上の点を取得する関数。
        /// </param>
        /// <returns>
        /// Catmull Spline 上のある位置での回転。
        /// </returns>
        public static Vector3 Rotation(float t, System.Func<int, ControlPoint> ctrlPointFunc)
        {
            var i = Mathf.FloorToInt(t);
            t -= i;

            return Position(t,
                            ctrlPointFunc(i - 1).rotation,
                            ctrlPointFunc(i).rotation,
                            ctrlPointFunc(i + 1).rotation,
                            ctrlPointFunc(i + 2).rotation);

            // とりあえず角度を示すベクトルを、座標を示すベクトルと同じ方法で算出しています。
            // 回転方向が制御できない問題が残っています。
            // 現在のところ緩やかに変化しますが、どちらに回転するかは制御することができません。
        }

        /// <summary>
        /// Catumull Spline 上のある位置での曲率を算出します。
        /// </summary>
        /// <param name="t">
        /// 位置を示す係数 t.
        /// </param>
        /// <param name="ctrlPointFunc">
        /// Catmull Spline 上の点を取得する関数。
        /// </param>
        /// <returns>
        /// Catmull Spline 上のある位置での曲率。
        /// </returns>
        public static float Curvature(float t, System.Func<int, ControlPoint> controlPoint)
        {
            var i = Mathf.FloorToInt(t);
            t -= i;

            return Curvature(t,
                             controlPoint(i - 1).position,
                             controlPoint(i).position,
                             controlPoint(i + 1).position,
                             controlPoint(i + 2).position);
        }

        /// <summary>
        /// Catmull Spline 上の座標を算出します。
        /// </summary>
        /// <param name="t">係数 t.</param>
        /// <param name="p0">制御点 1.</param>
        /// <param name="p1">通過点 1.</param>
        /// <param name="p2">通過点 2.</param>
        /// <param name="p3">制御点 2.</param>
        /// <returns>
        /// Catmull Spline 上の座標。
        /// </returns>
        public static Vector3 Position(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var tm1 = t - 1f;
            var tm2 = tm1 * tm1;
            var t2 = t * t;

            var m1 = 0.5f * (p2 - p0);
            var m2 = 0.5f * (p3 - p1);

            return (1f + 2f * t) * tm2 * p1 + t * tm2 * m1 + t2 * (3 - 2f * t) * p2 + t2 * tm1 * m2;
        }

        /// <summary>
        /// Catmull Spline 上の速度を算出します。
        /// </summary>
        /// <param name="t">係数 t.</param>
        /// <param name="p0">制御点 1.</param>
        /// <param name="p1">通過点 1.</param>
        /// <param name="p2">通過点 2.</param>
        /// <param name="p3">制御点 2.</param>
        /// <returns>
        /// Catmull Spline 上の速度。
        /// </returns>
        public static Vector3 Velocity(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var tm1 = (t - 1f);
            var t6tm1 = 6f * t * tm1;

            var m1 = 0.5f * (p2 - p0);
            var m2 = 0.5f * (p3 - p1);

            return t6tm1 * p1 + (3f * t - 1f) * tm1 * m1 - t6tm1 * p2 + t * (3f * t - 2f) * m2;
        }

        /// <summary>
        /// Catmull Spline 上の加速度を算出します。
        /// </summary>
        /// <param name="t">係数 t.</param>
        /// <param name="p0">制御点 1.</param>
        /// <param name="p1">通過点 1.</param>
        /// <param name="p2">通過点 2.</param>
        /// <param name="p3">制御点 2.</param>
        /// <returns>
        /// Catmull Spline 上の加速度。
        /// </returns>
        public static Vector3 Acceleration(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var t2m1 = 2f * t - 1f;
            var t3 = 3f * t;

            var m1 = 0.5f * (p2 - p0);
            var m2 = 0.5f * (p3 - p1);

            return 6f * t2m1 * p1 + 2f * (t3 - 2f) * m1 - 6f * t2m1 * p2 + 2f * (t3 - 1f) * m2;
        }

        /// <summary>
        /// Catmull Spline 上の曲率を算出します。
        /// </summary>
        /// <param name="t">係数 t.</param>
        /// <param name="p0">制御点 1.</param>
        /// <param name="p1">通過点 1.</param>
        /// <param name="p2">通過点 2.</param>
        /// <param name="p3">制御点 2.</param>
        /// <returns>
        /// Catmull Spline 上の曲率。
        /// </returns>
        public static float Curvature(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var a = Acceleration(t, p0, p1, p2, p3);
            var v = Velocity(t, p0, p1, p2, p3);
            var vmag = v.magnitude;

            return Vector3.Cross(v, a).magnitude / (vmag * vmag * vmag);
        }
    }
}