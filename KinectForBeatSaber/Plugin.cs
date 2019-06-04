using IllusionPlugin;
using UnityEngine.SceneManagement;
using System;
using System.IO.Pipes;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Kinect.Toolkit;
using System.Linq;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using Microsoft.Kinect;
using System.Threading;
using UnityEngine;

namespace KinectForBeatSaber
{
    public class Plugin : IPlugin
    {
        public string Name => "Kinect for Beat Saber (Xbox 360)";
        public string Version => "0.0.1";
        public static Plugin Instance;

        public static bool CompanionConnected = false;
        private AnonymousPipeClientStream client;
        internal static ConcurrentQueue<Skeleton[]> skeletonsToProcess = new ConcurrentQueue<Skeleton[]>();
        private Thread thread;

        internal static Material trackingPointMaterial;

        public void OnApplicationStart()
        {
            Instance = this;
            Log("Kinect for Custom Avatars is loading!");
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("KinectClientHandle"))
                {
                    CompanionConnected = true;
                    Log("Companion client handle found! Establishing connection...");
                    client = new AnonymousPipeClientStream(PipeDirection.In, arg.Split('=').Last());
                    client.ReadMode = PipeTransmissionMode.Byte;
                    using (StreamReader sr = new StreamReader(client))
                    {
                        Log("Attempting to establish sync with companion application...");
                        string temp;
                        do
                        {
                            temp = sr.ReadLine();
                        }
                        while (!temp.StartsWith("SYNC"));
                        Log("Connections synced! We're ready for a good time!");
                        thread = new Thread(new ThreadStart(HandleSkeletonData));
                        thread.Start();
                    }
                }
            }
            new GameObject("Kinect to Avatars").AddComponent<KinectToBS>();
            Process.GetCurrentProcess().Exited += Exit;
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private static void HandleSkeletonData()
        {
            Log("Separate thread created!");
            try
            {
                using (StreamReader sr = new StreamReader(Instance.client))
                {
                    string temp;
                    while ((temp = sr.ReadLine()) != null)
                    {
                        List<byte> bytes = new List<byte>();
                        foreach(string part in temp.Split(',')) bytes.Add(Convert.ToByte(part));
                        MemoryStream memStrem = new MemoryStream();
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStrem.Write(bytes.ToArray(), 0, bytes.Count);
                        memStrem.Seek(0, SeekOrigin.Begin);
                        Skeleton[] skeletons = (Skeleton[])binForm.Deserialize(memStrem);
                        skeletonsToProcess.Enqueue(skeletons);
                    }
                }
            }
            catch {
                Log("Connection died.");
                CompanionConnected = false;
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            client.Close();
            thread.Abort();
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            foreach (Material mat in Resources.FindObjectsOfTypeAll<Material>())
            {
                if (mat.name == "GlassHandle") trackingPointMaterial = mat;
            }
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }
        public void OnLevelWasLoaded(int level) { }
        public void OnLevelWasInitialized(int level) { }
        public void OnUpdate() { }
        public void OnFixedUpdate() { }
        public static void Log(string message)
        {
            Console.WriteLine($"[Kinect for Custom Avatars] {message}");
        }
    }
}
