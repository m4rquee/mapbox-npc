using Mapbox.Unity.Location;
using UnityEngine.Serialization;

namespace Custom
{
    using System;
    using Mapbox.Unity.Utilities;
    using Mapbox.Utils;
    using UnityEngine;

    /// <summary>
    /// The DirectionsLocationProvider is responsible for providing location and heading data based on the directions
    /// api from mapbox.
    /// </summary>
    public class DirectionsLocationProvider : AbstractEditorLocationProvider
    {
        [SerializeField] private Transform targetTransform;
        private bool _started;

        [Tooltip("The coordinates to start the map around")]
        public Vector2d startLatitudeLongitude;

        protected override void SetLocation()
        {
            var map = LocationProviderFactory.Instance.mapManager;
            _currentLocation.UserHeading = targetTransform.eulerAngles.y;
            if (_started)
                _currentLocation.LatitudeLongitude =
                    targetTransform.GetGeoPosition(map.CenterMercator, map.WorldRelativeScale);
            else
            {
                _started = true;
                _currentLocation.LatitudeLongitude = startLatitudeLongitude;
            }

            _currentLocation.Accuracy = _accuracy;
            _currentLocation.Timestamp = UnixTimestampUtils.To(DateTime.UtcNow);
            _currentLocation.IsLocationUpdated = true;
            _currentLocation.IsUserHeadingUpdated = true;
            _currentLocation.IsLocationServiceEnabled = true;
        }
    }
}