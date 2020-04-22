using System;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace KinectForBeatSaberApp
{
    class Program
    {
        private static KinectSensor sensor;

        private static AnonymousPipeServerStream serverStream;
        private static KinectSensorChooser chooser;
        private static StreamWriter sw;
        private static Process beatSaber;

        static void Main(string[] args)
        {
            Console.WriteLine("Kinect for Beat Saber | By Caeden117");
            Console.WriteLine("Any launch args for this application will transfer over to Beat Saber! (\"--verbose\" and \"fpfc\", for example.)");
            Console.WriteLine("Current launch args: " + string.Join(" ", args));
            Console.WriteLine("=============================");
            Console.WriteLine("Waiting for Kinect...");
            chooser = new KinectSensorChooser();
            chooser.KinectChanged += KinectUpdate;
            chooser.Start();
            Process.GetCurrentProcess().Exited += Exit;
            Console.WriteLine("Kinect active. Launching Beat Saber...");

            serverStream = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable)
            {
                ReadMode = PipeTransmissionMode.Byte
            };

            beatSaber = new Process();
            beatSaber.StartInfo.FileName = "Beat Saber.exe"; //This NEEDS to be placed in the root Beat Saber folder
            beatSaber.StartInfo.Arguments = $"{string.Join(" ", args)} KinectClientHandle={serverStream.GetClientHandleAsString()}";
            beatSaber.StartInfo.UseShellExecute = false;
            beatSaber.Start();

            serverStream.DisposeLocalCopyOfClientHandle();

            try
            {
                Console.WriteLine("Attempting to establish connection...");
                sw = new StreamWriter(serverStream)
                {
                    AutoFlush = true
                };
                sw.WriteLine("SYNC");
                serverStream.WaitForPipeDrain();
                Console.WriteLine("Connection established! We are good to go!");
            }
            catch(IOException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Connection has been terminated.");
            }

            beatSaber.WaitForExit();
            beatSaber.Close();
            Console.WriteLine("Beat Saber terminated. Terminating self...");
            //Console.ReadLine();
            Process.GetCurrentProcess().Close();
        }

        private static void Exit(object sender, EventArgs e)
        {
            sw.WriteLine("TERMINATED");
            sw?.Close();
            serverStream?.Dispose();
            chooser.KinectChanged -= KinectUpdate;
            chooser?.Stop();
            beatSaber?.Dispose();
            if (sensor != null)
            {
                sensor.SkeletonFrameReady -= SkeletonFrameReady;
                sensor.Stop();
            }
        }

        private static void KinectUpdate(object sender, KinectChangedEventArgs e)
        {
            if (sensor != null)
            {
                sensor.SkeletonStream.Disable();
                sensor.SkeletonFrameReady -= SkeletonFrameReady;
                sensor.Stop();
            }
            sensor = e.NewSensor;
            if (sensor != null)
            {
                Console.WriteLine($"Kinect updated! {e.NewSensor.Status}");
                sensor.SkeletonStream.Enable();
                sensor.SkeletonFrameReady += SkeletonFrameReady;
                sensor.Start();
            }
        }

        private static void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            try
            {
                if (!serverStream.IsConnected) return;
                Skeleton[] array = new Skeleton[0];
                using (SkeletonFrame frame = e.OpenSkeletonFrame())
                {
                    if (frame == null) return;
                    array = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(array);
                    byte[] bytes = ObjectToByteArray(array);
                    try
                    {
                        sw.AutoFlush = true;
                        sw.WriteLine(string.Join(",", bytes));
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            }
            catch { }
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
