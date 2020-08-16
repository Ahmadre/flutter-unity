using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;
using UnityEditor.Callbacks;
//using static UtilityServices;
using UnityEditor.SceneManagement;

namespace BrainFailProductions.PivotModder
{



    [CustomEditor(typeof(PivotModderHost))]
    public class InspectorDrawer : Editor
    {

        //Unity's built-in editor
        Editor defaultTransformEditor;
        Transform transform;

        static Texture icon;
        static Vector3 positionWorld = Vector3.zero;
        static Quaternion rotationWorld = Quaternion.identity;
        static Quaternion handleRotation = Quaternion.identity;
        static float adjustmentFactorPos = 0.1f;
        static float adjustmentFactorRot = 15;
        static bool toolMainFoldout = true;
        //static bool preserveActualMesh = true;
        static bool allowPivotMod = false;
        static bool isPositionSelected = true;
        private bool isPressed;
        private UtilityServices.HandleOrientation handleOrientation;
        //private Tool userSelectedTool;
        private bool isMeshlessObject;
        private static string iconPath = "Assets/PivotModder/icons/";
        private static bool snapToVertex;
        private static bool snapToGrid;
        private Vector3 newHandlePos;
        private Quaternion newHandleRot;
        private Vector3? currentSnapPoint = null;
        //private Quaternion? currentSnapRot = null;
        private static Vector3 gridPosSnapValues = new Vector3(1, 1, 1);
        private static Vector3 gridRotSnapValues = new Vector3(15, 15, 15);

        private bool gotSnapped;
        private static float snapMovementThreshold = 0.2f;
        //private Vector3 currentMousePos;
        //private Vector3 prevMousePos;
        private Type snapType = null;
        private PropertyInfo rotSnap = null;
        private PropertyInfo movSnap = null;
        private float tempFloat;
        private Vector3 tempVector3;
        private bool isVersionOk;
        private GameObject thisGameObject;

        private bool freshChangesMade;
        private bool doRepaint;
        private bool isBone;
        private bool isMorphed;
        private static Transform PrevSelection { get { return UtilityServices.dataContainer.prevSelection; } set { UtilityServices.dataContainer.prevSelection = value; } }


        public void OnDestroy()
        {

            bool sameTarget = PrevSelection && Selection.activeTransform && Selection.activeTransform.GetHashCode().Equals(PrevSelection.GetHashCode());

            if (!sameTarget)
            {
                if (thisGameObject != null)
                {
                    PivotModderHost host = thisGameObject.GetComponent<PivotModderHost>();
                    if (host != null) { DestroyImmediate(host); }
                }
            }



            if (Application.isEditor && thisGameObject == null && !Application.isPlaying)
            {
                int key = thisGameObject.GetHashCode();

                if (UtilityServices.dataContainer.objectsHistory.ContainsKey(key))
                {
                    //Debug.Log("Destroyed Undo Histroy for object");
                    UtilityServices.UndoRedoOps ops;

                    if (UtilityServices.dataContainer.objectsHistory.TryGetValue(key, out ops))
                    {
                        ops.Destruct();
                        UtilityServices.dataContainer.objectsHistory.Remove(key);
                    }

                }
            }
        }



        static float SetAndReturnfloatPref(string name, float val)
        {
            EditorPrefs.SetFloat(name, val);
            return val;
        }


        static int SetAndReturnIntPref(string name, int val)
        {
            EditorPrefs.SetInt(name, val);
            return val;
        }


        static bool SetAndReturnBoolPref(string name, bool val)
        {
            EditorPrefs.SetBool(name, val);
            return val;
        }


        static string SetAndReturnStringPref(string name, string val)
        {
            EditorPrefs.SetString(name, val);
            return val;
        }


        static Vector3 SetAndReturnVectorPref(string nameX, string nameY, string nameZ, Vector3 value)
        {
            EditorPrefs.SetFloat(nameX, value.x);
            EditorPrefs.SetFloat(nameY, value.y);
            EditorPrefs.SetFloat(nameZ, value.z);

            return value;
        }



        void OnEnable()
        {

            //When this inspector is created, also create the built-in inspector
            //defaultTransformEditor = Editor.CreateEditor(targets, Type.GetType("UnityEditor.TransformInspector, UnityEditor"));
            //transform = target as Transform;


            snapMovementThreshold = EditorPrefs.HasKey("snapMovementThreshold") ? EditorPrefs.GetFloat("snapMovementThreshold") : SetAndReturnfloatPref("snapMovementThreshold", snapMovementThreshold);
            gridPosSnapValues = EditorPrefs.HasKey("gridPosSnapValuesX") ? new Vector3(EditorPrefs.GetFloat("gridPosSnapValuesX"), EditorPrefs.GetFloat("gridPosSnapValuesY"), EditorPrefs.GetFloat("gridPosSnapValuesZ")) : SetAndReturnVectorPref("gridPosSnapValuesX", "gridPosSnapValuesY", "gridPosSnapValuesZ", gridPosSnapValues);
            gridRotSnapValues = EditorPrefs.HasKey("gridRotSnapValuesX") ? new Vector3(EditorPrefs.GetFloat("gridRotSnapValuesX"), EditorPrefs.GetFloat("gridRotSnapValuesY"), EditorPrefs.GetFloat("gridRotSnapValuesZ")) : SetAndReturnVectorPref("gridRotSnapValuesX", "gridRotSnapValuesY", "gridRotSnapValuesZ", gridRotSnapValues);
            adjustmentFactorPos = EditorPrefs.HasKey("adjustmentFactorPos") ? EditorPrefs.GetFloat("adjustmentFactorPos") : SetAndReturnfloatPref("adjustmentFactorPos", adjustmentFactorPos);
            adjustmentFactorRot = EditorPrefs.HasKey("adjustmentFactorRot") ? EditorPrefs.GetFloat("adjustmentFactorRot") : SetAndReturnfloatPref("adjustmentFactorRot", adjustmentFactorRot);
            string version = Application.unityVersion.Trim();

            isVersionOk = version.Contains("2017.1") || version.Contains("2017.2") ? false : true;
            if (version.Contains("2015")) { isVersionOk = false; }

            Selection.selectionChanged -= SelectionChanged;
            Selection.selectionChanged += SelectionChanged;


            thisGameObject = Selection.activeGameObject;

            //e2099408-4216-4c3b-bb32-49fc6c34a7a0
            UtilityServices.containerObject = GameObject.Find("e2099408-4216-4c3b-bb32-49fc6c34a7a0");


            if (!UtilityServices.containerObject)
            {
                //Debug.Log("Creating container object");
                UtilityServices.containerObject = new GameObject();
                UtilityServices.containerObject.name = "e2099408-4216-4c3b-bb32-49fc6c34a7a0";
                UtilityServices.containerObject.hideFlags = HideFlags.HideAndDontSave;
                UtilityServices.containerObject.AddComponent<DataContainer>();
                //DontDestroyOnLoad(containerObject);
            }

            UtilityServices.dataContainer = UtilityServices.containerObject.GetComponent<DataContainer>();

            if (UtilityServices.dataContainer.objectsHistory == null)
            {
                UtilityServices.dataContainer.objectsHistory = new ObjectsHistory();
            }


            SelectionChanged();
        }


        void OnDisable()
        {

            //MethodInfo disableMethod = defaultTransformEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            //if (disableMethod != null) { disableMethod.Invoke(defaultTransformEditor, null); }

            //DestroyImmediate(defaultTransformEditor);


            Selection.selectionChanged -= SelectionChanged;
        }



