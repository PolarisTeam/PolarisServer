using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolarisServer.Models
{
    class PSOObject
    {
        class PSOObjectThing
        {
            UInt32 data;
        }

        int ObjectID { get; set; }
        MysteryPositions Position { get; set; }
        string Name;
        PSOObjectThing[] things;

    }
}
