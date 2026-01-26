using OpenTK.Mathematics;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace Physics_Engine
{
    internal class CameraController : IPlayerController
    {
        // Private fields for camera properties
        private Matrix4 _projectionMatrix;

        private float
            _nearClip = 0.1f,
            _farClip = 1000f,
            _aspectRatio = 16f / 9f,
            _sensitivity = 0.0025f,
            _fov = MathHelper.DegreesToRadians(105),
            _minFOV = MathHelper.DegreesToRadians(60),
            _maxFOV = MathHelper.DegreesToRadians(180);
        
        // Public access to private fields
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
                _fov = float.Clamp(value, _minFOV, _maxFOV);
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

            _keyStates.Add("Up",   new(Keys.Space,     new Vector3(0,  1, 0)));
            _keyStates.Add("Down", new(Keys.LeftShift, new Vector3(0, -1, 0)));
        }

        public override void Update()
        {
            // if (Global.MouseButtonStates[MouseButton.Left])
            {
                Vector2 mouseDelta = Global.MouseDelta;

                _yaw   -= mouseDelta.X * _sensitivity;
                _pitch -= mouseDelta.Y * _sensitivity;

                _pitch = MathHelper.Clamp(
                    _pitch,
                    MathHelper.DegreesToRadians(-89f),
                    MathHelper.DegreesToRadians(89f)
                );

                Quaternion rotation =
                    Quaternion.FromAxisAngle(Vector3.UnitY, _yaw) *
                    Quaternion.FromAxisAngle(Vector3.UnitX, _pitch);

                Vector3 pos = _transform.ExtractTranslation();
                _transform = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(pos);
            }

            if (Global.MouseDeltaScroll != 0)
                FOV -= MathHelper.DegreesToRadians(Global.MouseDeltaScroll * 5);

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
