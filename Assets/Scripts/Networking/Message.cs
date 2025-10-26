using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking
{
    public class Message
    {
        public MessageId Id { get; private set; }

        private MemoryStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;
        private bool isClose = false;

        //dung cho read
        public Message(byte[] data)
        {
            stream = new MemoryStream(data);
            reader = new BinaryReader(stream, Encoding.UTF8);//cái này k cần true vì 1 message chỉ có read hoặc write
            this.Id = (MessageId)ReadShort();
        }

        //dung cho write
        public Message(MessageId idMessage)
        {
            stream = new MemoryStream();
            writer = new BinaryWriter(stream, Encoding.UTF8);//cái này k cần true vì 1 message chỉ có read hoặc write
            this.Id = (MessageId)idMessage;
            WriteShort((short)idMessage);
        }


        public byte[] GetData()
        {
            writer.Flush();
            return stream.ToArray();
        }


        public void WriteString(string value)
        {
            writer.Write(value);
        }

        public void WriteLong(long value)
        {
            writer.Write(value);
        }

        public void WriteInt(int value)
        {
            writer.Write(value);
        }

        public void WriteShort(short value)
        {
            writer.Write(value);
        }

        public void WriteByte(byte value)
        {
            writer.Write(value);
        }

        public void WriteSByte(sbyte value)
        {
            writer.Write(value);
        }

        public void WriteBool(bool value)
        {
            writer.Write(value);
        }


        public string ReadString()
        {
            return reader.ReadString();
        }
        public long ReadLong()
        {
            return reader.ReadInt64();
        }

        public int ReadInt()
        {
            return reader.ReadInt32();
        }

        public short ReadShort()
        {
            return reader.ReadInt16();
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        public sbyte ReadSByte()
        {
            return reader.ReadSByte();
        }

        public bool ReadBool()
        {
            return reader.ReadBoolean();
        }

        public void Close()
        {
            if (isClose)
            {
                return;
            }
            isClose = true;
            reader?.Close();
            writer?.Close();
            stream?.Close();
        }
    }
}
