#GIF Import for Unity

This simple editor script allows you to import a GIF file and convert it 
to a valid Unity sprite animation.

#Dependencies

The repo itself relies on [Magick.Net](https://github.com/dlemstra/Magick.NET) to open images, which is already included in the packages folder.

#Usage

In Unity, select Tools -> GIF Import

// image with the window

After an image's (GIF) been selected, you will have the following two options (only one really): you can watch a quick preview of the frames, or decompress (import) the GIF immediately.

Each preview frame will be imported with exactly the same import settings as the original image. So I advise you to be comfortable with your import settings before importing the GIF.

// image with import settings

After you're done admiring each frame, you can proceed to import the whole animation. To do that, press the "Import GIF" button.

// button highlight

To avoid making a mess with your project files, select/create a special folder destination to decompress the GIF's frames into.

After the whole process is done: you'll find an AnimatorController, AnimationClip, and Frames.asset file. The animator controller can be used to animate a 2d sprite (gameObject that uses SpriteRenderer). 

The Frames .asset file contains a list of all the GIF frames, in case you want to use them for something else.

// asset image

#Limitations
The AnimationClip only animates a gameObject that uses a SpriteRenderer component (No UI). UI support will be added in future updates.