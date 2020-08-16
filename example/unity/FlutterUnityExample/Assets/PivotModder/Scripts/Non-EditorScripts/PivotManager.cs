#if UNITY_EDITOR


using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.AI;


namespace BrainFailProductions.PivotModder
{


    public class PivotManager : MonoBehaviour
    {

        //static bool destroyColliderOnRotation = false;
        //static bool createChildCollider = false;
        static GameObject selectedObject;
        static Mesh selectedObjectMesh;
        static Vector3 selectedObjCenterLocalSpace;
        static UtilityServices.ChildStateTuple[] childrenStatesBeforePivotMod;
        static UtilityServices.ColliderState collStateBeforeMod;
        //static Dictionary<int, ObjectColliderPair> objectsColliders = new Dictionary<int, ObjectColliderPair>();
        //static Dictionary<int, byte> keyEntriesToDelete = new Dictionary<int, byte>();
        static string meshName;
        static Resources resources;


        struct Resources
        {
            //#pragma warning disable CS0649
#pragma warning disable
            public Texture componentIcon;
        }



        struct ObjectColliderPair
        {
            public GameObject parentObject;
#pragma warning disable CS0649
            public GameObject childObject;
            public UtilityServices.ColliderState firstState;
        }









        /// <summary>
        /// When a selection change notification is received - this is an Editor predefined function.
        /// </summary>
        static void OnSelectionChange()
        {
            RecognizeObject(Selection.activeGameObject);
        }




