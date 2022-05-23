using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Mapbox.Unity.Location;
using Mapbox.Utils;

namespace Custom
{
    /// <summary>
    /// Parses location/state data and returns Location objects.
    /// </summary>
    public class ExtraSensoryReader : LocationLogAbstractBase, IDisposable
    {
        private new enum LogfileColumns
        {
#if !ENABLE_WINMD_SUPPORT
            [Description("location service enabled")]
#endif
            LocationServiceEnabled = 0,
#if !ENABLE_WINMD_SUPPORT
            [Description("location service intializing")]
#endif
            LocationServiceInitializing = 1,
#if !ENABLE_WINMD_SUPPORT
            [Description("location updated")]
#endif
            LocationUpdated = 2,
#if !ENABLE_WINMD_SUPPORT
            [Description("userheading updated")]
#endif
            UserHeadingUpdated = 3,
#if !ENABLE_WINMD_SUPPORT
            [Description("location provider")]
#endif
            LocationProvider = 4,
#if !ENABLE_WINMD_SUPPORT
            [Description("location provider class")]
#endif
            LocationProviderClass = 5,
#if !ENABLE_WINMD_SUPPORT
            [Description("time device [utc]")]
#endif
            UtcTimeDevice = 6,
#if !ENABLE_WINMD_SUPPORT
            [Description("time location [utc]")]
#endif
            UtcTimeOfLocation = 7,
#if !ENABLE_WINMD_SUPPORT
            [Description("latitude")]
#endif
            Latitude = 8,
#if !ENABLE_WINMD_SUPPORT
            [Description("longitude")]
#endif
            Longitude = 9,
#if !ENABLE_WINMD_SUPPORT
            [Description("accuracy [m]")]
#endif
            Accuracy = 10,
#if !ENABLE_WINMD_SUPPORT
            [Description("user heading [�]")]
#endif
            UserHeading = 11,
#if !ENABLE_WINMD_SUPPORT
            [Description("device orientation [�]")]
#endif
            DeviceOrientation = 12,
#if !ENABLE_WINMD_SUPPORT
            [Description("speed [km/h]")]
#endif
            Speed = 13,
#if !ENABLE_WINMD_SUPPORT
            [Description("has gps fix")]
#endif
            HasGpsFix = 14,
#if !ENABLE_WINMD_SUPPORT
            [Description("satellites used")]
#endif
            SatellitesUsed = 15,
#if !ENABLE_WINMD_SUPPORT
            [Description("satellites in view")]
#endif
            SatellitesInView = 16,
#if !ENABLE_WINMD_SUPPORT
            [Description("build category")]
#endif
            BuildCategory = 17,
#if !ENABLE_WINMD_SUPPORT
            [Description("device muted")]
#endif
            DeviceMuted = 18,
#if !ENABLE_WINMD_SUPPORT
            [Description("battery level")]
#endif
            BatteryLevel = 19
        }

        [DebuggerDisplay("BuildCategory = {BuildCategory}; DeviceMuted = {DeviceMuted}; BatteryLevel = {BatteryLevel}")]
        public struct UserState
        {
            public string BuildCategory;
            public bool DeviceMuted;
            public float BatteryLevel;
        }

        public ExtraSensoryReader(byte[] contents)
        {
            var ms = new MemoryStream(contents);
            _textReader = new StreamReader(ms);
        }


        private bool _disposed;
        private TextReader _textReader;


        #region idisposable

        ~ExtraSensoryReader()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (_disposed) return;
            if (disposeManagedResources)
            {
                if (null != _textReader)
                {
#if !NETFX_CORE
                    _textReader.Close();
#endif
                    _textReader.Dispose();
                    _textReader = null;
                }
            }

            _disposed = true;
        }

        #endregion


        /// <summary>
        /// Returns 'Location, UserState' tuples from the data passed in. Loops through the data.
        /// </summary>
        /// <returns>'Location, UserState' tuples and loops through the data.</returns>
        public IEnumerator<(Location location, UserState userState)> GetLocations()
        {
            while (true)
            {
                string line;

                while (true)
                {
                    line = _textReader.ReadLine();
                    // rewind if end of log (or last empty line) reached
                    if (line == null || string.IsNullOrEmpty(line))
                    {
                        ((StreamReader) _textReader).BaseStream.Position = 0;
                        ((StreamReader) _textReader).DiscardBufferedData();
                        continue;
                    }

                    // skip comments
                    if (!line.StartsWith("#"))
                        break;
                }

                var tokens = line.Split(Delimiter.ToCharArray());
                // simple safety net: check if number of columns matches
                if (tokens.Length != 20) Debug.Fail("unsupported log file");

                var location = new Location
                {
                    IsLocationServiceEnabled = true,
                    IsLocationServiceInitializing = false,
                    IsLocationUpdated = true,
                    IsUserHeadingUpdated = false,
                    Provider = "unity",
                    ProviderClass = "DeviceLocationProvider"
                };

                var latTxt = tokens[(int) LogfileColumns.Latitude];
                var lngTxt = tokens[(int) LogfileColumns.Longitude];
                if (
                    !double.TryParse(latTxt, NumberStyles.Any, _invariantCulture, out var lat)
                    || !double.TryParse(lngTxt, NumberStyles.Any, _invariantCulture, out var lng)
                )
                    location.LatitudeLongitude = Vector2d.zero;
                else
                    location.LatitudeLongitude = new Vector2d(lat, lng);

                var userState = new UserState
                {
                    BuildCategory = tokens[(int) LogfileColumns.BuildCategory],
                    DeviceMuted = bool.Parse(tokens[(int) LogfileColumns.DeviceMuted]),
                    BatteryLevel = float.Parse(tokens[(int) LogfileColumns.BatteryLevel])
                };

                yield return (location, userState);
            }
        }
    }
}