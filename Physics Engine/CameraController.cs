using OpenTK.Mathematics;
using System.Diagnostics;

namespace Physics_Engine
{
    internal class CameraController : PlayerController
    {
        // Private fields for camera properties
        private Matrix4 _projectionMatrix;

        private float _nearClip = 0.1f;
        private float _farClip = 1000f;
        private float _fov = MathHelper.DegreesToRadians(90);
        private float _aspectRatio = 16f / 9f;
        private float _sensitivity = 0.0025f;
        
        // Public access to neccisary private fields
        public Matrix4 ProjectionMatrix => _projectionMatrix;
        public float Sensitivity {
            get => _sensitivity;
            set => _sensitivity = value;
        }

        public float NearClip {
            get => _nearClip;
            set {
                _nearClip = value;
                CalculateProjectionMatrix();
            }
        }
        public float FarClip {
            get => _farClip;
            set {
                _farClip = value;
                CalculateProjectionMatrix();
            }
        }
        public float FOV {
            get => _fov;
            set {
                _fov = MathHelper.DegreesToRadians(value);
                CalculateProjectionMatrix();
            }
        }
        public float AspectRatio {
            get => _aspectRatio;
            set {
                _aspectRatio = value;
                CalculateProjectionMatrix();
            }
        }

        public CameraController(Matrix4 transform, float aspectRatio) : base(transform)
        {
            AspectRatio = aspectRatio;
        }

        public override void Update()
        {
            if (Global.MouseDelta != Vector2.Zero)
            {
                // Adjust mouse delta by aspect ratio
                Vector2 mouseDelta = Global.MouseDelta / _aspectRatio;

                // Calculate pitch and yaw rotations based on mouse movement
                Quaternion pitch = Quaternion.FromAxisAngle(Vector3.UnitX, -mouseDelta.Y * _sensitivity);
                Quaternion yaw   = Quaternion.FromAxisAngle(Vector3.UnitY, -mouseDelta.X * _sensitivity);
                Quaternion rotation = _transform.ExtractRotation();

                // Apply yaw and pitch to the current rotation
                rotation = yaw * pitch * rotation;
                rotation.Normalize();
                rotation.Z = 0; // Prevent roll

                // Apply the new rotation to the transform while preserving position
                _transform = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(_transform.ExtractTranslation());
            }

            base.Update();
        }

        public void CalculateProjectionMatrix()
        {
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                _fov,
                _aspectRatio,
                _nearClip,
                _farClip);
        }
    }
}
