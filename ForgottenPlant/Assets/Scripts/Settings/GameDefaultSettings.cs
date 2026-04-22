using System;

[Serializable]
public class GameDefaultSettings
{
    // Default resolution option index used at first startup or after reset.
    public int resolutionIndex = 0;

    // Determines whether the game starts in fullscreen mode by default.
    public bool fullscreen = false;

    // Default master volume value.
    public float masterVolume = 1f;

    // Default sound effects volume value.
    public float sfxVolume = 0f;

    // Default speech / voice volume value.
    public float speechVolume = 0f;

    // Default music volume value.
    public float musicVolume = 1f;

    // Default mouse sensitivity offset.
    public float mouseSensitivity = 0f;

    // Determines whether vertical mouse movement is inverted by default.
    public bool invertY = false;

    // Default difficulty level index.
    public int difficultyIndex = 1;

    // Default music track index.
    public int trackIndex = 0;
}