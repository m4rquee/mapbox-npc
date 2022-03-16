using Mapbox.Unity.Map;

namespace Custom
{
    using System.Collections;
    using Mapbox.Unity.Location;
    using UnityEngine;

    public class InitializeMapWithLocationBundle : MonoBehaviour
    {
        [SerializeField] private AbstractMap map;

        private ILocationProvider _locationProvider;

        private void Awake()
        {
            map.InitializeOnStart = false; // prevent double initialization of the map
        }

        protected virtual IEnumerator Start()
        {
            yield return null;
            _locationProvider = LocationBundle.Instance.DefaultLocationProvider;
            _locationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
        }

        private void LocationProvider_OnLocationUpdated(Location location)
        {
            _locationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;
            map.Initialize(location.LatitudeLongitude, map.AbsoluteZoom);
        }
    }
}