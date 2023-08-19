namespace Subtitles;

// The Language File will overwrite the default assignments of each of these.
// The comments define the default assignments.
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
    HOSTILE,        // Not in sounds directory, manually assigned in the lang file.
    MENU,           // Not in sounds directory, manually assigned in the lang file.
    MUSIC,          // Not in sounds directory, determined by EnumSoundType.MUSIC
    PASSIVE,        // Not in sounds directory, manually assigned in the lang file.
    PLAYER,
    TOOL,
    VOICE,
    WALK,
    WEARABLE,
    WEATHER
}