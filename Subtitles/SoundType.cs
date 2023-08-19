namespace Subtitles;

public enum SoundType
{
    UNKNOWN,        // Not in sounds directory, fallback Enum.
    AMBIENT,        // Not in sounds directory, determined by EnumSoundType.AMBIENT
    BLOCK,
    CREATURE,
    EFFECT,
    ENVIRONMENT,
    GLITCHED,       // Not in sounds directory, determined by EnumSoundType.AmbientGlitchunaffected & EnumSoundType.SoundGlitchunaffected
    HELD,
    MUSIC,          // Not in sounds directory, determined by EnumSoundType.MUSIC
    PLAYER,
    TOOL,
    VOICE,
    WALK,
    WEARABLE,
    WEATHER
}