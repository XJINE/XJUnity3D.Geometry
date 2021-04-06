using UnityEngine;
using UnityEditor;
using nobnak.Config;
using System;

namespace XJUnity3D.Geometry
{
    /// <summary>
    /// Spline を編集するためのエディタ。
    /// </summary>
    [CustomEditor(typeof(Spline))]
    public class SplineEditor : Editor
    {
        public const float SCALE_GUI = 0.05f;
        public const float SCALE_PICK = 0.07f;
        public const float SCALE_POS = 0.5f;

        public const float JET_K_MIN = 0.01f;
        public const float JET_K_MAX = 0.1f;

        /// <summary>
        /// 現在選択されている制御点のインデックス。
        /// </summary>
        private int selectedCtrlPointIndex = -1;

        /// <summary>
        /// Spline を生成するためのメニューを追加します。
        /// </summary>
        [MenuItem("Assets/Create/XJSpline")]
        public static void CreateSpline()
        {
            ScriptableObjUtil.CreateAsset<Spline>();
        }

        /// <summary>
        /// Inspector の描画時に実行されます。
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var curve = (Spline)target;
            var controlPoints = curve.controlPoints;

            if (controlPoints == null)
            {
                curve.controlPoints = new ControlPoint[0];
                controlPoints = curve.controlPoints;
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                var size = 5f;
                var pos = (selectedCtrlPointIndex >= 0) ?
                          curve.Position(selectedCtrlPointIndex + 0.5f) : (size * UnityEngine.Random.onUnitSphere);

                var newControlPoint = new ControlPoint()
                {
                    position = pos
                };

                if (0 <= selectedCtrlPointIndex && selectedCtrlPointIndex < controlPoints.Length)
                {
                    nobnak.Collection.Array.Insert(ref controlPoints,
                                                   newControlPoint,
                                                   selectedCtrlPointIndex + 1);
                }
                else
                {
                    nobnak.Collection.Array.Insert(ref controlPoints,
                                                   newControlPoint,
                                                   controlPoints.Length);
                }

                curve.controlPoints = controlPoints;
                selectedCtrlPointIndex++;

                EditorUtility.SetDirty(curve);
            }

