#if UNITY_EDITOR


using UnityEngine;

namespace BrainFailProductions.PivotModder
{
    [ExecuteInEditMode]
    public class PivotModderHost : MonoBehaviour
    {
        void OnAwake()
        {
            if (!Application.isEditor || Application.isPlaying) { DestroyImmediate(this); }
        }
    }
}


#endif