        void OnSceneGUI()
        {

            doRepaint = false;

            if (snapType == null)
            {
                snapType = AppDomain.CurrentDomain.GetAssemblies()

                .SelectMany(assembly => assembly.GetTypes())

                .FirstOrDefault(t => t.FullName == "UnityEditor.SnapSettings");

                rotSnap = PrivateValueAccessor.GetPrivatePropertyInfo(snapType, "rotation");
                movSnap = PrivateValueAccessor.GetPrivatePropertyInfo(snapType, "move");

            }

            if (HandleControlsUtility.handleControls == null) { HandleControlsUtility.handleControls = new HandleControlsUtility(); return; }

            bool flag = false;

            if (!Selection.activeTransform || !Selection.activeTransform.gameObject.activeInHierarchy)
            {
                return;
            }

            if (!Selection.activeTransform.GetComponent<MeshRenderer>())
            {
                flag = true;
            }

            if (!flag && !Selection.activeTransform.GetComponent<MeshRenderer>().enabled)
            {
                flag = true;
            }

            if (!flag && !Selection.activeTransform.GetComponent<MeshFilter>())
            {
                flag = true;
            }

            if (!flag && !Selection.activeTransform.GetComponent<MeshFilter>().sharedMesh)
            {
                flag = true;
            }



            if (flag)
            {
                if (UtilityServices.CheckIfMeshless(Selection.activeTransform)) { isMeshlessObject = true; }
                else { return; }
            }



            var activeSelection = Selection.activeTransform;

            if (Tools.current == Tool.Move) { isPositionSelected = true; }
            if (Tools.current == Tool.Rotate) { isPositionSelected = false; }


            //currentMousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;


            if (Selection.activeTransform.hasChanged)
            {
                Selection.activeTransform.hasChanged = false;
                positionWorld = Selection.activeTransform.position;
                rotationWorld = Selection.activeTransform.rotation;
                //Repaint();
                newHandlePos = positionWorld;
                newHandleRot = rotationWorld;

            }


            if (!allowPivotMod) { Tools.hidden = false; }

            if (activeSelection != null && allowPivotMod)
            {
                Tools.hidden = true;

                if (Tools.current == Tool.Move && allowPivotMod)
                {

                    isPositionSelected = true;

                    if (handleOrientation == UtilityServices.HandleOrientation.localAligned)
                    {
                        handleRotation = new Quaternion(rotationWorld.x, rotationWorld.y, rotationWorld.z, rotationWorld.w);
                    }
                    else
                    {
                        handleRotation = Quaternion.identity;
                    }

                    #region Draw custom handles for position

                    if (Event.current.type == EventType.Repaint)
                    {
                        Handles.color = UtilityServices.HexToColor("#fcd116");

                        Handles.SphereHandleCap(0, positionWorld, handleRotation, HandleUtility.GetHandleSize(positionWorld) / 3.5f, EventType.Repaint);
                    }


                    if (!snapToGrid && !snapToVertex)
                    {
                        Vector3 oldPosition = positionWorld;

                        positionWorld = Handles.DoPositionHandle(positionWorld, handleRotation);
                        newHandlePos = positionWorld;

                        //if (oldPosition != positionWorld) { Repaint(); } 
                        if (oldPosition != positionWorld) { doRepaint = true; }
                    }


                    else if (snapToVertex && !UtilityServices.CheckIfMeshless(Selection.activeTransform) && isPositionSelected)
                    {

                        Mesh mesh = Selection.activeTransform.GetComponent<MeshFilter>().sharedMesh;
                        Vector3 closestVertex = Vector3.zero;
                        Vector3 oldHandlePos = newHandlePos;

                        newHandlePos = Handles.DoPositionHandle(newHandlePos, handleRotation);


                        if (!oldHandlePos.Equals(newHandlePos))
                        {


                            closestVertex = UtilityServices.GetClosestVertex(newHandlePos, mesh, Selection.activeTransform);

                            float distFromCurrentSnapPoint = Mathf.Infinity;
                            //currentMousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;



                            if (currentSnapPoint != null) { distFromCurrentSnapPoint = Vector3.Distance(newHandlePos, (Vector3)currentSnapPoint); }
                            //distFromCurrentSnapPoint = Vector2.Distance(Event.current.delta, currentMousePos); 

                            //float d1 = Vector3.Distance(closestVertex, prevMousePos);
                            //float d2 = Vector3.Distance(closestVertex, currentMousePos);

                            //Debug.Log("d1 Close to old " +d1 + "  d2 Close to new " + d2   +  "  prevMousePos  "+prevMousePos.ToString("F4") + "  current mouse pos  " + currentMousePos.ToString("F4") + "  DistFromCurrSnap   " +distFromCurrentSnapPoint + "  Delta  " + Event.current.delta.magnitude);
                            if (distFromCurrentSnapPoint >= snapMovementThreshold && Event.current.delta.magnitude >= 2)
                            //if (distFromCurrentSnapPoint >= snapMovementThreshold && (d1 >= d2) && Event.current.delta.magnitude >= 2)
                            {
                                // Snapping happens here
                                //Debug.Log("Me run closest vertex is  " + closestVertex.ToString("F4") + "  currentMousePos  " + currentMousePos.ToString("F4") + "  old Mouse POS   " + prevMousePos.ToString("F4") + "  Distance close to old  " + d1 + "  Distance close to new  " +d2);
                                positionWorld = closestVertex;
                                newHandlePos = closestVertex;
                                currentSnapPoint = closestVertex;

                            }

                            else if (currentSnapPoint != null)
                            {
                                newHandlePos = (Vector3)currentSnapPoint;
                                positionWorld = newHandlePos; //baw did

                            }

                            else
                            {
                                newHandlePos = positionWorld;
                            }

                        }

                        //prevMousePos = new Vector3(currentMousePos.x, currentMousePos.y, currentMousePos.z);



                        //if (oldHandlePos != positionWorld) { Repaint(); }  
                        if (oldHandlePos != positionWorld) doRepaint = true;
                    }


                    else if (snapToGrid && isPositionSelected && allowPivotMod)
                    {


                        Vector3 pointToSnapTo = Vector3.zero;
                        Vector3 oldHandlePos = newHandlePos;
                        Vector3 originalSnapVal = Vector3.zero;
                        Vector3? customSnapVal = null;
                        var selectedControl = HandleControlsUtility.handleControls.GetCurrentSelectedControl();
                        HandleControlsUtility.HandleType handleType = HandleControlsUtility.handleControls.GetHandleType(selectedControl);
                        bool flag1 = false;



                        if (selectedControl == HandleControlsUtility.HandleControls.xAxisMoveHandle)
                        {
                            customSnapVal = new Vector3(gridPosSnapValues.x, 0, 0);
                        }

                        else if (selectedControl == HandleControlsUtility.HandleControls.yAxisMoveHandle)
                        {
                            customSnapVal = new Vector3(0, gridPosSnapValues.y, 0);
                        }

                        else if (selectedControl == HandleControlsUtility.HandleControls.zAxisMoveHandle)
                        {
                            customSnapVal = new Vector3(0, 0, gridPosSnapValues.z);
                        }

                        else if (selectedControl == HandleControlsUtility.HandleControls.xyAxisMoveHandle)
                        {
                            customSnapVal = new Vector3(gridPosSnapValues.x, gridPosSnapValues.y, 0);
                        }

                        else if (selectedControl == HandleControlsUtility.HandleControls.xzAxisMoveHandle)
                        {
                            customSnapVal = new Vector3(gridPosSnapValues.x, 0, gridPosSnapValues.z);
                        }

                        else if (selectedControl == HandleControlsUtility.HandleControls.yzAxisMoveHandle)
                        {
                            customSnapVal = new Vector3(0, gridPosSnapValues.y, gridPosSnapValues.z);
                        }

                        else if (selectedControl == HandleControlsUtility.HandleControls.allAxisMoveHandle)
                        {
                            customSnapVal = gridPosSnapValues;
                        }


                        if (!isVersionOk)
                        {
                            customSnapVal = (Vector3)PrivateValueAccessor.GetPrivatePropertyValue(snapType, "move", null);
                        }


                        else if (handleType == HandleControlsUtility.HandleType.position && customSnapVal != null)
                        {
                            flag1 = true;
                            originalSnapVal = (Vector3)PrivateValueAccessor.GetPrivatePropertyValue(snapType, "move", null);
                            movSnap.SetValue(snapType, customSnapVal, null);
                        }


                        if (customSnapVal != null)
                        {
                            if (Application.platform == RuntimePlatform.OSXEditor) { Event.current.command = true; }
                            else { Event.current.control = true; }
                        }


                        newHandlePos = Handles.DoPositionHandle(newHandlePos, handleRotation);


                        if (!oldHandlePos.Equals(newHandlePos))
                        {
                            currentSnapPoint = newHandlePos;
                            positionWorld = newHandlePos;
                            //Repaint();  
                            doRepaint = true;
                        }


                        if (customSnapVal != null)
                        {
                            if (Application.platform == RuntimePlatform.OSXEditor) { Event.current.command = false; }
                            else { Event.current.control = false; }
                        }


                        if (flag1 || handleType == HandleControlsUtility.HandleType.position)
                        {
                            //positionWorld = newHandlePos;

                            if (flag1)
                            {
                                movSnap.SetValue(snapType, originalSnapVal, null);
                            }

                            //Repaint();  
                            doRepaint = true;
                        }

                        //prevMousePos = new Vector3(currentMousePos.x, currentMousePos.y, currentMousePos.z);

                    }

                }


                #endregion Draw custom handles for position


            }

            if (Tools.current == Tool.Rotate && allowPivotMod)
            {

                isPositionSelected = false;

                if (!snapToGrid && !snapToVertex)
                {
                    Vector3 oldRotation = rotationWorld.eulerAngles;

                    if (Event.current.type == EventType.Repaint)
                    {
                        Handles.color = UtilityServices.HexToColor("#fcd116");

                        Handles.SphereHandleCap(0, positionWorld, handleRotation, HandleUtility.GetHandleSize(positionWorld) / 3.5f, EventType.Repaint);
                    }


                    rotationWorld = Handles.DoRotationHandle(rotationWorld, positionWorld);
                    newHandleRot = rotationWorld;
                    Handles.DoPositionHandle(positionWorld, rotationWorld);


                    //if(!oldRotation.Equals(rotationWorld.eulerAngles)) { Repaint(); }  
                    if (!oldRotation.Equals(rotationWorld.eulerAngles)) { doRepaint = true; }
                }


                else if (snapToGrid && !isPositionSelected)
                {

                    Quaternion oldHandleRot = newHandleRot;

                    float rotationSnapVal = 0;
                    float customSnapVal = -1;
                    var selectedControl = HandleControlsUtility.handleControls.GetCurrentSelectedControl();
                    HandleControlsUtility.HandleType handleType = HandleControlsUtility.handleControls.GetHandleType(selectedControl);
                    bool flag1 = false;



                    if (selectedControl == HandleControlsUtility.HandleControls.xAxisRotateHandle)
                    {
                        customSnapVal = gridRotSnapValues.x;
                    }

                    else if (selectedControl == HandleControlsUtility.HandleControls.yAxisRotateHandle)
                    {
                        customSnapVal = gridRotSnapValues.y;
                    }

                    else if (selectedControl == HandleControlsUtility.HandleControls.zAxisRotateHandle)
                    {
                        customSnapVal = gridRotSnapValues.z;
                    }


                    if (!isVersionOk)
                    {
                        customSnapVal = (float)PrivateValueAccessor.GetPrivatePropertyValue(snapType, "rotation", null);
                        rotationWorld = newHandleRot;
                    }


                    else if (handleType == HandleControlsUtility.HandleType.rotation && customSnapVal > 0)
                    {
                        flag1 = true;
                        rotationSnapVal = (float)PrivateValueAccessor.GetPrivatePropertyValue(snapType, "rotation", null);
                        rotSnap.SetValue(snapType, customSnapVal, null);
                    }


                    if (customSnapVal != 0)
                    {
                        if (Application.platform == RuntimePlatform.OSXEditor) { Event.current.command = true; }
                        else { Event.current.control = true; }
                    }


                    newHandleRot = Handles.DoRotationHandle(newHandleRot, positionWorld);
                    Handles.DoPositionHandle(positionWorld, rotationWorld);


                    if (!oldHandleRot.Equals(newHandleRot))
                    {
                        //currentSnapRot = newHandleRot;
                        rotationWorld = newHandleRot;

                        //Repaint();  
                        doRepaint = true;
                    }

                    if (customSnapVal != 0)
                    {
                        if (Application.platform == RuntimePlatform.OSXEditor) { Event.current.command = false; }
                        else { Event.current.control = false; }
                    }


                    if (flag1 || handleType == HandleControlsUtility.HandleType.rotation)
                    {
                        rotationWorld = newHandleRot;

                        if (flag1)
                        {
                            rotSnap.SetValue(snapType, rotationSnapVal, null);
                        }

                        //Repaint();  
                        doRepaint = true;
                    }

                    //prevMousePos = new Vector3(currentMousePos.x, currentMousePos.y, currentMousePos.z);

                }


                //if (oldRotation.Equals(rotationWorld.eulerAngles)) { Repaint(); }

            }


            if (doRepaint) { Repaint(); }

        }



        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();

