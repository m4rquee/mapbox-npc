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
            [Description("latitude")]
#endif
            Latitude = 0,
#if !ENABLE_WINMD_SUPPORT
            [Description("longitude")]
#endif
            Longitude = 1,
#if !ENABLE_WINMD_SUPPORT
            [Description("LYING_DOWN")]
#endif
            LyingDown = 2
        }

        [DebuggerDisplay("LyingDown = {LyingDown}")]
        public struct UserState
        {
            public bool LyingDown;
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
                if (tokens.Length != 3) Debug.Fail("unsupported log file");

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
                    LyingDown = bool.Parse(tokens[(int) LogfileColumns.LyingDown])
                };

                yield return (location, userState);
            }
        }
    }
}