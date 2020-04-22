using IPA.Old;
using IPA.Loader;
using UnityEngine.SceneManagement;
using System;
using System.IO.Pipes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using Microsoft.Kinect;
using System.Threading.Tasks;
using UnityEngine;
using KinectForBeatSaber.Utils;

namespace KinectForBeatSaber
{
    #pragma warning disable CS0618 //BSIPA 4 can still load IPlugins. If it aint broke, don't fix it.
    public class Plugin : IPlugin
    {
        public string Name => "Kinect for Beat Saber (Xbox 360)";
        public string Version => "0.0.1";

        public static Plugin Instance;
        public static bool CompanionConnected = false;
        public static Config config;
        public static KinectToBS KinectInfo;

        private AnonymousPipeClientStream client;
        private Task processingTask;

        internal static List<Skeleton[]> skeletonsToProcess = new List<Skeleton[]>();

        public void OnApplicationStart()
        {
            Instance = this;
            Log("Kinect for Beat Saber is loading!");
            config = Config.Load();
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
                        string temp = "";
                        do
                        {
                            try
                            {
                                temp = sr.ReadLine();
                            }
                            catch { }
                        }
                        while (!temp.StartsWith("SYNC"));
                        Log("Connections synced! We're ready for a good time!");
                        processingTask = new Task(HandleSkeletonData);
                        processingTask.Start();
                    }
                }
            }
            GameObject KinectGo = new GameObject("Kinect for Beat Saber");
            KinectGo.transform.position = config.PositionOffset;
            KinectGo.transform.rotation = Quaternion.Euler(config.RotationOffset);
            KinectGo.transform.localScale = Vector3.one * config.Scale;
            KinectInfo = KinectGo.AddComponent<KinectToBS>();
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
                        if (temp == "TERMINATED")
                        {
                            Log("Connection died.");
                            CompanionConnected = false;
                            break;
                        }
                        List<byte> bytes = new List<byte>();
                        foreach(string part in temp.Split(',')) bytes.Add(Convert.ToByte(part));
                        MemoryStream memStrem = new MemoryStream();
                        BinaryFormatter binForm = new BinaryFormatter();
                        memStrem.Write(bytes.ToArray(), 0, bytes.Count);
                        memStrem.Seek(0, SeekOrigin.Begin);
                        Skeleton[] skeletons = (Skeleton[])binForm.Deserialize(memStrem);
                        skeletonsToProcess.Add(skeletons);
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
            processingTask.Dispose();
        }

        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene arg1)
        {

        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (!skeletonsToProcess.Any()) return;
            Skeleton[] last = skeletonsToProcess.Last();
            skeletonsToProcess.Clear();
            if (last != null) skeletonsToProcess.Add(last);
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
            Console.WriteLine($"[Kinect for Beat Saber] {message}");
        }
    }
}
