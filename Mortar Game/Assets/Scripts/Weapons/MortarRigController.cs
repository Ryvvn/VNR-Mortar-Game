using UnityEngine;
using MortarGame.Config;

namespace MortarGame.Weapons
{
    // Controls a 3‑piece mortar rig: base (yaw), elevation cylinder (pitch), and barrel/tube.
    // Hook this up to the separate FBX parts: baseYawPivot, elevationPivotCylinder, barrelTube, and optional muzzle tip.
    public class MortarRigController : MonoBehaviour
    {
        [Header("Rig Pieces")]
        [Tooltip("Rotate around Y to aim (azimuth)")] public Transform baseYawPivot;
        [Tooltip("Elevation cylinder that should SLIDE up/down (no rotation)")] public Transform elevationPivotCylinder;
        [Tooltip("Barrel/Tube piece (will follow elevation)")] public Transform barrelTube;
        [Tooltip("Tip of the barrel for projectile spawn")] public Transform muzzle;

        [Header("Limits (degrees)")]
        public float yawMinDeg = -180f;
        public float yawMaxDeg = 180f;
        public float elevationMinDeg = -180;  // Typical playable range
        public float elevationMaxDeg = 180;

        [Header("Runtime State")]
        [SerializeField] private float _yawDeg;
        [SerializeField] private float _elevationDeg;


        public enum Axis { X, Y, Z };
        [Header("Axes & Offsets")]
        [Tooltip("Axis to rotate base yaw around (local space of baseYawPivot)")] public Axis yawAxis = Axis.Y;
        [Tooltip("Axis to rotate BARREL pitch around (local space of barrel or its pivot)")] public Axis elevationAxis = Axis.X;
        [Tooltip("Invert yaw direction (multiplies angle by -1)")] public bool invertYaw = false;
        [Tooltip("Invert elevation direction for barrel rotation (multiplies angle by -1)")] public bool invertElevation = false;
        [Tooltip("Use the initial local rotation of each piece as the baseline offset")] public bool useInitialRotationOffset = true;

        private Quaternion _baseInitialRot = Quaternion.identity;
        private Quaternion _elevInitialRot = Quaternion.identity;
        private Quaternion _barrelInitialRot = Quaternion.identity;
        private Vector3 _elevInitialLocalPos = Vector3.zero;

        [Header("Elevation Cylinder Translation")]
        [Tooltip("If true, the elevation cylinder will SLIDE up/down instead of rotating")] public bool translateElevationCylinder = true;
        [Tooltip("Local axis ON THE CYLINDER along which it slides (e.g., Z if the model has X:-90 import)")] public Axis elevationMoveAxis = Axis.Z;
        [Tooltip("Minimum slide distance from initial local position (in meters/Unity units)")] public float elevationSlideMin = 0f;
        [Tooltip("Maximum slide distance from initial local position (in meters/Unity units)")] public float elevationSlideMax = 0.40f;
        [Tooltip("Invert the slide direction")] public bool invertElevationSlide = false;
        [Tooltip("Preserve the cylinder's initial local position as the baseline")] public bool useInitialPositionOffset = true;

        [Header("Input (optional)")]
        public bool handleInputLocally = false;
        public float mouseYawSensitivity = 0.2f; // mouse X * sensitivity * 10
        public float coarseElevationStepDeg = 2f;
        public float fineElevationStepDeg = 0.5f;
        public float fineYawStepDeg = 1f; // Q/E

        [Header("Drive from MortarController (optional)")]
        public bool driveFromMortarController = true;
        public MortarController mortarController;

        private MissionConfig.MortarSettings _ms;

        [Header("Barrel Behavior")]
        [Tooltip("If true, rotate the barrel to match elevation angle. If false, barrel will not pitch here.")]
        public bool rotateBarrelPitch = true;
        [Tooltip("Use incremental rotation (Transform.Rotate) based on elevation delta instead of absolute quaternion setting")] public bool useIncrementalBarrelRotation = true;

        [Header("Update/Change Handling")]
        [Tooltip("Only apply transform changes when yaw/elevation values change")] public bool applyOnChangeOnly = true;
        [Tooltip("Minimum change in degrees required to trigger an update")] public float changeEpsilonDeg = 0.0001f;
        private float _lastYawDeg = float.NaN;
        private float _lastElevationDeg = float.NaN;

        private void Start()
        {
            // Try auto‑link MortarController
            if (mortarController == null) mortarController = GetComponent<MortarController>();
            _ms = MortarGame.Core.GameManager.Instance?.Config?.mortar;

            // Cache initial local rotations so model import offsets (e.g., X:-90) are preserved
            if (useInitialRotationOffset)
            {
                if (baseYawPivot) _baseInitialRot = baseYawPivot.localRotation; else _baseInitialRot = Quaternion.identity;
                if (elevationPivotCylinder) _elevInitialRot = elevationPivotCylinder.localRotation; else _elevInitialRot = Quaternion.identity;
                if (barrelTube) _barrelInitialRot = barrelTube.localRotation; else _barrelInitialRot = Quaternion.identity;
            }

            // Cache initial local position for the elevation cylinder
            if (elevationPivotCylinder)
            {
                _elevInitialLocalPos = elevationPivotCylinder.localPosition;
            }

            // Initialize last values to current values to avoid a large first delta
            _lastYawDeg = _yawDeg;
            _lastElevationDeg = _elevationDeg;

            // Ensure barrel starts from its initial orientation before incremental rotation
            if (barrelTube && rotateBarrelPitch && useIncrementalBarrelRotation)
            {
                barrelTube.localRotation = useInitialRotationOffset ? _barrelInitialRot : Quaternion.identity;
            }
        }

