using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace csharp_client
{
    class Message
    {
        private int _type;
        public int type
        {set{_type = type;} get{return _type;}}

        private int _length;

        private string _content;
        public string content
        { set { _content = content; } get { return _content; } }

        int serializeToBytes(out byte[] data)
        {
            data = new byte[1];


            return 0;
        }

        void ParseFromBytes(byte[] data)
        {
 
        }
    }
}
