using UnityEngine;
using MortarGame.Core;
using MortarGame.Config;
using MortarGame.Gameplay;
using MortarGame.Combat;
using MortarGame.UI;
using MortarGame.Quiz; // added for quiz integration
using MortarGame.Targets; // for Target distance
using MortarGame.Effects;
using System.Collections;
using System.Collections.Generic;


namespace MortarGame.Weapons
{
    public class MortarController : MonoBehaviour
    {
        public enum InputMode { ControllerDriven, RigLocal }
        [Header("Control Mode")] public InputMode inputMode = InputMode.ControllerDriven;
        [Header("Transforms")] public Transform mortarPivot; public Transform muzzle;
        [Header("Rig")] public MortarRigController rig;
        [Header("Camera Parenting")] public Camera observeCamera; public float observeZoomFOV = 45f; private float _defaultFOV;
        [Header("Projectile Follow (Hold Key)")]
        [Tooltip("Allow holding a key to make the camera follow the last fired projectile (War Thunder style)")] public bool enableProjectileFollow = true;
        [Tooltip("Key to hold for following the projectile")] public KeyCode projectileFollowKey = KeyCode.Space;
        [Tooltip("Distance behind the projectile to place the camera")] public float followDistance = 3f;
        [Tooltip("Vertical offset above the projectile")] public float followHeight = 0.75f;
        [Tooltip("Smooth damp toward follow position (higher is snappier)")] public float followSmooth = 10f;
        [Tooltip("Reduce FOV while following for a more focused view")] public bool followAdjustFOV = true;
        [Tooltip("Field of view while in follow mode")] public float followFOV = 35f;
        [Tooltip("How long the camera stays at the explosion point when holding the follow key after the projectile explodes")] public float followHoldReturnDelaySec = 1f;

        [Header("Camera Return Safety")]
        [Tooltip("If true, runs a short coroutine to ensure the camera actually returns to its pre-follow position/FOV.")] public bool enableReturnSafetyCheck = true;
        [Tooltip("Max time to keep checking before giving up")] public float returnSafetyTimeoutSec = 0.75f;
        [Tooltip("Position tolerance (meters) for considering the camera returned")] public float returnPositionTolerance = 0.02f;
        [Tooltip("Rotation tolerance (degrees) for considering the camera returned")] public float returnRotationToleranceDeg = 1.0f;
        [Tooltip("FOV tolerance for considering the camera returned")] public float returnFOVTolerance = 0.25f;
        [Header("Camera Parenting")]
        [Tooltip("Detach camera from its parent while following to prevent parent-driven motion from overriding follow/return")] public bool detachCameraDuringFollow = true;
        [Header("Aim Camera Attachment")]
        [Tooltip("While aiming (not following projectile), parent the observeCamera to a mount so it rotates with mortar yaw/pitch.")] public bool attachCameraToMortarWhileAiming = true;
        [Tooltip("Mount to parent the camera to during aiming; if null, uses rig.baseYawPivot or mortarPivot.")] public Transform cameraAimMount;
        [Header("Input")] public float yawSensitivity = 0.2f; public float fineYawStepDeg = 2f;
        [Header("Projectile")]
        public GameObject Projectile;

        [Header("Ammo Selection")] public KeyCode selectHEKey = KeyCode.Alpha1; public KeyCode selectHEPlusKey = KeyCode.Alpha2; public KeyCode selectSmokeKey = KeyCode.Alpha3;
        private AmmoType _selectedAmmoType = AmmoType.HE;
        [Header("VFX/SFX")]
        [Tooltip("Sound played when firing the mortar")] public AudioClip fireSFX;
        [Tooltip("Particle system for muzzle flash")] public ParticleSystem muzzleFlashPrefab;
        [Tooltip("Sound when a target is hit")] public AudioClip hitSFX;
        [Tooltip("Sound when a target is destroyed")] public AudioClip targetDestroyedSFX;

        private MissionConfig.MortarSettings _ms;
        private float _bearingDeg; // Compass heading
        private float _rangeM;     // Desired range in meters

        private float _nextFireTime;
        private bool _spotterReady = true; private float _spotterCooldownEnd;
        private bool _spotterActiveForNextShot;
        // Follow state
        private Projectile _lastProjectile;
        private bool _isFollowing;
        private bool _explodedWhileFollowing; // set true on projectile explosion if we were following
        private Vector3 _camStartPos; private Quaternion _camStartRot; private float _camStartFOV;
        // Save parenting + local transforms to restore precisely
        private Transform _camStartParent; private Vector3 _camStartLocalPos; private Quaternion _camStartLocalRot;
        private Coroutine _returnRoutine; // return-to-start scheduler
        private Coroutine _returnSafetyRoutine; // safety checker to ensure camera returned

