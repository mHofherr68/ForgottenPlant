using System;

[Serializable]
public class GameDefaultSettings
{
    public int resolutionIndex = 0;
    public bool fullscreen = false;

    public float masterVolume = 1f;
    public float sfxVolume = 1f;
    public float speechVolume = 1f;
    public float musicVolume = 1f;

    public float mouseSensitivity = 0f;
    public bool invertY = false;

    public int difficultyIndex = 1;
    public int trackIndex = 0;
}