            if (GUILayout.Button("Remove"))
            {
                if (0 <= selectedCtrlPointIndex && selectedCtrlPointIndex < controlPoints.Length)
                {
                    nobnak.Collection.Array.Remove(ref controlPoints, selectedCtrlPointIndex);
                }

                curve.controlPoints = controlPoints;
                selectedCtrlPointIndex--;
                EditorUtility.SetDirty(curve);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// SceneView の描画時に実行されます。
        /// </summary>
        void OnSceneGUI()
        {
            var spline = (Spline)target;

            DrawControlPointHandles(spline);
            CheckAndUpdatSelectedControlPoint(spline);
        }

        /// <summary>
        /// Editor が有効になった時点で実行されます。
        /// </summary>
        void OnEnable()
        {
            SceneView.duringSceneGui += DrawSceneGUI;
        }

        /// <summary>
        /// Editor が無効になった時点で実行されます。
        /// </summary>
        void OnDisable()
        {
            SceneView.duringSceneGui -= DrawSceneGUI;
        }

        /// <summary>
        /// SceneView に Gizmo を描画します。
        /// </summary>
        /// <param name="sceneView">
        /// Gizmo を描画する SceneView。
        /// </param>
        void DrawSceneGUI(SceneView sceneView)
        {
            DrawLines();
            OnSceneGUI();
        }

        /// <summary>
        /// SceneView に Spline 曲線を描画します。
        /// </summary>
        void DrawLines()
        {
            const int CURVE_SMOOTH_LEVEL = 10;

            var spline = (Spline)target;
            var controlPoints = spline.controlPoints;

            Func<int, ControlPoint> getCtrlPointFunc = spline.GetControlPoint;

            if (controlPoints == null || controlPoints.Length < 2)
            {
                return;
            }

            var dt = 1f / CURVE_SMOOTH_LEVEL;
            var kMin = float.MaxValue;
            var kMax = 0f;

            for (var i = 0; i < controlPoints.Length; i++)
            {
                var t = (float)i;

                for (var j = 0; j < CURVE_SMOOTH_LEVEL; j++)
                {
                    var k = CatmullSplineUtil.Curvature(t, getCtrlPointFunc);
                    k = Mathf.Clamp(k, JET_K_MIN, JET_K_MAX);

                    if (k < kMin)
                    {
                        kMin = k;
                    }
                    else if (kMax < k)
                    {
                        kMax = k;
                    }

                    t += dt;
                }
            }

            var jetA = 0.66666f / (kMin - kMax);
            var jetB = -jetA * kMax;
            var startPos = CatmullSplineUtil.Position(0f, getCtrlPointFunc);

            for (var i = 0; i < controlPoints.Length; i++)
            {
                var t = (float)i;

                for (var j = 0; j < CURVE_SMOOTH_LEVEL; j++)
                {
                    var k = CatmullSplineUtil.Curvature(t, getCtrlPointFunc);
                    k = Mathf.Clamp(k, kMin, kMax);

                    Handles.color = Color.HSVToRGB(jetA * k + jetB, 1f, 1f);

                    var endPos = CatmullSplineUtil.Position(t += dt, getCtrlPointFunc);

                    Handles.DrawLine(startPos, endPos);

                    startPos = endPos;
                }
            }
        }

        /// <summary>
        /// SceneView に Spline 曲線の制御点を描画します。
        /// </summary>
        /// <param name="spline">
        /// 更新する曲線。
        /// </param>
        void DrawControlPointHandles(Spline spline)
        {
            ControlPoint[] controlPoints = spline.controlPoints;

            // 制御点に点 (Handles.DotCap) の Handle を描画します。
            // 始点は緑、終点は赤、それ以外は白い Handle となります。

            if (controlPoints == null || controlPoints.Length == 0)
            {
                return;
            }

            for (var i = 0; i < controlPoints.Length; i++)
            {
                var ctrlPoint = controlPoints[i];
                var handleSize = HandleUtility.GetHandleSize(ctrlPoint.position);

                Handles.color = Color.white;

                // インデックス番号を描画します。
                Handles.Label(ctrlPoint.position, i.ToString());

                // 制御点の向いている方向を線で描画します。

                Vector3 direction = Quaternion.Euler(ctrlPoint.rotation) * Vector3.forward;
                Vector3 directionEnd = ctrlPoint.position + direction * handleSize;
                Vector3 directionCenter = ctrlPoint.position + direction * handleSize / 2;

                Vector3 upVector = Quaternion.Euler(ctrlPoint.rotation.x,
                                                    ctrlPoint.rotation.y,
                                                    0)
                                   * (Vector3.up * handleSize / 4);

                upVector = Quaternion.AngleAxis(ctrlPoint.rotation.z, direction.normalized)
                           * upVector
                           + directionCenter;

                Handles.DrawLine(ctrlPoint.position, directionEnd);

                Handles.DrawWireDisc(directionCenter, direction.normalized, handleSize / 4);

                Handles.color = Color.cyan;

                Handles.DrawLine(directionCenter, upVector);

                // 終点と始点は色を分けます。

                if (i == 0)
                {
                    Handles.color = Color.green;
                }
                else if (i == (controlPoints.Length - 1))
                {
                    Handles.color = Color.red;
                }
                else
                {
                    Handles.color = Color.white;
                }

                // 選択されたときは、選択中の頂点情報を更新します。

                if (Handles.Button(ctrlPoint.position,
                                   Quaternion.identity,
                                   handleSize * SCALE_GUI,
                                   handleSize * SCALE_PICK,
                                   Handles.DotHandleCap))
                {
                    this.selectedCtrlPointIndex = i;

                    Repaint();
                }
            }
        }

        /// <summary>
        /// 現在選択している制御点が更新されているかどうかをチェックし、
        /// 更新されているときは曲線のデータを更新します。
        /// </summary>
        /// <param name="spline">
        /// 更新する曲線。
        /// </param>
        void CheckAndUpdatSelectedControlPoint(Spline spline)
        {
            // 現在選択している制御点を取得し、更新されているかどうかを確認します。
            // 更新されているとき、データを更新します。

            ControlPoint[] controlPoints = spline.controlPoints;
            ControlPoint selectedControlPoint = null;

            if (0 <= this.selectedCtrlPointIndex
                && controlPoints != null
                && this.selectedCtrlPointIndex < controlPoints.Length)
            {
                selectedControlPoint = controlPoints[this.selectedCtrlPointIndex];
            }
            else
            {
                this.selectedCtrlPointIndex = -1;
            }

            if (selectedControlPoint == null)
            {
                return;
            }

            // 選択中のツールに合わせて Handle を描画し、値を更新します。

            EditorGUI.BeginChangeCheck();

            if (Tools.current == Tool.Move)
            {
                Vector3 position = Handles.PositionHandle(selectedControlPoint.position,
                                                          Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    selectedControlPoint.position = position;
                    EditorUtility.SetDirty(spline);
                }
            }
            else if (Tools.current == Tool.Rotate)
            {
                Quaternion rotation
                    = Handles.RotationHandle(Quaternion.Euler(selectedControlPoint.rotation),
                                                              selectedControlPoint.position);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedControlPoint.rotation = rotation.eulerAngles;
                    EditorUtility.SetDirty(spline);
                }
            }
        }
    }
}