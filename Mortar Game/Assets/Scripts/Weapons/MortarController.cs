using UnityEngine;
using MortarGame.Core;
using MortarGame.Config;
using MortarGame.Gameplay;
using MortarGame.Combat;
using MortarGame.UI;

namespace MortarGame.Weapons
{
    public class MortarController : MonoBehaviour
    {
        public enum InputMode { ControllerDriven, RigLocal }
        [Header("Control Mode")] public InputMode inputMode = InputMode.ControllerDriven;
        [Header("Transforms")] public Transform mortarPivot; public Transform muzzle;
        [Header("Rig")] public MortarRigController rig;
        [Header("Camera")] public Camera observeCamera; public float observeZoomFOV = 45f; private float _defaultFOV;
        [Header("Projectile Follow (Hold Key)")]
        [Tooltip("Allow holding a key to make the camera follow the last fired projectile (War Thunder style)")] public bool enableProjectileFollow = true;
        [Tooltip("Key to hold for following the projectile")] public KeyCode projectileFollowKey = KeyCode.Space;
        [Tooltip("Distance behind the projectile to place the camera")] public float followDistance = 3f;
        [Tooltip("Vertical offset above the projectile")] public float followHeight = 0.75f;
        [Tooltip("Smooth damp toward follow position (higher is snappier)")] public float followSmooth = 10f;
        [Tooltip("Reduce FOV while following for a more focused view")] public bool followAdjustFOV = true;
        [Tooltip("Field of view while in follow mode")] public float followFOV = 35f;
        [Header("Input")] public float yawSensitivity = 0.2f; public float fineYawStepDeg = 2f;
        [Header("Projectile")]
        public GameObject Projectile;

        private MissionConfig.MortarSettings _ms;
        private float _bearingDeg; // Compass heading
        private float _rangeM;     // Desired range in meters

        private float _nextFireTime;
        private bool _spotterReady = true; private float _spotterCooldownEnd;
        private bool _spotterActiveForNextShot;
        // Follow state
        private Projectile _lastProjectile;
        private bool _isFollowing;
        private Vector3 _camStartPos; private Quaternion _camStartRot; private float _camStartFOV;

        private HUDController _hud;

        private void Start()
        {
            _ms = GameManager.Instance.Config.mortar;
            _rangeM = Mathf.Min(20f, _ms.rangeMaxM);
            if (!observeCamera) observeCamera = Camera.main;
            _defaultFOV = observeCamera ? observeCamera.fieldOfView : 60f;
            _hud = FindObjectOfType<HUDController>();
        }

        private void Update()
        {
            HandleInput();
            UpdatePivot();
            UpdateHUD();
        }

        private void HandleInput()
        {
            if (inputMode == InputMode.ControllerDriven)
            {
                // Mouse rotates azimuth
                float mouseX = Input.GetAxis("Mouse X");
                _bearingDeg = (360f + _bearingDeg + mouseX * (yawSensitivity * 10f)) % 360f;

                // Q/E fine-yaw ±2°
                if (Input.GetKeyDown(KeyCode.Q)) _bearingDeg = (360f + _bearingDeg - fineYawStepDeg) % 360f;
                if (Input.GetKeyDown(KeyCode.E)) _bearingDeg = (360f + _bearingDeg + fineYawStepDeg) % 360f;

                // Range control: ↑/↓ with coarse/fine steps
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    var step = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? _ms.fineStepM : _ms.coarseStepM;
                    _rangeM = Mathf.Clamp(_rangeM + step, 0f, _ms.rangeMaxM);
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    var step = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? _ms.fineStepM : _ms.coarseStepM;
                    _rangeM = Mathf.Clamp(_rangeM - step, 0f, _ms.rangeMaxM);
                }
            }
            else // RigLocal
            {
                // When using local input on the rig, read yaw/elevation from the rig
                if (rig)
                {
                    _bearingDeg = rig.GetYawDegrees();
                    float elev = rig.GetElevationDegrees();
                    // Invert linear mapping used in the rig: elevationMin/Max -> 0..rangeMax
                    float t = Mathf.InverseLerp(rig.elevationMinDeg, rig.elevationMaxDeg, elev);
                    _rangeM = Mathf.Clamp(t * _ms.rangeMaxM, 0f, _ms.rangeMaxM);
                }
            }

            // Observe zoom: hold RMB
            if (observeCamera)
            {
                observeCamera.fieldOfView = Input.GetMouseButton(1) ? observeZoomFOV : _defaultFOV;
            }

            // Projectile follow (hold key)
            HandleProjectileFollow();

            // Spotter (F): reduces next-shot dispersion; 20s cooldown
            if (Input.GetKeyDown(KeyCode.F) && _spotterReady)
            {
                _spotterActiveForNextShot = true;
                _spotterReady = false;
                _spotterCooldownEnd = Time.time + 20f;
            }
            if (!_spotterReady && Time.time >= _spotterCooldownEnd)
            {
                _spotterReady = true;
            }

