using System.Collections.Generic;
using Mapbox.Unity.Location;
using UnityEngine;

namespace Custom
{
    /// <summary>
    /// <para>
    /// The LocationProviderExtraSensory is responsible for providing mock location data via log file generated from the Extra Sensory dataset
    /// </para>
    /// </summary>
    public class LocationProviderExtraSensory : AbstractEditorLocationProvider
    {
        /// <summary>
        /// The mock "latitude, longitude" location, represented with a string.
        /// You can search for a place using the embedded "Search" button in the inspector.
        /// This value can be changed at runtime in the inspector.
        /// </summary>
        [SerializeField] private TextAsset locationLogFile;


        private ExtraSensoryReader _logReader;
        private IEnumerator<(Location location, ExtraSensoryReader.UserState userState)> _locationEnumerator;


#if UNITY_EDITOR
        protected override void Awake()
        {
            base.Awake();
            _logReader = new ExtraSensoryReader(locationLogFile.bytes);
            _locationEnumerator = _logReader.GetLocations();
        }
#endif


        private void OnDestroy()
        {
            if (_locationEnumerator != null)
            {
                _locationEnumerator.Dispose();
                _locationEnumerator = null;
            }

            if (_logReader == null) return;
            _logReader.Dispose();
            _logReader = null;
        }

        public ExtraSensoryReader.UserState UserState { get; private set; }

        protected override void SetLocation()
        {
            if (null == _locationEnumerator) return;

            // no need to check if 'MoveNext()' returns false as LocationLogReader loops through log file
            _locationEnumerator.MoveNext();
            _currentLocation = _locationEnumerator.Current!.location;
            UserState = _locationEnumerator.Current!.userState;
        }
    }
}