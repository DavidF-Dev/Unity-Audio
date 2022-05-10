# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
- Added Music scriptable object.
- Added Playback properties for 3D spatialisation.
- Added Playback property to ignore the audio listener.
- Added helper method to convert db to 0-1 range logarithmically.
- Replaced SoundEffect constructor with static method.
- Renamed Audio.cs to AudioHelper.cs.

## [1.0.3] - 2022-02-27
- Added JetBrain annotations.
- Replaced exceptions with Debug.LogError.

## [1.0.2] - 2022-01-30
- Fixed SoundEffect asset not setting default values when resetting.

## [1.0.1] - 2021-12-25
- Added DavidFDev folder to contain component and scriptable object in create menus.

## [1.0.0] - 2021-12-18
- Initial release.