            // Fire: LMB
            if (Input.GetMouseButtonDown(0))
            {
                TryFire(AmmoType.HE);
            }
        }

        private void UpdatePivot()
        {
            if (rig)
            {
                // Only drive the rig from MortarController if the rig has this enabled.
                // If using rig local input, we do not rotate any separate pivot here.
                if (rig.driveFromMortarController)
                {
                    rig.SetFromBearingRange(_bearingDeg, _rangeM, _ms);
                }
            }
            else if (mortarPivot)
            {
                // No rig present: rotate a simple pivot by bearing
                mortarPivot.rotation = Quaternion.Euler(0f, _bearingDeg, 0f);
            }
        }

        private void UpdateHUD()
        {
            if (_hud == null) return;
            _hud.UpdateCompass(_bearingDeg);
            _hud.UpdateRange(_rangeM);
            _hud.UpdateAmmo();
            var spotterRemain = _spotterReady ? 0f : Mathf.Max(0f, _spotterCooldownEnd - Time.time);
            _hud.UpdateSpotterCooldown(spotterRemain);
        }

        private void HandleProjectileFollow()
        {
            if (!enableProjectileFollow || observeCamera == null) return;

            bool keyDown = Input.GetKeyDown(projectileFollowKey);
            bool keyHeld = Input.GetKey(projectileFollowKey);
            bool keyUp = Input.GetKeyUp(projectileFollowKey);

            // Stop following if the projectile no longer exists
            if (_isFollowing && (_lastProjectile == null))
            {
                keyUp = true;
            }

            if (keyDown && _lastProjectile != null)
            {
                _isFollowing = true;
                _camStartPos = observeCamera.transform.position;
                _camStartRot = observeCamera.transform.rotation;
                _camStartFOV = observeCamera.fieldOfView;
                if (followAdjustFOV) observeCamera.fieldOfView = followFOV;
            }

            if (keyUp && _isFollowing)
            {
                _isFollowing = false;
                // Restore camera state
                observeCamera.transform.position = _camStartPos;
                observeCamera.transform.rotation = _camStartRot;
                observeCamera.fieldOfView = _camStartFOV;
            }

            if (_isFollowing && keyHeld && _lastProjectile != null)
            {
                // Compute follow position behind the projectile along its velocity, with some height offset
                Vector3 vel = _lastProjectile.GetVelocity();
                Vector3 dir = vel.sqrMagnitude > 0.0001f ? vel.normalized : _lastProjectile.transform.forward;
                Vector3 targetPos = _lastProjectile.transform.position - dir * followDistance + Vector3.up * followHeight;
                Quaternion targetRot = Quaternion.LookRotation(_lastProjectile.transform.position - observeCamera.transform.position, Vector3.up);

                // Smooth move/rotate
                observeCamera.transform.position = Vector3.Lerp(observeCamera.transform.position, targetPos, Time.deltaTime * followSmooth);
                observeCamera.transform.rotation = Quaternion.Slerp(observeCamera.transform.rotation, targetRot, Time.deltaTime * followSmooth);
            }
        }

        private void TryFire(AmmoType type)
        {
            if (Time.time < _nextFireTime) return; // reload
            if (!GameManager.Instance.ammoManager.TryConsume(type))
            {
                Debug.Log("No ammo of selected type.");
                return;
            }

            float dispersion = _spotterActiveForNextShot ? _ms.spotterSpreadM : _ms.baseSpreadM;
            Vector3 origin = (rig && rig.muzzle) ? rig.muzzle.position : (muzzle ? muzzle.position : transform.position);
            Vector3 impact = BallisticsModel.ComputeImpactPoint(origin, _bearingDeg, _rangeM, dispersion);
            float flightTime = BallisticsModel.ComputeFlightTime(_rangeM, _ms);

            // Spawn projectile oriented with the muzzle so physics flight uses forward direction
            var rotation = (rig && rig.muzzle) ? rig.muzzle.rotation : (mortarPivot ? mortarPivot.rotation : transform.rotation);
            var proj = Instantiate(Projectile, origin, rotation);
            var projComp = proj.GetComponent<Projectile>();
            projComp.Setup(type, origin, impact, flightTime);
            _lastProjectile = projComp;


            // Reload
            _nextFireTime = Time.time + _ms.reloadSec;

            // Impact feedback
            float missMeters = 0f; // In this lite model, we compute miss vs desired range minus actual L2 distance on ground
            var groundStart = new Vector3(origin.x, 0f, origin.z);
            var groundImpact = new Vector3(impact.x, 0f, impact.z);
            float actualDist = Vector3.Distance(groundStart, groundImpact);
            missMeters = actualDist - _rangeM;
            if (_hud)
            {
                _hud.ShowImpactFeedback(missMeters);
                _hud.ShowQOLSuggestion(missMeters);
            }

            // Consume spotter effect if used
            _spotterActiveForNextShot = false;
        }

        // Expose values for rig driving
        public float GetBearingDegrees() => _bearingDeg;
        public float GetDesiredRangeMeters() => _rangeM;
    }
}