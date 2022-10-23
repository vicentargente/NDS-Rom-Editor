using System.Collections.Generic;
using System.IO;

namespace NDSRom.DataStructures.FileNameTable
{
    class FNTDirectory : Node
    {
        public Dictionary<string, Node> Children { get; }
        public FNTDirectory(ushort Id) : base(Id)
        {
            this.Children = new Dictionary<string, Node>();
        }

        public void AddChild(string name, Node child)
        {
            this.Children.Add(name, child);
        }

        public Node GetChild(string name)
        {
            Node res;
            if (!this.Children.TryGetValue(name, out res)) throw new FileNotFoundException();
            return res;
        }
    }
}