        private HUDController _hud;
        private QuizManager _quiz; // cache
        private QuestionPanelController _panel; // cache
        private bool _hudHiddenByQuiz; // track HUD hidden state while quiz is open
        private bool _useHighAngle = true;
        // Target selection + lead indicator
        private Transform _selectedTarget;
        private int _selectedTargetIndex = -1;
        [SerializeField] private bool showLeadIndicator = true;
        [SerializeField] private float leadLineWidth = 0.12f;
        [SerializeField] private Color leadLineColor = Color.yellow;
        private LineRenderer _leadLine;
        // Selected target outline settings
        [SerializeField] private Color selectedOutlineColor = Color.yellow;
        [SerializeField] private float selectedOutlineThickness = 0.035f;
        private SelectionOutline _currentOutline;

        private void Start()
        {
            _ms = GameManager.Instance.Config.mortar;
            _rangeM = Mathf.Min(20f, _ms.rangeMaxM);
            if (!observeCamera) observeCamera = Camera.main;
            _defaultFOV = observeCamera ? observeCamera.fieldOfView : 60f;
            _hud = FindObjectOfType<HUDController>();
            _quiz = FindObjectOfType<QuizManager>();
            _panel = QuestionPanelController.Instance ?? FindObjectOfType<QuestionPanelController>();
            // Initialize HUD ammo display with current selection
            _hud?.UpdateAmmoWithSelection(_selectedAmmoType);
            // Initialize lead indicator line renderer
            if (showLeadIndicator)
            {
                var go = new GameObject("LeadIndicator");
                _leadLine = go.AddComponent<LineRenderer>();
                _leadLine.useWorldSpace = true;
                _leadLine.startWidth = leadLineWidth;
                _leadLine.endWidth = leadLineWidth;
                _leadLine.positionCount = 2;
                // Use default sprite shader for simple colored line
                var mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = leadLineColor;
                _leadLine.material = mat;
                _leadLine.startColor = leadLineColor;
                _leadLine.endColor = leadLineColor;
                _leadLine.enabled = false;
            }
        }

        private void Update()
        {
            HandleInput();
            UpdatePivot();
            UpdateHUDVisibility();
            if (!IsQuizOpen())
            {
                UpdateHUD();
            }
            // Update world-space lead indicator
            UpdateLeadIndicator();
            // Ensure the aim camera follows mortar rotation when not following projectile
            EnsureAimCameraAttachment();
        }

        private bool IsQuizOpen()
        {
            return _panel != null && _panel.IsOpen;
        }

        private void HandleInput()
        {
            // Hotkey: open quiz panel for playtesting convenience
            if (Input.GetKeyDown(KeyCode.G))
            {
                var panel = QuestionPanelController.Instance ?? _panel ?? FindObjectOfType<QuestionPanelController>();
                if (panel != null && !panel.IsOpen)
                {
                    // Ensure quiz rewards match the currently selected ammo type
                    GameManager.Instance.lastAttemptedAmmoType = _selectedAmmoType;
                    panel.ShowNextQuestion();
                    _panel = panel; // cache
                }
            }

            // While quiz panel is open, disable input for rig/camera
            if (IsQuizOpen())
            {
                return;
            }

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
                    float v = GetLaunchSpeed();
                    float g = Mathf.Max(0.0001f, Physics.gravity.magnitude);
                    // Use the muzzle/barrel world pitch instead of internal elevation degrees, since
                    // your "elevation 0" corresponds to the longest range (≈45° world pitch)
                    Vector3 fwd = rig.muzzle ? rig.muzzle.forward : (rig.barrelTube ? rig.barrelTube.forward : transform.forward);
                    float horiz = Mathf.Sqrt(fwd.x * fwd.x + fwd.z * fwd.z);
                    float thetaRad = Mathf.Atan2(Mathf.Max(0f, fwd.y), horiz);
                    float predictedRange = (v * v / g) * Mathf.Sin(2f * thetaRad);
                    // Clamp to mission range for sanity
                    predictedRange = Mathf.Clamp(predictedRange, 0f, _ms.rangeMaxM);
                    _rangeM = predictedRange;
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

            // Ammo selection hotkeys
            if (Input.GetKeyDown(selectHEKey)) { _selectedAmmoType = AmmoType.HE; GameManager.Instance.lastAttemptedAmmoType = _selectedAmmoType; _hud?.UpdateAmmoWithSelection(_selectedAmmoType); }
            if (Input.GetKeyDown(selectHEPlusKey)) { _selectedAmmoType = AmmoType.HEPlus; GameManager.Instance.lastAttemptedAmmoType = _selectedAmmoType; _hud?.UpdateAmmoWithSelection(_selectedAmmoType); }
            if (Input.GetKeyDown(selectSmokeKey)) { _selectedAmmoType = AmmoType.Smoke; GameManager.Instance.lastAttemptedAmmoType = _selectedAmmoType; _hud?.UpdateAmmoWithSelection(_selectedAmmoType); }

            // Target selection controls
            if (Input.GetKeyDown(KeyCode.T)) { SelectNearestTarget(); }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                bool prev = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                CycleSelectedTarget(prev ? -1 : 1);
            }
            if (Input.GetKeyDown(KeyCode.C)) { ClearSelectedTarget(); }

