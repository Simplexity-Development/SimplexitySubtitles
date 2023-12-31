using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Cairo;
using Vintagestory.API.MathTools;

namespace Subtitles;

public class SubtitleBox : HudElement
{
    private SubtitlesModSystem mod;
    public Subtitle subtitle;


    public SubtitleBox(ICoreClientAPI api, SubtitlesModSystem mod) : base(api)
    {
        this.mod = mod;

        ElementBounds bounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.RightBottom)
            .WithFixedPadding(10);
        SingleComposer = api.Gui.CreateCompo(SubtitlesModSystem.MOD_ID, bounds);

        ElementBounds innerBounds = ElementBounds.FixedSize(300, 450);
        bounds.WithChild(innerBounds);
        subtitle = new Subtitle(api, innerBounds);
        SingleComposer.AddInteractiveElement(subtitle, "subtitles");

        SingleComposer.Compose(false);
    }

    public void AddSound(Sound sound)
    {
        subtitle.AddSound(sound);
    }
}

public class Subtitle : GuiElement
{
    // TODO: Is 15 necessary? Does the max actually shown depend on UI Scale?
    private static readonly int MAX_SOUNDS = 15;
    private static readonly int MAX_LIFESPAN_SECONDS = 4;

    private LoadedTexture textTexture;
    private TextDrawUtil textDrawUtil;
    private CairoFont font;

    private List<Sound> sounds;

    public Subtitle(ICoreClientAPI api, ElementBounds bounds) : base(api, bounds)
    {
        sounds = new List<Sound>();
        textTexture = new LoadedTexture(api);
        font = CairoFont.WhiteSmallText();
        textDrawUtil = new TextDrawUtil();
    }

    public override void ComposeElements(Context context, ImageSurface surface)
    {
        font.SetupContext(context);
        Bounds.CalcWorldBounds();
        Recompose();
    }

    public override void RenderInteractiveElements(float deltaTime)
    {
        Update(deltaTime);
        Recompose();
        api.Render.Render2DTexturePremultipliedAlpha(textTexture.TextureId, Bounds.renderX, Bounds.renderY,
            Bounds.InnerWidth, Bounds.InnerHeight);
    }

    public void Recompose()
    {
        ImageSurface surface = new ImageSurface(Format.ARGB32, (int)Bounds.InnerWidth, (int)Bounds.InnerHeight);
        Context context = genContext(surface);
        DrawText(context);
        generateTexture(surface, ref textTexture);

        context.Dispose();
        surface.Dispose();
    }

    public void Update(float deltaTime)
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            sounds[i].age += deltaTime;
            if (sounds[i].age < MAX_LIFESPAN_SECONDS) continue;
            sounds.RemoveAt(i);
            i--;
        }
    }

    public void DrawText(Context context)
    {
        const double X = 0;
        const double WIDTH = 300;
        const double HEIGHT = 30;
        font.SetupContext(context);

        for (int i = 0; i < sounds.Count; i++)
        {
            if (sounds[i] == null)
            {
                sounds.RemoveAt(i);
                i--;
                continue;
            }

            double y = 30 * (MAX_SOUNDS - i - 1);

            Sound sound = sounds[i];
            if (sound.textWidth < 0) sound.textWidth = context.TextExtents(sound.name).Width;

            // sound.volume is always 0-1 so Math.Max() will always return 1.
            double brightness = (1 - sound.age / MAX_LIFESPAN_SECONDS) * Math.Max(1, sound.volume) / 2 + 0.5;

            context.SetSourceRGBA(0, 0, 0, 0.25 + brightness / 2);
            context.Rectangle(X, y, WIDTH, HEIGHT);
            context.Fill();

            context.SetSourceRGBA(sound.color.R, sound.color.G, sound.color.B, brightness);
            textDrawUtil.DrawTextLine(context, font, sound.name, 150 - sound.textWidth / 2, y + 2);

            if (sound.location == null) return;

            // TODO: Up (˄), Down (˅), Level (-) Sound Arrows
            DrawYawArrows(context, sound, y);
        }
    }
    
    public void DrawYawArrows(Context context, Sound sound, double y) {
        const double RIGHT_SIDE = 280;
        const double LEFT_SIDE = 10;
            
        // TODO: Make "right on top of you" distance configurable.
        Vec3d dirVec = sound.location - api.World.Player.Entity.Pos.XYZ;
        double dist = dirVec.Length();
        if (dist < 2) return;
            
        double soundYaw = Math.Atan2(dirVec.Z, dirVec.X);
        double playerYaw = api.World.Player.CameraYaw;
        double pi = GameMath.PI;
        double dir = GameMath.Mod((soundYaw + playerYaw) / GameMath.TWOPI * 12, 12);
            
        if (dir >= 2 && dir <= 4) textDrawUtil.DrawTextLine(context, font, "»", RIGHT_SIDE, y);
        else if (dir >= 1 && dir <= 5) textDrawUtil.DrawTextLine(context, font, "›", RIGHT_SIDE, y);
            
        if (dir >= 8 && dir <= 10) textDrawUtil.DrawTextLine(context, font, "«", LEFT_SIDE, y);
        else if (dir >= 7 && dir <= 11) textDrawUtil.DrawTextLine(context, font, "‹", LEFT_SIDE, y);
    }

    public void AddSound(Sound sound)
    {
        // TODO: Make configurable which sounds are to be hidden.
        if (sound.type == SoundType.UNKNOWN) return;
        
        foreach (Sound soundElement in sounds)
        {
            if (soundElement.Equals(sound))
            {
                soundElement.age = sound.age;
                soundElement.volume = sound.volume;
                soundElement.location = sound.location;
                return;
            }
        }

        if (sounds.Count >= MAX_SOUNDS) return;
        sounds.Add(sound);
    }

    public override void Dispose()
    {
        textTexture?.Dispose();
    }
}

