using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficSafety.Model
{
    public class GlobalConst
    {
        public const double TURNING_RATIO = 0.4;//0.3
        public const double STRAIGHT_RATIO = 1.01; //1.02
        public const double FLOW_RATIO = 0.1;//当前流量和最大流量的比率
        public const bool CONSIDER_TUNING = true;
    }
}
