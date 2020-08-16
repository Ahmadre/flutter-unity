#if UNITY_EDITOR


using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;


namespace BrainFailProductions.PivotModder
{



    public class UtilityServices : EditorWindow
    {



        private const int undoLimit = 10;  //10
        private const int objectRecordLimit = 200;   //200
        public  const string DEFAULT_SAVE_PATH = "Assets/";




        public enum HandleOrientation
        {
            localAligned,
            globalAligned
        }


        [System.Serializable]
        public struct ObjectRecord
        {
            public GameObject duplicateRef;
            public Vector3 currPos;
            public Vector3 posToUpdate;
            public Vector3 changeInPos;
            public Quaternion currRot;
            public bool isMeshless;

            public ObjectRecord(GameObject duplicateRef, Vector3 currPos, Vector3 posToUpdate, Quaternion rotation, bool isMeshless)
            {
                this.duplicateRef = duplicateRef;
                this.currPos = currPos;
                this.posToUpdate = posToUpdate;
                this.currRot = rotation;
                this.isMeshless = isMeshless;
                changeInPos = currPos - posToUpdate;
            }


            public void Destruct()
            {
                if (duplicateRef) { DestroyImmediate(duplicateRef); }
            }
        }


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


        [System.Serializable]
        public struct ColliderState
        {
            public ColliderType type;
            public Vector3 center;
            public Quaternion rotation;
        }


        [System.Serializable]
        public struct UndoRedoOps
        {
            public GameObject gameObject;
            public List<ObjectRecord> undoOperations;
            public List<ObjectRecord> redoOperations;

            public UndoRedoOps(GameObject gameObject, List<ObjectRecord> undoOperations, List<ObjectRecord> redoOperations)
            {
                this.gameObject = gameObject;
                this.undoOperations = undoOperations;
                this.redoOperations = redoOperations;
            }


            public void Destruct()
            {
                if (undoOperations != null)
                {
                    foreach (var operation in undoOperations)
                    {
                        operation.Destruct();
                    }
                }

                if (redoOperations != null)
                {
                    foreach (var operation in redoOperations)
                    {
                        operation.Destruct();
                    }
                }

            }
        }


        public static GameObject containerObject;
        public static DataContainer dataContainer;




        public static ObjectRecord CreateObjectRecord(GameObject forObject, bool isMeshless, Vector3 posToUpdate)
        {

            /*
            CollRecognize colliderObj = forObject.GetComponentInChildren<CollRecognize>();
            GameObject duplicateColl = null;

            if (colliderObj)
            {
                duplicateColl = DuplicateGameObject(colliderObj.gameObject, "_duplicate" + Guid.NewGuid().ToString(), true, true);
                //duplicate.hideFlags = HideFlags.HideAndDontSave;  
            }
            */

            //var childStates = SaveChildrenStates(forObject);
            //ColliderState collState = SaveColliderState(forObject);

            Vector3 position = forObject.transform.position;
            Quaternion rotation = forObject.transform.rotation;

            ObjectRecord record = new ObjectRecord(null, position, posToUpdate, rotation, isMeshless);

            return record;
        }




        public static void SaveRecord(GameObject forObject, ObjectRecord record, bool isUndo)
        {

            UndoRedoOps operations = new UndoRedoOps();
            int key = forObject.GetHashCode();

            bool containsEntry = dataContainer.objectsHistory.TryGetValue(key, out operations);


            if (containsEntry)
            {
                if (isUndo)
                {
                    if (operations.undoOperations.Count == undoLimit)
                    {
                        operations.undoOperations[0].Destruct();
                        operations.undoOperations.RemoveAt(0);
                    }

                    operations.undoOperations.Add(record);
                }

                else
                {
                    if (operations.redoOperations.Count == undoLimit)
                    {
                        operations.redoOperations[0].Destruct();
                        operations.redoOperations.RemoveAt(0);
                    }

                    operations.redoOperations.Add(record);
                }

            }

            else
            {

                if (dataContainer.objectsHistory.Count == objectRecordLimit)
                {
                    dataContainer.objectsHistory[0].Destruct();
                    dataContainer.objectsHistory.Remove(0);
                }


                operations = new UndoRedoOps();
                operations.gameObject = forObject;
                operations.undoOperations = new List<ObjectRecord>();
                operations.redoOperations = new List<ObjectRecord>();

                if (isUndo) { operations.undoOperations.Add(record); }
                else { operations.redoOperations.Add(record); }

                dataContainer.objectsHistory.Add(key, operations);
            }

        }




