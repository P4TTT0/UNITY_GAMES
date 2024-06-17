using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Functions
{
    public class VectorUtils : MonoBehaviour
    {
        private const float Tolerance = 0.0001f;

        public static bool IsNormalized(Vector2 vector)
        {
            return Mathf.Abs(vector.magnitude - 1.0f) < Tolerance;
        }

        public static bool IsNormalized(Vector3 vector)
        {
            return Mathf.Abs(vector.magnitude - 1.0f) < Tolerance;
        }
    }
}
