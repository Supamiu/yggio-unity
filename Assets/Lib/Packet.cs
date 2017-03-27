using UnityEngine;

namespace YggioUnity
{
    public abstract class Packet
    {
        public abstract void Decode();

        public static void send(PacketType packetType, int opcode, params object[] data)
        {
            ByteBuffer byteBuffer = new ByteBuffer(packetType, opcode);
            foreach (object obj in data)
            {
                Debug.Log("Data type: " + obj.GetType().ToString().ToUpper());
                switch (obj.GetType().ToString().ToUpper())
                {
                    case "SYSTEM.BYTE":
                        byteBuffer.WriteByte((byte) obj);
                        break;
                    case "SYSTEM.INT32":
                        byteBuffer.WriteInt((int) obj);
                        break;
                    case "SYSTEM.DOUBLE":
                        byteBuffer.WriteDouble((double) obj);
                        break;
                    case "SYSTEM.SINGLE":
                        byteBuffer.WriteFloat((float) obj);
                        break;
                    case "SYSTEM.BOOLEAN":
                        byteBuffer.WriteBoolean((bool) obj);
                        break;
                    case "SYSTEM.STRING":
                        byteBuffer.WriteString((string) obj);
                        break;
                    case "SYSTEM.CHAR":
                        byteBuffer.WriteChar((char) obj);
                        break;
                    case "SYSTEM.VECTOR2":
                        byteBuffer.WriteVector2((Vector2) obj);
                        break;
                    case "SYSTEM.VECTOR3":
                        byteBuffer.WriteVector3((Vector3) obj);
                        break;
                }
            }
            byteBuffer.Send();
        }
    }
}