        public static bool DoUndoRedoOperation(GameObject forObject, bool doUndo)
        {

            UndoRedoOps operations = new UndoRedoOps();
            int key = forObject.GetHashCode();
            Quaternion collRot = Quaternion.identity;
            Vector3 collPos = Vector3.zero;
            CollRecognize collRecog = forObject.GetComponentInChildren<CollRecognize>();
            //ColliderState colliderState = SaveColliderState(forObject);
            //ChildStateTuple[] childrenStates = SaveChildrenStates(forObject);


            if (collRecog)
            {
                collRot = collRecog.gameObject.transform.rotation;
                collPos = collRecog.gameObject.transform.position;
            }



            bool containsEntry = dataContainer.objectsHistory.TryGetValue(key, out operations);

            if (containsEntry)
            {

                int lastIndex = -1;
                ObjectRecord operation;


                if (doUndo)
                {
                    lastIndex = operations.undoOperations.Count - 1;
                    operation = operations.undoOperations[lastIndex];
                    operations.undoOperations.RemoveAt(lastIndex);
                }

                else
                {
                    lastIndex = operations.redoOperations.Count - 1;
                    operation = operations.redoOperations[lastIndex];
                    operations.redoOperations.RemoveAt(lastIndex);
                }


                if (!operation.isMeshless)
                {
                    if (operation.currPos != forObject.transform.position)
                    {
                        PivotManager.MovePivot(forObject, forObject.transform.position + operation.changeInPos, true);
                    }

                    if (!operation.currRot.Equals(forObject.transform.rotation))
                    {
                        PivotManager.RotatePivot(forObject, operation.currRot, true);
                    }
                }

                else
                {
                    if (!operation.currPos.Equals(forObject.transform.position))
                    {
                        PivotManager.MovePivotForMeshless(forObject, forObject.transform.position + operation.changeInPos, true);
                    }

                    if (!operation.currRot.Equals(forObject.transform.rotation))
                    {
                        PivotManager.RotatePivotForMeshless(forObject, operation.currRot, true);
                    }
                }


                //RestoreColliderState(forObject, operation.colliderState);
                //RestoreChildrenStates(operation.childrenStates);

                //RestoreColliderState(forObject, colliderState);
                //RestoreChildrenStates(childrenStates);


                if (collRecog)
                {
                    collRecog.gameObject.transform.rotation = collRot;
                    collRecog.gameObject.transform.position = collPos;
                }


                return true;
            }


            else { return false; }

        }




