using Mapbox.Unity.Map;

namespace Custom
{
    using Mapbox.Unity.Location;
    using UnityEngine;

    public class ImmediatePositionWithLocationBundle : MonoBehaviour
    {
        private int _index;

        private bool _isInitialized;

        private ILocationProvider _locationProvider;

        private AbstractMap _map;

        private ILocationProvider LocationProvider
        {
            get
            {
                if (_locationProvider != null) return _locationProvider;
                _index = LocationBundle.Instance.Register();
                _locationProvider = LocationBundle.Instance[_index];
                return _locationProvider;
            }
        }

        private void Start()
        {
            LocationBundle.Instance.mapManager.OnInitialized += () =>
            {
                _map = LocationBundle.Instance.mapManager;
                var latLong = LocationProvider.CurrentLocation.LatitudeLongitude;
                _map.SetCenterLatitudeLongitude(latLong);
                _isInitialized = true;
            };
        }

        private void LateUpdate()
        {
            if (!_isInitialized) return;
            transform.localPosition = _map.GeoToWorldPosition(LocationProvider.CurrentLocation.LatitudeLongitude);
        }
    }
}