#if UNITY_EDITOR


using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BrainFailProductions.PivotModder
{

    [System.Serializable]
    public class ObjectsHistory : SerializableDictionary<int, UtilityServices.UndoRedoOps> { }


    public class DataContainer : MonoBehaviour
    {

        public ObjectsHistory objectsHistory;
        public UnityEngine.Object polyFewResetter;
        public MethodInfo refreshPolyFewObjectMeshPairs;
        public MethodInfo resetPolyFewToInitialState;
        public Transform prevSelection;
    }
}


#endif
