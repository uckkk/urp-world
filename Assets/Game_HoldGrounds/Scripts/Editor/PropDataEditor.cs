using UnityEditor;
using UnityEngine;

namespace Game_HoldGrounds.Scripts
{
    /// <summary>
    /// Helper class to show the Item picture as thumbnail in the scriptable object (use visual mode in Project window).
    /// </summary>
    [CustomEditor(typeof(PropData))]
    public class PropDataEditor : Editor
    {
        // =============================================================================================================
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var item = (PropData)target;
            //ICON IMAGE
            EditorGUI.BeginChangeCheck();
            item.SetPicture = (Sprite) EditorGUILayout.ObjectField
            (
                "Thumbnail",       // string
                item.GetPicture,   // Texture2D
                typeof(Sprite),    // Texture2D object, of course
                false              // allowSceneObjects
            );
            //CHECK FOR IMAGE CHANGE
            if (EditorGUI.EndChangeCheck())
            {
                AssetDatabase.SaveAssets();
                Repaint();    
            }
            //BASE GUI
            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(item); //required or changes in editor may not be saved when exit Unity
        }
        // =============================================================================================================
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var item = (PropData)target;

            if (item == null || item.GetPicture == null)
                return null;

            // item.GetPicture must be a supported format: ARGB32, RGBA32, RGB24,
            // Alpha8 or one of float formats
            var tex = new Texture2D (width, height);
            if (AssetPreview.GetAssetPreview(item.GetPicture) == null)
                return null;
            EditorUtility.CopySerialized (AssetPreview.GetAssetPreview(item.GetPicture), tex);

            return tex;
        }
        // =============================================================================================================
    }
}