using Mapbox.Geocoding;
using static UnityEngine.AudioSettings;

namespace Mapbox.Unity.Location
{
    using Mapbox.Utils;
    using System;
    using System.IO;
    using System.Text;
    using UnityEngine;


    /// <summary>
    /// Writes location data into Application.persistentDataPath
    /// </summary>
    public class LocationLogWriter : LocationLogAbstractBase, IDisposable
    {
        private Vector2d _coordinate;
        private Geocoder _geocoder;
        private Location _location;
        private ReverseGeocodeResource _resource;
        private bool muted = false;

        public LocationLogWriter()
        {
            string fileName = "MBX-location-log-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
            string persistentPath = Application.persistentDataPath;
            string fullFilePathAndName = Path.Combine(persistentPath, fileName);
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA
			// use `GetFullPath` on that to sanitize the path: replaces `/` returned by `Application.persistentDataPath` with `\`
			fullFilePathAndName = Path.GetFullPath(fullFilePathAndName);
#endif
            Debug.Log("starting new log file: " + fullFilePathAndName);

            _fileStream = new FileStream(fullFilePathAndName, FileMode.Create, FileAccess.Write);
            _textWriter = new StreamWriter(_fileStream, new UTF8Encoding(false));
            _textWriter.WriteLine("#" + string.Join(Delimiter, HeaderNames) + ";category;device muted;battery level");

            _geocoder = MapboxAccess.Instance.Geocoder;
            _resource = new ReverseGeocodeResource(_coordinate);
            Mobile.OnMuteStateChanged += (val) => muted = val;
        }


        private bool _disposed;
        private FileStream _fileStream;
        private TextWriter _textWriter;
        private long _lineCount;


        #region idisposable

        ~LocationLogWriter()
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
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    Debug.LogFormat("{0} locations logged", _lineCount);
                    if (null != _textWriter)
                    {
                        _textWriter.Flush();
                        _fileStream.Flush();
#if !NETFX_CORE
                        _textWriter.Close();
#endif
                        _textWriter.Dispose();
                        _fileStream.Dispose();

                        _textWriter = null;
                        _fileStream = null;
                    }
                }

                _disposed = true;
            }
        }

        #endregion

        private void HandleWriting(ReverseGeocodeResponse res)
        {
            var properties = res.Features[0].Properties;
            var category = "";
            foreach (var prop in properties)
                if (prop.Key == "category")
                    category = prop.Value.ToString();

            var lineTokens = new[]
            {
                _location.IsLocationServiceEnabled.ToString(),
                _location.IsLocationServiceInitializing.ToString(),
                _location.IsLocationUpdated.ToString(),
                _location.IsUserHeadingUpdated.ToString(),
                _location.Provider,
                LocationProviderFactory.Instance.DefaultLocationProvider.GetType().Name,
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss.fff"),
                UnixTimestampUtils.From(_location.Timestamp).ToString("yyyyMMdd-HHmmss.fff"),
                string.Format(_invariantCulture, "{0:0.00000000}", _location.LatitudeLongitude.x),
                string.Format(_invariantCulture, "{0:0.00000000}", _location.LatitudeLongitude.y),
                string.Format(_invariantCulture, "{0:0.0}", _location.Accuracy),
                string.Format(_invariantCulture, "{0:0.0}", _location.UserHeading),
                string.Format(_invariantCulture, "{0:0.0}", _location.DeviceOrientation),
                nullableAsStr(_location.SpeedKmPerHour, "{0:0.0}"),
                nullableAsStr(_location.HasGpsFix, "{0}"),
                nullableAsStr(_location.SatellitesUsed, "{0}"),
                nullableAsStr(_location.SatellitesInView, "{0}"),
                category,
                nullableAsStr<bool>(muted, "{0}"),
                string.Format(_invariantCulture, "{0:0.0}", SystemInfo.batteryLevel)
            };

            _lineCount++;
            var logMsg = string.Join(Delimiter, lineTokens);
            Debug.Log(logMsg);
            _textWriter.WriteLine(logMsg);
            _textWriter.Flush();
        }


        private void getClosestBuilding(Location location)
        {
            _location = location;
            _resource.Query = location.LatitudeLongitude;
            _resource.Types = new[] {"poi"};
            _geocoder.Geocode(_resource, HandleWriting);
        }


        public void Write(Location location)
        {
            getClosestBuilding(location);
        }


        private string nullableAsStr<T>(T? val, string formatString = null) where T : struct
        {
            if (null == val && null == formatString)
            {
                return "[not supported by provider]";
            }

            if (null == val && null != formatString)
            {
                return string.Format(_invariantCulture, formatString, "[not supported by provider]");
            }

            if (null != val && null == formatString)
            {
                return val.Value.ToString();
            }

            return string.Format(_invariantCulture, formatString, val);
        }
    }
}