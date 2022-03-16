using Mapbox.Unity.Location;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Custom
{
    public class LocationStatusExtraSensory : MonoBehaviour
    {
        [SerializeField] private Text statusText;

        private AbstractLocationProvider _locationProvider;

        private void Start()
        {
            if (_locationProvider != null) return;
            _locationProvider =
                LocationProviderFactory.Instance.DefaultLocationProvider as AbstractLocationProvider;
        }


        private void Update()
        {
            var currLoc = _locationProvider.CurrentLocation;
            var userState = ((LocationProviderExtraSensory) _locationProvider).UserState;

            if (currLoc.IsLocationServiceInitializing)
                statusText.text = "location services are initializing";
            else
            {
                if (!currLoc.IsLocationServiceEnabled)
                    statusText.text = "location services not enabled";
                else
                {
                    if (currLoc.LatitudeLongitude.Equals(Vector2d.zero))
                        statusText.text = "Waiting for location ....";
                    else
                    {
                        statusText.text = $"{currLoc.LatitudeLongitude} - LyingDown = {userState.LyingDown}";
                        statusText.color = userState.LyingDown ? Color.green : Color.red;
                    }
                }
            }
        }
    }
}