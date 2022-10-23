namespace NDSRom.DataStructures.FileNameTable
{
    abstract class Node
    {
        public Node()
        {
            this.Id = 0;
        }
        public Node(ushort Id)
        {
            this.Id = Id;
        }

        public ushort Id
        {
            get;
            set;
        }
    }
}