            //defaultTransformEditor.OnInspectorGUI();

            isMeshlessObject = false;

            bool flag = false;

            if (!Selection.activeTransform || !Selection.activeTransform.gameObject.activeInHierarchy)
            {
                return;
            }

            if (!flag && !Selection.activeTransform.GetComponent<MeshRenderer>())
            {
                flag = true;
            }

            if (!flag && !Selection.activeTransform.GetComponent<MeshRenderer>().enabled)
            {
                flag = true;
            }

            if (!flag && !Selection.activeTransform.GetComponent<MeshFilter>())
            {
                flag = true;
            }

            if (!flag && !Selection.activeTransform.GetComponent<MeshFilter>().sharedMesh)
            {
                flag = true;
            }


            if (flag)
            {
                if (UtilityServices.CheckIfMeshless(Selection.activeTransform)) { isMeshlessObject = true; }
                else { return; }
            }



            if (isBone) { return; }
            if(isMorphed) { return; }

            if (allowPivotMod) { Tools.hidden = true; }
            else { Tools.hidden = false; }




            toolMainFoldout = EditorGUILayout.Foldout(toolMainFoldout, "");


            EditorGUILayout.BeginVertical("GroupBox");


            #region Title Header

            EditorGUILayout.BeginHorizontal();
            GUIContent content = new GUIContent();

            string path = iconPath + "icon.png";
            icon = EditorGUIUtility.Load(path) as Texture;
            if (icon) GUILayout.Label(icon, GUILayout.Width(30), GUILayout.MaxHeight(30));
            GUILayout.Space(6);

