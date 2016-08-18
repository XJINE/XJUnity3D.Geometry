using UnityEngine;

namespace XJUnity3D.Geometry
{
    public class Tracker : MonoBehaviour
    {
        #region Field

        public Spline spline;
        public Transform trainTransform;
        public float speed = 1f;
        public bool rotate = false;

        public Vector3 rotationSmoothTimeSec = new Vector3(1f, 1f, 1f);

        protected float splinePositionT = 0f;
        protected float rotationSpeedX;
        protected float rotationSpeedY;

        #endregion Field

        #region Property

        public float TrainTime
        {
            get
            {
                return this.splinePositionT;
            }
        }

        #endregion Property

        #region Method

        public virtual void Start()
        {
            this.trainTransform.position
                = this.spline.Position(this.splinePositionT);

            this.trainTransform.rotation
                = Quaternion.LookRotation(this.spline.Velosity(this.splinePositionT), Vector3.up);
        }

        public virtual void Update()
        {
            Vector3 velocity = this.spline.Velosity(this.splinePositionT);

            try
            {
                checked
                {
                    this.splinePositionT += (this.speed / velocity.magnitude) * Time.deltaTime;
                    this.splinePositionT %= this.spline.controlPoints.Length;
                }
            }
            catch (System.OverflowException e)
            {
                // Unity 5.4.x 現在のところ、checked によるオーバーフローの検出は機能していないようです。

                this.splinePositionT -= float.MaxValue;
            }

            this.trainTransform.position = this.spline.Position(this.splinePositionT);

            if (this.rotate)
            {
                Vector3 nextRotation = this.spline.Rotation(this.splinePositionT);
                this.trainTransform.rotation = Quaternion.Euler(nextRotation);
            }
            else
            {
                var trainRotation = trainTransform.rotation.eulerAngles;
                var nextRotation = Quaternion.LookRotation(velocity, Vector3.up).eulerAngles;

                trainRotation.x = Mathf.SmoothDampAngle
                    (trainRotation.x,
                     nextRotation.x,
                     ref this.rotationSpeedX,
                     this.rotationSmoothTimeSec.x);

                trainRotation.y = Mathf.SmoothDampAngle
                    (trainRotation.y,
                     nextRotation.y,
                     ref this.rotationSpeedY,
                     this.rotationSmoothTimeSec.y);

                this.trainTransform.rotation = Quaternion.Euler(trainRotation);
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (this.trainTransform == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawRay(this.trainTransform.position, 30f * this.trainTransform.forward);
        }

        #endregion Method
    }
}