        public static void DrawHorizontalLine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));

            r.height = thickness;
            r.y += padding / 2;
            r.x -= 10;
            r.width += 20;
            EditorGUI.DrawRect(r, color);
        }




        public static void DrawVerticalLine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(padding + thickness));

            r.width = thickness;
            r.x += padding / 2;
            r.y -= 10;
            r.height += 20;
            EditorGUI.DrawRect(r, color);
        }



        public static Texture2D MakeColoredTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }



        public static Texture2D CreateTexture2DCopy(Texture2D original)
        {

            Texture2D result = new Texture2D(original.width, original.height);
            result.SetPixels(original.GetPixels());
            result.Apply();
            return result;

        }




        public static Color HexToColor(string hex)
        {

            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return new Color32(r, g, b, a);

        }







        public static bool CheckIfMeshless(Transform transform)
        {
            MeshFilter hasMeshFilter = transform.GetComponent<MeshFilter>();

            return (!hasMeshFilter || (hasMeshFilter && !hasMeshFilter.sharedMesh));
        }



        public static Vector3 GetClosestVertex(Vector3 point, Mesh mesh, Transform obj)
        {

            if (mesh == null) { return Vector3.zero; }


            Vector3 closestVertex = Vector3.zero;
            float minDist = Mathf.Infinity;
            point = obj.InverseTransformPoint(point);

            for (int a = 0; a < mesh.vertexCount; a++)
            {
                Vector3 vertexPos = mesh.vertices[a];
                float distance = Vector3.Distance(vertexPos, point);

                if (distance < minDist)
                {
                    minDist = distance;
                    closestVertex = vertexPos;
                }
            }

            return obj.TransformPoint(closestVertex);

        }






        public static Vector3 GetSnapPoint(Vector3 position, Quaternion rotation, Vector3 snapVector, Vector3 dragDirection, HandleOrientation handlesOrientation)
        {

            var selectedControl = HandleControlsUtility.handleControls.GetCurrentSelectedControl();
            Vector3 result = Vector3.zero;

            if (handlesOrientation == HandleOrientation.globalAligned)
            {
                rotation = Quaternion.identity;
            }


            if (selectedControl == HandleControlsUtility.HandleControls.xAxisMoveHandle)
            {
                result = GetXSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.yAxisMoveHandle)
            {
                result = GetYSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.zAxisMoveHandle)
            {
                result = GetZSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.xyAxisMoveHandle)
            {
                //Vector3 localAxisDir = GetXaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.xAxisMoveHandle;
                result = GetXSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);


                //localAxisDir = GetYaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.yAxisMoveHandle;
                result = GetYSnappedPos(result, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.yzAxisMoveHandle)
            {
                //Vector3 localAxisDir = GetYaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.yAxisMoveHandle;
                result = GetYSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);

                //localAxisDir = GetZaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.zAxisMoveHandle;
                result = GetZSnappedPos(result, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.xzAxisMoveHandle)
            {
                //Vector3 localAxisDir = GetXaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.xAxisMoveHandle;
                result = GetXSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);

                //localAxisDir = GetZaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.zAxisMoveHandle;
                result = GetZSnappedPos(result, rotation, dragDirection, snapVector, selectedControl);
            }

            else if (selectedControl == HandleControlsUtility.HandleControls.allAxisMoveHandle)
            {
                //Vector3 localAxisDir = GetXaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.xAxisMoveHandle;
                result = GetXSnappedPos(position, rotation, dragDirection, snapVector, selectedControl);

                //localAxisDir = GetYaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.yAxisMoveHandle;
                result = GetXSnappedPos(result, rotation, dragDirection, snapVector, selectedControl);

                //localAxisDir = GetZaxisinWorld(rotation);
                selectedControl = HandleControlsUtility.HandleControls.zAxisMoveHandle;
                result = GetXSnappedPos(result, rotation, dragDirection, snapVector, selectedControl);
            }

            return (result);

        }



        private static Vector3 GetXSnappedPos(Vector3 position, Quaternion rotation, Vector3 dragDirection, Vector3 snapVector, HandleControlsUtility.HandleControls selectedControl)
        {


            Vector3 result = Vector3.zero;
            Vector3 localAxisDir = GetXaxisinWorld(rotation);
            float dot = Vector3.Dot(localAxisDir, dragDirection);
            //float angle = Vector3.Angle(dragDirection, localAxisDir);
            if (dot < 0) { localAxisDir *= -1; }

            result = position + (snapVector.x * localAxisDir);

            if (dot >= 0 && dot <= 1) { result = position; }

            return result;
        }


        private static Vector3 GetYSnappedPos(Vector3 position, Quaternion rotation, Vector3 dragDirection, Vector3 snapVector, HandleControlsUtility.HandleControls selectedControl)
        {
            Vector3 result = Vector3.zero;
            Vector3 localAxisDir = GetYaxisinWorld(rotation);
            float dot = Vector3.Dot(localAxisDir, dragDirection);

            if (dot < 0) { localAxisDir *= -1; }

            result = position + (snapVector.y * localAxisDir);

            if (dot >= 0 && dot <= 1f) { result = position; }

            return result;
        }


        private static Vector3 GetZSnappedPos(Vector3 position, Quaternion rotation, Vector3 dragDirection, Vector3 snapVector, HandleControlsUtility.HandleControls selectedControl)
        {
            Vector3 result = Vector3.zero;
            Vector3 localAxisDir = GetZaxisinWorld(rotation);
            float dot = Vector3.Dot(localAxisDir, dragDirection);

            if (dot < 0) { localAxisDir *= -1; }

            result = position + (snapVector.z * localAxisDir);

            if (dot >= 0 && dot <= 1f) { result = position; }

            return result;
        }




        public static Vector3? CorrectHandleValues(Vector3 pointToCorrect, Vector3 oldValueOfPoint)
        {

            if (HandleControlsUtility.handleControls == null) { return null; }


            Vector3 corrected = Vector3.zero;

            using (HandleControlsUtility handleControls = HandleControlsUtility.handleControls)
            {
                switch (handleControls.GetCurrentSelectedControl())
                {
                    case HandleControlsUtility.HandleControls.xAxisMoveHandle:
                        corrected = new Vector3(pointToCorrect.x, oldValueOfPoint.y, oldValueOfPoint.z);
                        break;

                    case HandleControlsUtility.HandleControls.yAxisMoveHandle:
                        corrected = new Vector3(oldValueOfPoint.x, pointToCorrect.y, oldValueOfPoint.z);
                        break;

                    case HandleControlsUtility.HandleControls.zAxisMoveHandle:
                        corrected = new Vector3(oldValueOfPoint.x, oldValueOfPoint.y, pointToCorrect.z);
                        break;

                    case HandleControlsUtility.HandleControls.xyAxisMoveHandle:
                        corrected = new Vector3(pointToCorrect.x, pointToCorrect.y, oldValueOfPoint.z);
                        break;

                    case HandleControlsUtility.HandleControls.xzAxisMoveHandle:
                        corrected = new Vector3(pointToCorrect.x, oldValueOfPoint.y, pointToCorrect.z);
                        break;

                    case HandleControlsUtility.HandleControls.yzAxisMoveHandle:
                        corrected = new Vector3(oldValueOfPoint.x, pointToCorrect.y, pointToCorrect.z);
                        break;
                }
            }

            return corrected;

        }




        public void RunAfter(Action command, YieldInstruction yieldInstruction)
        {
            this.StartCoroutine(CommandEnumerator(command, yieldInstruction));
        }



        public static Vector3 GetXaxisinWorld(Quaternion rotation)
        {
            return rotation * Vector3.right;
        }


        public static Vector3 GetYaxisinWorld(Quaternion rotation)
        {
            return rotation * Vector3.up;
        }


        public static Vector3 GetZaxisinWorld(Quaternion rotation)
        {
            return rotation * Vector3.forward;
        }



        public static float CalcEulerSafeAngle(float angle)
        {
            if (angle >= -90 && angle <= 90)
                return angle;
            angle = angle % 180;
            if (angle > 0)
                angle -= 180;
            else
                angle += 180;
            return angle;
        }





        private static IEnumerator CommandEnumerator(Action command, YieldInstruction yieldInstruction)
        {
            yield return yieldInstruction;
            command();
        }





        public static GameObject DuplicateGameObject(GameObject toDuplicate, string newName, bool duplicateFromRoot, bool duplicateChildren)
        {
            if (toDuplicate == null) { return null; }

            GameObject selectedObject = Selection.activeGameObject;
            GameObject duplicate = null;


            if (!selectedObject.GetHashCode().Equals(toDuplicate.GetHashCode()))
            {
                Selection.activeGameObject = toDuplicate;
            }

            //#pragma warning disable CS0618
#pragma warning disable
            GameObject rootParent = (GameObject)PrefabUtility.GetPrefabParent(toDuplicate);

            if (duplicateFromRoot && rootParent) { Selection.activeGameObject = rootParent; }


            SceneView.lastActiveSceneView.Focus();
            EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));

            duplicate = Selection.activeGameObject;
            Selection.activeGameObject.name = newName;
            Selection.activeGameObject = selectedObject;

            if (!duplicateChildren)
            {

                while (duplicate.transform.childCount > 0)
                {
                    DestroyImmediate(duplicate.transform.GetChild(0).gameObject);
                }

            }

            duplicate.transform.parent = null;

            return duplicate;
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




        /// <summary> Restores the children states to the ones before pivot modification. </summary>

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




        public static ColliderState SaveColliderState(GameObject forObject)
        {

            Collider selectedObjectCollider = forObject.GetComponent<Collider>();
            Transform selectedTransform = forObject.transform;
            ColliderState colliderState = new ColliderState();

            if (selectedObjectCollider)
            {
                if (selectedObjectCollider is BoxCollider)
                {
                    colliderState.center = selectedTransform.TransformPoint(((BoxCollider)selectedObjectCollider).center);
                    colliderState.type = ColliderType.BoxCollider;
                }
                else if (selectedObjectCollider is CapsuleCollider)
                {
                    colliderState.center = selectedTransform.TransformPoint(((CapsuleCollider)selectedObjectCollider).center);
                    colliderState.type = ColliderType.CapsuleCollider;
                }
                else if (selectedObjectCollider is SphereCollider)
                {
                    colliderState.center = selectedTransform.TransformPoint(((SphereCollider)selectedObjectCollider).center);
                    colliderState.type = ColliderType.SphereCollider;
                }
                else if (selectedObjectCollider is MeshCollider)
                {
                    colliderState.type = ColliderType.MeshCollider;
                    //colliderState.center = selectedTransform.TransformPoint(((MeshCollider)selectedObjectCollider).bounds.center);
                }
            }

            return colliderState;

        }





        public /// <summary> Restore the collider orientation.</summary>
    static void RestoreColliderState(GameObject forObject, ColliderState colliderState)
        {


            Collider selectedObjectCollider = forObject.GetComponent<Collider>();
            Transform selectedTransform = forObject.transform;


            if (selectedObjectCollider)
            {
                if (selectedObjectCollider is BoxCollider)
                {
                    if (colliderState.type == ColliderType.BoxCollider)
                    {
                        ((BoxCollider)selectedObjectCollider).center = selectedTransform.InverseTransformPoint(colliderState.center);
                    }
                }
                else if (selectedObjectCollider is CapsuleCollider)
                {
                    if (colliderState.type == ColliderType.CapsuleCollider)
                    {
                        ((CapsuleCollider)selectedObjectCollider).center = selectedTransform.InverseTransformPoint(colliderState.center);
                    }
                }
                else if (selectedObjectCollider is SphereCollider)
                {
                    if (colliderState.type == ColliderType.SphereCollider)
                    {
                        ((SphereCollider)selectedObjectCollider).center = selectedTransform.InverseTransformPoint(colliderState.center);
                    }
                }
                else if (selectedObjectCollider is MeshCollider)
                {

                    /*
                    MeshCollider meshColl = (MeshCollider)selectedObjectCollider;

                    bool isConvex = meshColl.convex;

                    meshColl.convex = false;

                    meshColl.sharedMesh = selectedObjectMesh;

                    if (isConvex)
                    {

                        if (selectedObjectMesh.vertexCount >= 2000)
                        {

                            Debug.Log("<b><i><color=#008000ff> PLEASE WAIT... while the convex property on the mesh collider does some calculations.The editor won't be usable until the MeshCollider finishes its calculations.</color></i></b>");
                            new UtilityServices().RunAfter(() => { meshColl.convex = true; }, new WaitForSeconds(0.2f));
                        }

                        else { meshColl.convex = true; } 

                    }
                    */
                }
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

                //Transform rootBone = renderer.rootBone;

                //if(rootBone == null) { continue; }

                //if(rootBone.GetHashCode().Equals(transform.GetHashCode())) { return true; }

                //if(transform.IsChildOf(rootBone)) { return true; }
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



        public static bool HasSubmeshes(Transform inspectedObject)
        {
            MeshFilter mFilter = inspectedObject.GetComponent<MeshFilter>();
            SkinnedMeshRenderer sRenderer = inspectedObject.GetComponent<SkinnedMeshRenderer>();

            if(mFilter != null && mFilter.sharedMesh != null)
            {
                return mFilter.sharedMesh.subMeshCount > 1;
            }

            else if(sRenderer != null && sRenderer.sharedMesh != null)
            {
                return sRenderer.sharedMesh.subMeshCount > 1;
            }

            else
            {
                return false;
            }
        }



        public static GameObject CreateTestObj(PrimitiveType type, Vector3 position, Vector3 scale, string name = "")
        {
            var go = GameObject.CreatePrimitive(type);
            go.transform.localScale = scale;
            go.transform.position = position;

            if (name != "") { go.name = name; }

            return go;
        }






        private static Vector3 SubtractAngles(Vector3 rotation1, Vector3 rotation2)
        {

            float xDif = 0;
            float yDif = 0;
            float zDif = 0;

            if (AreAnglesSame(rotation1.x, rotation2.x)) { xDif = 0; }
            else { xDif = rotation1.x - rotation2.x; }

            if (AreAnglesSame(rotation1.y, rotation2.y)) { yDif = 0; }
            else { yDif = rotation1.y - rotation2.y; }

            if (AreAnglesSame(rotation1.z, rotation2.z)) { zDif = 0; }
            else { zDif = rotation1.z - rotation2.z; }

            return new Vector3(xDif, yDif, zDif);
        }



        private static bool AreAnglesSame(float angle1, float angle2)
        {

            if (Mathf.Approximately((Mathf.Cos(angle1) * Mathf.Deg2Rad), (Mathf.Cos(angle2) * Mathf.Deg2Rad)))
            {
                if (Mathf.Approximately((Mathf.Sin(angle1) * Mathf.Deg2Rad), (Mathf.Sin(angle2) * Mathf.Deg2Rad)))
                {
                    //Debug.Log("equal");
                    return true;
                }
            }

            return false;
        }


        public static Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.x = NormalizeAngle(angles.x);
            angles.y = NormalizeAngle(angles.y);
            angles.z = NormalizeAngle(angles.z);
            return angles;
        }


        static float NormalizeAngle(float angle)
        {
            while (angle > 180)
                angle -= 360;

            return angle;
        }


        public static Vector3 Absolute(Vector3 vector)
        {
            return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
        }





        public static GameObject CreateChildCollider(GameObject forObject, bool forPosition, Vector3? newPivotPos, Quaternion? newPivotRot)
        {
            string colliderName = "";

            var colliders = forObject.GetComponents<Collider>();

            if (colliders == null || colliders.Length == 0) { return null; }


#region Setting name of the child collider and disabling original colliders

            ColliderType lastType = ColliderType.None;

            foreach (Collider collider in colliders)
            {

                ColliderType currentType;
                //collider.enabled = false;


                if (collider is BoxCollider)
                {
                    currentType = ColliderType.BoxCollider;
                    colliderName = forObject.name + "_" + Enum.GetName(typeof(ColliderType), currentType);

                    if (lastType != ColliderType.None)
                    {
                        if (lastType != currentType)
                        {
                            colliderName = forObject.name + "_MixedColliders";
                            break;
                        }

                        else
                        {
                            colliderName = forObject.name + "_" + Enum.GetName(typeof(ColliderType), currentType) + "s";
                        }
                    }

                    lastType = currentType;
                }

                else if (collider is CapsuleCollider)
                {
                    currentType = ColliderType.CapsuleCollider;
                    colliderName = forObject.name + "_" + Enum.GetName(typeof(ColliderType), currentType);

                    if (lastType != ColliderType.None)
                    {
                        if (lastType != currentType)
                        {
                            colliderName = forObject.name + "_MixedColliders";
                            break;
                        }

                        else
                        {
                            colliderName = forObject.name + "_" + Enum.GetName(typeof(ColliderType), currentType) + "s";
                        }
                    }

                    lastType = currentType;
                }

                else if (collider is SphereCollider)
                {
                    currentType = ColliderType.SphereCollider;
                    colliderName = forObject.name + "_" + Enum.GetName(typeof(ColliderType), currentType);

                    if (lastType != ColliderType.None)
                    {
                        if (lastType != currentType)
                        {
                            colliderName = forObject.name + "_MixedColliders";
                            break;
                        }

                        else
                        {
                            colliderName = forObject.name + "_" + Enum.GetName(typeof(ColliderType), currentType) + "s";
                        }
                    }

                    lastType = currentType;
                }

                else if (collider is MeshCollider)
                {
                    currentType = ColliderType.MeshCollider;
                    colliderName = forObject.name + "_" + Enum.GetName(typeof(ColliderType), currentType);

                    if (lastType != ColliderType.None)
                    {
                        if (lastType != currentType)
                        {
                            colliderName = forObject.name + "_MixedColliders";
                            break;
                        }

                        else
                        {
                            colliderName = forObject.name + "_" + Enum.GetName(typeof(ColliderType), currentType) + "s";
                        }
                    }

                    lastType = currentType;
                }
            }

#endregion Setting name of the child collider and disabling original colliders


            colliderName = "**" + colliderName + "_DON'T DELETE**";
            GameObject childCollider = UtilityServices.DuplicateGameObject(forObject, colliderName, false, false);

            foreach (Component component in childCollider.GetComponents<Component>())
            {
                if (component is Transform || component is Collider) { continue; }

                DestroyImmediate(component);
            }



            foreach (Collider collider in forObject.GetComponents<Collider>())
            {
                if (collider.enabled && forPosition) { collider.enabled = true; }

                else { collider.enabled = false; }
            }

            childCollider.transform.parent = forObject.transform;
            childCollider.AddComponent<CollRecognize>();

            Collider coll = forObject.GetComponent<Collider>();

            if (coll && coll is MeshCollider)
            {
                if (newPivotPos != null)
                {
                    //childCollider.transform.position = (Vector3)newPivotPos;
                }

                else if (newPivotRot != null)
                {
                    //childCollider.transform.rotation = (Quaternion)newPivotRot;
                }

            }

            return childCollider;


        }





        public static GameObject CreateChildNavMeshObs(GameObject forObject, bool forPosition)
        {
            string navMeshObsName = "";

            var navMeshObstacle = forObject.GetComponent<NavMeshObstacle>();

            if (navMeshObstacle == null) { return null; }


            navMeshObsName = forObject.name + "_NavMeshObstacle";
            navMeshObsName = "**" + navMeshObsName + "_DON'T DELETE**";

            GameObject childNavMeshObs = UtilityServices.DuplicateGameObject(forObject, navMeshObsName, false, false);

            foreach (Component component in childNavMeshObs.GetComponents<Component>())
            {
                if (component is Transform || component is NavMeshObstacle) { continue; }

                DestroyImmediate(component);
            }

            navMeshObstacle.enabled = false;

            childNavMeshObs.transform.position = forObject.transform.position;
            childNavMeshObs.transform.rotation = forObject.transform.rotation;

            childNavMeshObs.transform.parent = forObject.transform;
            childNavMeshObs.AddComponent<NavObsRecognize>();


            return childNavMeshObs;

        }



        public static bool IsUniformScale(Vector3 toCheck)
        {
            return (Mathf.Approximately(toCheck.x, toCheck.y) && Mathf.Approximately(toCheck.y, toCheck.z));
        }


        public static void SaveMeshAsAsset(Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh)
        {

            string path = EditorPrefs.HasKey("savePathPivotModded") ? EditorPrefs.GetString("savePathPivotModded") : DEFAULT_SAVE_PATH;


            string rootPath = path;
            string uniqueParentPath;

            if (!string.IsNullOrWhiteSpace(rootPath))
            {

                if (rootPath.EndsWith("/")) { rootPath.Remove(rootPath.Length - 1, 1); }

                if (AssetDatabase.IsValidFolder(rootPath))
                {
                    rootPath = rootPath + "/";
                }

                else
                {
                    Debug.LogWarning($"The save path: \"{path}\" is not valid or does not exist. A default path will be used to save the modified mesh assets.");
                    rootPath = DEFAULT_SAVE_PATH + "/";
                }

            }

            else
            {
                Debug.LogWarning($"The save path: \"{path}\" is not valid or does not exist. A default path will be used to save the modified assets.");
                rootPath = DEFAULT_SAVE_PATH + "/";
            }



            Mesh meshToSave = (makeNewInstance) ? UnityEngine.Object.Instantiate(mesh) as Mesh : mesh;

            if (optimizeMesh) { MeshUtility.Optimize(meshToSave); }

            if (!AssetDatabase.Contains(mesh))
            {
                rootPath += meshToSave.name + ".mesh";

                uniqueParentPath = AssetDatabase.GenerateUniqueAssetPath(rootPath);

                if (!String.IsNullOrWhiteSpace(uniqueParentPath))
                {
                    rootPath = uniqueParentPath;
                }

                AssetDatabase.CreateAsset(meshToSave, rootPath);
                AssetDatabase.SaveAssets();
            }

        }



        public static string GetValidFolderPath(string folderPath)
        {
            string path = "";

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                //return "Assets/";
                return "";
            }

            path = FileUtil.GetProjectRelativePath(folderPath);


            if (!AssetDatabase.IsValidFolder(path))
            {
                //return "Assets/";
                return "";
            }

            return path;
        }




        public static bool IsPathInAssetsDir(string folderPath)
        {

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return false;
            }

            folderPath = FileUtil.GetProjectRelativePath(folderPath);

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                return false;
            }

            return true;
        }


        public static string SetAndReturnStringPref(string name, string val)
        {
            EditorPrefs.SetString(name, val);
            return val;
        }


        public enum ColliderType
        {
            BoxCollider,
            SphereCollider,
            CapsuleCollider,
            MeshCollider,
            None
        }


        public static void ResetPolyFewToInitialState()
        {
            if (dataContainer != null)
            {
                if (dataContainer.polyFewResetter == null)
                {
                    // If PolyFew Exists
                    if (Type.GetType("BrainFailProductions.PolyFew.PolyFewResetter") != null)
                    {
                        string[] finds = AssetDatabase.FindAssets("PolyFewResetter");

                        if (finds.Length == 1)
                        {
                            string resetterPath = AssetDatabase.GUIDToAssetPath(finds[0]);
                            dataContainer.polyFewResetter = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(resetterPath);
                        }
                    }
                }

                if (dataContainer.resetPolyFewToInitialState == null)
                {
                    // If PolyFew Exists
                    Type resetterType = Type.GetType("BrainFailProductions.PolyFew.PolyFewResetter");

                    if (resetterType != null)
                    {
                        dataContainer.resetPolyFewToInitialState = resetterType.GetMethod("ResetToInitialState", BindingFlags.Public | BindingFlags.Static);
                    }
                }

                if (dataContainer.polyFewResetter != null && dataContainer.resetPolyFewToInitialState != null)
                {
                    // If PolyFew Exists
                    if (Type.GetType("BrainFailProductions.PolyFew.PolyFewResetter") != null)
                    {
                        dataContainer.resetPolyFewToInitialState.Invoke(dataContainer.polyFewResetter, null);
                    }
                }
            }
        }


        public static void RefreshPolyFewObjectMeshPairs(GameObject forObject)
        {
            if (dataContainer != null)
            {
                if (dataContainer.polyFewResetter == null)
                {
                    // If PolyFew Exists
                    if (Type.GetType("BrainFailProductions.PolyFew.PolyFewResetter") != null)
                    {
                        string[] finds = AssetDatabase.FindAssets("PolyFewResetter");

                        if (finds.Length == 1)
                        {
                            string resetterPath = AssetDatabase.GUIDToAssetPath(finds[0]);
                            dataContainer.polyFewResetter = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(resetterPath);
                        }
                    }
                }
                
                if (dataContainer.refreshPolyFewObjectMeshPairs == null)
                {
                    // If PolyFew Exists
                    Type resetterType = Type.GetType("BrainFailProductions.PolyFew.PolyFewResetter");

                    if (resetterType != null)
                    {
                        dataContainer.refreshPolyFewObjectMeshPairs = resetterType.GetMethod("RefreshObjectMeshPairs", BindingFlags.Public | BindingFlags.Static);
                    }
                }

                if (dataContainer.polyFewResetter != null && dataContainer.refreshPolyFewObjectMeshPairs != null)
                {
                    // If PolyFew Exists
                    if (Type.GetType("BrainFailProductions.PolyFew.PolyFewResetter") != null)
                    {
                        if(forObject == null) { return; }
                        dataContainer.refreshPolyFewObjectMeshPairs.Invoke(dataContainer.polyFewResetter, new System.Object[] { forObject });
                    }
                }
            }
        }


        /*
        public static GameObject DuplicateGameObject(string newName, bool duplicateFromRoot, bool duplicateChildren)
        {
            if (Selection.activeGameObject == null) { return null; }

            GameObject selectedObject = Selection.activeGameObject;
            GameObject duplicate = null;

            string name = Selection.activeGameObject.name;

            GameObject rootParent = (GameObject)PrefabUtility.GetPrefabParent(Selection.activeGameObject);
            if (duplicateFromRoot) { Selection.activeGameObject = rootParent; }


            SceneView.lastActiveSceneView.Focus();
            EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));

            duplicate = Selection.activeGameObject;
            Selection.activeGameObject.name = newName;
            Selection.activeGameObject = selectedObject;

            if (!duplicateChildren)
            {
                foreach (Transform child in duplicate.transform) { DestroyImmediate(child.gameObject); }
            }

            return duplicate;
        }
        */
    }

}


#endif