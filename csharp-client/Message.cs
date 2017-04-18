using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace csharp_client
{
    class Message
    {
        public Message(){ }

        private int _type;
        public int Type
        {
            set{_type = value;}
            get{return _type;}
        }

        private int _length;

        private string _content;
        public string Content
        {
            set{_content = value;}
            get{return _content;}
        }

        public int serializeToBytes(out byte[] data)
        {
            _length = 4 + (int)_content.Length;
            data = new byte[_length];
            byte[] bType = BitConverter.GetBytes(_type);
            Array.Copy(bType, 0, data, 0, 4);
            byte[] bContent = System.Text.Encoding.Default.GetBytes(_content);
            Array.Copy(bContent, 0, data, 4, _length - 4);
            //data[_length] = (byte)('\0');
            return _length;
        }

        public void ParseFromBytes(byte[] data , int length)
        {
            if (data == null || data.Length == 0) { return; }
            if (length < 4) { return; }
            _length = length - 4;
            _type = BitConverter.ToInt32(data,0);
            _content = System.Text.Encoding.Default.GetString(data, 4, _length);   
        }
    }
}
