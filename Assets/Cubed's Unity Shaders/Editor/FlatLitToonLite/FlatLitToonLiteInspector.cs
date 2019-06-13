using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CubedsUnityShaders
{
    public class FlatLitToonLiteInspector : ShaderGUI
    {
        public enum CullingMode
        {
            Off,
            Front,
            Back
        }

        MaterialProperty m_mainTexture;
        MaterialProperty m_color;
        MaterialProperty m_colorMask;
        MaterialProperty m_shadow;
        MaterialProperty m_emissionMap;
        MaterialProperty m_emissionColor;
        MaterialProperty m_normalMap;
        MaterialProperty m_cullingMode;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            { //Find Properties
                m_mainTexture = FindProperty("_MainTex", properties);
                m_color = FindProperty("_Color", properties);
                m_colorMask = FindProperty("_ColorMask", properties);
                m_shadow = FindProperty("_Shadow", properties);
                m_emissionMap = FindProperty("_EmissionMap", properties);
                m_emissionColor = FindProperty("_EmissionColor", properties);
                m_normalMap = FindProperty("_BumpMap", properties);
                m_cullingMode = FindProperty("_Cull", properties);
            }

            //Shader Properties GUI
            EditorGUIUtility.labelWidth = 0f;

            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = m_cullingMode.hasMixedValue;
                var cMode = (CullingMode)m_cullingMode.floatValue;

                EditorGUI.BeginChangeCheck();
                cMode = (CullingMode)EditorGUILayout.Popup("Culling Mode", (int)cMode, System.Enum.GetNames(typeof(CullingMode)));
                if (EditorGUI.EndChangeCheck())
                {
                    materialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                    m_cullingMode.floatValue = (float)cMode;
                }
                EditorGUI.showMixedValue = false;
                EditorGUILayout.Space();

                materialEditor.TexturePropertySingleLine(new GUIContent("Main Texture", "Main Color Texture (RGB)"), m_mainTexture, m_color);
                EditorGUI.indentLevel += 1;
                materialEditor.TexturePropertySingleLine(new GUIContent("Color Mask", "Masks Color Tinting (G)"), m_colorMask);
                EditorGUI.indentLevel -= 1;

                materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map", "Normal Map (RGB)"), m_normalMap);
                materialEditor.TexturePropertySingleLine(new GUIContent("Emission", "Emission (RGB)"), m_emissionMap, m_emissionColor);
                EditorGUI.BeginChangeCheck();
                materialEditor.TextureScaleOffsetProperty(m_mainTexture);
                if (EditorGUI.EndChangeCheck())
                {
                    m_emissionMap.textureScaleAndOffset = m_mainTexture.textureScaleAndOffset;
                }

                EditorGUILayout.Space();
                materialEditor.ShaderProperty(m_shadow, "Shadow");
            }
            EditorGUI.EndChangeCheck();

        }
    } 
}