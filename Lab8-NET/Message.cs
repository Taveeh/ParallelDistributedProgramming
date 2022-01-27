using System;
using System.Collections.Generic;
using System.Text;

namespace Lab8_NET
{
    [Serializable]
    public class Message
    {
        public long mpiId;
        public long change;
        public bool first;
        public long value;
        public long time;

        public Message() { }

        public Message(long mpiId, long change, bool first, long value, long time)
        {
            this.mpiId = mpiId;
            this.change = change;
            this.first = first;
            this.value = value;
        }

        public override string ToString()
        {
            return "Message { id: " + mpiId.ToString() + ", change: " + change.ToString() + ", first: " + first.ToString() + ", value "
                + value.ToString() + ", time: " + time + " }";
        }
    }

}
