using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ImageMagick;
using GIFImport.Frames;
using System.Linq;

namespace GIFImport.Editor
{
    /// <summary>
    ///     Handles GIF file import
    /// </summary>
    public class GifReader
    {
        public Texture2D GifTexture = null;
        public TextureImporterType GifImportType = TextureImporterType.Default;

        public string absolutePathToGif, relativePathToGif;
        public string absoluteFolder, relativeFolder;

        private ReadGifWindow window = null;

        public GifReader(ReadGifWindow wnd) { window = wnd; }

        public void ReadGifFile()
        {
            MagickImageInfo info = null;
            // Read from file.
            try
            {
                // Check if GIF first...
                info = new MagickImageInfo(absolutePathToGif);
            }
            catch (MagickException)
            {
                Debug.LogError("Couldn't open image");
            }

            if (info.Format != MagickFormat.Gif)
            {
                window.Notify("Given image is not a GIF!");
                return;
            }
            relativePathToGif = AssetDatabase.GetAssetPath(GifTexture);
            // Write all frames to gif relative folder
            WriteFrames(
                // get frames
                new MagickImageCollection(absolutePathToGif)
            );
        }

        private void WriteFrames(MagickImageCollection frames)
        {
            // Write all frames to folder
            int frameNum = 0;
            string imgFramePath, frameName;
            string[] framesPaths = new string[frames.Count];
            // each frame will be named with "imgName_N.jpg"
            string filenameFormat = GifTexture.name + "_{0}.jpg";

            foreach (var frame in frames)
            {
                frameName = string.Format(filenameFormat, frameNum);
                framesPaths[frameNum] = relativeFolder + frameName;
                imgFramePath = absoluteFolder + frameName;

                // write it to disk
                frame.Write(imgFramePath);
                // tell Unity to import it
                AssetDatabase.ImportAsset(relativeFolder + frameName);
                frameNum++;
            }
            // free all frames
            frames.Dispose();
            // copy import settings of original gif file
            CopyImportSettings(framesPaths);
            // save GIFFrames .asset file
            SaveFrameAsset(framesPaths);
        }

        private void SaveFrameAsset(string[] framesPaths)
        {
            GifAnimCreator anim;
            ScriptableObject asset;

            if (GifImportType == TextureImporterType.Sprite)
            {
                asset = SaveSpriteFrameAsset(framesPaths);
                anim = new GifAnimCreator(
                    asset as GIFSpriteFrames, GifTexture.name + "_clip.anim"
                );
                anim.CreateAnimationClip(relativeFolder);
                Debug.Log("Animation clip has been created!");
            }
            else
                SaveTexture2DFrameAsset(framesPaths);
        }

        private ScriptableObject SaveSpriteFrameAsset(string[] framesPaths)
        {
            var instance = GIFSpriteFrames.CreateInstance();

            instance.name = GifTexture.name + "_Frames.asset";

            foreach (string framePath in framesPaths)
            {
                var objects = AssetDatabase.LoadAllAssetsAtPath(framePath);
                var sprites = objects.Where(q => q is Sprite).Cast<Sprite>();
                // there's only one sprite per image
                instance.Frames.Add(sprites.ElementAt(0));
            }
            // save .asset file
            AssetDatabase.CreateAsset(instance, relativeFolder + instance.name);

            Debug.Log("Frames .asset has been created!");

            return instance;
        }

        private ScriptableObject SaveTexture2DFrameAsset(string[] framesPaths)
        {
            var instance = GIFTextureFrames.CreateInstance();

            instance.name = GifTexture.name + "_Frames.asset";

            foreach (string framePath in framesPaths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(framePath);
                // there's only one sprite per image
                instance.Frames.Add(asset);
            }
            // save .asset file
            AssetDatabase.CreateAsset(instance, relativeFolder + instance.name);

            Debug.Log("Frames .asset has been created!");
            return instance;
        }

        private void CopyImportSettings(string[] framesPaths)
        {
            TextureImporterSettings baseSettings = new TextureImporterSettings();
            TextureImporter baseImporter =
                (TextureImporter)TextureImporter.GetAtPath(relativePathToGif);

            // move data from baseImporter => baseSettings
            baseImporter.ReadTextureSettings(baseSettings);

            GifImportType = baseSettings.textureType;

            foreach (var framePath in framesPaths)
                ReimportImage(framePath, baseSettings);

            Debug.Log("All GIF frames have been imported!");
        }

        private void ReimportImage(string imgPath, TextureImporterSettings baseSettings)
        {
            TextureImporter importer =
                (TextureImporter)TextureImporter.GetAtPath(imgPath);

            // assigns and reads from baseSettings => importer
            importer.SetTextureSettings(baseSettings);

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }
}