        private void Update()
        {
            if (driveFromMortarController && mortarController != null && _ms != null)
            {
                //SetFromBearingRange(mortarController.GetBearingDegrees(), mortarController.GetDesiredRangeMeters(), _ms);
            }
            else if (handleInputLocally)
            {
                HandleLocalInput();
            }

            ApplyTransforms();
        }

        private void OnValidate()
        {
            // Keep sensible ranges to avoid accidental 360° flips or negatives
            yawMinDeg = Mathf.Clamp(yawMinDeg, -360f, 360f);
            yawMaxDeg = Mathf.Clamp(yawMaxDeg, -360f, 360f);
            if (yawMinDeg > yawMaxDeg) { float tmp = yawMinDeg; yawMinDeg = yawMaxDeg; yawMaxDeg = tmp; }

            elevationMinDeg = Mathf.Clamp(elevationMinDeg, -360f, 360);
            elevationMaxDeg = Mathf.Clamp(elevationMaxDeg, elevationMinDeg, 360);

            coarseElevationStepDeg = Mathf.Clamp(coarseElevationStepDeg, 0.1f, 10f);
            fineElevationStepDeg = Mathf.Clamp(fineElevationStepDeg, 0.05f, coarseElevationStepDeg);
            fineYawStepDeg = Mathf.Clamp(fineYawStepDeg, 0.1f, 10f);

            if (elevationSlideMin > elevationSlideMax)
            {
                float t = elevationSlideMin; elevationSlideMin = elevationSlideMax; elevationSlideMax = t;
            }
        }

        public void SetYawDegrees(float yawDeg)
        {
            // Clamp incoming value first
            float clamped = Mathf.Clamp(yawDeg, yawMinDeg, yawMaxDeg);
            // If we're only applying changes when values differ, early‑out to avoid redundant updates/logs
            if (applyOnChangeOnly && !float.IsNaN(_yawDeg) && Mathf.Abs(clamped - _yawDeg) <= changeEpsilonDeg)
            {
                return;
            }
            _yawDeg = clamped;
        }

        public void SetElevationDegrees(float elevationDeg)
        {
            // Clamp incoming value first
            float clamped = Mathf.Clamp(elevationDeg, elevationMinDeg, elevationMaxDeg);
            // If we're only applying changes when values differ, early‑out to avoid redundant updates/logs
            if (applyOnChangeOnly && !float.IsNaN(_elevationDeg) && Mathf.Abs(clamped - _elevationDeg) <= changeEpsilonDeg)
            {
                return;
            }
            _elevationDeg = clamped;
            // Optional: uncomment for targeted debugging
            // if (verboseDebug) Debug.Log($"SetElevationDegrees -> {_elevationDeg}");
        }

        public void NudgeYawDegrees(float delta)
        {
            SetYawDegrees(_yawDeg + delta);
        }

        public void NudgeElevationDegrees(float delta)
        {

            SetElevationDegrees(_elevationDeg + delta);
        }

        // Map desired range (0..rangeMax) to elevation angle linearly within configured limits for a PUBG‑lite feel.
        public void SetFromBearingRange(float bearingDeg, float desiredRangeM, MissionConfig.MortarSettings ms)
        {
            SetYawDegrees(bearingDeg);
            float t = Mathf.Clamp01(desiredRangeM / Mathf.Max(0.01f, ms.rangeMaxM));
            float elev = Mathf.Lerp(elevationMinDeg, elevationMaxDeg, t);
            SetElevationDegrees(elev);
        }

        // Map desired range to elevation using a simple ballistic model: R = v^2/g * sin(2θ)
        // If useHighAngle is true, chooses the high-angle solution (θ_high = 90° - θ_low)
        public void SetFromBearingRangeBallistic(float bearingDeg, float desiredRangeM, float launchSpeed, bool useHighAngle = true)
        {
            SetYawDegrees(bearingDeg);
            float g = Mathf.Max(0.0001f, Physics.gravity.magnitude);
            float v = Mathf.Max(0.0001f, launchSpeed);
            float s = desiredRangeM * g / (v * v);
            s = Mathf.Clamp(s, 0f, 1f);
            float thetaLowDeg = 0.5f * Mathf.Rad2Deg * Mathf.Asin(s);
            float thetaDeg = useHighAngle ? (90f - thetaLowDeg) : thetaLowDeg;
            // Clamp to rig limits
            SetElevationDegrees(Mathf.Clamp(thetaDeg, elevationMinDeg, elevationMaxDeg));
        }

