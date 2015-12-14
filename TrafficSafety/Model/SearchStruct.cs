using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficSafety.Model
{
    public struct RoadSearch
    {
       public bool[] isReached; //查询标识，如果某条道路查询过了就为true
       public double[] t0;
       public int[] coverCount;
       public int passTurningCount;
    }
    class SearchStruct
    {
        public double T12, T23, t0;
        public double q1; //事发流量
        public double q2; //民警处理后流量
        public double q3;
        public int numOfLaneLeft { get; set; }// 剩余车道数
        public int spreadCase; //情况1,2,3
        public RoadSection road;//当前路段
        private W W1 { get; set; }//各列交通波的存根
        private W W2 { get; set; }
        private W W3 { get; set; }
        private W W1b { get; set; }
        private W W2b { get; set; }


        public SearchStruct(RoadSection road, int numOfLaneLeft,double q1, double q2, double q3, double T12, double T23, double t0, int spreadCase)
        {
            this.road = road;
            this.numOfLaneLeft = numOfLaneLeft;
            this.T12 = T12;
            this.T23 = T23;
            this.t0 = t0;            
            this.spreadCase = spreadCase;
            this.q1 = q1;
            this.q2 = q2;
            this.q3 = q3;
        }

        public void makeMainW()
        {
            K k = new K(this.road.qm_perLane * this.road.numOfLane, this.road.kj);

            this.q2 = this.numOfLaneLeft * this.road.qm_perLane;
            double k1 = k.getK(this.q1, false);
            double k2 = k.getK(q2, true);
            W1 = new W(q1, q2, k1, k2, this.road);

            this.q3 = (this.numOfLaneLeft - 1) * this.road.qm_perLane;
            double k3 = k.getK(q3, true);
            W2 = new W(q1, q3, k1, k3, this.road);


            W1b = new W(q1, q3, k1, k3, this.road);
            W3 = new W(2 * this.road.qm_perLane * this.road.numOfLane / this.road.kj);//！
            W2b = new W(2 * this.road.qm_perLane * this.road.numOfLane / this.road.kj);//！
            W1.kind = 1; W2.kind = 2; W3.kind = 4; W1b.kind = 3; W2b.kind = 5;
        }

        public void makeW()
        {
            K k = new K(this.road.qm_perLane * this.road.numOfLane, this.road.kj);
            if (q1 > this.road.qm_perLane * this.road.numOfLane)
            //if (true)
            {
                double ratio1 = q2 / q1;
                double ratio2 = q3 / q2;
                q1 = this.road.qm_perLane * this.road.numOfLane * 0.8;
                q2 = q1 * ratio1;
                q3 = q2 * ratio2;
            }
            ////测试代码
            //this.q1 = this.road.qm_perLane * this.road.numOfLane * 0.8;
            //this.q2 = this.numOfLaneLeft * this.road.qm_perLane;
            //this.q3 = (this.numOfLaneLeft - 1) * this.road.qm_perLane;
            ////测试代码结束

            System.Diagnostics.Debug.WriteLine(this.q1 + ",,," + this.q2 + ",,," + this.q3);
            double k1 = k.getK(this.q1, false);

            double k2 = k.getK(q2, true);
            W1 = new W(q1, q2, k1, k2, this.road);


            double k3 = k.getK(q3, true);
            W2 = new W(q1, q3, k1, k3, this.road);



            if (double.IsNaN(q1) || double.IsNaN(q3) || double.IsNaN(k1) || double.IsNaN(k3))
            {
                throw new Exception("NAN");
            }
            W1b = new W(q1, q3, k1, k3, this.road);

            //W3 = new W(2 * this.road.qm_perLane * numOfLane / this.road.kj);//！
            //W2b = new W(2 * this.road.qm_perLane * numOfLane / this.road.kj);//！
            W3 = new W(q3, this.road.qm_perLane * this.road.numOfLane, k3, 0.5 * k3, this.road);
            W3.v2 = this.road.Vl;
            W2b = new W(q3, this.road.qm_perLane * this.road.numOfLane, k3, 0.5 * k3, this.road);
            W2b.v2 = road.Vl;
            //System.Diagnostics.Debug.WriteLine("t0="+this.t0+"...W3="+this.W3.getW()+"...W2:"+this.W2.getW()+"...W1="+this.W1.getW()+"...W1b="+this.W1b.getW());
            if (W3.getW() < W1b.getW()) throw new Exception("WRONG");
            if (W2.getW() < W1.getW()) throw new Exception("WRONG");
            if (W2b.getW() < W1.getW()) throw new Exception("WRONG");
            W1.kind = 1; W2.kind = 2; W3.kind = 4; W1b.kind = 3; W2b.kind = 5;
        }
    }
}
