using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BrainFailProductions.PivotModderRuntime
{

    public class UtilityServicesRuntime : MonoBehaviour
    {

        [System.Serializable]
        public struct ChildStateTuple
        {
            public Transform transform;
            public Vector3 position;
            public Quaternion rotation;

            public ChildStateTuple(Transform transform, Vector3 position, Quaternion rotation)
            {
                this.transform = transform;
                this.position = position;
                this.rotation = rotation;
            }
        }




        public static Transform[] GetTopLevelChildren(Transform Parent)
        {
            Transform[] Children = new Transform[Parent.childCount];
            for (int a = 0; a < Parent.childCount; a++)
            {
                Children[a] = Parent.GetChild(a);
            }
            return Children;
        }


        public static Transform GetTopLevelParent(Transform forObject)
        {
            if (forObject == null) { return null; }

            Transform nextParent = forObject;

            while (true)
            {
                if (nextParent.parent == null) { return nextParent; }
                nextParent = nextParent.parent;
            }

        }


        public static bool IsBone(Transform inspectedObject)
        {
            Transform topParent = GetTopLevelParent(inspectedObject);
            SkinnedMeshRenderer[] skinnedRenderers = topParent.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            if (skinnedRenderers == null && skinnedRenderers.Length == 0) { return false; }

            foreach (var renderer in skinnedRenderers)
            {
                Transform[] bones = renderer.bones;

                if (bones != null && bones.Length > 0)
                {
                    foreach (var bone in bones)
                    {
                        if (bone.GetHashCode().Equals(inspectedObject.GetHashCode())) { return true; }
                    }
                }
            }

            return false;
        }




        public static bool IsMorphed(Transform inspectedObject)
        {
            MeshFilter mFilter = inspectedObject.GetComponent<MeshFilter>();
            SkinnedMeshRenderer sRenderer = inspectedObject.GetComponent<SkinnedMeshRenderer>();

            if (mFilter != null && mFilter.sharedMesh != null)
            {
                return mFilter.sharedMesh.blendShapeCount > 0;
            }

            else if (sRenderer != null && sRenderer.sharedMesh != null)
            {
                return sRenderer.sharedMesh.blendShapeCount > 0;
            }

            else
            {
                return false;
            }
        }



        public static bool MeshFilterHasSubmeshes(Transform inspectedObject)
        {
            MeshFilter mFilter = inspectedObject.GetComponent<MeshFilter>();

            if (mFilter != null && mFilter.sharedMesh != null)
            {
                return mFilter.sharedMesh.subMeshCount > 1;
            }

            else
            {
                return false;
            }
        }


        public static ChildStateTuple[] SaveChildrenStates(GameObject forObject)
        {

            var children = GetTopLevelChildren(forObject.transform);

            ChildStateTuple[] childrenStates = new ChildStateTuple[children.Length];

            for (int a = 0; a < children.Length; a++)
            {
                childrenStates[a].transform = children[a];
                childrenStates[a].position = children[a].position;
                childrenStates[a].rotation = children[a].rotation;
            }

            return childrenStates;

        }


        public static void RestoreChildrenStates(ChildStateTuple[] childStates)
        {

            if (childStates == null) { return; }

            for (int a = 0; a < childStates.Length; a++)
            {
                if (childStates[a].transform == null) { continue; }

                childStates[a].transform.position = childStates[a].position;
                childStates[a].transform.rotation = childStates[a].rotation;
            }

        }



        public static bool IsUniformScale(Vector3 toCheck)
        {
            return (Mathf.Approximately(toCheck.x, toCheck.y) && Mathf.Approximately(toCheck.y, toCheck.z));
        }
    }

}