            // Fire: LMB
            if (Input.GetMouseButtonDown(0))
            {
                TryFire(_selectedAmmoType);
            }
        }

        private void UpdatePivot()
        {
            if (rig)
            {
                // Only drive the rig from MortarController if the rig has this enabled.
                if (rig.driveFromMortarController)
                {
                    // Use ballistic mapping so range corresponds to physics. High-angle solution for mortar feel.
                    float v = GetLaunchSpeed();
                    // Toggle ballistic solution mode: High/Low (press L)
                    if (Input.GetKeyDown(KeyCode.L))
                    {
                        _useHighAngle = !_useHighAngle;
                    }
                    rig.SetFromBearingRangeBallistic(_bearingDeg, _rangeM, v, useHighAngle: _useHighAngle);
                }
            }
            else if (mortarPivot)
            {
                // No rig present: rotate a simple pivot by bearing
                mortarPivot.rotation = Quaternion.Euler(0f, _bearingDeg, 0f);
            }
        }

        private float GetLaunchSpeed()
        {
            try
            {
                if (Projectile != null)
                {
                    var pc = Projectile.GetComponent<Projectile>();
                    if (pc != null && pc.launchSpeed > 0f) return pc.launchSpeed;
                }
            }
            catch { }
            return 30f; // sensible default if prefab missing or not assigned
        }

        private float? GetNearestTargetDistanceMeters()
        {
            try
            {
                var targets = FindObjectsOfType<Target>();
                if (targets == null || targets.Length == 0) return null;
                Vector3 origin = (rig && rig.muzzle) ? rig.muzzle.position : (muzzle ? muzzle.position : transform.position);
                Vector3 groundOrigin = new Vector3(origin.x, 0f, origin.z);
                float minDist = float.MaxValue;
                foreach (var t in targets)
                {
                    Vector3 tp = t.transform.position;
                    Vector3 groundTarget = new Vector3(tp.x, 0f, tp.z);
                    float d = Vector3.Distance(groundOrigin, groundTarget);
                    if (d < minDist) { minDist = d; }
                }
                return minDist;
            }
            catch { return null; }
        }

