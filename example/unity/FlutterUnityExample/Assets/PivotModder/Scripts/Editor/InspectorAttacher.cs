using UnityEditor;
using UnityEngine;

namespace BrainFailProductions.PivotModder
{

#pragma warning disable
    public class InspectorAttacher 
    {

        private static HideFlags oldFlags;


        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGizmoForMyScript(Transform scr, GizmoType gizmoType)
        {
            if(PivotModderMenu.IsAutoAttachEnabled())
            {
                AttachInspector();
            }
            
        }



        public static void AttachInspector()
        {

            if (Selection.activeGameObject == null) { return; }
            if (Selection.activeTransform == null || Selection.activeTransform is RectTransform) { return; }

            if (Selection.activeGameObject.GetComponent<PivotModderHost>() != null) { return; }


            // Attach the inspector hosting script
            if (Selection.activeGameObject != null)
            {
                //Debug.Log("Adding hosting script to gameobject  " +Selection.activeGameObject.name);

                oldFlags = Selection.activeGameObject.hideFlags;

                PivotModderHost host = Selection.activeGameObject.AddComponent(typeof(PivotModderHost)) as PivotModderHost;
                host.hideFlags = HideFlags.DontSave;
                Selection.activeGameObject.hideFlags = HideFlags.DontSave;


                Selection.activeGameObject.hideFlags = oldFlags;
                Selection.activeGameObject.GetComponent<PivotModderHost>().hideFlags = HideFlags.DontSave;

            }
        }

    
    }


}


