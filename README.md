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
- Sounds fade out of Subtitles after 4 seconds.
  - Sounds of the same kind refresh the Subtitle.
- Automatically named sounds.
  - Sound Names are based on the file names and formatted better.

## Plans

Here are the plans...

- Subtitles lack directional arrows for pitch (up/level/down).
- Subtitles are derived directly from the file names.
  - Planning on adding a language file to pull names from, but this is the fallback system.
- Subtitles are colored based on their categories.
  - Planning to allow the player to toggle which categories are Subtitled.
  - Planning to allow the player to select which color represents each category.
- Subtitles currently are based on yaw.
  - If you hear something to the left and you sidestep past it, the Subtitle will still say it's left.
  - This is really not a big deal. Not sure if I want to change it to depend on position.

There are a lot of TODOs scattered around the code for things I want to add and do.