            EditorGUILayout.BeginVertical();
            GUILayout.Space(7);

            var style = GUI.skin.label;
            style.richText = true;  // #FF6347ff4

            //EditorGUILayout.LabelField("<size=13><b><color=#A52A2AFF>PIVOT MODDER</color></b></size>", style);

            if (GUILayout.Button("<size=13><b><color=#A52A2AFF>PIVOT MODDER</color></b></size>", style)) { toolMainFoldout = !toolMainFoldout; }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();


            if (toolMainFoldout)
            {

                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(40);

                content.text = "<size=10><b><color=#307D7E>Allow pivot modification</color></b></size>";
                content.tooltip = "Check this option to allow pivot modifications.Without this option checked no pivot modifications will take place.This option enables the object's pivot to be freely manipulated using the transform handles. You should uncheck this option after use to allow normal rotation and movement of the object.";

                style = GUI.skin.label;

                if(GUILayout.Button(content, style, GUILayout.Width(140)))
                {
                    allowPivotMod = !allowPivotMod;
                }
                allowPivotMod = EditorGUILayout.Toggle(allowPivotMod);

                //if (allowPivotMod) { userSelectedTool = Tools.current; }


                #region Change Save Path


                style = GUI.skin.button;
                style.richText = true;


                content = new GUIContent();
                content.text = "<b><size=11><color=#006699>Change Save Path</color></size></b>";
                content.tooltip = "Change the path where the modified mesh assets will be saved.";

                if (GUILayout.Button(content, style, GUILayout.Width(134), GUILayout.Height(20), GUILayout.ExpandWidth(false)))
                {

                    string toOpen = EditorPrefs.HasKey("savePathPivotModded") ? EditorPrefs.GetString("savePathPivotModded") : "Assets/";


                    if (!String.IsNullOrWhiteSpace(toOpen))
                    {
                        if (toOpen.EndsWith("/")) { toOpen.Remove(toOpen.Length - 1, 1); }

                        if (!AssetDatabase.IsValidFolder(toOpen))
                        {
                            toOpen = "Assets/";
                        }
                    }


                    string savePath = EditorUtility.OpenFolderPanel("Choose Save path", toOpen, "");

                    //Validate the save path. It might be outside the assets folder   

                    // User pressed the cancel button
                    if (string.IsNullOrWhiteSpace(savePath)) { }

                    else if (!UtilityServices.IsPathInAssetsDir(savePath))
                    {
                        EditorUtility.DisplayDialog("Invalid Path", "The path you chose is not valid.Please choose a path that points to a directory that exists in the project's Assets folder.", "Ok");
                    }

                    else
                    {
                        savePath = UtilityServices.GetValidFolderPath(savePath);

                        if (!string.IsNullOrWhiteSpace(savePath))
                        {
                            UtilityServices.SetAndReturnStringPref("savePathPivotModded", savePath);
                        }
                    }

                }

                EditorGUI.EndDisabledGroup();

                #endregion Change Save Path



                EditorGUILayout.EndHorizontal();



                bool flag2 = false;
                Mesh m = null;

                if (!isMeshlessObject)
                {
                    m = Selection.activeTransform.GetComponent<MeshFilter>().sharedMesh;
                    if (AssetDatabase.Contains(m))
                    {
                        flag2 = true;
                    }

                    else { flag2 = false; }

                }



                EditorGUI.BeginDisabledGroup(isMeshlessObject || flag2);

                GUILayout.Space(1);

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                style = GUI.skin.button;
                style.richText = true;


                content = new GUIContent();
                content.text = "<b><size=11><color=#006699>Save Mesh</color></size></b>";
                content.tooltip = "Save the modded mesh as an asset. This is required if you want to save this object as a prefab. If you create a prefab without saving the mesh as an asset, the prefab will save without any mesh. You only have to save the mesh once. Please do save this scene after saving the mesh.";

                int h = isMeshlessObject ? 20 : 20;


                if (GUILayout.Button(content, style, GUILayout.Width(97), GUILayout.Height(h), GUILayout.ExpandWidth(false)))
                {
                    UtilityServices.SaveMeshAsAsset(m, m.name, false, true);
                    EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }


                GUILayout.EndHorizontal();

                EditorGUI.EndDisabledGroup();



                GUILayout.Space(4);


                UtilityServices.DrawHorizontalLine(Color.black, 1, 8);

                #endregion Title Header


                #region Position/rotation section layout


                #region Position/rotation section header


                GUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();

                style = new GUIStyle(GUI.skin.box);
                style.normal.background = UtilityServices.MakeColoredTexture(2, 2, new Color(1, 1, 240 / 255f));
                style.margin = new RectOffset(0, 0, 0, 0);
                style.padding = new RectOffset(0, 4, 5, 3);
                style.richText = true;

                content = new GUIContent();

                if (isPositionSelected)
                {

                    path = iconPath + "position.png";
                    content.text = "<color=#A52A2AFF><b>Pivot Position</b></color>";
                    content.tooltip = "You can use this section to manipulate the pivots' position";
                }
                else
                {
                    path = iconPath + "rotate.png";
                    content.text = "<color=#A52A2AFF><b>Pivot Rotation</b></color>";
                    content.tooltip = "You can use this section to manipulate the pivots' rotation";
                    style.padding = new RectOffset(0, 4, 6, 1);

                }

                icon = EditorGUIUtility.Load(path) as Texture;

                content.image = icon;


                GUILayout.Box(content, style, GUILayout.Width(128), GUILayout.MaxHeight(29));

                GUILayout.Space(8);

                #region Switch between position and rotation


                EditorGUILayout.BeginHorizontal();

                style = new GUIStyle();
                content = new GUIContent();

                if (isPositionSelected)
                {
                    path = iconPath + "rot.png";
                    content.tooltip = "Switch to rotation mode?";
                }
                else
                {
                    path = iconPath + "pos.png";
                    content.tooltip = "Switch to position mode?";
                }

                icon = EditorGUIUtility.Load(path) as Texture;
                style = GUI.skin.button;
                content.image = icon;
                style.onNormal.background = GUI.skin.button.onActive.background;

                if (GUILayout.Button(content, style, GUILayout.Width(30), GUILayout.MaxHeight(28)))
                {
                    isPositionSelected = !isPositionSelected;

                    if (isPositionSelected) { Tools.current = Tool.Move; }
                    else { Tools.current = Tool.Rotate; }
                }


                GUILayout.Space(10);

                var guiContent = EditorGUIUtility.IconContent("ToolHandleGlobal");



                if (handleOrientation == UtilityServices.HandleOrientation.globalAligned)
                {
                    guiContent = EditorGUIUtility.IconContent("ToolHandleLocal");
                    guiContent.tooltip = "Switch tool handle to orient to local pivot rotation?";
                    path = iconPath + "global.png";
                    content.image = EditorGUIUtility.Load(path) as Texture;
                }

                else
                {
                    guiContent = EditorGUIUtility.IconContent("ToolHandleGlobal");
                    guiContent.tooltip = "Switch tool handle to orient to global axis rotation?";
                    path = iconPath + "local.png";
                    content.image = EditorGUIUtility.Load(path) as Texture;
                }



                if (GUILayout.Button(guiContent, GUILayout.Width(28), GUILayout.MaxHeight(isMeshlessObject ? 28 : 30)))
                {
                    if (handleOrientation == UtilityServices.HandleOrientation.globalAligned)
                    {
                        handleOrientation = UtilityServices.HandleOrientation.localAligned;
                    }
                    else
                    {
                        handleOrientation = UtilityServices.HandleOrientation.globalAligned;
                    }
                }


                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();


                #endregion Switch between position and rotation


                EditorGUI.BeginDisabledGroup(!allowPivotMod || !freshChangesMade);

                style = GUI.skin.button;
                style.richText = true;

                Color originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                //# ffc14d   60%
                //# F0FFFF   73%
                //# F5F5DC   75%
                GUI.backgroundColor = UtilityServices.HexToColor("#F5F5DC");

                content = new GUIContent();
                content.text = "<size=11> <b><color=#000000>Apply</color></b> </size>";
                content.tooltip = "Apply the changes you have made. If you don't apply the changes they will be lost when this object gets out of focus.";

                bool didPress = GUILayout.Button(content, style, GUILayout.Width(70), GUILayout.Height(28), GUILayout.ExpandWidth(true));

                GUI.backgroundColor = originalColor;

                EditorGUI.EndDisabledGroup();

                freshChangesMade = false;

                #region Apply position/rotation changes here



                if (didPress && allowPivotMod)
                {

                    GameObject selectedObj = Selection.activeGameObject;
                    int key = selectedObj.GetHashCode();


                    if (positionWorld != Selection.activeTransform.position)
                    {

                        if (UtilityServices.dataContainer.objectsHistory.ContainsKey(key))
                        {
                            UtilityServices.UndoRedoOps operations = UtilityServices.dataContainer.objectsHistory[key];

                            GameObject temp1 = operations.gameObject;
                            List<UtilityServices.ObjectRecord> temp2 = operations.undoOperations;
                            List<UtilityServices.ObjectRecord> temp3 = new List<UtilityServices.ObjectRecord>();

                            UtilityServices.dataContainer.objectsHistory[key] = new UtilityServices.UndoRedoOps(temp1, temp2, temp3);
                            // reset the redo record on apply
                        }


                        var objectRecord = UtilityServices.CreateObjectRecord(selectedObj, isMeshlessObject, positionWorld);
                        UtilityServices.SaveRecord(selectedObj, objectRecord, true);


                        if (isMeshlessObject) { PivotManager.MovePivotForMeshless(selectedObj, positionWorld, true); }
                        else
                        {
                            UtilityServices.ResetPolyFewToInitialState();
                            PivotManager.MovePivot(selectedObj, positionWorld, true);
                            UtilityServices.RefreshPolyFewObjectMeshPairs(selectedObj);
                        }

                        positionWorld = Selection.activeTransform.position;
                        newHandlePos = positionWorld;
                        newHandleRot = rotationWorld;

                    }

                    if (!rotationWorld.Equals(Selection.activeTransform.rotation))
                    {
                        if (UtilityServices.dataContainer.objectsHistory.ContainsKey(key))
                        {
                            UtilityServices.UndoRedoOps operations = UtilityServices.dataContainer.objectsHistory[key];

                            GameObject temp1 = operations.gameObject;
                            List<UtilityServices.ObjectRecord> temp2 = operations.undoOperations;
                            List<UtilityServices.ObjectRecord> temp3 = new List<UtilityServices.ObjectRecord>();

                            UtilityServices.dataContainer.objectsHistory[key] = new UtilityServices.UndoRedoOps(temp1, temp2, temp3);
                            // reset the redo record on apply
                        }



                        var objectRecord = UtilityServices.CreateObjectRecord(selectedObj, isMeshlessObject, positionWorld);
                        UtilityServices.SaveRecord(selectedObj, objectRecord, true);
                        

                        if (isMeshlessObject) { PivotManager.RotatePivotForMeshless(selectedObj, rotationWorld, true); }

                        else
                        {
                            if(!UtilityServices.IsUniformScale(selectedObj.transform.lossyScale))
                            {
                                Debug.Log(string.Format("<b><i><color=#0080ffff> The selected object has non uniform scaling applied. Rotational modifications of pivots for non uniformly scaled objects can result in mesh skewing. It's best to first uniformly scale such an object before applying rotational modifications to the pivot. </color></i></b>"));
                            }

                            UtilityServices.ResetPolyFewToInitialState();
                            PivotManager.RotatePivot(selectedObj, rotationWorld, true);
                            UtilityServices.RefreshPolyFewObjectMeshPairs(selectedObj);
                        }


                        rotationWorld = Selection.activeTransform.rotation;
                        newHandleRot = rotationWorld;

                    }

                    //allowPivotMod = false;
                    isMeshlessObject = false;


                    /*
                                foreach(var obj in objectsHistory)
                                    {
                                        Debug.Log(""); Debug.Log(""); Debug.Log("This is the history for    " + obj.Value.gameObject.name);

                                        foreach(var record in obj.Value.undoOperations)
                                        {
                                            Debug.Log("POSITION:   " +record.position.ToString("F3") + "   ROTATION:   " + record.rotation.ToString("F3"));
                                        }

                                    }
                    */
                    GUIUtility.ExitGUI();
                }


                #endregion Apply position/rotation changes here


                EditorGUILayout.EndHorizontal();

                #endregion Position/rotation section header


                #region Position/Rotation section body

                GUILayout.Space(25);

                #region Centering Pivot buttons And Undo Redo Buttons

                EditorGUILayout.BeginHorizontal();

                //GUILayout.Space(2);

                EditorGUI.BeginDisabledGroup(!allowPivotMod);


                style = GUI.skin.button;
                style.richText = true;

                style.margin = new RectOffset(0, 0, 0, 0);

                originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                //# ffc14d   60%
                //# F0FFFF   73%
                //# F5F5DC   75%
                //GUI.backgroundColor = HexToColor("#F5F5DC");

                content = new GUIContent();

                if (isPositionSelected)
                {
                    content.text = "<b><size=11><color=#006699>Position Average</color></size></b>";
                    content.tooltip = "Place the pivot at the average of the positions of the first level children of this object. The calculation would exclude the placeholder collider and the placeholder NavMeshObstacles child objects.Note that you must click \"Apply\" for the changes to take effect.";
                }
                else
                {
                    content.text = "<b><size=11><color=#006699>Zero Rotation</color></size></b>";
                    content.tooltip = "Zero out pivots' rotation values on all three axes.";
                }


                int height = isMeshlessObject ? 18 : 20;

                didPress = GUILayout.Button(content, style, GUILayout.Width(125), GUILayout.MaxHeight(height));

                bool toBoundsCenter = false;


                if (isPositionSelected)
                {
                    GUILayout.Space(4);

                    content.text = "<b><size=11><color=#006699>Bounds Center</color></size></b>";
                    content.tooltip = "Place the pivot at the average of the bounds centers of the children (including this object and sub children) of this object.Note that you must click \"Apply\" for the changes to take effect.";

                    toBoundsCenter = GUILayout.Button(content, style, GUILayout.Width(120), GUILayout.MaxHeight(height));
                }



                if (didPress && allowPivotMod)
                {

                    if (isPositionSelected)
                    {

                        Vector3 averagePos = Vector3.zero;
                        int childCount = Selection.activeTransform.childCount;

                        foreach (Transform child in Selection.activeTransform)
                        {
                            if (child.GetComponent<CollRecognize>() || child.GetComponent<NavObsRecognize>()) { childCount--; continue; }
                            averagePos += child.position;
                        }

                        if (childCount != 0)
                        {
                            averagePos = averagePos / childCount;
                            positionWorld = averagePos;
                            newHandlePos = positionWorld;
                            newHandleRot = rotationWorld;
                        }

                    }

                    else
                    {
                        rotationWorld = Quaternion.identity;
                        newHandleRot = rotationWorld;
                    }

                }

                else if (toBoundsCenter && allowPivotMod)
                {
                    Vector3 averagePos = Vector3.zero;
                    bool childrenMeshless = true;
                    MeshFilter[] filters = Selection.activeTransform.GetComponentsInChildren<MeshFilter>(true);

                    if (filters != null)
                    {
                        foreach (MeshFilter filter in filters)
                        {
                            if (!filter || !filter.sharedMesh) { continue; }

                            Mesh mesh = filter.sharedMesh;

                            childrenMeshless = false;
                            averagePos += PivotManager.GetMeshWorldCenterPoint(mesh, filter.transform);
                        }

                        if (filters.Length != 0 && !childrenMeshless)
                        {
                            averagePos = averagePos / filters.Length;
                            positionWorld = averagePos;
                            newHandlePos = positionWorld;
                            newHandleRot = rotationWorld;
                        }
                    }




                }



                GUI.backgroundColor = originalColor;


                #region Undo / Redo buttons

                GUILayout.FlexibleSpace();
                content.tooltip = "Undo the last applied change";


                UtilityServices.UndoRedoOps ops;
                int kee = Selection.activeGameObject.GetHashCode();

                bool flag1 = !UtilityServices.dataContainer.objectsHistory.TryGetValue(kee, out ops) || ops.undoOperations.Count == 0;

                EditorGUI.BeginDisabledGroup(flag1);
                content.text = "";
                content.image = EditorGUIUtility.Load(iconPath + "undo.png") as Texture;

                if (GUILayout.Button(content, style, GUILayout.Width(35), GUILayout.MaxHeight(height)))
                {
                    // undo
                    Vector3 posToUpdate = ops.undoOperations[ops.undoOperations.Count - 1].changeInPos + Selection.activeTransform.position;
                    var objectRecord = UtilityServices.CreateObjectRecord(Selection.activeGameObject, isMeshlessObject, posToUpdate);
                    UtilityServices.SaveRecord(Selection.activeGameObject, objectRecord, false);

                    UtilityServices.DoUndoRedoOperation(Selection.activeGameObject, true);

                    GUIUtility.ExitGUI();
                }

                EditorGUI.EndDisabledGroup();



                GUILayout.Space(4);

                content.tooltip = "Redo the last undo operation";
                content.image = EditorGUIUtility.Load(iconPath + "redo.png") as Texture;


                kee = Selection.activeGameObject.GetHashCode();

                flag1 = !UtilityServices.dataContainer.objectsHistory.TryGetValue(kee, out ops) || ops.redoOperations.Count == 0;

                EditorGUI.BeginDisabledGroup(flag1);

                if (GUILayout.Button(content, style, GUILayout.Width(35), GUILayout.MaxHeight(height)))
                {
                    //redo
                    Vector3 posToUpdate = ops.redoOperations[ops.redoOperations.Count - 1].changeInPos + Selection.activeTransform.position;
                    var objectRecord = UtilityServices.CreateObjectRecord(Selection.activeGameObject, isMeshlessObject, posToUpdate);
                    UtilityServices.SaveRecord(Selection.activeGameObject, objectRecord, true);

                    UtilityServices.DoUndoRedoOperation(Selection.activeGameObject, false);

                    GUIUtility.ExitGUI();
                }

                EditorGUI.EndDisabledGroup();



                #endregion Undo / Redo buttons


                EditorGUI.EndDisabledGroup();



                EditorGUILayout.EndHorizontal();

                content = new GUIContent();


                #region Centralize Pivot Button
                if (!isMeshlessObject && isPositionSelected)
                {
                    GUILayout.Space(4);
                    height = isMeshlessObject ? 18 : 20;
                    content.text = "<b><size=11><color=#006699>Centralize Pivot</color></size></b>";
                    content.tooltip = "Place the pivot at the center of this object.This center is defined by the mesh bounding volume. Note that you must click \"Apply\" for the changes to take effect.";

                    EditorGUI.BeginDisabledGroup(!allowPivotMod);

                    if (GUILayout.Button(content, style, GUILayout.Width(125), GUILayout.MaxHeight(height)))
                    {
                        if (Selection.activeTransform.GetComponent<MeshFilter>())
                        {
                            Mesh mesh = Selection.activeTransform.GetComponent<MeshFilter>().sharedMesh;

                            if (mesh)
                            {
                                positionWorld = PivotManager.GetMeshWorldCenterPoint(mesh, Selection.activeTransform);
                                newHandlePos = positionWorld;
                                newHandleRot = rotationWorld;
                            }
                        }
                    }

                    EditorGUI.EndDisabledGroup();

                }
                #endregion Centralize Pivot Button


                #endregion Centering Pivot buttons And Undo Redo Buttons

                GUILayout.Space(12);


                #region Get the entered pivot position/rotation here



                if (isPositionSelected)
                {

                    GUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(!allowPivotMod);

                    style = GUI.skin.label;
                    style.richText = true;
                    //content.text = "<b>Current Values</b>";
                    content.text = "Current Values";
                    content.tooltip = "The current position values for the pivot in world space";
                    EditorGUILayout.LabelField(content, style, GUILayout.Width(130));
                    positionWorld = EditorGUILayout.Vector3Field("", positionWorld);
                    newHandlePos = positionWorld;

                    EditorGUI.EndDisabledGroup();

                    GUILayout.EndHorizontal();

                }

                else
                {
                    Vector3 eulerRot = rotationWorld.eulerAngles;

                    GUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(!allowPivotMod);

                    style = GUI.skin.label;
                    style.richText = true;
                    //content.text = "<b>Current Values</b>";
                    content.text = "Current Values";
                    content.tooltip = "The current rotation values for the pivot in world space";
                    EditorGUILayout.LabelField(content, style, GUILayout.Width(130));
                    rotationWorld = Quaternion.Euler(EditorGUILayout.Vector3Field("", eulerRot));
                    newHandleRot = rotationWorld;

                    EditorGUI.EndDisabledGroup();

                    GUILayout.EndHorizontal();

                }

                #endregion Get the entered pivot position/rotation values here


                GUILayout.Space(15);
                UtilityServices.DrawHorizontalLine(new Color(105 / 255f, 105 / 255f, 105 / 255f), 1, 5);


                GUILayout.Space(8);
                content = new GUIContent();
                style = GUI.skin.label;
                style.richText = true;


                bool prevVertexSnap = snapToVertex;
                bool prevGridSnap = snapToGrid;

                //content.text = "<b><color=#307D7E>Vertex Snap</color></b>";
                //content.text = "<b>Vertex Snap</b>"; 
                content.text = "Vertex Snap";
                content.tooltip = "Check this option to allow snapping the pivot to the nearest vertex when moved.";

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(content, style, GUILayout.Width(90));
                snapToVertex = EditorGUILayout.Toggle(snapToVertex, GUILayout.Width(47), GUILayout.ExpandWidth(false));

                //content.text = "<b><color=#307D7E>Movement Threshold</color></b>";
                //content.text = "<b>Movement Threshold</b>";
                content.text = "Movement Threshold";
                content.tooltip = "Minimum amount of movement of the handles that must be made for the pivot to get snapped to the nearest vertex. On lower values even the slightest of handle movement cause the pivot to snap to nearby vertices.";

                EditorGUI.BeginDisabledGroup(snapToGrid);

                EditorGUILayout.LabelField(content, style, GUILayout.Width(130), GUILayout.ExpandWidth(true));

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                //content.text = "<b><color=#307D7E>Grid Snap</color></b>";
                //content.text = "<b>Grid Snap</b>";
                content.text = "Grid Snap";

                if (isVersionOk)
                {
                    content.tooltip = "Check this option to allow snapping the pivot to custom values when moved or rotated. The values for individual axes for movement and rotation can be set in the fields below.";
                }
                else
                {
                    if (isPositionSelected)
                    {
                        content.tooltip = "Check this option to allow snapping the pivot to custom values when moved. The values for individual axes can be set in the snap settings window in the Editor.";
                    }
                    else
                    {
                        content.tooltip = "Check this option to allow snapping the pivot to custom values when rotated. The value for rotation snapping can be set in the snap settings window in the Editor.";
                    }
                }

                GUILayout.Space(2);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(content, style, GUILayout.Width(90));
                snapToGrid = EditorGUILayout.Toggle(snapToGrid, GUILayout.Width(50), GUILayout.ExpandWidth(false));

                style = GUI.skin.textField;
                content = new GUIContent();
                content.text = "";
                content.tooltip = "Minimum distance required for the pivot to be moved near a vertex to get snapped.";

                EditorGUI.BeginDisabledGroup(snapToGrid);

                tempFloat = snapMovementThreshold;
                snapMovementThreshold = EditorGUILayout.FloatField(content, snapMovementThreshold, style, GUILayout.Width(157), GUILayout.ExpandWidth(true));

                if (tempFloat != snapMovementThreshold)
                {
                    EditorPrefs.SetFloat("snapMovementThreshold", snapMovementThreshold);
                }



                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);