public class Sound
{
    // TODO: Swap age with a decrementing feature so some sounds messages can optionally last longer.
    public double age;
    public readonly string name;
    public double textWidth;
    public Vec3d location;
    public double volume;
    public readonly SoundType type;
    public Color color;

    public Sound(string name, Vec3d location, double volume, SoundType type)
    {
        this.name = name;
        this.location = location;
        this.volume = volume;
        this.type = type;
        color = DetermineColor(this.type);

        // TODO: Let the music message last longer.
        if (this.type == SoundType.MUSIC) this.name = "♫ " + this.name + " ♫";

        age = 0;
        textWidth = -1;
    }

    public static Color DetermineColor(SoundType type)
    {
        // TODO: Make it so that these pull from a configuration option to determine which color for each category.
        switch (type)
        {
            case SoundType.UNKNOWN:
                return SoundColor.DARK_GRAY;
            case SoundType.AMBIENT:
                return SoundColor.GRAY;
            case SoundType.BLOCK:
                return SoundColor.GRAY;
            case SoundType.CREATURE:
                return SoundColor.YELLOW;
            case SoundType.EFFECT:
                return SoundColor.LIGHT_PURPLE;
            case SoundType.ENVIRONMENT:
                return SoundColor.WHITE;
            case SoundType.GLITCHED:
                return SoundColor.LIGHT_PURPLE;
            case SoundType.HELD:
                return SoundColor.GRAY;
            case SoundType.HOSTILE:
                return SoundColor.RED;
            case SoundType.MENU:
                return SoundColor.GRAY;
            case SoundType.MUSIC:
                return SoundColor.DARK_GRAY;
            case SoundType.PASSIVE:
                return SoundColor.GREEN;
            case SoundType.PLAYER:
                return SoundColor.WHITE;
            case SoundType.TOOL:
                return SoundColor.GRAY;
            case SoundType.VOICE:
                return SoundColor.AQUA;
            case SoundType.WALK:
                return SoundColor.GRAY;
            case SoundType.WARNING:
                return SoundColor.RED;
            case SoundType.WEARABLE:
                return SoundColor.WHITE;
            case SoundType.WEATHER:
                return SoundColor.GRAY;
            default:
                return SoundColor.DARK_GRAY;
        }
    }

    public override bool Equals(Object obj)
    {
        if (obj == null || !(GetType() == obj.GetType())) return false;
        Sound sound = (Sound) obj;
        return name == sound.name && type == sound.type;
    }

    public override int GetHashCode()
    {
        return name.GetHashCode() ^ type.GetHashCode();
    }
}

public class SoundColor
{
    // Look familiar?
    public static readonly Color BLACK = new Color(0, 0, 0);
    public static readonly Color DARK_BLUE = new Color(0, 0, (2 / 3.0));
    public static readonly Color DARK_GREEN = new Color(0, (2 / 3.0), 0);
    public static readonly Color DARK_AQUA = new Color(0, (2 / 3.0), (2 / 3.0));
    public static readonly Color DARK_RED = new Color((2 / 3.0), 0, 0);
    public static readonly Color DARK_PURPLE = new Color((2 / 3.0), 0, (2 / 3.0));
    public static readonly Color GOLD = new Color(1, (2 / 3.0), 0);
    public static readonly Color GRAY = new Color((2 / 3.0), (2 / 3.0), (2 / 3.0));
    public static readonly Color DARK_GRAY = new Color((1 / 3.0), (1 / 3.0), (1 / 3.0));
    public static readonly Color BLUE = new Color((1 / 3.0), (1 / 3.0), 1);
    public static readonly Color GREEN = new Color((1 / 3.0), (2 / 3.0), (1 / 3.0));
    public static readonly Color AQUA = new Color((1 / 3.0), 1, 1);
    public static readonly Color RED = new Color(1, (1 / 3.0), (1 / 3.0));
    public static readonly Color LIGHT_PURPLE = new Color(1, (1 / 3.0), 1);
    public static readonly Color YELLOW = new Color(1, 1, (1 / 3.0));
    public static readonly Color WHITE = new Color(1, 1, 1);
}