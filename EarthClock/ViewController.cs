using Foundation;
using System;
using UIKit;

using CoreLocation;
using SceneKit;

namespace EarthClock
{
    public partial class ViewController : UIViewController
    {
        private CLLocationManager LocationManager;

        private SCNScene Scene;
        private CameraControl Camera;
        private PlanetaryBody Sun;
        private PlanetaryBody Earth;
        private GeoCoordinate GeoPoint;

        public ViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            Initalize();
            AccessLocation();
        }

        private void Initalize()
        {
            // set scene & camera
            Scene = new SCNScene();
            Camera = new CameraControl(SceneView, Scene);
            SceneView.Scene = Scene;
            SceneView.BackgroundColor = UIColor.Black;
            SceneView.AllowsCameraControl = false;

            // sun
            Sun = new PlanetaryBody(0.1f);
            Sun.Material.Diffuse.ContentColor = UIColor.Yellow;
            Sun.Material.SelfIllumination.ContentColor = UIColor.White;
            Sun.Node.Light = SCNLight.Create();
            Sun.Node.Light.LightType = SCNLightType.Omni;
            Sun.Node.Position = new SCNVector3(0, 0, 0);
            Sun.AddToParent(Scene);

            // earth
            Earth = new PlanetaryBody(1);
            Earth.SetRevolutionAngle(new SCNVector3(0, TimeEngine.YearAngle(), 0));
            Earth.SetPosition(new SCNVector3(0, 0, -10));
            Earth.SetAxisAngle(new SCNVector3(TimeEngine.DegreesToRadians(23.5f), 0, 0));
            Earth.SetRotationAngle(new SCNVector3(0, TimeEngine.DayAngle(), 0));
            Earth.SetRotationAnimation(86400);
            Earth.SetTexture("Textures/Earth/Months/june.jpg");
            Earth.AddToParent(Sun);

            // earth textures & shaders
            Earth.SetTexture("Textures/Earth/Months/june.jpg");
            Earth.Material.Emission.ContentImage = UIImage.FromFile("Textures/Earth/EarthLights.jpg");
            /*SCNMaterialProperty Emission = SCNMaterialProperty.Create(UIImage.FromFile("Textures/Earth/EarthLights.jpg"));
            Earth.Material.SetValueForKey(Emission, new NSString("emissionTexture"));
            string EarthLightFragment = @"
            uniform sampler2D emissionTexture;

            vec3 light = _lightingContribution.diffuse;
            float lum = max(0.0, 1 - (0.2126*light.r + 0.7152*light.g + 0.0722*light.b));
            vec4 emission = texture2D(emissionTexture, _surface.diffuseTexcoord) * lum;
            _output.color += emission;";
            Earth.Material.ShaderModifiers = new SCNShaderModifiers() { EntryPointFragment = EarthLightFragment };*/

            Camera.Begin(Earth);
        }

        private bool LocationAuthorizeStatusGood(CLAuthorizationStatus Status)
        {
            return Status == CLAuthorizationStatus.Authorized || Status == CLAuthorizationStatus.AuthorizedWhenInUse || Status == CLAuthorizationStatus.AuthorizedAlways;
        }

        private void AccessLocation()
        {
            LocationManager = new CLLocationManager();
            LocationManager.AuthorizationChanged += (object sender, CLAuthorizationChangedEventArgs e) =>
            {
                if(LocationAuthorizeStatusGood(e.Status) == true)
                {
                    LocationManager.LocationsUpdated += HandleTrackingUpdate;
                    LocationManager.StartUpdatingLocation();
                }
                else
                {
                    LocationManager.StopUpdatingLocation();
                    LocationManager.LocationsUpdated -= HandleTrackingUpdate;
                }
            };
            if (CLLocationManager.LocationServicesEnabled == true && LocationAuthorizeStatusGood(CLLocationManager.Status) == false)
            {
                LocationManager.RequestWhenInUseAuthorization();
            }
        }

        private void HandleTrackingUpdate(object sender, CLLocationsUpdatedEventArgs e)
        {
            CLLocation Location = e.Locations[0];
            if(GeoPoint == null)
            {
                GeoPoint = Earth.AddCoordinate(SCNNode.Create(), (float)Location.Coordinate.Latitude, (float)Location.Coordinate.Longitude, 0);
                GeoPoint.ChildNode.Geometry = SCNSphere.Create(0.02f);
                GeoPoint.ChildNode.Geometry.FirstMaterial.Diffuse.ContentColor = UIColor.Green;
                GeoPoint.ChildNode.Geometry.FirstMaterial.LightingModelName = SCNLightingModel.Constant;
            }
            else
            {
                GeoPoint.UpdateCoordinates((float)Location.Coordinate.Latitude, (float)Location.Coordinate.Longitude);
            }
        }
    }
}