using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDR
{
    public class HDResult
    {
        public MyImageDouble HDR { get; set; }
        public double[][] response { get; set; }

        public HDResult(MyImageDouble HDR)
        {
            this.HDR = HDR;
            response = new double[HDR.numCh][];
            for (int i = 0; i < HDR.numCh; ++i)
                response[i] = new double[256];
        }
    }
}
