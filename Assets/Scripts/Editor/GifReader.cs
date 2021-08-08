using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ImageMagick;
using GIFImport.Frames;
using System.Linq;
using System.Threading.Tasks;

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

        public bool IsValidImage()
        {
            MagickImageInfo info;
            try
            {
                // Check if GIF first...
                info = new MagickImageInfo(absolutePathToGif);
            }
            catch (MagickException)
            {
                window.Log("Couldn't open image, normally this happens with Unity's internal images", MessageType.Error);
                return false;
            }

            return info.Format == MagickFormat.Gif;
        }

        public async Task<List<MagickImage>> ReadTimeline(int numFrames)
        {
            List<MagickImage> res = new List<MagickImage>();
            MagickImageCollection frames;
            int spacing;

            frames = new MagickImageCollection(absolutePathToGif);
            spacing = frames.Count / numFrames;

            if (spacing == 0)
                spacing = 1;

            for (int i = 0; i < frames.Count; i += spacing)
            {
                res.Add( new MagickImage(frames.ElementAt(i)) );
            }

            frames.Dispose();

            return res;
        }

        public void ReadGifFile()
        {
            if (!IsValidImage())
            {
                window.Log("Given image is not a GIF!", MessageType.Error);
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