        private void UpdateHUD()
        {
            if (!_hud) return;

            // Compass
            _hud.UpdateCompass(_bearingDeg);

            // Range display: show both dialed and predicted-from-angle
            var v = GetLaunchSpeed();
            var g = Physics.gravity.magnitude;
            float ballisticMaxM = (v * v) / g;

            float predictedM = _rangeM;
            if (rig)
            {
                // Predict from the muzzle/barrel's actual world pitch so Elevation 0 (≈45°) shows max range
                Vector3 fwd = rig.muzzle ? rig.muzzle.forward : (rig.barrelTube ? rig.barrelTube.forward : transform.forward);
                float horiz = Mathf.Sqrt(fwd.x * fwd.x + fwd.z * fwd.z);
                float thetaRad = Mathf.Atan2(Mathf.Max(0f, fwd.y), horiz);
                predictedM = (v * v / g) * Mathf.Sin(2f * thetaRad);
                predictedM = Mathf.Clamp(predictedM, 0f, _ms.rangeMaxM);
            }

            // Prefer the composite method if available
            _hud.UpdateRangeWithPredicted(_rangeM, predictedM, ballisticMaxM);

            // Elevation angle
            if (rig) _hud.UpdateElevation(rig.GetElevationDegrees());

            // Target distance, delta, and lead
            var target = _selectedTarget != null ? _selectedTarget : GetClosestTarget();
            bool hasTarget = target != null;
            bool isSelected = _selectedTarget != null && target == _selectedTarget;

            if (hasTarget)
            {
                float dx = Vector3.Distance(transform.position, target.position);
                _hud.UpdateTargetDistance(dx, true, isSelected);
                _hud.UpdateRangeDelta(_rangeM - dx);

                // Compute simple ground-plane lead suggestion if selected moving enemy
                float leadM = 0f;
                bool leadActive = false;
                if (isSelected)
                {
                    var enemy = target.GetComponent<MortarGame.Enemies.EnemyController>();
                    if (enemy != null)
                    {
                        Vector3 origin = (rig && rig.muzzle) ? rig.muzzle.position : (muzzle ? muzzle.position : transform.position);
                        Vector3 groundOrigin = new Vector3(origin.x, 0f, origin.z);
                        Vector3 tp = target.position;
                        Vector3 groundTarget = new Vector3(tp.x, 0f, tp.z);
                        Vector3 radial = groundTarget - groundOrigin;
                        float dist = radial.magnitude;
                        if (dist > 0.01f)
                        {
                            Vector3 radialDir = radial / dist;
                            Vector3 vEnemy = enemy.GetGroundVelocity();
                            float tFlight = BallisticsModel.ComputeFlightTime(_rangeM, _ms);
                            leadM = Vector3.Dot(vEnemy, radialDir) * tFlight;
                            leadActive = Mathf.Abs(leadM) > 0.05f;
                        }
                    }
                }
                _hud.UpdateLead(leadM, leadActive);
            }
            else
            {
                _hud.UpdateTargetDistance(0f, false, false);
                _hud.UpdateRangeDelta(0f);
                _hud.UpdateLead(0f, false);
            }

            // Solution indicator
            _hud.UpdateSolution(_useHighAngle);

            // Ammo counts + selection highlight
            _hud.UpdateAmmoWithSelection(_selectedAmmoType);
        }

        private Transform GetClosestTarget()
        {
            Vector3 origin = (rig && rig.muzzle) ? rig.muzzle.position : (muzzle ? muzzle.position : transform.position);
            Vector3 groundOrigin = new Vector3(origin.x, 0f, origin.z);
            float minDist = float.MaxValue;
            Transform closest = null;

            // Static targets
            var targets = FindObjectsOfType<MortarGame.Targets.Target>();
            if (targets != null && targets.Length > 0)
            {
                foreach (var t in targets)
                {
                    Vector3 tp = t.transform.position;
                    Vector3 groundTarget = new Vector3(tp.x, 0f, tp.z);
                    float d = Vector3.Distance(groundOrigin, groundTarget);
                    if (d < minDist) { minDist = d; closest = t.transform; }
                }
            }

            // Moving enemies (includes tanks with EnemyController)
            var enemies = FindObjectsOfType<MortarGame.Enemies.EnemyController>();
            if (enemies != null && enemies.Length > 0)
            {
                foreach (var e in enemies)
                {
                    Vector3 ep = e.transform.position;
                    Vector3 groundEnemy = new Vector3(ep.x, 0f, ep.z);
                    float d = Vector3.Distance(groundOrigin, groundEnemy);
                    if (d < minDist) { minDist = d; closest = e.transform; }
                }
            }

            return closest;
        }

        private void UpdateHUDVisibility()
        {
            if (_hud == null) return;
            bool open = IsQuizOpen();
            if (open && !_hudHiddenByQuiz)
            {
                _hud.gameObject.SetActive(false);
                _hudHiddenByQuiz = true;
            }
            else if (!open && _hudHiddenByQuiz)
            {
                _hud.gameObject.SetActive(true);
                _hudHiddenByQuiz = false;
            }
        }

