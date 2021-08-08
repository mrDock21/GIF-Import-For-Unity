# GIF Import for Unity

This simple editor script allows you to import a GIF file and convert it 
to a valid Unity sprite animation.

# Dependencies

The repo itself relies on [Magick.Net](https://github.com/dlemstra/Magick.NET) to open images, which is already included in the packages folder.

# Usage

In Unity, select Tools -> GIF Import

![GIF Import window](https://user-images.githubusercontent.com/43834462/128621201-fd65d535-89e7-4fb7-ae4c-5160d930bce1.PNG)

After an image (GIF) has been selected, you will have the following two options (only one really): either you can watch a quick preview of the frames or decompress (import) the GIF immediately.

Each preview frame will be imported with exactly the same import settings as the original image. So I advise you to be comfortable with your import settings before importing the GIF.

![GIF import settings](https://user-images.githubusercontent.com/43834462/128621202-01c21577-5f1d-4f9a-80a9-7a8caf463f1c.png)

After you finished admiring each frame, you can proceed to import the whole animation. To do that, press the "Import GIF" button.

To avoid messing with your project files, select or create a folder destination to decompress the GIF's frames.

![GIF frames folder](https://user-images.githubusercontent.com/43834462/128621204-b63e0f4b-6866-4c92-8a70-19a28d82f112.PNG)

After the whole process is done: you'll find an AnimatorController, an AnimationClip, and Frames (.asset) file. The animator controller is used to animate a 2d sprite (gameObject that uses SpriteRenderer).

![Animator controller and AnimationClip](https://user-images.githubusercontent.com/43834462/128621205-9a4c378f-c44e-4350-9e57-c0c584c840ee.PNG)

The frames (asset) file contains a list of all the imported GIF frames in case you want to use them for something else.

![Frames .asset file](https://user-images.githubusercontent.com/43834462/128621206-b92ca1e3-6110-49aa-91c0-b5536f5832a3.PNG)

# Limitations
The AnimationClip only animates a gameObject that uses a SpriteRenderer component (so no UI for you, sir). UI support will be added in future updates. *Hint* You can workaround this (for now) by using the frames (asset) file.