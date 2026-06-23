using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace WaterStylizedShader
{
    [RequireComponent(typeof(Rigidbody))]
    public class FloatingObject : MonoBehaviour
    {
        public Transform[] floaters;
        public float underWaterDrag = 3f;
        public float underWaterAngularDrag = 1f;
        public float airWaterDrag = 0f;
        public float airWaterAngularDrag = 0.05f;

        public float floatingPower = 15f;

        public float baseWaterHeight = 0f;
        public float waterHeightVariation = 2f;
        public float waveSpeed = 1.0f;
        public float waterHeight;

        Rigidbody rb;
        int floatersUnderwater;
        bool underwater;
        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            waterHeight = baseWaterHeight + Mathf.Sin(Time.time * waveSpeed) * (waterHeightVariation / 2f);

            Debug.Log(
                "FloaterY: " + floaters[0].position.y +
                " | WaterHeight: " + waterHeight
            );

            for (int i = 0; i < floaters.Length; i++)
            {
                float diff = floaters[i].position.y - waterHeight;

                if (diff < 0)
                {
                    Debug.Log("UNDERWATER");

                    rb.AddForceAtPosition(
                        Vector3.up * floatingPower * Mathf.Abs(diff),
                        floaters[i].position,
                        ForceMode.Force
                    );
                }
            }
        }

        void SwitchState(bool isUnderwater)
        {
            if (isUnderwater)
            {
                rb.linearDamping = underWaterDrag;
                rb.angularDamping = underWaterAngularDrag;
            }
            else
            {
                rb.linearDamping = airWaterDrag;
                rb.angularDamping = airWaterAngularDrag;
            }
        }
    }
}

