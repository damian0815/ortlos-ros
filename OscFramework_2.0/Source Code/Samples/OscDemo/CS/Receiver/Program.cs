using System;
using System.Net;
using System.Collections.Generic;
using Bespoke.Common;
using Bespoke.Common.Osc;

using LogicData.VirtualHandset;

namespace Receiver
{
    public enum DemoType
    {
        Udp,
        Tcp,
        Multicast
    }

	public class Program
	{
		public static void Main(string[] args)
		{
            OscServer oscServer;
            DemoType demoType = /*GetDemoType();*/ DemoType.Udp;
            switch (demoType)
            {
                case DemoType.Udp:
                    oscServer = new OscServer(TransportType.Udp, IPAddress.Any, Port);
                    break;

                case DemoType.Tcp:
                    oscServer = new OscServer(TransportType.Tcp, IPAddress.Loopback, Port);
                    break;

                case DemoType.Multicast:
                    oscServer = new OscServer(IPAddress.Parse("224.25.26.27"), Port);
                    break;

                default:
                    throw new Exception("Unsupported receiver type.");
            }
            
            oscServer.FilterRegisteredMethods = false;
			oscServer.RegisterMethod(AliveMethod);
            oscServer.RegisterMethod(TestMethod);
            oscServer.RegisterMethod(MouseMethod);
            oscServer.BundleReceived += new EventHandler<OscBundleReceivedEventArgs>(oscServer_BundleReceived);
			oscServer.MessageReceived += new EventHandler<OscMessageReceivedEventArgs>(oscServer_MessageReceived);
            oscServer.ReceiveErrored += new EventHandler<ExceptionEventArgs>(oscServer_ReceiveErrored);
            oscServer.ConsumeParsingExceptions = false;

            handsets = new VirtualHandset[HANDSET_COUNT];
            motorMin = new int[HANDSET_COUNT];
            motorMax = new int[HANDSET_COUNT];

            String[] comPorts = { "COM9", "COM4", "COM5", "COM6", "COM7", "COM8" };
            for ( int i=0; i<HANDSET_COUNT; i++ ) {
                VirtualHandset handset = new VirtualHandset();
                int result = handset.Connect(comPorts[i]);
                if (result != 0) {
                    // -20 = virtual com port device not found (USB unplugged)
                    // -30 = com port found but couldn't connect to hardware
                    Console.WriteLine("error " + result + " opening " + comPorts[i]);
                    continue;
                }

                VirtualHandset.ControlUnitSettings settings = new VirtualHandset.ControlUnitSettings();
                result = handset.GetAllSettings( ref settings );
                if (result == 0)
                {
                    Console.Write(comPorts[i]+": OEM Information: "+(char)settings.OEMInformation[0]+" "+(char)settings.OEMInformation[1]+
                        " "+(char)settings.OEMInformation[2]+" "+(char)settings.OEMInformation[3]+" (" +(settings.OEMInformation.Length-4)+" more) ");

                }
                else
                {
                    Console.WriteLine("error " + result + " getting control unit settings for " + comPorts[i]);
                    continue;
                }

                int handsetIndex = 0;
                if ((char)settings.OEMInformation[0] == 'L')
                    handsetIndex = 0;
                else if ((char)settings.OEMInformation[0] == 'R')
                    handsetIndex = 3;
                else
                {
                    Console.WriteLine(comPorts[i] + ": invalid OEMInformation[0] '" + (char)settings.OEMInformation[0] + "' (should be 'L' or 'R')");
                    continue;
                }

                if ((char)settings.OEMInformation[1] == '1')
                    handsetIndex += 0;
                else if ((char)settings.OEMInformation[1] == '2')
                    handsetIndex += 1;
                else if ((char)settings.OEMInformation[1] == '3')
                    handsetIndex += 2;
                else
                {
                    Console.WriteLine(comPorts[i] + ": invalid OEMInformation[1] '" + (char)settings.OEMInformation[1] + "' (should be '1', '2' or '3')");
                    continue;
                }

                handsets[handsetIndex] = handset;
                motorMin[handsetIndex] = settings.LowerLimit[0] + 1; // don't drive right to the very limit
                motorMax[handsetIndex] = settings.UpperLimit[0] - 1; // don't drive right to the very limit
                Console.WriteLine( " lower " + motorMin[handsetIndex] + " upper " + motorMax[handsetIndex] );
                Console.WriteLine("  -> assigning handset to index " + handsetIndex);

                // add event-handler for synchronizing the PC after driving
                //handsets[handsetIndex].OnSynchronizeAfterDriving += new VirtualHandset.OnSynchronizeAfterDrivingDelegate(onSynchronizeAfterDriving);

            }

            for (int i = 0; i < HANDSET_COUNT; i++)
            {
                if (handsets[i] == null)
                {
                    Console.WriteLine("handset " + i + " missing, filling in with dummy");
                    handsets[i] = new VirtualHandset();
                }
               // moveMotorToPosition(i, INITIAL_POS);
            }

            oscServer.Start();

            // figure out my ip address
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
			Console.WriteLine("Osc Receiver: " + demoType.ToString() + " listening on address " + localIP + " port " + Port );
			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();

			oscServer.Stop();

            for (int i = 0; i < HANDSET_COUNT; i++)
            {
                if (handsets[i] == null)
                    continue;
                handsets[i].Disconnect();
                handsets[i].Dispose();
            }

		}

