#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace BrainFailProductions.PivotModder
{
    public class PivotModderMenu : MonoBehaviour
    {


        [MenuItem("Window/Brainfail Products/PivotModder/Enable Auto UI Attaching", false, 0)]
        static void EnableAutoUIAttaching()
        {
            EditorPrefs.SetBool("pivotModderAutoAttach", true);
            InspectorAttacher.AttachInspector();
        }

        
        [MenuItem("Window/Brainfail Products/PivotModder/Disable Auto UI Attaching", false, 1)]
        static void DisableAutoUIAttaching()
        {
            EditorPrefs.SetBool("pivotModderAutoAttach", false);
        }


        [MenuItem("Window/Brainfail Products/PivotModder/Attach PivotModder to Object", false, 2)]
        static void AttachPivotModderToObject()
        {
            EditorPrefs.SetBool("pivotModderAutoAttach", false);
            InspectorAttacher.AttachInspector();
        }


        public static bool IsAutoAttachEnabled()
        {
            bool isAutoAttach;

            if (!EditorPrefs.HasKey("pivotModderAutoAttach"))
            {
                EditorPrefs.SetBool("pivotModderAutoAttach", true);
                isAutoAttach = true;
            }
            else
            {
                isAutoAttach = EditorPrefs.GetBool("pivotModderAutoAttach");
            }

            return isAutoAttach;
        }


#region VALIDATORS

        [MenuItem("Window/Brainfail Products/PivotModder/Enable Auto UI Attaching", true)]
        static bool CheckEnableAttachingButton()
        {
            bool isAutoAttach = IsAutoAttachEnabled();

            if (isAutoAttach) { return false; }
            else { return true; }
        }

        [MenuItem("Window/Brainfail Products/PivotModder/Disable Auto UI Attaching", true)]
        static bool CheckDisableAttachingButton()
        {
            bool isEnableButtOn  = CheckEnableAttachingButton();

            if (isEnableButtOn) { return false; }
            else { return true; }
        }

#endregion VALIDATORS
    }

}

#endif