        private void HandleProjectileFollow()
        {
            if (!enableProjectileFollow || observeCamera == null) return;
            if (IsQuizOpen()) return; // disable camera follow while quiz is open

            bool keyDown = Input.GetKeyDown(projectileFollowKey);
            bool keyHeld = Input.GetKey(projectileFollowKey);
            bool keyUp = Input.GetKeyUp(projectileFollowKey);

            // If an explosion occurred while following, resolve return based on current key state
            if (_explodedWhileFollowing)
            {
                if (keyHeld)
                {
                    // Keep camera at last position briefly, then return
                    ScheduleReturnAfter(followHoldReturnDelaySec);
                }
                else
                {
                    // Player released follow key at or by explosion time: return immediately
                    ForceReturnToStartImmediate();
                }
                _explodedWhileFollowing = false;
            }

            if (keyDown && _lastProjectile != null)
            {
                _isFollowing = true;
                // Save both world and local + parent state
                _camStartPos = observeCamera.transform.position;
                _camStartRot = observeCamera.transform.rotation;
                _camStartFOV = observeCamera.fieldOfView;
                _camStartParent = observeCamera.transform.parent;
                _camStartLocalPos = observeCamera.transform.localPosition;
                _camStartLocalRot = observeCamera.transform.localRotation;
                if (detachCameraDuringFollow) observeCamera.transform.SetParent(null, true);
                if (followAdjustFOV) observeCamera.fieldOfView = followFOV;
            }

            if (keyUp && _isFollowing)
            {
                // Stop any scheduled delayed return
                _isFollowing = false;
                if (_returnRoutine != null) { StopCoroutine(_returnRoutine); _returnRoutine = null; }
                // Restore camera immediately and run safety
                ForceReturnToStartImmediate();
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
            // Record attempted type so quiz reward matches it
            GameManager.Instance.lastAttemptedAmmoType = type;
            if (!GameManager.Instance.ammoManager.TryConsume(type))
            {
                Debug.Log("No ammo of selected type.");
                // Offer a quiz to earn ammo when out of rounds
                var panel = QuestionPanelController.Instance ?? _panel ?? FindObjectOfType<QuestionPanelController>();
                if (panel != null)
                {
                    if (!panel.IsOpen)
                        panel.ShowNextQuestion();
                    _panel = panel;
                }
                else
                {
                    Debug.LogWarning("QuestionPanelController not found in scene; cannot show quiz panel.");
                }
                return;
            }

            float dispersion = _spotterActiveForNextShot ? _ms.spotterSpreadM : _ms.baseSpreadM;
            Vector3 origin = (rig && rig.muzzle) ? rig.muzzle.position : (muzzle ? muzzle.position : transform.position);
            Vector3 impact = BallisticsModel.ComputeImpactPoint(origin, _bearingDeg, _rangeM, dispersion);
            float flightTime = BallisticsModel.ComputeFlightTime(_rangeM, _ms);

            // Spawn projectile oriented with the muzzle so physics flight uses forward direction
            var rotation = (rig && rig.muzzle) ? rig.muzzle.rotation : (mortarPivot ? mortarPivot.rotation : transform.rotation);

            // Firing SFX
            if (fireSFX != null)
            {
                AudioSource.PlayClipAtPoint(fireSFX, origin, 0.85f);
            }
            // Muzzle flash VFX
            if (muzzleFlashPrefab != null)
            {
                var parent = (rig && rig.muzzle) ? rig.muzzle : null;
                var flash = Instantiate(muzzleFlashPrefab, origin, rotation, parent);
                flash.Play();
                Destroy(flash.gameObject, 2f);
            }

            var proj = Instantiate(Projectile, origin, rotation);
            var projComp = proj.GetComponent<Projectile>();
            projComp.Setup(type, origin, impact, flightTime);
            _lastProjectile = projComp;

            // Subscribe to impact to show feedback/suggestion only after explosion
            float desiredRangeAtShot = _rangeM; // capture dialed range at fire time
            projComp.OnExploded += (impactPos) =>
            {
                // Compute plain ground miss distance
                var groundStart = new Vector3(origin.x, 0f, origin.z);
                var groundImpact = new Vector3(impactPos.x, 0f, impactPos.z);
                var actualDist = Vector3.Distance(groundStart, groundImpact);
                var missMeters = actualDist - desiredRangeAtShot;

                // Detect hit on target/enemy using the same explosion radius as Projectile
                float radius;
                switch (projComp.ammoType)
                {
                    case AmmoType.HEPlus: radius = 5.5f; break;
                    case AmmoType.Smoke: radius = 4f; break;
                    case AmmoType.HE:
                    default: radius = 4f; break;
                }
                var cols = Physics.OverlapSphere(impactPos, radius);
                bool hitSomething = false;
                var impactedTargets = new List<MortarGame.Targets.Target>();
                var impactedEnemies = new List<MortarGame.Enemies.EnemyController>();
                foreach (var c in cols)
                {
                    var t = c.GetComponent<MortarGame.Targets.Target>();
                    if (t != null)
                    {
                        hitSomething = true;
                        if (!impactedTargets.Contains(t)) impactedTargets.Add(t);
                    }
                    var e = c.GetComponent<MortarGame.Enemies.EnemyController>();
                    if (e != null)
                    {
                        hitSomething = true;
                        if (!impactedEnemies.Contains(e)) impactedEnemies.Add(e);
                    }
                }

                if (hitSomething)
                {
                    _hud?.ShowHitTarget();
                    _hud?.ClearSuggestion();
                    if (hitSFX != null)
                    {
                        AudioSource.PlayClipAtPoint(hitSFX, impactPos, 0.8f);
                    }
                    StartCoroutine(CheckDestroyedEntitiesLater(impactedTargets, impactedEnemies));
                }
                else
                {
                    _hud?.ShowImpactFeedback(missMeters);
                    _hud?.ShowQOLSuggestion(missMeters);
                }

                // Camera shake on explosion
                float shakeMag = projComp.ammoType == AmmoType.HEPlus ? 0.22f : 0.16f;
                StartCoroutine(ShakeCamera(shakeMag, 0.25f));

                // Mark projectile ref null so follow logic reliably detects explosion state
                _lastProjectile = null;

                // Flag for follow handler to resolve return on next Update based on input state
                if (_isFollowing)
                {
                    _explodedWhileFollowing = true;
                }
            };

            // Reload
            _nextFireTime = Time.time + _ms.reloadSec;

            // Consume spotter effect if used
            _spotterActiveForNextShot = false;
        }

        // Utility coroutine: check destroyed entities shortly after explosion
        private IEnumerator CheckDestroyedEntitiesLater(List<MortarGame.Targets.Target> initialTargets, List<MortarGame.Enemies.EnemyController> initialEnemies)
        {
            // Wait a short moment so the damage system can process and destroy targets/enemies
            yield return new WaitForSeconds(0.12f);
            bool anyDestroyed = false;
            Vector3 sfxPos = transform.position;

            // Check static targets
            if (initialTargets != null)
            {
                foreach (var t in initialTargets)
                {
                    if (t != null) sfxPos = t.transform.position;
                    if (t == null || t.currentHP <= 0f)
                    {
                        anyDestroyed = true;
                        break;
                    }
                }
            }

            // Check moving enemies
            if (!anyDestroyed && initialEnemies != null)
            {
                foreach (var e in initialEnemies)
                {
                    if (e != null) sfxPos = e.transform.position;
                    if (e == null || e.currentHP <= 0f)
                    {
                        anyDestroyed = true;
                        break;
                    }
                }
            }

            if (anyDestroyed)
            {
                _hud?.ShowTargetDestroyed();
                if (targetDestroyedSFX != null)
                {
                    AudioSource.PlayClipAtPoint(targetDestroyedSFX, sfxPos, 0.9f);
                }
            }
        }

        // Camera shake helper
        private IEnumerator ShakeCamera(float magnitude, float duration)
        {
            if (observeCamera == null) yield break;
            var cam = observeCamera.transform;
            Vector3 start = cam.localPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float damper = 1f - Mathf.Clamp01(t / duration);
                cam.localPosition = start + Random.insideUnitSphere * magnitude * damper;
                yield return null;
            }
            cam.localPosition = start;
        }

