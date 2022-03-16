using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mapbox.Directions;
using Mapbox.Unity;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Modifiers;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using UnityEngine;

namespace Custom
{
    public class PathFollowerFactory : MonoBehaviour
    {
        [SerializeField] private AbstractMap map;

        [SerializeField] private MeshModifier[] meshModifiers;
        [SerializeField] private Material material;

        private List<Vector3> _cachedWaypoints;

        [SerializeField] [Range(1, 10)] private float updateFrequency = 2;

        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Transform nextPoint;

        private Directions _directions;
        private int _counter;

        GameObject _directionsGO;
        private bool _recalculateNext;

        protected virtual void Awake()
        {
            if (map == null)
                map = FindObjectOfType<AbstractMap>();

            _directions = MapboxAccess.Instance.Directions;
            map.OnInitialized += Query;
            map.OnUpdated += Query;
        }

        public void Start()
        {
            foreach (var modifier in meshModifiers)
                modifier.Initialize();

            StartCoroutine(QueryTimer());
            _recalculateNext = false;
        }

        protected virtual void OnDestroy()
        {
            map.OnInitialized -= Query;
            map.OnUpdated -= Query;
        }

        private void Query()
        {
            var count = waypoints.Length;
            var wp = new Vector2d[count];
            for (var i = 0; i < count; i++)
                wp[i] = waypoints[i].GetGeoPosition(map.CenterMercator, map.WorldRelativeScale);

            var directionResource = new DirectionResource(wp, RoutingProfile.Walking);
            directionResource.Steps = true;
            _directions.Query(directionResource, HandleDirectionsResponse);
        }

        private IEnumerator QueryTimer()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateFrequency);
                for (var i = 0; i < waypoints.Length; i++)
                {
                    if (waypoints[i].position == _cachedWaypoints[i]) continue;
                    _recalculateNext = true;
                    _cachedWaypoints[i] = waypoints[i].position;
                }

                if (!_recalculateNext) continue;
                Query();
                _recalculateNext = false;
            }
        }

        private void HandleDirectionsResponse(DirectionsResponse response)
        {
            if (response?.Routes == null || response.Routes.Count < 1)
                return;

            var meshData = new MeshData();
            var dat = response.Routes[0].Geometry.Select(point =>
                Conversions.GeoToWorldPosition(point.x, point.y, map.CenterMercator, map.WorldRelativeScale)
                    .ToVector3xz()).ToList();

            if (dat.Count > 1) nextPoint.position = dat[1];

            var feat = new VectorFeatureUnity();
            feat.Points.Add(dat);

            foreach (var mod in meshModifiers.Where(x => x.Active))
                mod.Run(feat, meshData, map.WorldRelativeScale);

            CreateGameObject(meshData);
        }

        private void CreateGameObject(MeshData data)
        {
            if (_directionsGO != null) _directionsGO.Destroy();

            _directionsGO = new GameObject("direction waypoint entity");
            var mesh = _directionsGO.AddComponent<MeshFilter>().mesh;
            mesh.subMeshCount = data.Triangles.Count;

            mesh.SetVertices(data.Vertices);
            _counter = data.Triangles.Count;
            for (var i = 0; i < _counter; i++)
            {
                var triangle = data.Triangles[i];
                mesh.SetTriangles(triangle, i);
            }

            _counter = data.UV.Count;
            for (var i = 0; i < _counter; i++)
            {
                var uv = data.UV[i];
                mesh.SetUVs(i, uv);
            }

            mesh.RecalculateNormals();
            _directionsGO.AddComponent<MeshRenderer>().material = material;
        }
    }
}