using System;
using System.Globalization;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using HarmonyLib;
using Vintagestory.Client.NoObf;
using Vintagestory.API.MathTools;
using System.Text.RegularExpressions;
using Vintagestory.API.Config;

[assembly: ModInfo("SimplexityDev.Subtitles")]

namespace Subtitles;

public class SubtitlesModSystem : ModSystem
{
    public static readonly string MOD_ID = "SimplexityDev.Subtitles";
    public static readonly string REGEX_SOUND_CATEGORY = "sounds\\/([^\\/]*)\\/(?:(?:.*)\\/)*[^\\/]*\\.ogg";
    public static readonly string REGEX_SOUND_NAME = ".*\\/([^\\/]*?)\\d*\\.ogg";
    public static TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
    
    public ICoreClientAPI api;
    public Harmony harmony;
    public SubtitleBox subtitleBox;

    public override bool AllowRuntimeReload => true;

    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return forSide.IsClient();
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        this.api = api;
        base.StartClientSide(api);

        api.Logger.Debug("Subtitles: Applying Harmony patches...");
        harmony = new Harmony(MOD_ID);
        harmony.PatchAll();
        api.Logger.Debug("Subtitles: Finished applying patches!");

        Patch_ClientPlatformWindows_AudioListener.api = api;
        Patch_ClientPlatformWindows_AudioListener.mod = this;

        api.Event.IsPlayerReady += (ref EnumHandling handling) =>
        {
            subtitleBox = new SubtitleBox(api, this);
            subtitleBox.TryOpen();
            return true;
        };
    }

    public void ProcessSound(SoundParams sound, AudioData data)
    {
        if (subtitleBox == null) return;
        if (sound.Volume == 0) return;

        IClientPlayer player = api.World.Player;
        if (player == null) return;
        Vec3f soundPos3F = sound.Position;

        SoundType type = DetermineSoundType(sound);
        string soundName = DetermineSoundName(sound);
        
        if (soundPos3F == null)
        {
            subtitleBox.AddSound(new Sound(soundName, null, sound.Volume, type));
            return;
        }
        Vec3d soundPos = soundPos3F.ToVec3d();
        if ((player.Entity.Pos.XYZ - soundPos).Length() > sound.Range) return;
        
        subtitleBox.AddSound(new Sound(soundName, soundPos, sound.Volume, type));
    }

    public SoundType DetermineSoundType(SoundParams sound)
    {
        string lang = GetLang(sound);
        if (lang == null) return DetermineSoundTypeFallback(sound);
        int specialCharIndex = lang.IndexOf('$');
        string typeSubstring = specialCharIndex == -1 ? null : lang.Substring(0, specialCharIndex);
        SoundType type;
        if (typeSubstring == null || SoundType.TryParse(typeSubstring, true, out type)) return DetermineSoundTypeFallback(sound);
        return type;
    }

    public SoundType DetermineSoundTypeFallback(SoundParams sound)
    {
        // TODO: Maybe we do fancy things with these types?
        EnumSoundType enumSoundType = sound.SoundType;
        if (enumSoundType == EnumSoundType.Music) return SoundType.MUSIC;
        if (enumSoundType == EnumSoundType.Ambient) return SoundType.AMBIENT;
        if (enumSoundType == EnumSoundType.Weather) return SoundType.WEATHER;
        if (enumSoundType == EnumSoundType.AmbientGlitchunaffected ||
            enumSoundType == EnumSoundType.SoundGlitchunaffected) return SoundType.GLITCHED;
            
        string soundPath = sound.Location.Path;
        Match m = Regex.Match(soundPath, REGEX_SOUND_CATEGORY, RegexOptions.Singleline);
        if (!m.Success) return SoundType.UNKNOWN;
        SoundType type;
        if (SoundType.TryParse(m.Groups[1].Value, true, out type)) return type;
        return SoundType.UNKNOWN;
    }

    public string DetermineSoundName(SoundParams sound)
    {
        string lang = GetLang(sound);
        if (lang == null) return DetermineSoundNameFallback(sound);
        // TODO: Utilize a lang file for names of sounds via key. I just wanted a fallback made first.
        int specialCharIndex = lang.IndexOf('$');
        if (specialCharIndex == -1) return lang;
        // THREE$FOUR
        return lang.Substring(specialCharIndex + 1, lang.Length - specialCharIndex - 1);
    }

    public string DetermineSoundNameFallback(SoundParams sound)
    {
        Match m = Regex.Match(sound.Location.Path, REGEX_SOUND_NAME, RegexOptions.Singleline);
        if (!m.Success) return "Unknown Sound";
        string name = m.Groups[1].Value;
        name = name.Replace("-", " ");
        name = name.Replace("_", " ");
        name = textInfo.ToTitleCase(name);
        return name;
    }

    public string GetLang(SoundParams sound)
    {
        string lang = Lang.GetIfExists("subtitles:" + sound.Location.Path);
        if (sound.Location.Path == lang) return null;
        return lang;
    }

    public override void Dispose()
    {
        harmony.UnpatchAll(MOD_ID);
    }
}

[HarmonyPatch(typeof(ClientPlatformWindows), nameof(ClientPlatformWindows.CreateAudio))]
public class Patch_ClientPlatformWindows_AudioListener
{
    public static ICoreClientAPI api;
    public static SubtitlesModSystem mod;

    public static void Postfix(ClientPlatformWindows __instance, ILoadedSound __result, SoundParams sound,
        AudioData data)
    {
        if (__result == null) return;
        mod.ProcessSound(sound, data);
    }
}