        private void HandleLocalInput()
        {
            float mouseX = Input.GetAxis("Mouse X");
            NudgeYawDegrees(mouseX * (mouseYawSensitivity * 10f));

            // Q/E fine yaw
            if (Input.GetKeyDown(KeyCode.Q)) NudgeYawDegrees(-fineYawStepDeg);
            if (Input.GetKeyDown(KeyCode.E)) NudgeYawDegrees(+fineYawStepDeg);

            // Up/Down adjust elevation (Shift for fine)
            bool fine = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float step = fine ? fineElevationStepDeg : coarseElevationStepDeg;
            if (Input.GetKeyDown(KeyCode.W))
            {

                NudgeElevationDegrees(+step);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {

                NudgeElevationDegrees(-step);
            }
        }

        private void ApplyTransforms()
        {
            bool yawChanged = Mathf.Abs(_yawDeg - _lastYawDeg) > changeEpsilonDeg || !applyOnChangeOnly || float.IsNaN(_lastYawDeg);
            bool elevChanged = Mathf.Abs(_elevationDeg - _lastElevationDeg) > changeEpsilonDeg || !applyOnChangeOnly || float.IsNaN(_lastElevationDeg);

            if (baseYawPivot)
            {
                if (yawChanged)
                {
                    var axis = AxisVector(yawAxis);
                    float sign = invertYaw ? -1f : 1f;
                    var rot = Quaternion.AngleAxis(_yawDeg * sign, axis);
                    baseYawPivot.localRotation = useInitialRotationOffset ? (_baseInitialRot * rot) : rot;
                }
            }

            // Elevation cylinder should slide up/down (no rotation)
            if (elevationPivotCylinder)
            {
                if (translateElevationCylinder)
                {
                    if (elevChanged)
                    {
                        // Map elevation angle to a 0..1 factor
                        float t = Mathf.InverseLerp(elevationMinDeg, elevationMaxDeg, _elevationDeg);
                        if (invertElevationSlide) t = 1f - t;
                        float offset = Mathf.Lerp(elevationSlideMin, elevationSlideMax, t);

                        // Direction in parent's local space that corresponds to the cylinder's chosen local axis
                        Vector3 axisLocal = AxisVector(elevationMoveAxis);
                        Vector3 axisWorld = elevationPivotCylinder.TransformDirection(axisLocal);
                        Vector3 axisInParent = elevationPivotCylinder.parent ? elevationPivotCylinder.parent.InverseTransformDirection(axisWorld) : axisWorld;

                        Vector3 basePos = useInitialPositionOffset ? _elevInitialLocalPos : Vector3.zero;
                        elevationPivotCylinder.localPosition = basePos + axisInParent * offset;

                        // Keep the cylinder's original local rotation (no pitch rotation)
                        if (useInitialRotationOffset) elevationPivotCylinder.localRotation = _elevInitialRot;
                    }
                }
                else
                {
                    if (elevChanged)
                    {
                        // Fallback: rotate cylinder if sliding is disabled
                        var axis = AxisVector(elevationAxis);
                        float sign = invertElevation ? -1f : 1f;
                        var rot = Quaternion.AngleAxis(_elevationDeg * sign, axis);
                        elevationPivotCylinder.localRotation = useInitialRotationOffset ? (_elevInitialRot * rot) : rot;
                    }
                }
            }

            // Pitch the barrel to match elevation (optional). If the barrel isn't parented under the elevation cylinder,
            // we still rotate it locally. If it IS parented, we rotate it so the tube visually aims while the cylinder slides.
            if (barrelTube && rotateBarrelPitch)
            {
                if (useIncrementalBarrelRotation)
                {
                    if (elevChanged)
                    {

                        float deltaElev = _elevationDeg - _lastElevationDeg;


                        float sign = invertElevation ? -1f : 1f;
                        var axis = AxisVector(elevationAxis);

                        barrelTube.Rotate(axis, deltaElev * sign, Space.Self);
                    }
                }
                else
                {
                    // var axis = AxisVector(elevationAxis);
                    // float sign = invertElevation ? -1f : 1f;
                    // var rot = Quaternion.AngleAxis(_elevationDeg * sign, axis);
                    // barrelTube.localRotation = useInitialRotationOffset ? (_barrelInitialRot * rot) : rot;
                }
            }

            // Update last values after transforms are applied
            if (yawChanged) _lastYawDeg = _yawDeg;
            if (elevChanged) _lastElevationDeg = _elevationDeg;
        }

        private static Vector3 AxisVector(Axis axis)
        {
            switch (axis)
            {
                case Axis.X: return Vector3.right;
                case Axis.Y: return Vector3.up;
                case Axis.Z: return Vector3.forward;
                default: return Vector3.up;
            }
        }

        // Utility getters for other systems
        public float GetYawDegrees() => _yawDeg;
        public float GetElevationDegrees() => _elevationDeg;
    }
}