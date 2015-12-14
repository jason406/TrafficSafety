using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficSafety.Model
{

    class AccidentInfo
    {
        public double T12, T23, t0; 
        public double q1; //事发流量
        //public double q2; //
        public int numOfLane, numOfLaneLeft;
        public RoadSection road;
        public AccidentInfo(double t0, double T12, double T23, double q1, int numOfLane, int numOfLaneLeft, double accidentPoint)
        {
            
            this.t0 = t0;
            this.T12 = T12;
            this.T23 = T23;
            this.q1 = q1;
            this.numOfLane = numOfLane;
            this.numOfLaneLeft = numOfLaneLeft;

        }
    }
}
