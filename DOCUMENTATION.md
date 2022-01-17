# Documentation
It is recommended to view this file in a markdown viewer.
View on [GitHub](https://github.com/DavidF-Dev/Unity-Audio/blob/main/DOCUMENTATION.md).

## Usage
No setup is required. To access the scripts, include the ``DavidFDev.Audio`` namespace at the top of your file. The core functionality of the asset is contained in the ``Audio`` static class, which exposes methods for playing audio clips, sound effects & music.
- ``Play(AudioClip, Position [optional], AudioMixerGroup [optional])``: play the provided audio clip.
- ``Play(Path, Position [optional], AudioMixerGroup[optional])``: play an audio clip loaded from the Resources folder.
- ``PlaySfx(SoundEffect, Position [optional])``: play the provided sound effect.
- ``PlaySfx(Path, Position [optional])``: play a sound effect loaded from the Resources folder.
- ``PlayMusic(AudioClip, FadeIn, FadeOut)``: play the provided music track.
- ``PlayMusic(Path, FadeIn, FadeOut)``: play a music track loaded from the Resources folder.

</br>Calling ``Play()`` or ``PlaySfx()`` will return an instance of Playback which allows control of the Audio Source indirectly.
E.g.
```cs
Playback playback = Audio.PlaySfx(sfx);
playback.Pause();
playback.Volume = 0.4f;
playback.Loop = true;
```

</br>Sound effects are scriptable objects that can be added to your Assets. They are used for setting up specific types of sounds. An instance can be played by calling the ``Play()`` method or by passing it in to ``Audio.PlaySfx()``.

### Examples
```cs
Audio.Play("Audio/death_scream", enemy.position);
```
This call will load the `death_scream` Audio Clip from the Resources folder and play it at the enemy's position.
</br></br>
```cs
Audio.PlayMusic(music, fadeIn: 1f, fadeOut: 0.75f);
Debug.Log($"Current music track: {Audio.CurrentMusic.name}");
```
Playing a music track will crossfade the new track with the old track, if one was already playing. The currently playing music track can be queried through ``Audio.CurrentMusic``.
</br></br>
```cs
Playback playback = Audio.Play(clip, position: new Vector3(0f, 1f, 0f), output: output);
playback.ForceFinish();
```
An Audio Mixer Group can be provided to control where the audio signal is routed. Also, a sound can be stopped prematurely by calling ``ForceFinish()`` on the Playback instance. Note that - unlike with Audio Sources - Playback instances cannot be replayed once the audio has finished playing.
