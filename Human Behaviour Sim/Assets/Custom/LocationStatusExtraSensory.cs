using Mapbox.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Custom
{
    public class LocationStatusExtraSensory : MonoBehaviour
    {
        [SerializeField] private Text statusText;

        [SerializeField] private Material baseMaterial;

        [SerializeField] private Material selectedMaterial;

        [SerializeField] private GameObject canvas;

        private Camera _camera;
        private LocationProviderExtraSensory _selectedNpc;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (_selectedNpc != null) UpdateText();
            if (!Input.GetMouseButtonDown(0)) return;

            // Unselect the current NPC:
            if (_selectedNpc != null)
            {
                foreach (var r in _selectedNpc.GetComponentsInChildren<Renderer>())
                    r.material = baseMaterial;
                _selectedNpc = null;
                statusText.text = "";
            }

            // Check if clicked on a NPC:
            var hit = Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hitInfo);
            if (!hit || hitInfo.transform == null || !hitInfo.transform.gameObject.CompareTag("NPC")) return;

            // Select the clicked NPC:
            _selectedNpc = hitInfo.transform.gameObject.GetComponentInParent<LocationProviderExtraSensory>();
            foreach (var r in _selectedNpc.GetComponentsInChildren<Renderer>())
                r.material = selectedMaterial;

            // Activate the info panel:
            canvas.SetActive(true);
        }

        private void UpdateText()
        {
            var currLoc = _selectedNpc.CurrentLocation;
            var userState = _selectedNpc.UserState;

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
                        statusText.text = $"{currLoc.LatitudeLongitude} \n- LyingDown = {userState.LyingDown}";
                        statusText.color = userState.LyingDown ? Color.green : Color.red;
                    }
                }
            }
        }
    }
}