                GUILayout.BeginHorizontal();

                EditorGUI.BeginDisabledGroup(!snapToGrid);

                style = GUI.skin.label;
                style.richText = true;
                //content.text = "<b>Current Values</b>";

                if (isPositionSelected && isVersionOk)
                {
                    content.tooltip = "Set values for grid snapping on individual axes for positioning the pivot. A value of 0 for any axis will stop snapping behaviour for that axis.";
                    content.text = "Snap Values (P)";
                    EditorGUILayout.LabelField(content, style, GUILayout.Width(131));
                    tempVector3 = gridPosSnapValues;
                    gridPosSnapValues = EditorGUILayout.Vector3Field("", gridPosSnapValues);

                    if (tempVector3 != gridPosSnapValues)
                    {
                        SetAndReturnVectorPref("gridPosSnapValuesX", "gridPosSnapValuesY", "gridPosSnapValuesZ", gridPosSnapValues);
                    }
                }
                else if (isVersionOk)
                {
                    content.tooltip = "Set values for grid snapping on individual axes for rotating the pivot. A value of 0 for any axis will stop snapping behaviour for that axis.";
                    content.text = "Snap Values (R)";
                    EditorGUILayout.LabelField(content, style, GUILayout.Width(131));
                    tempVector3 = gridRotSnapValues;
                    gridRotSnapValues = EditorGUILayout.Vector3Field("", gridRotSnapValues);

                    if (tempVector3 != gridRotSnapValues)
                    {
                        SetAndReturnVectorPref("gridRotSnapValuesX", "gridRotSnapValuesY", "gridRotSnapValuesZ", gridRotSnapValues);
                    }
                }



                EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();



                if (prevVertexSnap != snapToVertex && snapToVertex)
                {
                    snapToGrid = false;
                    currentSnapPoint = null;
                }
                else if (prevGridSnap != snapToGrid && snapToGrid)
                {
                    snapToVertex = false;
                    currentSnapPoint = null;
                }



                GUILayout.Space(8);
                UtilityServices.DrawHorizontalLine(new Color(105 / 255f, 105 / 255f, 105 / 255f), 1, 5);
                GUILayout.Space(8);


                GUILayout.BeginHorizontal();

                content = new GUIContent();
                style = GUI.skin.label;
                style.richText = true;
                RectOffset prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                style.padding.left = -2;

                //content.text = "<b>Adjustment Factor</b>";
                if (isPositionSelected)
                {
                    content.text = "Adjustment Factor (P)";
                    content.tooltip = "By how much amount to increment or decrement the position values when you press the increment/decrement buttons.";
                }

                else
                {
                    content.text = "Adjustment Factor (R)";
                    content.tooltip = "By how much amount to increment or decrement the rotation values when you press the increment/decrement buttons.";
                }

                GUILayout.Space(4);

                EditorGUILayout.LabelField(content, style, GUILayout.Width(140));
                style.padding = prevPadding;

                style = GUI.skin.textField;

