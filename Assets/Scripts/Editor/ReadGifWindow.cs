using UnityEngine;
using UnityEditor;
using ImageMagick;
using System.Linq;

namespace GIFImport.Editor
{

    public class ReadGifWindow : EditorWindow
    {
        Texture2D gifSprite = null;

        private Texture2D previewTex = null,
                          prevTexture = null;
        private MagickImage cacheImg = null;
        private TextureImporterType GifImportType;

        private GifReader reader = null;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Tools/GIF Import")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            ReadGifWindow window = 
                (ReadGifWindow)EditorWindow.GetWindow(typeof(ReadGifWindow));

            window.Initialize();
            window.Show();
        }

        private void Initialize()
        {
            previewTex = prevTexture = null;
            cacheImg = null;

            reader = new GifReader(wnd:this);
        }

        private void OnDestroy() => cacheImg?.Dispose();

        void OnGUI()
        {
            var style = new GUIStyle();
            style.fontSize = 20;
            style.fontStyle = FontStyle.Bold;
            
            EditorGUILayout.PrefixLabel("GIF Import for Unity", style);
            EditorGUILayout.PrefixLabel("Gif to import");
            gifSprite = (Texture2D)EditorGUILayout.ObjectField(gifSprite, typeof(Texture2D), true);

            EditorGUILayout.HelpBox("Helpbox", MessageType.Info);

            if (GUILayout.Button("Transform GIF"))
                ReadGifFile();

            if (prevTexture != gifSprite)
                UpdateAttributes();

            // OnGUI is called a lot of times!
            if (gifSprite != null)
            {
                if (previewTex == null)
                    UpdatePreview();

                EditorGUI.PrefixLabel(
                    new Rect(10, 200, 100, 10), new GUIContent("Preview:")
                );
                EditorGUI.DrawTextureTransparent(
                    new Rect(0, 211, position.width, position.height / 2), 
                    previewTex, ScaleMode.ScaleAndCrop
                );
            }
        }

        /// <summary>
        ///     Show message to user
        /// </summary>
        public void Notify(string msg, double delay=2.0d) => 
            ShowNotification(new GUIContent(msg), delay);

        private void UpdateAttributes()
        {
            string path = AssetDatabase.GetAssetPath(gifSprite),
                   appPath = Application.dataPath;
            int fileNameLength = (gifSprite.name + gifSprite.format.ToString()).Length;

            // get absolute path of image (and removes repeated "Assets")
            reader.absolutePathToGif = appPath.Substring(0, appPath.Length - 6) + path;
            // get relative folder of gif
            reader.relativeFolder = path.Substring(0, path.Length - fileNameLength);

            prevTexture = gifSprite;
            previewTex = null;
            Debug.Log("Path => " + reader.absolutePathToGif);
        }

        private void UpdatePreview()
        {
            try
            {
                // free older image
                cacheImg?.Dispose();
                // read the new one
                cacheImg = new MagickImage(reader.absolutePathToGif);
                // for some reason, it's flipped
                cacheImg.Flop();
                // get new texture2D
                previewTex = MagickToTex2D(cacheImg, cacheImg.Width, cacheImg.Height);

            }
            catch (MagickException e)
            {
                ShowNotification(new GUIContent(e.Message));
            }
        }

        private void ReadGifFile()
        {
            string unpackFolder = EditorUtility.OpenFolderPanel("Unpack frames to...", "", ""),
                absoluteFolder = unpackFolder + "/", relativeFolder;

            if (gifSprite == null)
            {
                ShowNotification(new GUIContent("Choose an image first!"), 2);
                return;
            }

            if (unpackFolder.Length <= 0)
            {
                ShowNotification(new GUIContent("Canceled"), 2);
                return;
            }

            relativeFolder = absoluteFolder.Substring(
                absoluteFolder.IndexOf("Assets")
            );

            reader.absoluteFolder = absoluteFolder;
            reader.relativeFolder = relativeFolder;
            reader.GifTexture = gifSprite;

            Debug.Log("Absolute folder path => " + absoluteFolder);
            Debug.Log("Folder path => " + relativeFolder);

            reader.ReadGifFile();
        }

        private Texture2D MagickToTex2D(MagickImage img, int width, int height)
        {
            Color[] colors = new Color[width * height];
            Color aux = new Color();
            int pixel = 0;
            float maxValue = (float)ushort.MaxValue;
            //Copy the new texture
            Texture2D tex = new Texture2D(
                width, height, TextureFormat.RGBA32, false
            );

            // returns pixels from finish=>start, 
            //  we reverse that to get start=>finish
            foreach (var pxl in img.GetPixels().Reverse())
            {
                var color = pxl.ToColor();
                aux.r = (float)color.R;
                aux.g = (float)color.G;
                aux.b = (float)color.B;
                aux.a = (float)color.A;
                colors[pixel++] =
                    new Color(aux.r / maxValue, aux.g / maxValue, aux.b / maxValue, aux.a / maxValue);
            }
            tex.SetPixels(colors, 0);
            tex.Apply();

            return tex;
        }
    }
}
