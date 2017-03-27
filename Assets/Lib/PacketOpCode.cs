using System;

namespace YggioUnity
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
    public class PacketOpcode : Attribute
    {
        public int Value { get; private set; }

        public PacketOpcode(int opcode)
        {
            if (opcode < 1)
                throw new Exception("The packet opcode must be a value greater than zero!");
            Value = opcode;
        }
    }
}