                content.text = "";
                string isPos = isPositionSelected ? "position" : "rotation";
                content.tooltip = "By how much amount to increment or decrement the " + isPos + " values when you press the increment/decrement buttons.";

                if (isPositionSelected)
                {
                    tempFloat = adjustmentFactorPos;
                    adjustmentFactorPos = EditorGUILayout.FloatField(content, adjustmentFactorPos, style, GUILayout.Width(170), GUILayout.ExpandWidth(true));

                    if (tempFloat != adjustmentFactorPos)
                    {
                        EditorPrefs.SetFloat("adjustmentFactorPos", adjustmentFactorPos);
                    }
                }

                else
                {
                    tempFloat = adjustmentFactorRot;
                    adjustmentFactorRot = EditorGUILayout.FloatField(content, adjustmentFactorRot, style, GUILayout.Width(170), GUILayout.ExpandWidth(true));

                    if (tempFloat != adjustmentFactorRot)
                    {
                        EditorPrefs.SetFloat("adjustmentFactorRot", adjustmentFactorRot);
                    }
                }

                GUILayout.EndHorizontal();

                #region Get  X Y Z Values individually here


                EditorGUI.BeginDisabledGroup(!allowPivotMod);


                #region X INCREMENT REGION

                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();

                GUILayout.Space(4);
                style = new GUIStyle();
                style.richText = true;
                style.border = new RectOffset(12, 0, 20, 50);

                content.image = EditorGUIUtility.Load(iconPath + "dec.png") as Texture;


                if (GUILayout.Button(content, GUILayout.Width(25), GUILayout.Height(18)))
                {
                    if (isPositionSelected)
                    {
                        Quaternion rotation = handleOrientation == UtilityServices.HandleOrientation.globalAligned ? Quaternion.identity : rotationWorld;
                        Vector3 localAxisDir = UtilityServices.GetXaxisinWorld(rotation);
                        positionWorld += localAxisDir.normalized * (-adjustmentFactorPos);
                        newHandlePos = positionWorld;
                    }
                    else
                    {
                        float angle = UtilityServices.CalcEulerSafeAngle(-adjustmentFactorRot);
                        rotationWorld *= Quaternion.AngleAxis(angle, Vector3.right);
                        newHandleRot = rotationWorld;
                    }
                }

                //GUILayout.Space(2);

                path = iconPath + "x.png";
                icon = EditorGUIUtility.Load(path) as Texture;

                GUILayout.BeginVertical(GUILayout.Width(20));
                GUILayout.Space(4);
                if (icon) GUILayout.Label(icon, GUILayout.Width(20), GUILayout.MaxHeight(16));
                GUILayout.EndVertical();

                content.image = EditorGUIUtility.Load(iconPath + "inc.png") as Texture;


