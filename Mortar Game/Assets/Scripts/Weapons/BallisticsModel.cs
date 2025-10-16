using UnityEngine;
using MortarGame.Config;

namespace MortarGame.Weapons
{
    public static class BallisticsModel
    {
        // Linear interpolation of flight time based on desired range fraction.
        public static float ComputeFlightTime(float desiredRangeM, MissionConfig.MortarSettings ms)
        {
            float t = Mathf.Clamp01(desiredRangeM / Mathf.Max(0.01f, ms.rangeMaxM));
            return Mathf.Lerp(ms.flightTimeMin, ms.flightTimeMax, t);
        }

        // Computes a world-space impact point based on a bearing and desired range, with dispersion.
        public static Vector3 ComputeImpactPoint(Vector3 origin, float bearingDegrees, float desiredRangeM, float dispersionM)
        {
            // Base impact along ground plane XZ
            var rad = bearingDegrees * Mathf.Deg2Rad;
            var dir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            var ideal = origin + dir * desiredRangeM;

            // Dispersion ellipse around ideal impact
            var offset = Random.insideUnitCircle * dispersionM; // X/Y in 2D plane
            var impact = ideal + new Vector3(offset.x, 0f, offset.y);
            return impact;
        }
    }
}