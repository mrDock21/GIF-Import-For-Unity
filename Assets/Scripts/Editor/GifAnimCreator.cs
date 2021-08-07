using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GIFImport.Frames;

namespace GIFImport.Editor
{
    /// <summary>
    ///     Creates an animator controller which animates a <see cref="SpriteRenderer"/>
    /// </summary>
    public class GifAnimCreator
    {
        public float FrameSpacing;
        public GIFSpriteFrames GifFrames;
        public bool IsLoop;
        private string name;

        /// <summary>
        ///     Creates a new animation clip and a new controller with given frames
        /// </summary>
        /// <param name="gifFrames">        Gif frames                   </param>
        /// <param name="name">             The name of the animation    </param>
        /// <param name="frameSpacing">    Seconds between each frame   </param>
        public GifAnimCreator(GIFSpriteFrames gifFrames, string name, float frameSpacing=0.1f)
        {
            FrameSpacing = frameSpacing;
            GifFrames = gifFrames;
            this.name = name;
            IsLoop = true;
        }

        /// <summary>
        ///     Creates <see cref="AnimationClip"/> for sprite animation
        /// </summary>
        /// <param name="path"> Where to save the anim  (relative to the project path)</param>
        public void CreateAnimationClip(string path)
        {
            AnimationClip animClip = new AnimationClip();
            // First you need to create e Editor Curve Binding
            EditorCurveBinding curveBinding = new EditorCurveBinding();

            // I want to change the sprites of the sprite renderer, so I put the typeof(SpriteRenderer) as the binding type.
            curveBinding.type = typeof(SpriteRenderer);
            // Regular path to the gameobject that will be changed (empty string means root)
            curveBinding.path = "";
            // This is the property name to change the sprite of a sprite renderer
            curveBinding.propertyName = "m_Sprite";

            // An array to hold the object keyframes
            var keyFrames = new ObjectReferenceKeyframe[GifFrames.Frames.Count];
            for (int i = 0; i < keyFrames.Length; i++)
            {
                keyFrames[i] = new ObjectReferenceKeyframe();
                // set the time
                keyFrames[i].time = i * FrameSpacing;
                // set reference for the sprite you want
                keyFrames[i].value = GifFrames.Frames[i];
            }
            animClip.name = name;
            animClip.wrapMode = IsLoop ? WrapMode.Loop : WrapMode.PingPong;
            AnimationUtility.SetObjectReferenceCurve(animClip, curveBinding, keyFrames);
            AssetDatabase.CreateAsset(animClip, path + animClip.name);

            CreateAnimController(path, name, animClip);
        }

        private void CreateAnimController(string path, string name, AnimationClip clip)
        {
            // Creates the controller
            var controller =
                UnityEditor.Animations.AnimatorController
                .CreateAnimatorControllerAtPath(path + name + ".controller");

            // Add State
            controller.AddMotion(clip, 0);
        }
    }
}
