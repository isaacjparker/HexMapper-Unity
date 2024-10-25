using System;
using UnityEngine;

namespace HexMapper
{
    public enum HexType
    {
        POINT,
        FLAT
    }

    public enum HexSnapSource
    {
        SCALE,
        CUSTOM
    }

    [ExecuteInEditMode]
    public class HexMapper : MonoBehaviour
    {
        [Header("Hexagon Type")]
        public HexType HexType;

        [Header("Hex Grid Settings")]
        [Tooltip("Activate or deactive X,Z snapping.")]
        public bool HexGridSnapActive = true;
        [Tooltip("Snap hex based on Custom grid size or object scale.")]
        public HexSnapSource HexSnapSource;
        [Tooltip("Requires apothem, not circumcircle radius.")]
        public float customHexSize = 1.0f; // Distance from center to the midpoint of any edge. https://www.omnicalculator.com/math/hexagon

        [Header("Y Snap Settings")]
        [Tooltip("Activate or deactivate Y snapping.")]
        public bool ySnapActive = false;
        public float ySnap = 1.0f;   // Interval for snapping along the y-axis


        const float CircumRadiusRatio = 1.1547f;    // Just hexagon math. Don't touch.

        private void Update()
        {
            // Only execute in edit mode and when not playing
            if (!Application.isPlaying)
            {
                SnapToGrid(GetSnappingInterval());
            }
        }

        /// <summary>
        /// Snaps the object to the nearest hex grid position, including y-axis snapping.
        /// </summary>
        void SnapToGrid(float snapInterval)
        {
            Vector3 position = transform.position;

            // Convert position to cube coordinates
            Vector3 cubeCoords = WorldToCube(position, snapInterval);

            // Round cube coordinates to nearest hex
            Vector3 roundedCubeCoords = CubeRound(cubeCoords);

            // Convert rounded cube coordinates back to world position
            Vector3 snappedPosition = CubeToWorld(roundedCubeCoords, snapInterval);

            // Snap the y-position separately
            if (ySnapActive == true)
            {
                snappedPosition.y = (float)Math.Round(position.y / ySnap, MidpointRounding.AwayFromZero) * ySnap;
                transform.position = new Vector3(transform.position.x, snappedPosition.y, transform.position.z);
            }

            // Return before snapping if grid snapping is inactive
            if (!HexGridSnapActive) return;

            // Update the object's position
            transform.position = snappedPosition;

        }

        /// <summary>
        /// Converts world position to cube coordinates.
        /// </summary>
        Vector3 WorldToCube(Vector3 position, float snapInterval)
        {
            float circumRadius = ApothemToCircumcircleRadius(snapInterval);

            float x = 0f;
            float z = 0f;
            float y = 0f;

            if (HexType == HexType.POINT)
            {
                x = ((Mathf.Sqrt(3f) / 3f) * position.x - (1f / 3f) * position.z) / circumRadius;
                z = ((2f / 3f) * position.z) / circumRadius;
            }

            if (HexType == HexType.FLAT)
            {
                x = (2f / 3f * position.x) / circumRadius;
                z = (-1f / 3f * position.x + Mathf.Sqrt(3f) / 3f * position.z) / circumRadius;
            }

            y = -x - z;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Rounds cube coordinates to the nearest whole number while ensuring x + y + z = 0.
        /// </summary>
        Vector3 CubeRound(Vector3 cubeCoords)
        {
            float rx = (float)Math.Round(cubeCoords.x, MidpointRounding.AwayFromZero);
            float ry = (float)Math.Round(cubeCoords.y, MidpointRounding.AwayFromZero);
            float rz = (float)Math.Round(cubeCoords.z, MidpointRounding.AwayFromZero);

            float x_diff = Mathf.Abs(rx - cubeCoords.x);
            float y_diff = Mathf.Abs(ry - cubeCoords.y);
            float z_diff = Mathf.Abs(rz - cubeCoords.z);

            if (x_diff > y_diff && x_diff > z_diff)
            {
                rx = -ry - rz;
            }
            else if (y_diff > z_diff)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            return new Vector3(rx, ry, rz);
        }

        /// <summary>
        /// Converts cube coordinates back to world position.
        /// </summary>
        Vector3 CubeToWorld(Vector3 cubeCoords, float snapInterval)
        {
            float circumRadius = ApothemToCircumcircleRadius(snapInterval);

            float x = 0f;
            float z = 0f;

            if (HexType == HexType.POINT)
            {
                x = circumRadius * (Mathf.Sqrt(3f) * cubeCoords.x + (Mathf.Sqrt(3f) / 2f) * cubeCoords.z);
                z = circumRadius * (1.5f * cubeCoords.z);

                x = (float)Math.Round(x, MidpointRounding.AwayFromZero);
            }

            if (HexType == HexType.FLAT)
            {
                x = circumRadius * (3f / 2f * cubeCoords.x);
                z = circumRadius * (Mathf.Sqrt(3f) / 2f * cubeCoords.x + Mathf.Sqrt(3f) * cubeCoords.z);
            }

            return new Vector3(x, transform.position.y, z); // Adjust y later is snapped
        }

        /// <summary>
        /// Converts hex apothem (hex size) to circumcircle radius
        /// </summary>
        float ApothemToCircumcircleRadius(float apothem)
        {
            return apothem * CircumRadiusRatio;
        }

        /// <summary>
        /// Get the appropriate hex size depending on whether snapping is set to
        /// the object X scale, or a custom snapping interval.
        /// </summary>
        private float GetSnappingInterval()
        {
            if (HexSnapSource == HexSnapSource.SCALE)
            {
                return transform.localScale.x;
            }

            return customHexSize;
        }
    }
}

