// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEngine;
using System.Collections;

namespace SVGImporter
{
    public class SVGImporterSettings : ScriptableObject 
    {
        protected static SVGImporterSettings _instance;
        public static SVGImporterSettings Get
        {
            get {
                if(_instance == null)
                {
                    _instance = Resources.Load<SVGImporterSettings>("svgImporterSettings");
                    if(_instance == null)
                    {
                        Debug.LogError("Cannot Load SVG Importer Settings! Please Move SVG Importer Settings in to Resource folder.");
                    }
                }

                return _instance;
            }
        }

        public static void UpdateAntialiasing(float screenWidth = 1f, float screenHeight = 1f)
        {
            float inverseWidth = 0f;
            if(screenWidth > 0f) inverseWidth = 1f / screenWidth;
            float inverseHeight = 0f;
            if(screenHeight > 0f) inverseHeight = 1f / screenHeight;

            Shader.SetGlobalVector("SVG_GRADIENT_ANTIALIASING_WIDTH", new Vector4(Get.defaultAntialiasingWidth * inverseWidth, Get.defaultAntialiasingWidth * inverseHeight, 0f, 0f));
            Shader.SetGlobalVector("SVG_SOLID_ANTIALIASING_WIDTH", new Vector4(Get.defaultAntialiasingWidth * inverseWidth, Get.defaultAntialiasingWidth * inverseHeight, 0f, 0f));
        }

        protected static string _version = "1.1.3";
        public static string version
        {
            get {
                return _version;
            }
        }

        public SVGAssetFormat defaultSVGFormat = SVGAssetFormat.Transparent;
        public SVGUseGradients defaultUseGradients = SVGUseGradients.Always;
        public bool defaultAntialiasing = false;
        public float defaultAntialiasingWidth = 2f;
        public SVGMeshCompression defaultMeshCompression = SVGMeshCompression.Off;
        public int defaultVerticesPerMeter = 1000;
        public float defaultScale = 0.01f;
        public float defaultDepthOffset = 0.01f;
        public bool defaultCompressDepth = true;
        public bool defaultCustomPivotPoint = false;
        public Vector2 defaultPivotPoint = new Vector2(0.5f, 0.5f);
        public bool defaultGenerateCollider = false;
        public bool defaultKeepSVGFile = true;
        public bool defaultUseLayers = false;
        public bool defaultIgnoreSVGCanvas = true;
        public bool defaultOptimizeMesh = true;
        public bool defaultGenerateNormals = false;
        public bool defaultGenerateTangents = false;
        public Texture2D defaultSVGIcon;

        public bool ignoreImportExceptions = true;
    }

}