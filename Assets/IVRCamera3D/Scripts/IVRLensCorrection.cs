/************************************************************************************

Filename    :   IVRLensCorrection.cs
Content     :   Full screen image effect. 
				This script is used to add full-screen lens correction on a camera
				component
Created     :   January 17, 2016
Authors     :   Xinjie Wang

Copyright   :   Copyright 2016 Immersion Co., Ltd. All Rights reserved.

Use of this software is subject to the terms of the Immersion LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/

using UnityEngine;

//-------------------------------------------------------------------------------------
// ***** IVRLensCorrection
//
// IVRLensCorrection contains the variables required to set material properties
// for the lens correction image effect.
//

namespace IVR
{
    [RequireComponent(typeof(Camera))]
    public class IVRLensCorrection : MonoBehaviour
    {
        [HideInInspector]
        public Vector2 _Center = new Vector2(0.5f, 0.5f);
        [HideInInspector]
        public Vector2 _ScaleIn = new Vector2(1.0f, 1.0f);
        [HideInInspector]
        public Vector2 _Scale = new Vector2(1.0f, 1.0f);
        [HideInInspector]
        public Vector4 _HmdWarpParam = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);

        // We will search for camera controller and set it here for access to its members
        private IVRCameraController CameraController = null;

        /// Provides a shader property that is set in the inspector
        /// and a material instantiated from the shader
        public Material material;

        void Start()
        {
            // Get the IVRCameraController
            CameraController = gameObject.transform.parent.parent.GetComponent<IVRCameraController>();

            if (CameraController == null)
                Debug.LogWarning("WARNING: IVRCameraController not found!");
        }

        // OnRenderImage
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // Use either source input or CameraTexutre, if it exists
            RenderTexture SourceTexture = source;

            // Replace null material with lens correction material
            Material material = null;

            if (CameraController.LensCorrection == true)
            {
                //if (CameraController.Chromatic == true)
                //    material = GetComponent<IVRLensCorrection>().GetMaterial_CA(CameraController.PortraitMode);
                //else
                //    material = GetComponent<IVRLensCorrection>().GetMaterial(CameraController.PortraitMode);
                material = GetMaterial(false);
            }

            if (material != null)
            {
                // Render with distortion
                Graphics.Blit(SourceTexture, destination, material);
            }
            else
            {
                // Pass through
                Graphics.Blit(SourceTexture, destination);
            }

        }

        // Called by camera to get lens correction values
        public Material GetMaterial(bool portrait)
        {
            // Set material properties
            SetPortraitProperties(portrait, ref material);

            material.SetVector("_HmdWarpParam", _HmdWarpParam);

            return material;
        }

        // Used for chromatic aberration
        //public Material material_CA;
        //[HideInInspector]
        //public Vector4 _ChromaticAberration = new Vector4(0.996f, 0.992f, 1.014f, 1.014f);

        // Called by camera to get lens correction values w/Chromatic aberration
        //public Material GetMaterial_CA(bool portrait)
        //{
        //    // Set material properties
        //    SetPortraitProperties(portrait, ref material_CA);

        //    material_CA.SetVector("_HmdWarpParam", _HmdWarpParam);

        //    Vector4 _CA = _ChromaticAberration;
        //    float rSquaredCoeffR = _CA[1] - _CA[0];
        //    float rSquaredCoeffB = _CA[3] - _CA[2];
        //    _CA[1] = rSquaredCoeffR;
        //    _CA[3] = rSquaredCoeffB;

        //    material_CA.SetVector("_ChromaticAberration", _CA);

        //    return material_CA;
        //}

        // SetPortraitProperties
        private void SetPortraitProperties(bool portrait, ref Material m)
        {
            if (portrait == true)
            {
                Vector2 tmp = Vector2.zero;
                tmp.x = _Center.y;
                tmp.y = _Center.x;
                m.SetVector("_Center", tmp);
                tmp.x = _Scale.y;
                tmp.y = _Scale.x;
                m.SetVector("_Scale", tmp);
                tmp.x = _ScaleIn.y;
                tmp.y = _ScaleIn.x;
                m.SetVector("_ScaleIn", tmp);
            }
            else
            {
                m.SetVector("_Center", _Center);
                m.SetVector("_Scale", _Scale);
                m.SetVector("_ScaleIn", _ScaleIn);
            }
        }
    }
}