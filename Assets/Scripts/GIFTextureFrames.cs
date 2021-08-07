using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIFImport.Frames
{
    /// <summary>
    ///     Holds all <see cref="Texture2D"/> frames for a GIF sequence 
    /// </summary>
    public class GIFTextureFrames : ScriptableObject
    {
        public List<Texture2D> Frames = new List<Texture2D>();

        /// <summary>
        ///     Creates instance of scriptable object
        /// </summary>
        public static GIFTextureFrames CreateInstance() =>
            ScriptableObject.CreateInstance<GIFTextureFrames>();
    }
}
