using BrainFailProductions.PivotModderRuntime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BrainFailProductions.PivotModderRuntime
{

    public class PivotModderRuntime : MonoBehaviour
    {

        #region PUBLIC METHODS


        /// <summary>
        /// Moves the pivot point of a GameObject to a new position provided. The method also preserves the orientations of the children objects. Any errors are reported as exceptions.
        /// </summary>
        /// <param name="forObject"> The objects whose pivot point will be modified. If the provided object is meshless then this method simply changes the position of the GameObject while preserving the transform states of the children objects.</param>
        /// <param name="moveTo"> The new point in space to move the pivot to.</param>
        /// <param name="recalculateNormalsTangents"> Should the mesh normals and tangents be recalculated after pivot modification.</param>
        public static void MovePivot(GameObject forObject, Vector3 moveTo, bool recalculateNormalsTangents )
        {

            if (forObject == null)
            {
                throw new ArgumentNullException("forObject", "You must provide a GameObject to move pivot for.");
            }

            Mesh mesh = SetupMeshForTempChanges(forObject);

            
            if(mesh != null)
            {
                if(UtilityServicesRuntime.IsBone(forObject.transform))
                {
                    throw new InvalidOperationException("The provided gameobject is a bone used by a SkinnedMeshRenderer. Modifying pivot for such an object can cause mesh distortion.");
                }

                if (UtilityServicesRuntime.IsMorphed(forObject.transform))
                {
                    throw new InvalidOperationException("Morphed objects(Meshes with blendshape animations) are not supported. Pivot modifications on such objects can result in incorrect blend shape animations.");
                }

                if (UtilityServicesRuntime.IsMorphed(forObject.transform))
                {
                    throw new InvalidOperationException("Morphed objects(Meshes with blendshape animations) are not supported. Pivot modifications on such objects can result in incorrect blend shape animations.");
                }
            }


            UtilityServicesRuntime.ChildStateTuple[] childrenStatesBeforePivotMod = UtilityServicesRuntime.SaveChildrenStates(forObject);


            // The object is meshless
            if (mesh == null)
            {
                forObject.transform.position = moveTo;             
            }

            else
            {
                Vector3[] vertices = mesh.vertices;

                Vector3 pivotOffsetLocalSpace = forObject.transform.InverseTransformVector(forObject.transform.position - moveTo);

                Matrix4x4 transformationMatrix = Matrix4x4.Translate(pivotOffsetLocalSpace);


                for (int a = 0; a < vertices.Length; a++)
                {
                    vertices[a] = transformationMatrix.MultiplyPoint3x4(vertices[a]);
                }


                mesh.vertices = vertices;    //Assign the vertex array back to the mesh


                if (recalculateNormalsTangents)
                {
                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();
                }

                mesh.RecalculateBounds();


                forObject.transform.position = moveTo;   // Move the object to the previous position

            }


            UtilityServicesRuntime.RestoreChildrenStates(childrenStatesBeforePivotMod);

        }


        /// <summary>
        /// Moves the pivot point of an object to its center. This center is defined by the mesh bounding volume. This method doesn't work on meshless objects.
        /// </summary>
        /// <param name="forObject"> The GameObject whose pivot point will be centered.</param>
        /// <param name="recalculateNormalsTangents"> Should the mesh normals and tangents be recalculated after pivot modification.</param>    
        public static void CentralizePivot(GameObject forObject, bool recalculateNormalsTangents)
        {

            if(forObject == null)
            {
                throw new ArgumentNullException("forObject", "You must provide a GameObject to centralize pivot for.");
            }

            Mesh mesh = GetStaticMesh(forObject);

            if (mesh == null)
            {
                throw new InvalidOperationException("The centralize pivot operation can only be run on GameObjects having a static mesh (Mesh rendered by a MeshFilter and MeshRenderer component). The provided GameObject is not feasible for this operation.");
            }
        

            Vector3 centerPoint = GetMeshWorldCenterPoint(mesh, forObject);

            MovePivot(forObject, centerPoint, recalculateNormalsTangents);
        }


        /// <summary>
        /// Move the pivot point to the average of the positions of the children of the provided GameObject. If the provided object has no children then the method does nothing.
        /// </summary>
        /// <param name="forObject"> The GameObject whose pivot will be moved.</param>
        /// <param name="takeNestedAverage"> If this argument is passed true then the pivot is moved to the average of the world positions of all the children of the given GameObject including the deep nested ones. If this is false then the new pivot position is the average of the first level children positions.</param>
        /// <param name="includeInactive"> Whether to consider the inactive children when calculating the average for the new pivot position.</param>
        /// <param name="recalculateNormalsTangents"> Should the mesh normals and tangents be recalculated after pivot modification.</param>    
        public static void PivotToPositionsAverage(GameObject forObject, bool takeNestedAverage, bool includeInactive, bool recalculateNormalsTangents)
        {

            if (forObject == null)
            {
                throw new ArgumentNullException("forObject", "You must provide a GameObject to move pivot for.");
            }

            Vector3 averagePos = Vector3.zero;
            int childCount = forObject.transform.childCount;

            if(childCount == 0) { return; }

            childCount = 0;

            if(takeNestedAverage)
            {
                var allChildren = forObject.GetComponentsInChildren<Transform>(true);

                // This also includes the top level parent as well so we skip it
                for(int a = 1; a < allChildren.Length; a++)
                {
                    Transform child = allChildren[a];

                    if(!includeInactive && !child.gameObject.activeSelf) { continue; }

                    averagePos += child.position;

                    childCount++;
                }
            }

            else
            {
                // This only iterates over the first level children and it also includes the inactive children
                foreach (Transform child in forObject.transform)
                {
                    if (!includeInactive && !child.gameObject.activeSelf) { continue; }

                    averagePos += child.position;

                    childCount++;
                }
            }


            averagePos = averagePos / childCount;

            MovePivot(forObject, averagePos, recalculateNormalsTangents);
        }


        /// <summary>
        /// Changes the rotation of the pivot point of a GameObject. The method also preserves the orientations of the children objects. Any errors are reported as exceptions. Please note that this operation can result in skewing of the object if it is non uniformly scaled.
        /// </summary>
        /// <param name="forObject"> The objects whose pivot point will be modified. If the provided object is meshless then this method simply changes the rotation of the GameObject.</param>
        /// <param name="newRotation"> The new rotation of the pivot point in world space.</param>
        /// <param name="recalculateNormalsTangents"> Should the mesh normals and tangents be recalculated after pivot modification.</param>
        public static void RotatePivotTo(GameObject forObject, Quaternion newRotation, bool recalculateNormalsTangents)
        {

            if (forObject == null)
            {
                throw new ArgumentNullException("forObject", "You must provide a GameObject to rotate pivot for.");
            }

            Mesh mesh = SetupMeshForTempChanges(forObject);


            if (mesh != null)
            {
                if (UtilityServicesRuntime.IsBone(forObject.transform))
                {
                    throw new InvalidOperationException("The provided gameobject is a bone used by a SkinnedMeshRenderer. Modifying pivot for such an object can cause mesh distortion.");
                }

                if (UtilityServicesRuntime.IsMorphed(forObject.transform))
                {
                    throw new InvalidOperationException("Morphed objects(Meshes with blendshape animations) are not supported. Pivot modifications on such objects can result in incorrect blend shape animations.");
                }

                if (!UtilityServicesRuntime.IsUniformScale(forObject.transform.lossyScale))
                {
                    throw new InvalidOperationException("The selected object has non uniform scaling applied. Rotational modifications of pivots for non uniformly scaled objects can result in mesh skewing. It's best to first uniformly scale such an object before applying rotational modifications to the pivot.");
                }
            }


            UtilityServicesRuntime.ChildStateTuple[] childrenStatesBeforePivotMod = UtilityServicesRuntime.SaveChildrenStates(forObject);


            // The object is meshless
            if (mesh == null)
            {
                forObject.transform.rotation = newRotation;
            }

            else
            {

                GameObject tempObject = new GameObject();
                tempObject.transform.parent = forObject.transform;
                tempObject.transform.rotation = newRotation;
                Quaternion inverseRotation = Quaternion.Inverse(tempObject.transform.localRotation);


                Quaternion prevRotation = forObject.transform.rotation;
                DestroyImmediate(tempObject);

                Matrix4x4 transformationMatrix = Matrix4x4.Rotate(inverseRotation);

                forObject.transform.rotation = newRotation;

                Vector3[] vertices = mesh.vertices;

                //Debug.Log("submeshes count  " + forObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount + "  vertex count   "  + vertices.Length);


                for (int a = 0; a < vertices.Length; a++)
                {
                    vertices[a] = transformationMatrix.MultiplyPoint3x4(vertices[a]);
                }

                mesh.vertices = vertices;

                if(recalculateNormalsTangents)
                {
                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();
                }

                mesh.RecalculateBounds();

            }


            UtilityServicesRuntime.RestoreChildrenStates(childrenStatesBeforePivotMod);

        }


        /// <summary>
        /// Zero out the pivot rotation values on all three axes for the given GameObject. The method also preserves the orientations of the children objects. Any errors are reported as exceptions. Please note that this operation can result in skewing of the object if it is non uniformly scaled.
        /// </summary>
        /// <param name="forObject"> The objects whose pivot point will be modified. If the provided object is meshless then this method simply changes the rotation of the GameObject.</param>
        /// <param name="recalculateNormalsTangents"> Should the mesh normals and tangents be recalculated after pivot modification.</param>    
        public static void ZeroPivotRotation(GameObject forObject, bool recalculateNormalsTangents)
        {
            RotatePivotTo(forObject, Quaternion.identity, recalculateNormalsTangents);
        }


        /// <summary>
        /// Gets the center point of a mesh in world space.
        /// </summary>
        /// <param name="mesh"> The mesh whose center point will be calculated.</param>
        /// <param name="target"> The GameObject with which this mesh is attached.</param>
        /// <returns> The mesh center point in world space. This center is defined by the mesh bounding volume.</returns>
        public static Vector3 GetMeshWorldCenterPoint(Mesh mesh, GameObject target)
        {

            if (mesh == null)
            {
                throw new ArgumentNullException("mesh", "You must provide a mesh whose center point will be calculated.");
            }


            if (target == null)
            {
                throw new ArgumentNullException("target", "You must provide a GameObject associated with the provided mesh.");
            }


            Vector3 center = mesh.bounds.center;

            Vector3 objCenterWorldSpace = target.transform.TransformPoint(center);

            return objCenterWorldSpace;

        }


        /// <summary>
        /// Gets the center point of a mesh in its local space.
        /// </summary>
        /// <param name="mesh"> The mesh whose center point will be calculated.</param>
        /// <returns> The mesh center point in its local space. This center is defined by the mesh bounding volume.</returns>
        public static Vector3 GetMeshLocalCenterPoint(Mesh mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException("mesh", "You must provide a mesh whose center point will be calculated.");
            }

            Vector3 center = mesh.bounds.center;

            return center;
        }


        #endregion PUBLIC METHODS



        #region PRIVATE METHODS

        private static Mesh SetupMeshForTempChanges(GameObject forObject)
        {
            if(forObject == null) { return null; }

            MeshFilter mf = forObject.GetComponent<MeshFilter>();

            if(mf == null) { return null; }

            Mesh instantiatedMesh = mf.mesh ;

            mf.sharedMesh = instantiatedMesh;

            return mf.sharedMesh;
        }


        private static Mesh GetStaticMesh(GameObject toCheck)
        {
            if (toCheck == null) { return null; }

            MeshFilter mf = toCheck.GetComponent<MeshFilter>();

            if (mf == null) { return null; }

            return mf.sharedMesh;
        }

        #endregion PRIVATE METHODS

    }

}


