using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIFImport.Frames
{
    /// <summary>
    ///     Holds all sprite frames for a GIF sequence 
    /// </summary>
    public class GIFSpriteFrames : ScriptableObject
    {
        public List<Sprite> Frames = new List<Sprite>();

        /// <summary>
        ///     Creates instance of scriptable object
        /// </summary>
        public static GIFSpriteFrames CreateInstance() =>
            ScriptableObject.CreateInstance<GIFSpriteFrames>();
    }
}

