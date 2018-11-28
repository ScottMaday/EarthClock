using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using SceneKit;
using CoreAnimation;

namespace EarthClock
{
    public class GeoCoordinate : IDisposable
    {
        private SCNNode LongitudeNode;
        private SCNNode LatitudeNode;
        public readonly SCNNode ParentNode;
        public readonly SCNNode ChildNode;
        
        public GeoCoordinate(SCNNode _ParentNode, SCNNode _ChildNode, float Lat, float Long)
        {
            ParentNode = _ParentNode;
            ChildNode = _ChildNode;
            LongitudeNode = SCNNode.Create();
            LatitudeNode = SCNNode.Create();
            ParentNode.AddChildNode(LongitudeNode);
            LongitudeNode.AddChildNode(LatitudeNode);
            LatitudeNode.AddChildNode(ChildNode);

            UpdateCoordinates(Lat, Long);
        }

        public void UpdateCoordinates(float Lat, float Long)
        {
            LatitudeNode.EulerAngles = new SCNVector3(-TimeEngine.DegreesToRadians(Lat), 0, 0);
            LongitudeNode.EulerAngles = new SCNVector3(0, TimeEngine.DegreesToRadians(Long), 0);
        }

        public void Dispose()
        {
            ChildNode.Dispose();
            LatitudeNode.Dispose();
            LongitudeNode.Dispose();
        }
    }

    public class PlanetaryBody
    {
        public SCNNode RootNode { private set; get; } // for revolution angle
        public SCNNode AttachmentNode { private set; get; } // for attachment of camera to remain perpendicular to solar plane
        public SCNNode Node { private set; get; } // actual body

        public SCNSphere Geometry { private set; get; }
        public SCNMaterial Material { private set; get; }

        public PlanetaryBody(float Radius)
        {
            RootNode = SCNNode.Create();
            AttachmentNode = SCNNode.Create();
            Node = SCNNode.Create();

            Geometry = SCNSphere.Create(Radius);
            Material = SCNMaterial.Create();
            Material.Diffuse.ContentColor = UIColor.Blue;
            Material.LightingModelName = SCNLightingModel.Phong;
            Geometry.FirstMaterial = Material;
            Node.Geometry = Geometry;

            RootNode.AddChildNode(AttachmentNode);
            AttachmentNode.AddChildNode(Node);
        }

        public float GetRadius()
        {
            return (float)Geometry.Radius;
        }

        public void SetRevolutionAngle(SCNVector3 EulerAngles)
        {
            RootNode.EulerAngles = EulerAngles;
        }
        public void SetPosition(SCNVector3 Position)
        {
            RootNode.Position = Position;
        }
        public void SetAxisAngle(SCNVector3 EulerAngles)
        {
            AttachmentNode.EulerAngles = EulerAngles;
        }
        public void SetRotationAngle(SCNVector3 EulerAngles)
        {
            Node.EulerAngles = EulerAngles;
        }
        public void SetRotationAnimation(double TimeToRoatate)
        {
            Node.RunAction(SCNAction.RepeatActionForever(SCNAction.RotateBy(0, (float)Math.PI * 2, 0, TimeToRoatate)));
        }

        public void SetTexture(string Location)
        {
            Material.Diffuse.ContentImage = UIImage.FromFile(Location);
        }

        public void AddToParent(SCNNode ParentNode)
        {
            ParentNode.AddChildNode(RootNode);
        }
        public void AddToParent(SCNScene ParentScene)
        {
            ParentScene.RootNode.AddChildNode(RootNode);
        }
        public void AddToParent(PlanetaryBody ParentBody)
        {
            ParentBody.Node.AddChildNode(RootNode);
        }
        public GeoCoordinate AddCoordinate(SCNNode ChildNode, float Lat, float Long, float DistanceFromRadius)
        {
            ChildNode.Position = new SCNVector3(0, 0, GetRadius() - DistanceFromRadius);
            return new GeoCoordinate(Node, ChildNode, Lat, Long);
        }
    }
}