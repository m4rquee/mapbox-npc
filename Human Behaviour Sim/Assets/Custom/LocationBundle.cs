using System;
using Mapbox.Unity.Location;
using Mapbox.Unity.Map;
using UnityEngine;

namespace Custom
{
    public class LocationBundle : MonoBehaviour
    {
        [SerializeField] public AbstractMap mapManager;

        /// <summary>
        /// The singleton instance of this factory.
        /// </summary>
        public static LocationBundle Instance { get; private set; }

        [SerializeField] bool dontDestroyOnLoad;

        [SerializeField] private AbstractEditorLocationProvider[] locationProviders;

        public ILocationProvider DefaultLocationProvider => locationProviders[0];

        private static int _moversRegistered;

        protected virtual void Awake()
        {
            if (locationProviders.Length == 0) throw new MissingFieldException("empty list of providers!");

            if (Instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }

        public int Register()
        {
            if (_moversRegistered == locationProviders.Length)
                throw new MissingFieldException("not enough location providers!");
            return _moversRegistered++;
        }

        public AbstractEditorLocationProvider this[int i] => locationProviders[i];
    }
}