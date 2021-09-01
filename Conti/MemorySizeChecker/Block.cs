using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemStackSizeChecker
{
    class Block
    {
        public string BlockName { get; set; }
        public string RamBlockName { get; set; }
        public string BlockLength { get; set; }
        public string BlockLengthBuffer { get; set; }        

        public List<string> ReturnBlock { get; set; }

        public Block()
        {
            ReturnBlock = new List<string>();
        }
        // The following constructor has parameters for two of the three 
        // properties. 
        public Block(string name, string ramblockname, string blocklength, string blocklengthbuffer)
        {
            BlockName = name;
            RamBlockName = ramblockname;
            BlockLength = blocklength;
            BlockLengthBuffer = blocklengthbuffer;
        }
    }
}
