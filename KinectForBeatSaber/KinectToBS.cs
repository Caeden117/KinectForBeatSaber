using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KinectForBeatSaber
{
    class KinectToBS : MonoBehaviour
    {
        private Dictionary<JointType, GameObject> trackingPoints = new Dictionary<JointType, GameObject>();

        private void Awake()
        {
            Plugin.Log("Kinect To Beat Saber GameObject created!");
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (!Plugin.skeletonsToProcess.IsEmpty)
            {
                if (Plugin.skeletonsToProcess.TryDequeue(out Skeleton[] skeletons))
                {
                    foreach (Skeleton skeleton in skeletons)
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked) RefreshTrackingPoints(skeleton);
                }
            }

            if (!Plugin.CompanionConnected)
            {
                foreach (GameObject obj in trackingPoints.Values) Destroy(obj);
                trackingPoints.Clear();
                Plugin.Log("Connection with the Kinect for Beat Saber console application failed - Destroying skeleton.");
                Destroy(gameObject);
            }
        }

        private void RefreshTrackingPoints(Skeleton skeleton)
        {
            for (int i = 0; i < 20; i++)
                RefreshJoint(skeleton, (JointType)i);
        }

        private void RefreshJoint(Skeleton skeleton, JointType jointType)
        {
            Joint joint = skeleton.Joints[jointType];
            if (!trackingPoints.ContainsKey(joint.JointType))
            {
                GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                primitive.GetComponent<Renderer>().material = Plugin.trackingPointMaterial;
                primitive.transform.localScale = Vector3.one * 0.1f;
                primitive.transform.parent = transform;
                trackingPoints.Add(joint.JointType, primitive);
            }
            trackingPoints[joint.JointType].transform.localPosition = SkeletonPointToVector3(joint.Position);
            Color trackingColor = Color.white;
            switch (joint.TrackingState)
            {
                case JointTrackingState.Inferred:
                    trackingColor = Color.yellow;
                    break;
                case JointTrackingState.NotTracked:
                    trackingColor = Color.red;
                    break;
                default: break;
            }
            trackingPoints[joint.JointType].GetComponent<Renderer>().material.color = trackingColor;
        }

        private Vector3 SkeletonPointToVector3 (SkeletonPoint point)
        {
            return new Vector3(point.X * -1, point.Y, point.Z);
        }
    }
}
