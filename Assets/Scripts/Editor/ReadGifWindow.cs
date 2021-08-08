using UnityEngine;
using UnityEditor;
using ImageMagick;
using System.Linq;
using System.Collections.Generic;

namespace GIFImport.Editor
{

    public class ReadGifWindow : EditorWindow
    {
        Texture2D gifSprite = null;

        private bool previewTexturesSafe, showPreview;
        private Texture2D previewTex = null,
                          prevTexture = null;
        private List<Texture2D> previews = null;
        private MagickImage cacheImg = null;

        private string helpBoxText;
        private MessageType helpboxType;

        private GifReader reader = null;

        private GUIStyle tittleStyle = null;

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

            helpBoxText = "Select an image to uncompress";
            helpboxType = MessageType.Info;
            showPreview = false;

            tittleStyle = new GUIStyle();
            tittleStyle.fontSize = 20;
            tittleStyle.fontStyle = FontStyle.Bold;
            tittleStyle.normal.textColor = Color.black;
            tittleStyle.alignment = TextAnchor.MiddleCenter;

            reader = new GifReader(wnd:this);
        }

        private void OnDestroy() => cacheImg?.Dispose();

        void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("GIF Import for Unity", tittleStyle);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.PrefixLabel("Gif to import");
            gifSprite = (Texture2D)EditorGUILayout.ObjectField(gifSprite, typeof(Texture2D), true);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(helpBoxText, helpboxType);
            EditorGUILayout.Space();

            if (GUILayout.Button("Transform GIF"))
                ReadGifFile();

            bool imageChanged = prevTexture != gifSprite;

            if (gifSprite != null && imageChanged)
                UpdateAttributes();

            // OnGUI is called a lot of times!
            if (gifSprite != null)
            {
                EditorGUILayout.Space();
                showPreview = GUILayout.Toggle(showPreview, new GUIContent("Show preview?"));
                if (showPreview)
                {
                    if (previews == null)
                        UpdatePreview();

                    if (previews != null)
                        ConstructPreview();
                }
            }
        }

        /// <summary>
        ///     Show message to user
        /// </summary>
        public void Notify(string msg, double delay=2.0d) => 
            ShowNotification(new GUIContent(msg), delay);

        public void Log(string msg, MessageType t)
        {
            helpBoxText = msg; helpboxType = t;
        }

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
            showPreview = false;
            Log("Press the button to import the GIF", MessageType.Info);
        }

        private void UpdatePreview()
        {
            if (!reader.IsValidImage())
            {
                previews?.Clear();
                previews = null;
                Log("Cannot show preview. Image given is not a GIF", MessageType.Warning);
                return;
            }
            var frames = reader.ReadTimeline(5);
            previews = new List<Texture2D>();

            foreach (var img in frames)
            {
                img.Flop();
                previews.Add(MagickToTex2D(img, img.Width, img.Height));
            }
        }

        private void ConstructPreview()
        {
            Rect containerRect = new Rect(0, 211, position.width, position.height / 2);
            Rect imgContainer = new Rect();
            Vector2 auxPos, gridCenter;
            List<Vector2> offsets = new List<Vector2>();
            float xSpacing = 10, ySpacing = 5;
            const float framesPerRow = 3.0f, maxRows = 2.0f;

            EditorGUI.PrefixLabel(
                new Rect(10, 200, 100, 10), new GUIContent("Preview:")
            );

            imgContainer.width = (containerRect.width / framesPerRow) - xSpacing * (framesPerRow - 1);
            imgContainer.height = (containerRect.height / maxRows) - ySpacing;

            gridCenter.x = (imgContainer.width + xSpacing) * framesPerRow / 2f;
            gridCenter.y = containerRect.center.y;

            for (int r = 0; r < maxRows; r++)
            {
                for (int i = 0; i < framesPerRow; i++)
                {
                    auxPos = new Vector2(
                        i * (imgContainer.width + xSpacing),
                        r * (imgContainer.height + ySpacing)
                    );
                    offsets.Add((containerRect.position + auxPos) - gridCenter);
                }
            }

            int frame = 0;
            foreach (var offset in offsets)
            {
                imgContainer.position = containerRect.center + offset; 
                EditorGUI.DrawTextureTransparent(
                    imgContainer,
                    previews[frame], ScaleMode.ScaleAndCrop
                );

                frame = (frame + 1) % previews.Count;
            }
        }

        private void ReadGifFile()
        {
            string unpackFolder = EditorUtility.OpenFolderPanel("Unpack frames to...", "", ""),
                absoluteFolder = unpackFolder + "/", relativeFolder;

            if (gifSprite == null)
            {
                Log("Choose an image first!", MessageType.Error);
                Notify("Choose an image first!", 2);
                return;
            }

            if (unpackFolder.Length <= 0)
            {
                Notify("Canceled", 2);
                return;
            }

            relativeFolder = absoluteFolder.Substring(
                absoluteFolder.IndexOf("Assets")
            );

            reader.absoluteFolder = absoluteFolder;
            reader.relativeFolder = relativeFolder;
            reader.GifTexture = gifSprite;

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
