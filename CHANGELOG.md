# Changelog

## [0.11.0] - 2026-11-08

### Added

- Components: avatar components that are processed and removed at build time with NDMF. They are always available and are not switched on in the settings window.
- PAT Animation Helper: build radial puppet and toggle menus that move transforms and blend shapes from entered values, with timed transitions on toggle and an optional reverse transition.
- PAT Face Keeper: at build time, raise face blend shape keys in every animation to the current values, including expressions added by FaceEmo or Modular Avatar. List the affected expressions along the menu and preview them before and after.
- PAT Bone Weight Transfer: at build time, move bone weights to another bone on a copy of the mesh without writing a mesh asset.

### Deprecated

- Blendshape Keeper is deprecated in favor of PAT Face Keeper. It still works.
- Bone Weight Collapser is deprecated in favor of PAT Bone Weight Transfer. It still works.

## [0.10.0] - 2026-11-05

Renamed from Candy Box to Poyo's Avatar Toolbox. The tools work the same way.

### Changed

- Renamed the package. The display name is "Poyo's Avatar Toolbox", the package ID is `net.maaaaa.avatar-toolbox`, and the settings window opens from `Tools > Poyo's Avatar Toolbox`. The repository moved to [Poyotoron/Avatar-Toolbox](https://github.com/Poyotoron/Avatar-Toolbox) and the documentation to [https://ghp.maaaaa.net/Avatar-Toolbox/](https://ghp.maaaaa.net/Avatar-Toolbox/).
- Candy Box is removed automatically. `legacyPackages` lists the old ID, so VCC / ALCOM removes Candy Box when this version is added. While both are in the same project, the Console shows a warning once asking you to remove Candy Box.
- Tools enabled in Candy Box stay enabled. The scripting define symbols `CANDY_BOX_TOOL_NN` are replaced with `AVATAR_TOOLBOX_TOOL_NN` in the same place.
- The tool data folder moved from `Assets/zzz_pytr/CandyBox/` to `Assets/zzz_pytr/Avatar-Toolbox/`. The folder is moved with its GUIDs, so blendshape backups saved by Candy Box keep working. If both folders exist, nothing is moved and the Console shows a warning.
- Blendshape Backuper saves to `Assets/zzz_pytr/Avatar-Toolbox/BlendShapeBackups` by default.
- Reworded text in the windows, for example "シェイプキー" is now "ブレンドシェイプ" and "めり込みチェック" is now "めり込みの確認".

## [0.9.0] - 2026-10-20

### Added

- FaceEmo Splitter: show the prefab variant tree, label variants and apply FaceEmo expressions per label so a variant's expressions no longer overwrite its base or siblings.
- FaceEmo Splitter: save the labels, stars and options to a JSON file in the project, matched to variants by asset GUID.
- FaceEmo Splitter: keep the install target and enabled state of the MA Menu Installer across applies.
- Parts Set Adjuster: choose the body part to fit against, so hats, tails, wings and bracelets can be fitted as well as hair.
- Parts Set Adjuster: move only part of an outfit by bone or by mesh, with a list of meshes that move together.

### Changed

- Hair Set Adjuster is renamed to Parts Set Adjuster. The tool ID and its enabled state are kept.
- Parts Set Adjuster: automatic fitting is available only when the part type is hair.

## [0.8.1] - 2026-10-05

### Added

- Hair Set Adjuster: automatic fitting of hair position and scale, starting from the head bone and reducing penetration, exposed scalp and floating hair, with an option to keep the scale.
- Hair Set Adjuster: the face tint now also shows the parts hidden behind the hair, semi-transparent.
- Hair Set Adjuster: penetrating hair faces are shown as filled triangles, selectable with points.

### Changed

- Hair Set Adjuster: sliders now recenter on the current value after each drag, with three selectable widths and a number field independent of the slider range.

### Removed

- Hair Set Adjuster: "snap to head bone" and the penetration-based position search, replaced by automatic fitting.

## [0.8.0] - 2026-10-04

### Added

- Hair Set Adjuster: fit hair made for another avatar with position, rotation and scale sliders, snap to the head bone, and search for a position with less penetration.
- Scene view face tint and penetration checks to see where hair intersects the head.
- Migration of hair PhysBone colliders to equivalent avatar colliders, with optional removal of unused ones.

### Changed

- Hair Tone Matcher: the statistics method now estimates gamma to match contrast and limits it when highlights would clip.
- AAO Merge Bone Helper: bones animated through the avatar descriptor or other components are now detected.

### Fixed

- Blendshape Keeper: the preview no longer sinks the body into the outfit, and values from the previously shown clip no longer remain.
- AAO Merge PhysBone Helper: the chain length warning is no longer shown for properties without curves.

## [0.7.0] - 2026-08-25

### Added

- BlendShape Backuper: save the blend shape weights of a skinned mesh as an asset and restore them later by shape name.
- Comparison of a backup against the current weights, grouped into changed, unchanged, backup-only and mesh-only shapes.
- Temporary preview of the selected weights before restoring them.

### Changed

- Disable the tool toggles and the apply and discard buttons while scripts are compiling.

## [0.6.1] - 2026-08-18

### Added

- Multiple collapse rules in Bone Weight Collapser, each with its own source bones, destination bone and blend ratio, applied in order in a single run.
- Duplicating and reordering rules.

### Changed

- Meshes are collected as soon as a target object is set, without pressing a button.
- One mesh asset is written per renderer regardless of how many rules are applied.

## [0.6.0] - 2026-08-18

### Added

- Bone Weight Collapser to move bone weights from selected bones onto another bone.
- Source bones can be listed directly or collected from the descendants of a bone.
- A dry run that reports affected vertices and the total weight to be moved, per mesh.
- A blend ratio for moving only part of the weight.
- Baked meshes are saved as new assets, leaving the original mesh untouched, and the renderer can be reverted to its previous mesh.

## [0.5.1] - 2026-08-16

### Added

- Toggles that turn the applied tone adjustment and each copied material setting on and off after applying, with direct editing of their values.
- A before/after texture preview of the pending edit.
- Re-baking of the generated texture from the edited values.

### Changed

- The default output creates a duplicated material and bakes the adjustment into a new texture, so the match survives a shader fallback.
- Copied material settings are grouped by feature, and value columns are labelled.
- Windows open at a larger size, and the settings window keeps its actions visible.

### Fixed

- The applied region now covers every renderer that shares the material, so the whole hair is adjusted.
- Semi-transparent strands are no longer left at their original color when baking.

## [0.5.0] - 2026-08-16

### Added

- Hair Tone Matcher to suggest color adjustments that bring replacement hair closer to the original hair.
- Material-based tone adjustment with a choice between creating and assigning a duplicate or overwriting the destination material.
- Statistical matching and point sampling from two textures.
- Selective copying of compatible material settings from the source hair.
- Region masks for excluding areas that should keep their original color.
- Support for lilToon and Poiyomi materials.
- Batch matching for multiple renderers and material slots, with adjustments calculated separately for each target material.
- An output mode that creates a newly adjusted main texture without changing the original texture.

### Changed

- Compatible material settings are grouped by feature and can be filtered by name before copying.

## [0.4.0] - 2026-08-10

### Added

- Helper for AAO Merge Bone to manage bone merge settings from a hierarchy tree.
- Chain thinning controls to select bones for merging at a fixed interval.

## [0.3.0] - 2026-08-10

### Added

- Helper for AAO Merge PhysBone to compare source values and suggest override values from minimum, maximum, mean, median, or mode.
- Curve-aware suggestions based on the effective value of each source PhysBone.

## [0.2.0] - 2026-08-10

### Added
- Blendshape Keeper: preview the original and modified animations side by side or by switching views.
- Optional dependency support for tools that integrate with external packages.
- Helper for MA Blendshape Sync to configure outfit blendshape bindings in bulk.

### Changed
- Blendshape Keeper: choose between overwriting source animations and saving modified copies.

## [0.1.0] - 2026-08-08

### Added
- Settings window at Tools > Candy Box to enable or disable each bundled tool.
- Disabled tools are excluded from compilation entirely.
- Blendshape Keeper: raise animation keys below the current blendshape weight.
