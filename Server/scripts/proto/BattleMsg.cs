using Net;
namespace Proto
{
    public class MsgMove : MsgBase
    {
        public MsgMove()
        {
            protoName = "MsgMove";
        }

        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class MsgAttack : MsgBase
    {
        public MsgAttack()
        {
            protoName = "MsgAttack";
        }

        private string desc = "";

        public string Desc
        {
            get
            {
                return desc == "" ? "127.0.0.1:6543" : desc;
            }
            set
            {
                desc = value;
            }
        }
    }
}