        static bool CheckObject(GameObject obj)
        {
            if (!obj)
            {
                Debug.Log("<b><i><color=#ff8000ff>No object selected in Hierarchy.</color></i></b>");
                return false;
            }
            if (!selectedObjectMesh)
            {
                Debug.Log("<b><i><color=#ff8000ff>Selected object does not have a Mesh specified.</color></i></b>");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gather references for the selected object and its components
        ///  and update the pivot vector if the object has a Mesh.
        /// </summary>
        static void RecognizeObject(GameObject forObject)
        {
            //selectedObjectMesh = null;  Baw did

            Transform recognizedTransform = forObject.transform;
            if (recognizedTransform)
            {

                if (forObject)
                {

                    MeshFilter selectedObjectMeshFilter = forObject.GetComponent<MeshFilter>();

                    if (selectedObjectMeshFilter)
                    {

                        meshName = selectedObjectMeshFilter.sharedMesh.name;
                        string path = AssetDatabase.GetAssetPath(selectedObjectMeshFilter.sharedMesh);

                        if (AssetDatabase.Contains(selectedObjectMeshFilter.sharedMesh) && !AssetDatabase.IsSubAsset(selectedObjectMeshFilter.sharedMesh) && !path.Substring(0, 7).Trim().ToLower().Equals("library"))
                        {
                            selectedObjectMesh = selectedObjectMeshFilter.sharedMesh;
                        }

                        else
                        {
                            if (meshName.Contains("_(-PIVOT_MODDED-c5da29ef-df25-4a8f-beb3-c6c32c5c75a8-)"))
                            {
                                meshName = meshName.Split(new string[] { "_(-PIVOT_MODDED-c5da29ef-df25-4a8f-beb3-c6c32c5c75a8-)" }, StringSplitOptions.None)[0];
                                meshName = meshName + "_(-PIVOT_MODDED-c5da29ef-df25-4a8f-beb3-c6c32c5c75a8-)";
                            }

                            else { meshName = meshName + "_(-PIVOT_MODDED-c5da29ef-df25-4a8f-beb3-c6c32c5c75a8-)"; }

                            Mesh meshCopy = Instantiate(selectedObjectMeshFilter.sharedMesh) as Mesh;
                            selectedObjectMesh = selectedObjectMeshFilter.sharedMesh = meshCopy;
                            selectedObjectMesh.name = meshName;
                        }

                    }

                }
            }
        }


        public static void MovePivot(GameObject forObject, Vector3 moveTo, bool preserveState)
        {

            RecognizeObject(forObject);

            if (!CheckObject(forObject)) { return; }

            NavMeshObstacle obs = forObject.GetComponent<NavMeshObstacle>();
            NavMeshObstacleShape shape = NavMeshObstacleShape.Box;

            if (preserveState)
            {

                var collRecognize = forObject.GetComponentsInChildren<CollRecognize>();
                var navObsRecognize = forObject.GetComponentsInChildren<NavObsRecognize>();

                if (collRecognize == null || collRecognize.Length == 0)
                {
                    GameObject childColl = UtilityServices.CreateChildCollider(forObject, true, moveTo, null);
                    if (childColl) Debug.Log(string.Format("<b><i><color=#0080ffff>The Collider(s) on \"{0}\" has been disabled.Due to pivot modification colliders get incorrectly oriented.To preserve the same collider orientation as the one before pivot modification, the original collider has been added to a child GameObject \"{1}\" use that as the collider for this GameObject. Please don't delete this child object otherwise any subsequent changes made to modify pivot cannot guarantee correct collider orientation. </color></i></b>", forObject.name, childColl.name));
                }


                if (navObsRecognize == null || navObsRecognize.Length == 0)
                {
                    GameObject childNavObs = UtilityServices.CreateChildNavMeshObs(forObject, true);
                    if (childNavObs) Debug.Log(string.Format("<b><i><color=#0080ffff>The NavMeshObstacle on \"{0}\" has been disabled.Due to pivot modifcation NavMeshObstacles get incorrectly oriented.To preserve the same orientation as the one before pivot modification, the original NavMeshObstacle has been added to a child GameObject \"{1}\" use that as the NavMeshObstacle for this GameObject. Please don't delete this child object otherwise any subsequent changes made to modify pivot cannot guarantee correct NavMeshObstacle orientation. </color></i></b>", forObject.name, childNavObs.name));
                }


                // Save children positions before pivotal modifications
                childrenStatesBeforePivotMod = UtilityServices.SaveChildrenStates(forObject);

                // Save the position and rotation of the collider before pivotal modification
                collStateBeforeMod = UtilityServices.SaveColliderState(forObject);

                if (obs)
                {
                    shape = obs.shape;
                    if (shape == NavMeshObstacleShape.Box) { obs.shape = NavMeshObstacleShape.Capsule; }
                    else { obs.shape = NavMeshObstacleShape.Box; }
                }
            }




            Vector3[] vertices = selectedObjectMesh.vertices;

            //Vector3 objCenterWorldSpace = selectedObject.transform.TransformPoint(selectedObjCenterLocalSpace);   // The world space position where the pivot will be moved and centered to the object

            Vector3 pivotOffsetLocalSpace = forObject.transform.InverseTransformVector(forObject.transform.position - moveTo);

            Matrix4x4 transformationMatrix = Matrix4x4.Translate(pivotOffsetLocalSpace);

            //var stopWatch = new System.Diagnostics.Stopwatch();
            //stopWatch.Start();


            for (int a = 0; a < vertices.Length; a++)
            {
                vertices[a] = transformationMatrix.MultiplyPoint3x4(vertices[a]);
            }


            //stopWatch.Stop();
            //UnityEngine.Debug.Log("Editor time for pivot centralization  " + stopWatch.ElapsedMilliseconds + "  for iterations  " + vertices.Length);

            selectedObjectMesh.vertices = vertices;    //Assign the vertex array back to the mesh

            selectedObjectMesh.RecalculateBounds();



            forObject.transform.position = moveTo;   // Move the object to the previous position


            if (preserveState)
            {
                if (obs) { obs.shape = shape; }

                UtilityServices.RestoreColliderState(forObject, collStateBeforeMod);
                // Restore children positions and rotations as they were before pivotal modifications
                //new UtilityServices().RunAfter(()=> { RestoreChildrenStates();  }, new WaitForEndOfFrame);
                UtilityServices.RestoreChildrenStates(childrenStatesBeforePivotMod);
            }


        }



        public static void RotatePivot(GameObject forObject, Quaternion newRotation, bool preserveState)
        {

            RecognizeObject(forObject);

            if (!CheckObject(forObject)) { return; }

            
            // Transform.TransformPoint and InversetransformPoint have high performance implications


            if (preserveState)
            {

                var collRecognize = forObject.GetComponentsInChildren<CollRecognize>();
                var navObsRecognize = forObject.GetComponentsInChildren<NavObsRecognize>();


                if (collRecognize == null || collRecognize.Length == 0)
                {
                    GameObject childColl = UtilityServices.CreateChildCollider(forObject, false, null, newRotation);
                    if (childColl) Debug.Log(string.Format("<b><i><color=#0080ffff>The Collider(s) on \"{0}\" has been disabled.Due to pivot modification colliders get incorrectly oriented.To preserve the same collider orientation as the one before pivot modification, the original collider has been added to a child GameObject \"{1}\" use that as the collider for this GameObject. Please don't delete this child object otherwise any subsequent changes made to modify pivot cannot guarantee correct collider orientation. </color></i></b>", forObject.name, childColl.name));
                }


                if (navObsRecognize == null || navObsRecognize.Length == 0)
                {
                    GameObject childNavObs = UtilityServices.CreateChildNavMeshObs(forObject, true);
                    if (childNavObs) Debug.Log(string.Format("<b><i><color=#0080ffff>The NavMeshObstacle on \"{0}\" has been disabled.Due to pivot modifcation NavMeshObstacles get incorrectly oriented.To preserve the same orientation as the one before pivot modification, the original NavMeshObstacle has been added to a child GameObject \"{1}\" use that as the NavMeshObstacle for this GameObject. Please don't delete this child object otherwise any subsequent changes made to modify pivot cannot guarantee correct NavMeshObstacle orientation. </color></i></b>", forObject.name, childNavObs.name));
                }

                // Save children positions before pivotal modifications
                childrenStatesBeforePivotMod = UtilityServices.SaveChildrenStates(forObject);
                // Save the postion and rotation of the collider before pivotal modification
                //SaveColliderState();

            }



            GameObject tempObject = new GameObject();
            tempObject.transform.parent = forObject.transform;
            tempObject.transform.rotation = newRotation;
            Quaternion inverseRotation = Quaternion.Inverse(tempObject.transform.localRotation);


            Quaternion prevRotation = forObject.transform.rotation;
            DestroyImmediate(tempObject);


            //Vector3 oldScale = forObject.transform.localScale;
            //forObject.transform.localScale = new Vector3(1, 1, 1);

            Matrix4x4 transformationMatrix = Matrix4x4.Rotate(inverseRotation);

            forObject.transform.rotation = newRotation;  

            Vector3[] vertices = selectedObjectMesh.vertices;
            Vector3[] normals = selectedObjectMesh.normals;

            
            //Debug.Log("submeshes count  " + forObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount + "  vertex count   "  + vertices.Length);


            for (int a = 0; a < vertices.Length; a++)
            {
                vertices[a] = transformationMatrix.MultiplyPoint3x4(vertices[a]);
                normals[a]  = transformationMatrix.MultiplyPoint3x4(normals[a]);
            }

            selectedObjectMesh.vertices = vertices;
            selectedObjectMesh.normals = normals;

            selectedObjectMesh.RecalculateBounds();



            if (preserveState)
            {
                // Restore children positions and rotations as they were before pivotal modifications
                //new UtilityServices().RunAfter(() => { RestoreChildrenStates(); }, new WaitForEndOfFrame);
                UtilityServices.RestoreChildrenStates(childrenStatesBeforePivotMod);
            }


        }



        /// <summary> Saves the world space positions and rotations of all child transforms of the provided object. </summary>

        /*
    static void SaveChildrenStates(GameObject forObject)
    {

        var children = GetTopLevelChildren(forObject.transform);

        childrenStatesBeforePivotMod = new ChildStateTuple[children.Length];

        for (int a = 0; a < children.Length; a++)
        {
            childrenStatesBeforePivotMod[a].transform = children[a];
            childrenStatesBeforePivotMod[a].position  = children[a].position;
            childrenStatesBeforePivotMod[a].rotation  = children[a].rotation;
        }

    }
    */






        /// <summary> Gets the central point of the object in its localSpace. </summary

        public static Vector3 GetMeshLocalCenterPoint(Mesh objectMesh)
        {

            Vector3 center = objectMesh.bounds.center;     // mesh.bounds.center returns the mesh bounding box center point in local space

            return center;
        }




        /// <summary> Gets the central point of the object in its localSpace. </summary

        public static Vector3 GetMeshWorldCenterPoint(Mesh objectMesh, Transform target)
        {

            Vector3 center = objectMesh.bounds.center;     // mesh.bounds.center returns the mesh bounding box center point in local space

            Vector3 objCenterWorldSpace = target.TransformPoint(center);   // The world space position where the pivot will be moved and centered to the object

            return objCenterWorldSpace;

        }




        public static void MovePivotForMeshless(GameObject meshlessObject, Vector3 newPosition, bool preserveState)
        {
            if (preserveState)
            {
                childrenStatesBeforePivotMod = UtilityServices.SaveChildrenStates(meshlessObject);
            }

            meshlessObject.transform.position = newPosition;

            //new UtilityServices().RunAfter(() => { RestoreChildrenStates(); }, new WaitForEndOfFrame());

            if (preserveState)
            {
                UtilityServices.RestoreChildrenStates(childrenStatesBeforePivotMod);
            }
        }




        public static void RotatePivotForMeshless(GameObject meshlessObject, Quaternion newRotation, bool preserveState)
        {

            if (preserveState)
            {
                childrenStatesBeforePivotMod = UtilityServices.SaveChildrenStates(meshlessObject);
            }

            meshlessObject.transform.rotation = newRotation;

            //new UtilityServices().RunAfter(()=> { RestoreChildrenStates(); }, new WaitForEndOfFrame());

            if (preserveState)
            {
                UtilityServices.RestoreChildrenStates(childrenStatesBeforePivotMod);
            }
        }


    }

}

#endif