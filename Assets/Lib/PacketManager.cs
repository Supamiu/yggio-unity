using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace YggioUnity
{
    public class PacketManager
    {
        public static Dictionary<int, Packet> Packets { get; private set; }

        public static void Init()
        {
            Packets = new Dictionary<int, Packet>();
            foreach (var type in Assembly.GetCallingAssembly().GetTypes())
            {
                Debug.Log("Type: " + type);
                if (type.BaseType != typeof(Packet)) continue;
                Debug.Log("Break A");
                var instance = (Packet) Activator.CreateInstance(type);
                Debug.Log("Break B");
                var customAttributes = Attribute.GetCustomAttributes(type);
                Debug.Log("Break C");
                var flag = false;
                foreach (var attribute in customAttributes)
                {
                    Debug.Log("Break D");
                    var packetOpcode = (PacketOpcode) attribute;
                    Debug.Log("Break E: " + packetOpcode);
                    if (packetOpcode == null)
                        Debug.Log("Opcode null");
                    if (instance == null)
                        Debug.Log("Packet null");
                    if (packetOpcode == null) continue;
                    Packets.Add(packetOpcode.Value, instance);
                    flag = true;
                    Debug.Log("Added packet [" + packetOpcode.Value + "] to the packet Dictionary<>.");
                }
                if (!flag)
                    Debug.LogError("The packet [" + typeof (Type).FullName + "] was found in the assembly, but did not have an [PacketOpcode(#)] attribute, this packet will not be handled.");
            }
        }
    }
}