using System;

[Serializable]
public class GameRuntimeSettings
{
    // Currently selected resolution option index.
    public int resolutionIndex = 0;

    // Determines whether the game is running in fullscreen mode.
    public bool fullscreen = false;

    // Current master volume value.
    public float masterVolume = 1f;

    // Current sound effects volume value.
    public float sfxVolume = 1f;

    // Current speech / voice volume value.
    public float speechVolume = 1f;

    // Current music volume value.
    public float musicVolume = 1f;

    // Current mouse sensitivity offset.
    public float mouseSensitivity = 0f;

    // Determines whether vertical mouse movement is inverted.
    public bool invertY = false;

    // Current difficulty level index.
    public int difficultyIndex = 1;

    // Currently selected music track index.
    public int trackIndex = 0;
}