                if (GUILayout.Button(content, GUILayout.Width(25), GUILayout.Height(18)))
                {
                    if (isPositionSelected)
                    {
                        Quaternion rotation = handleOrientation == UtilityServices.HandleOrientation.globalAligned ? Quaternion.identity : rotationWorld;
                        Vector3 localAxisDir = UtilityServices.GetXaxisinWorld(rotation);
                        positionWorld += localAxisDir.normalized * (adjustmentFactorPos);
                        newHandlePos = positionWorld;
                    }
                    else
                    {
                        float angle = UtilityServices.CalcEulerSafeAngle(adjustmentFactorRot);
                        rotationWorld *= Quaternion.AngleAxis(angle, Vector3.right);
                        newHandleRot = rotationWorld;
                    }
                }

                //EditorGUILayout.EndHorizontal();
                #endregion X INCREMENT REGION

                #region Y INCREMENT REGION

                GUILayout.Space(10);
                //EditorGUILayout.BeginHorizontal();

                GUILayout.Space(4);
                style = new GUIStyle();
                style.richText = true;
                style.border = new RectOffset(12, 0, 20, 50);

                content.image = EditorGUIUtility.Load(iconPath + "dec.png") as Texture;


                if (GUILayout.Button(content, GUILayout.Width(25), GUILayout.Height(18)))
                {
                    if (isPositionSelected)
                    {
                        Quaternion rotation = handleOrientation == UtilityServices.HandleOrientation.globalAligned ? Quaternion.identity : rotationWorld;
                        Vector3 localAxisDir = UtilityServices.GetYaxisinWorld(rotation);
                        positionWorld += localAxisDir.normalized * (-adjustmentFactorPos);
                        newHandlePos = positionWorld;
                    }
                    else
                    {
                        float angle = UtilityServices.CalcEulerSafeAngle(-adjustmentFactorRot);
                        rotationWorld *= Quaternion.AngleAxis(angle, Vector3.up);
                        newHandleRot = rotationWorld;
                    }
                }

                //GUILayout.Space(2);

                path = iconPath + "y.png";
                icon = EditorGUIUtility.Load(path) as Texture;

                GUILayout.BeginVertical(GUILayout.Width(20));
                GUILayout.Space(4);
                if (icon) GUILayout.Label(icon, GUILayout.Width(20), GUILayout.MaxHeight(16));
                GUILayout.EndVertical();

                content.image = EditorGUIUtility.Load(iconPath + "inc.png") as Texture;


                if (GUILayout.Button(content, GUILayout.Width(25), GUILayout.Height(18)))
                {
                    if (isPositionSelected)
                    {
                        Quaternion rotation = handleOrientation == UtilityServices.HandleOrientation.globalAligned ? Quaternion.identity : rotationWorld;
                        Vector3 localAxisDir = UtilityServices.GetYaxisinWorld(rotation);
                        positionWorld += localAxisDir.normalized * (adjustmentFactorPos);
                        newHandlePos = positionWorld;

                    }
                    else
                    {
                        float angle = UtilityServices.CalcEulerSafeAngle(adjustmentFactorRot);
                        rotationWorld *= Quaternion.AngleAxis(angle, Vector3.up);
                        newHandleRot = rotationWorld;
                    }

                }

                //EditorGUILayout.EndHorizontal();

                #endregion Y INCREMENT REGION

                #region Z INCREMENT REGION

                GUILayout.Space(10);
                //EditorGUILayout.BeginHorizontal();

                GUILayout.Space(4);
                style = new GUIStyle();
                style.richText = true;
                style.border = new RectOffset(12, 0, 20, 50);

                content.image = EditorGUIUtility.Load(iconPath + "dec.png") as Texture;


                if (GUILayout.Button(content, GUILayout.Width(25), GUILayout.Height(18)))
                {
                    if (isPositionSelected)
                    {
                        Quaternion rotation = handleOrientation == UtilityServices.HandleOrientation.globalAligned ? Quaternion.identity : rotationWorld;
                        Vector3 localAxisDir = UtilityServices.GetZaxisinWorld(rotation);
                        positionWorld += localAxisDir.normalized * (-adjustmentFactorPos);
                        newHandlePos = positionWorld;
                    }
                    else
                    {
                        float angle = UtilityServices.CalcEulerSafeAngle(-adjustmentFactorRot);
                        rotationWorld *= Quaternion.AngleAxis(angle, Vector3.forward);
                        newHandleRot = rotationWorld;
                    }
                }

                //GUILayout.Space(2);

                path = iconPath + "z.png";
                icon = EditorGUIUtility.Load(path) as Texture;

                GUILayout.BeginVertical(GUILayout.Width(20));
                GUILayout.Space(4);
                if (icon) GUILayout.Label(icon, GUILayout.Width(20), GUILayout.MaxHeight(16));
                GUILayout.EndVertical();

                content.image = EditorGUIUtility.Load(iconPath + "inc.png") as Texture;


                if (GUILayout.Button(content, GUILayout.Width(25), GUILayout.Height(18)))
                {
                    if (isPositionSelected)
                    {
                        Quaternion rotation = handleOrientation == UtilityServices.HandleOrientation.globalAligned ? Quaternion.identity : rotationWorld;
                        Vector3 localAxisDir = UtilityServices.GetZaxisinWorld(rotation);
                        positionWorld += localAxisDir.normalized * (adjustmentFactorPos);
                        newHandlePos = positionWorld;

                    }
                    else
                    {
                        float angle = UtilityServices.CalcEulerSafeAngle(adjustmentFactorRot);
                        rotationWorld *= Quaternion.AngleAxis(angle, Vector3.forward);
                        newHandleRot = rotationWorld;
                    }
                }

                EditorGUILayout.EndHorizontal();

                #endregion Z INCREMENT REGION


                EditorGUI.EndDisabledGroup();


                #endregion Get  X Y Z Values individually here



                #endregion Position/Rotation section body


                #endregion Position/Rotation section layout


                GUILayout.Space(6);

            }


            EditorGUILayout.EndVertical();


            if (positionWorld != Selection.activeTransform.position || (rotationWorld != Selection.activeTransform.rotation))
            {
                freshChangesMade = true;
            }

        }




        private void SelectionChanged()
        {

            PrevSelection = Selection.activeTransform;

            // Reset all defaults
            allowPivotMod = false;
            //preserveActualMesh = true;
            Tools.hidden = false;
            
            if (Selection.activeTransform != null)
            {

                positionWorld = Selection.activeTransform.position;
                rotationWorld = Selection.activeTransform.rotation;
                handleRotation = new Quaternion(rotationWorld.x, rotationWorld.y, rotationWorld.z, rotationWorld.w);
                Selection.activeTransform.hasChanged = false;
                newHandlePos = positionWorld;
                newHandleRot = rotationWorld;
                isBone = UtilityServices.IsBone(Selection.activeTransform);
                isMorphed = UtilityServices.IsMorphed(Selection.activeTransform);

                if (isBone)
                {
                    EditorUtility.DisplayDialog("Unsupported GameObject", "GameObjects that are amongst the bones assigned to a skinned mesh cannot have their pivots modified. Pivot modder won't show up for such objects.", "ok");
                }

                if (isMorphed)
                {
                    EditorUtility.DisplayDialog("Unsupported GameObject", "Morphed objects(Meshes with blendshape animations) are not supported for pivot modifications. Pivot modder won't show up for such objects.", "ok");
                }

                //hasSubmeshes = UtilityServices.HasSubmeshes(Selection.activeTransform); 
            }

        }

        [DidReloadScripts]
        public static void ScriptReloaded()
        {
            // Reset all defaults
            allowPivotMod = false;
            //preserveActualMesh = true;
            Tools.hidden = false;
        }

    }

}