        // Delayed camera return scheduler & coroutine
        private void ScheduleReturnAfter(float delaySec)
        {
            if (!_isFollowing) return;
            if (_returnRoutine != null) return;
            _returnRoutine = StartCoroutine(ReturnCameraToStartAfter(delaySec));
        }

        private IEnumerator ReturnCameraToStartAfter(float delaySec)
        {
            float t = 0f;
            while (t < delaySec)
            {
                // If follow was cancelled or a new projectile appeared, cancel the scheduled return
                if (!_isFollowing || _lastProjectile != null)
                {
                    _returnRoutine = null;
                    yield break;
                }
                t += Time.deltaTime;
                yield return null;
            }

            if (_isFollowing && observeCamera != null)
            {
                ForceReturnToStartImmediate();
            }
            _returnRoutine = null;
        }

        private void ForceReturnToStartImmediate()
        {
            if (observeCamera == null) return;

            // Cancel following state and any scheduled delayed return
            _isFollowing = false;
            if (_returnRoutine != null) { StopCoroutine(_returnRoutine); _returnRoutine = null; }

            // Restore camera parent and transform (local if parent existed, world otherwise)
            if (_camStartParent != null)
            {
                observeCamera.transform.SetParent(_camStartParent, false);
                observeCamera.transform.localPosition = _camStartLocalPos;
                observeCamera.transform.localRotation = _camStartLocalRot;
            }
            else
            {
                observeCamera.transform.SetParent(null, true);
                observeCamera.transform.position = _camStartPos;
                observeCamera.transform.rotation = _camStartRot;
            }

            // Restore FOV
            observeCamera.fieldOfView = _camStartFOV;

            // Start safety check to enforce return in case other scripts override
            StartReturnSafetyCheck();
        }

