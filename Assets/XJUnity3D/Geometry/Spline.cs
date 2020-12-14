using UnityEngine;

namespace XJUnity3D.Geometry
{
    /// <summary>
    /// 複数の座標から構成される Spline 曲線を示す。
    /// </summary>
    public class Spline : ScriptableObject
    {
        /// <summary>
        /// Spline を構成する制御点。
        /// </summary>
        public ControlPoint[] controlPoints;

        /// <summary>
        /// Spline 上の位置 t における座標を算出します。
        /// </summary>
        /// <param name="t">
        /// Spline 上の位置 t.
        /// </param>
        /// <returns>
        /// Spline 上の位置 t における座標。
        /// </returns>
        public Vector3 Position(float t)
        {
            var i = Mathf.FloorToInt(t);
            t -= i;

            return CatmullSplineUtil.Position(t,
                            GetControlPoint(i - 1).position,
                            GetControlPoint(i).position,
                            GetControlPoint(i + 1).position,
                            GetControlPoint(i + 2).position);
        }

        /// <summary>
        /// Spline 上の位置 t における速度を算出します。
        /// </summary>
        /// <param name="t">
        /// Spline 上の位置 t.
        /// </param>
        /// <returns>
        /// Spline 上の位置 t における速度。
        /// </returns>
        public Vector3 Velosity(float t)
        {
            var i = Mathf.FloorToInt(t);
            t -= i;

            return CatmullSplineUtil.Velocity(t,
                            GetControlPoint(i - 1).position,
                            GetControlPoint(i).position,
                            GetControlPoint(i + 1).position,
                            GetControlPoint(i + 2).position);
        }

        /// <summary>
        /// Catmull Spline 上の位置 t における回転を算出します。
        /// </summary>
        /// <param name="t">
        /// Spline 上の位置 t.
        /// </param>
        /// <returns>
        /// Spline 上の位置 t における回転。
        /// </returns>
        public Vector3 Rotation(float t)
        {
            return CatmullSplineUtil.Rotation(t, GetControlPoint);
        }

        /// <summary>
        /// 終端のインデックス(制御点の数)を取得します。
        /// 与えられた制御点が 1 つより小さいとき、0 を返します。
        /// </summary>
        /// <returns>
        /// 制御点の数。
        /// </returns>
        public float Period()
        {
            if (controlPoints == null || controlPoints.Length < 2)
            {
                return 0;
            }
            else
            {
                return controlPoints.Length;
            }
        }

        /// <summary>
        /// 指定したインデックスの制御点を取得します。
        /// </summary>
        /// <param name="index">
        /// 制御点のインデックス。
        /// </param>
        /// <returns>
        /// 制御点。
        /// </returns>
        public ControlPoint GetControlPoint(int index)
        {
            while (index < 0)
            {
                index += controlPoints.Length;
            }

            index = index % controlPoints.Length;

            return controlPoints[index];
        }
    }
}
