using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using SceneKit;
using CoreGraphics;

namespace EarthClock
{
    class CameraControl
    {
        private const float ZOOM_MIN = 2.1f;
        private const float ZOOM_DEFAULT = 4;
        private const float ZOOM_MAX = 6;

        private SCNView SceneView;
        private SCNNode CameraRootNode;
        private SCNNode CameraNode;
        private SCNCamera Camera;
        private PlanetaryBody Focus;

        private UIPanGestureRecognizer PanGesture;
        private CGPoint PanPreviousPoint;
        private UIPinchGestureRecognizer PinchGesture;
        private nfloat PinchPreviousScale;

        private float Zoom;

        public CameraControl(SCNView _SceneView, SCNScene Scene)
        {
            SceneView = _SceneView;
            CameraRootNode = SCNNode.Create();
            CameraNode = SCNNode.Create();
            Camera = SCNCamera.Create();

            CameraNode.Camera = Camera;
            Scene.RootNode.AddChildNode(CameraRootNode);
            CameraRootNode.AddChildNode(CameraNode);

            PanGesture = new UIPanGestureRecognizer(HandlePanGesture);
            PanGesture.MinimumNumberOfTouches = 1;
            PanGesture.MaximumNumberOfTouches = 1;
            PinchGesture = new UIPinchGestureRecognizer(HandlePinchGesture);

            SceneView.AddGestureRecognizer(PanGesture);
            SceneView.AddGestureRecognizer(PinchGesture);

            Zoom = ZOOM_DEFAULT;
        }


        private void HandlePanGesture()
        {
            CGPoint CurrentPoint = PanGesture.TranslationInView(SceneView);
            if (PanGesture.State == UIGestureRecognizerState.Began)
            {
                PanPreviousPoint = CurrentPoint;
            }
            else if (PanGesture.State == UIGestureRecognizerState.Changed)
            {
                SCNVector3 LastAngle = CameraRootNode.EulerAngles;
                float Scale = 0.01f;
                float XChange = (float)(PanPreviousPoint.X - CurrentPoint.X) * Scale;
                float YChange = (float)(PanPreviousPoint.Y - CurrentPoint.Y) * Scale;
                float YAngle = LastAngle.X + YChange;
                YAngle = Math.Max(Math.Min(YAngle, (float)Math.PI / 2), (float)Math.PI / -2);
                CameraRootNode.EulerAngles = new SCNVector3(YAngle, LastAngle.Y + XChange, 0);

                PanPreviousPoint = CurrentPoint;
            }
        }

        private void HandlePinchGesture()
        {
            nfloat CurrentScale = PinchGesture.Scale;
            if (PinchGesture.State == UIGestureRecognizerState.Began)
            {
                PinchPreviousScale = CurrentScale;
            }
            else if (PinchGesture.State == UIGestureRecognizerState.Changed)
            {
                float Scale = 1f;
                float ScaleChange = (float)(PinchPreviousScale - CurrentScale) * Scale;
                Zoom = Math.Max(Math.Min(Zoom + ScaleChange, ZOOM_MAX), ZOOM_MIN);
                Refocus();

                PinchPreviousScale = CurrentScale;
            }
        }

        private void Refocus()
        {
            CameraNode.Position = new SCNVector3(0, 0, Focus.GetRadius() * Zoom);
        }

        public void Begin(PlanetaryBody _Focus)
        {
            Focus = _Focus;
            Focus.RootNode.AddChildNode(CameraRootNode);
            Refocus();
        }
    }
}