        // Safety check to ensure camera actually returned to start state
        private void StartReturnSafetyCheck()
        {
            if (!enableReturnSafetyCheck || observeCamera == null) return;
            if (_returnSafetyRoutine != null) { StopCoroutine(_returnSafetyRoutine); _returnSafetyRoutine = null; }
            _returnSafetyRoutine = StartCoroutine(EnsureCameraReturnedSafety());
        }

        private IEnumerator EnsureCameraReturnedSafety()
        {
            float endTime = Time.time + returnSafetyTimeoutSec;
            while (Time.time < endTime)
            {
                // Apply after all Updates/LateUpdates to win against other scripts
                yield return new WaitForEndOfFrame();

                if (_isFollowing || observeCamera == null)
                {
                    _returnSafetyRoutine = null;
                    yield break;
                }
                bool posOK = (_camStartParent != null)
                    ? Vector3.Distance(observeCamera.transform.localPosition, _camStartLocalPos) <= returnPositionTolerance
                    : Vector3.Distance(observeCamera.transform.position, _camStartPos) <= returnPositionTolerance;
                bool rotOK = (_camStartParent != null)
                    ? Quaternion.Angle(observeCamera.transform.localRotation, _camStartLocalRot) <= returnRotationToleranceDeg
                    : Quaternion.Angle(observeCamera.transform.rotation, _camStartRot) <= returnRotationToleranceDeg;
                bool fovOK = Mathf.Abs(observeCamera.fieldOfView - _camStartFOV) <= returnFOVTolerance;

                if (posOK && rotOK && fovOK)
                {
                    _returnSafetyRoutine = null;
                    yield break;
                }

                // Force reapply if outside tolerance (at end of frame)
                if (_camStartParent != null)
                {
                    observeCamera.transform.localPosition = _camStartLocalPos;
                    observeCamera.transform.localRotation = _camStartLocalRot;
                }
                else
                {
                    observeCamera.transform.position = _camStartPos;
                    observeCamera.transform.rotation = _camStartRot;
                }
                observeCamera.fieldOfView = _camStartFOV;
            }
            _returnSafetyRoutine = null;
        }

        // Expose values for rig driving
        public float GetBearingDegrees() => _bearingDeg;
        public float GetDesiredRangeMeters() => _rangeM;

        // Target selection and lead indicator helpers
        private List<Transform> GetEngageableTargets()
        {
            var list = new List<Transform>();
            // Static targets
            var targets = FindObjectsOfType<MortarGame.Targets.Target>();
            if (targets != null)
            {
                foreach (var t in targets) if (t) list.Add(t.transform);
            }
            // Moving enemies
            var enemies = FindObjectsOfType<MortarGame.Enemies.EnemyController>();
            if (enemies != null)
            {
                foreach (var e in enemies) if (e) list.Add(e.transform);
            }
            return list;
        }

        private void SelectNearestTarget()
        {
            var all = GetEngageableTargets();
            if (all.Count == 0)
            {
                _selectedTarget = null;
                _selectedTargetIndex = -1;
                UpdateSelectionOutline();
                return;
            }
            Vector3 origin = (rig && rig.muzzle) ? rig.muzzle.position : (muzzle ? muzzle.position : transform.position);
            Vector3 groundOrigin = new Vector3(origin.x, 0f, origin.z);
            float minDist = float.MaxValue;
            int bestIdx = -1;
            for (int i = 0; i < all.Count; i++)
            {
                var t = all[i];
                Vector3 tp = t.position;
                Vector3 groundTarget = new Vector3(tp.x, 0f, tp.z);
                float d = Vector3.Distance(groundOrigin, groundTarget);
                if (d < minDist)
                {
                    minDist = d; bestIdx = i;
                }
            }
            if (bestIdx >= 0)
            {
                _selectedTarget = all[bestIdx];
                _selectedTargetIndex = bestIdx;
                UpdateSelectionOutline();
            }
        }

