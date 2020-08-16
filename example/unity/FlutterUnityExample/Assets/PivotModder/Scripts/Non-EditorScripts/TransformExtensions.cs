#if UNITY_EDITOR


using UnityEngine;
using System.Reflection;
using System;


namespace BrainFailProductions.PivotModder
{


    public static class TransformExtensions
    {
        public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
        {
            var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            return localToWorldMatrix.MultiplyPoint3x4(position);
        }

        public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
        {
            var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
            return worldToLocalMatrix.MultiplyPoint3x4(position);
        }



        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = PrivateValueAccessor.Flags;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }


        public static T AddComponentClone<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }
    }


}


#endif