using UnityEngine;

namespace JinYanMaoYing
{
    public sealed class QianQingRoom : MonoBehaviour
    {
        public RectTransform relic;
        public float floorLeft = -353f;
        public float floorRight = -353f;
        public float rampStart = -300f;
        public float rampEnd = 500f;

        public float GroundAt(float x)
        {
            if (rampEnd <= rampStart) return floorRight;
            return Mathf.Lerp(floorLeft, floorRight, Mathf.InverseLerp(rampStart, rampEnd, x));
        }
    }
}