        private void CycleSelectedTarget(int dir)
        {
            var all = GetEngageableTargets();
            if (all.Count == 0)
            {
                _selectedTarget = null;
                _selectedTargetIndex = -1;
                UpdateSelectionOutline();
                return;
            }
            // Determine current index in fresh list
            int current = -1;
            if (_selectedTarget != null)
            {
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i] == _selectedTarget) { current = i; break; }
                }
            }
            if (current < 0) current = 0; // start from first if none selected
            int next = (current + dir) % all.Count;
            if (next < 0) next += all.Count;
            _selectedTarget = all[next];
            _selectedTargetIndex = next;
            UpdateSelectionOutline();
        }

        private void ClearSelectedTarget()
        {
            _selectedTarget = null;
            _selectedTargetIndex = -1;
            if (_leadLine) _leadLine.enabled = false;
            UpdateSelectionOutline();
        }

        private void UpdateLeadIndicator()
        {
            if (!showLeadIndicator || _leadLine == null)
            {
                return;
            }
            if (_selectedTarget == null)
            {
                _leadLine.enabled = false;
                return;
            }
            var enemy = _selectedTarget.GetComponent<MortarGame.Enemies.EnemyController>();
            if (enemy == null)
            {
                _leadLine.enabled = false;
                return;
            }
            // Compute radial direction and enemy ground velocity component.
            Vector3 origin = (rig && rig.muzzle) ? rig.muzzle.position : (muzzle ? muzzle.position : transform.position);
            Vector3 groundOrigin = new Vector3(origin.x, 0f, origin.z);
            Vector3 tp = _selectedTarget.position;
            Vector3 groundTarget = new Vector3(tp.x, 0f, tp.z);
            Vector3 radial = groundTarget - groundOrigin;
            float dist = radial.magnitude;
            if (dist < 0.01f)
            {
                _leadLine.enabled = false;
                return;
            }
            Vector3 radialDir = radial / dist;
            Vector3 vEnemy = enemy.GetGroundVelocity();
            float tFlight = BallisticsModel.ComputeFlightTime(_rangeM, _ms);
            float leadM = Vector3.Dot(vEnemy, radialDir) * tFlight;
            if (Mathf.Abs(leadM) < 0.05f)
            {
                _leadLine.enabled = false;
                return;
            }
            // Draw a short line segment along the radial direction indicating where the target may be on impact
            Vector3 start = groundTarget + Vector3.up * 0.1f; // slight lift to avoid z-fighting
            Vector3 end = start + radialDir * leadM;
            _leadLine.enabled = true;
            _leadLine.SetPosition(0, start);
            _leadLine.SetPosition(1, end);
        }

        private void UpdateSelectionOutline()
        {
            // Disable and remove previous outline
            if (_currentOutline != null)
            {

                _currentOutline.Activate(false);
                _currentOutline = null;
            }
            if (_selectedTarget == null) return;

            var outline = _selectedTarget.GetComponent<SelectionOutline>();
            if (outline == null) outline = _selectedTarget.gameObject.AddComponent<SelectionOutline>();
            // Ensure the correct outline shader is assigned
            var shader = Shader.Find("MortarGame/Outline");
            if (shader != null) { outline.SetShader(shader); }
            outline.SetColor(selectedOutlineColor);
            outline.SetThickness(selectedOutlineThickness);
            outline.Activate(true);
            _currentOutline = outline;
        }



        private void EnsureAimCameraAttachment()
        {
            if (!attachCameraToMortarWhileAiming || observeCamera == null) return;
            // Do not alter parenting while following a projectile
            if (_isFollowing) return;

            Transform targetParent = cameraAimMount;
            if (targetParent == null)
            {
                if (rig != null && rig.baseYawPivot != null)
                {
                    targetParent = rig.baseYawPivot;
                }
                else if (mortarPivot != null)
                {
                    targetParent = mortarPivot;
                }
                else
                {
                    targetParent = transform;
                }
            }

            if (observeCamera.transform.parent != targetParent)
            {
                // Keep world position/rotation stable while reparenting so there's no visual jump
                observeCamera.transform.SetParent(targetParent, true);
            }
        }
    }
}