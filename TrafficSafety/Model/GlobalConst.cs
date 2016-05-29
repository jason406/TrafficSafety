using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficSafety.Model
{
    public class GlobalConst
    {
        public static double TURNING_RATIO = 0.3;//0.3
        public static double STRAIGHT_RATIO = 1.01; //1.02
        public static double FLOW_RATIO = 0.6;//当前流量和最大流量的比率
        public static bool CONSIDER_TUNING = true;
    }
}
