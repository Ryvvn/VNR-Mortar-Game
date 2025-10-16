Unity setup guide for "Night Withdrawal, 12/1946" (Week-1 scope)

1) Scene basics
- Create a new Scene (e.g., NightWithdrawal).
- Add a ground plane and rough street/alleys, place the mortar position.

2) GameManager
- Create an empty GameObject named GameManager.
- Add the component: MortarGame.Core.GameManager.
- Ensure presetFileName is preset_M3_RutLui_121946.json (default) and that StreamingAssets contains the JSON + CSV.

3) AmmoManager & EnemyManager
- GameManager will auto-create these if absent. Alternatively, add them manually:
  - Create empty GameObjects named AmmoManager and EnemyManager.
  - Add MortarGame.Gameplay.AmmoManager and MortarGame.Enemies.EnemyManager components.

4) Mortar Controller
- Create an empty GameObject named Mortar.
- Drag your separated mortar model (Assets/Models/ithappy/Military_FREE/Meshes/mortar_seperate.fbx) into the scene.
- Identify the three pieces:
  1) Base (baseplate + bipod) — assign to MortarRigController.baseYawPivot
  2) Elevation cylinder (holds barrel) — assign to MortarRigController.elevationPivotCylinder
  3) Barrel/Tube — assign to MortarRigController.barrelTube
- Create/assign a child Transform at the barrel tip (e.g., MuzzleTip) and assign to MortarRigController.muzzle.
- Add MortarGame.Weapons.MortarRigController to the Mortar GameObject and link the four Transforms above.
- Add MortarGame.Weapons.MortarController to the Mortar GameObject, and drag the MortarRigController into its "Rig" field.
- MortarController will drive yaw and elevation based on bearing/range; MortarRigController will physically rotate the pieces.
- Add MortarGame.Weapons.MortarController.
- Create a child pivot Transform (e.g., MortarPivot) and assign to mortarPivot.
- Create a child Transform (e.g., Muzzle) under the pivot and assign to muzzle.
- Optional: assign your main Camera to observeCamera for RMB zoom.

5) Projectile layer & damage targets
- Create a prefab for a static target using an empty GameObject with MortarGame.Targets.Target.
- Optionally add a mesh/renderer to visualize.

6) Enemies
- Create a prefab with MortarGame.Enemies.EnemyController.
- Give it a simple capsule/cube to visualize.
- EnemyManager will register spawned enemies via WaveManager.

7) Waves
- Add MortarGame.Waves.WaveManager to a GameObject.
- Assign leftLaneSpawn and rightLaneSpawn (empty Transforms placed along paths).
- Assign enemyPrefab and staticTargetPrefab.

8) HUD (UI)
- Create a Canvas and UI Texts for compassHeadingText, rangeText, ammoText, spotterText, streakText, impactText, suggestionText.
- Add MortarGame.UI.HUDController on the Canvas or a child and wire the Text references.

9) Quiz
- Add MortarGame.Quiz.QuizManager to a GameObject.
- At runtime, call NextQuestion() and SubmitAnswer(choice) from your UI (temporary buttons) to simulate the quiz loop.
  - Correct: +1 HE, streak increments, rewards at 3 (Smoke) and 5 (HE+).
  - Wrong: enemies gain +10% speed for 10 seconds and streak resets.

10) Playtesting
- Enter Play Mode.
- Use mouse to rotate azimuth; Up/Down to adjust range (Shift for fine), Q/E fine yaw; LMB to fire; RMB to observe; F toggles Spotter for next shot.
- Verify projectile flight time varies (1.2–2.4 s based on range), dispersion reduces with Spotter, smoke slows enemies 30% for 7s.
- Confirm WaveManager spawns squads and targets per timeline.

11) Notes
- SFX/VFX are placeholders: hook AudioSources and particle systems on fire/explosion.
- The movement/pathing and kill-cam are stubs; replace with NavMesh/path assets and camera effects later.
- Win/Lose evaluation on timeout is simplified; add priority target tracking to enforce the final conditions.