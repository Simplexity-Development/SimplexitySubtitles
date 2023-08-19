# Simplexity Subtitles

Vintage Story .NET 7 Mod for Subtitles

> **Note**
> 
> This project is not forked from [vsmod-Subtitles](https://github.com/chriswa/vsmod-Subtitles)
but the foundation was based on this project.

## Features
<p align="center">
  <img src="images/Demo%20Subtitle.png">
</p>

- Left and Right Directional Subtitles
- Color Coded Categories
  - Unknown category currently is not subtitled.
- Subtitles fade out after 4 seconds from the last time you heard the sound.
- Automatically named sounds.
  - Sound Names that are not present in the Language File are based on the file names and formatted better.

## How To Format The Lang File

The language file lets you tell the mod what the [SoundType](https://github.com/Simplexity-Development/SimplexitySubtitles/blob/main/Subtitles/SoundType.cs)
of a sound is and what the subtitle should say.

- The sound's type determines the color.
- "Equal Sounds" refresh themselves, so you will only see one from those sounds.
  - A sound is considered equal if the type and name are the same.

The language file is formatted in json, for example,
the English file is `en.json` and comes with this mod under `assets/subtitles/lang/en.json`.

The JSON format will look something like this...

```json
{
  "subtitles:sounds/block/rock-break-pickaxe.ogg": "BLOCK$Mining Rock",
  "subtitles:sounds/block/rock-hit-pickaxe.ogg": "BLOCK$Mining Rock"
}
```

Since we want to support many kinds of sounds, the key is `subtitles:{PATH}`.

The value is `{TYPE}${DESCRIPTION}`, where you provide a valid sound type,
see [SoundType.cs](https://github.com/Simplexity-Development/SimplexitySubtitles/blob/main/Subtitles/SoundType.cs)
for all the valid sound types. These are not case sensitive but for the sake of simplicity, I put types in all caps.

So to add any entries to it, just simply add a new key with a given asset path.
Asset paths begin at the mod assets root folder.
So for example, the built-in mod `survival` has a sound `arrow-impact.ogg`
located at `${VINTAGE_STORY}/assets/survival/sounds/arrow-impact.ogg`.

The key you would use for that is `subtitles:sounds/arrow-impact.ogg`.

If a sound's key is missing from the lang file,
a name is automatically assigned and formatted based on the name of the sound file.

## Plans

Here are the plans...

- Subtitles lack directional arrows for pitch (up/level/down).
- Subtitles are colored based on their categories.
  - Planning to allow the player to toggle which categories are Subtitled.
  - Planning to allow the player to select which color represents each category.

There are a lot of TODOs scattered around the code for things I want to add and do.
