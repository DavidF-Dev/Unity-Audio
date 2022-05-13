# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
- Minor changes and bug fixes.

## [1.0.4] - 2022-05-13
- Added playable Music asset type.
- Added Spatial Audio Settings asset type for 3D spatialisation.
- Added options for 3D spatialisation.
- Added options to ignore the audio listener.
- Added Playback 'Finished' and 'Paused' event.
- Added SoundEffect and Music 'Played' event.
- Added helper method to convert db to 0-1 range logarithmically.
- Added editor context/menu actions for controlling playback during runtime.
- Renamed Audio class to AudioHelper.

## [1.0.3] - 2022-02-27
- Added JetBrain annotations.
- Replaced exceptions with Debug.LogError.

## [1.0.2] - 2022-01-30
- Fixed SoundEffect asset not setting default values when resetting.

## [1.0.1] - 2021-12-25
- Added DavidFDev folder to contain component and scriptable object in create menus.

## [1.0.0] - 2021-12-18
- Initial release.