        private static DemoType GetDemoType()
        {
            Dictionary<ConsoleKey, DemoType> keyMappings = new Dictionary<ConsoleKey, DemoType>();
            keyMappings.Add(ConsoleKey.D1, DemoType.Udp);
            keyMappings.Add(ConsoleKey.D2, DemoType.Tcp);
            keyMappings.Add(ConsoleKey.D3, DemoType.Multicast);

            Console.WriteLine("\nWelcome to the Bespoke Osc Receiver Demo.\nPlease select the type of receiver you would like to use:");
            Console.WriteLine("  1. Udp\n  2. Tcp\n  3. Udp Multicast");

            ConsoleKeyInfo key = Console.ReadKey();
            while (keyMappings.ContainsKey(key.Key) == false)
            {
                Console.WriteLine("\nInvalid selection\n");
                Console.WriteLine("  1. Udp\n  2. Tcp\n  3. Udp Multicast");
                key = Console.ReadKey();
            }

            Console.Clear();

            return keyMappings[key.Key];
        }

        private static void oscServer_BundleReceived(object sender, OscBundleReceivedEventArgs e)
        {
            sBundlesReceivedCount++;
            /*
            OscBundle bundle = e.Bundle;
            Console.WriteLine(string.Format("\nBundle Received [{0}:{1}]: Nested Bundles: {2} Nested Messages: {3}", bundle.SourceEndPoint.Address, bundle.TimeStamp, bundle.Bundles.Count, bundle.Messages.Count));
            Console.WriteLine("Total Bundles Received: {0}", sBundlesReceivedCount);*/
        }

		private static void oscServer_MessageReceived(object sender, OscMessageReceivedEventArgs e)
		{
            sMessagesReceivedCount++;

            OscMessage message = e.Message;

            //Console.WriteLine(string.Format("\nMessage Received [{0}]: {1}", message.SourceEndPoint.Address, message.Address));
            //Console.WriteLine(string.Format("Message contains {0} objects.", message.Data.Count));

            if (message.Address == "/teleskop/moveTo")
            {
                int skopIndex = (int)message.Data[0];
                float positionFloat = (float)message.Data[1];

                if (skopIndex >= 0 && skopIndex < 6 && positionFloat >= 0.0f && positionFloat <= 1.0f)
                {
                    moveMotorToPosition(skopIndex, positionFloat);

                }
            }
            else if (message.Address == "/teleskop/stop")
            {
                int skopIndex = (int)message.Data[0];
                if (skopIndex >= 0 && skopIndex < 6)
                {
                    stopMotor(skopIndex);
                }
            }

            /*for (int i = 0; i < message.Data.Count; i++)
            {
                string dataString;

                if (message.Data[i] == null)
                {
                    dataString = "Nil";
                }
                else
                {
                    dataString = (message.Data[i] is byte[] ? BitConverter.ToString((byte[])message.Data[i]) : message.Data[i].ToString());
                }
                Console.WriteLine(string.Format("[{0}]: {1}", i, dataString));
            }

            Console.WriteLine("Total Messages Received: {0}", sMessagesReceivedCount);
             */
		}

        private static void stopMotor(int skopIndex)
        {
            Console.WriteLine(string.Format("Skop {0} stop", skopIndex ));
            int driverIndex = skopIndex;
            if (handsets[driverIndex] == null)
            {
                Console.WriteLine("stopMotor: driverIndex " + driverIndex + " is invalid, bailing out");
                return;
            }
            int result = handsets[driverIndex].EndMove();
            if (result != 0)
            {
                Console.WriteLine("stopMotor: bad result "+result+" calling EndMove on driver " + driverIndex);
            }

        }

        private static void moveMotorToPosition(int skopIndex, float positionFloat )
        {
            //Console.WriteLine(string.Format("Skop {0} to position {1}", skopIndex, positionFloat));
            int motorRange = motorMax[skopIndex] - motorMin[skopIndex];
            int position = (int)(positionFloat * motorRange + motorMin[skopIndex]);
            int driverIndex = skopIndex;
            if (handsets[driverIndex] == null)
            {
                Console.WriteLine("moveMotorToPosition: driverIndex " + driverIndex + " is invalid, bailing out");
                return;
            }
            
            
            VirtualHandset.MotorGroup mg = VirtualHandset.MotorGroup.MotorGroup1;
            Console.WriteLine(string.Format(" -> driver {0} motor {1} to position {2}", driverIndex, mg, position));

            // work out the motor group
            int result = handsets[driverIndex].BeginMoveToPosition(position, mg);
            if (result != 0)
            {
                Console.WriteLine("error " + result + " calling driver " + driverIndex + " BeginMoveToPosition(" + position + ", " + mg + ")");
                string err = "";
                result = handsets[driverIndex].GetDisplayedError(ref(err));
                Console.WriteLine(" -> displayed error '" + err + "'");
                if (result != 0)
                {
                    Console.WriteLine("    (got result " + result + " calling GetDisplayedError)");
                }
            }
        }

        private static void oscServer_ReceiveErrored(object sender, ExceptionEventArgs e)
        {
            Console.WriteLine("Error during reception of packet: {0}", e.Exception.Message);
        }

		private static readonly int Port = 5103;
		private static readonly string AliveMethod = "/osctest/alive";
        private static readonly string TestMethod = "/osctest/test";
        private static readonly string MouseMethod = "/mouse/position";

        private static int sBundlesReceivedCount;
        private static int sMessagesReceivedCount;

        private static readonly int HANDSET_COUNT = 6;
        private static readonly float INITIAL_POS = 0.0f;
        private static VirtualHandset[] handsets;
        private static int[] motorMin;
        private static int[] motorMax;
	}
}
