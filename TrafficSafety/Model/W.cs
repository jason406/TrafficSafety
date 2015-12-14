using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficSafety.Model
{
    class W //交通波
    {
        private double q1, q2, k1, k2;
        public double v1 { get;  set; }
        public double v2 { get;  set; }
        private double w; //波速
        private int numOfLaneLeft { get; set; }// 剩余车道数
        private int numOfLane { get; set; }// 总车道数
        public double timeOfArrive{ get; set; }//到达时间
        public double locationOfArrive { get; set; }//到达位置
        public double timeOfDeparture { get; set; }//离去时间
        public double locationOfDeparture { get; set; }//离去位置
        public bool isFinished { get; set; }//是否传播结束
        private RoadSection road;
        public int kind; //区分W1，W2，W3，W1b，分别为1,2,4,3 数字大的覆盖数字小的


        private const double w3 = 16 / 3.6; //启动波速
        
        private void calculateW()
        {
            w = (q1 - q2) / (k1 - k2) /3.6; //单位m/s
            v1 = q1 / k1; //单位km/h
            v2 = q2 / k2;
            //double vf = road.Vl;
            //double kj = road.kj;

            //v1 = vf * (1 - k1 / kj); //单位km/h
            //v2 = vf * (1 - k2 / kj);
        }
        public W(double vf, double kj)//启动波 Uw = u/(h*kj*u-1)...W3=16km/h
        {
            //w = -vf / 3.6;
            //v2 = vf;
            //double h = 2.18/3600; //车头时距
            //w = - vf / (h * kj * vf - 1)/3.6;
            //v2 = vf;

            

        }

        public W(double vf)//启动波
        {
            w = -w3;
            v2 = vf;
        }
        public W(double qm_perLane, double kj, double q1, int numOfLane, int numOfLaneLeft, bool isCongestion )
        {
            if (isCongestion)
            {

            } 
            else
            {
                this.q1 = q1;
                this.k1 = (kj / (2*qm_perLane*numOfLane)) * q1;
                this.q2 = qm_perLane * numOfLaneLeft;
                if (this.q2 > this.q1) this.q2 = this.q1;
                this.k2 = (2 * qm_perLane * numOfLane - q2) / (2 * qm_perLane * numOfLane / kj);
            
            }
            calculateW();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="zcn">总车道数</param>
        /// <param name="scn">剩余车道数</param>
        /// <param name="q1"></param>
        public W(int zcn, int scn, double q1)
        {
             this.numOfLane = zcn;
            this.numOfLaneLeft = scn;
            this.q1 = q1;
            this.q2 = 1600D * this.numOfLaneLeft;
            if (this.q1 <= this.q2)
                this.q2 = this.q1;
            K K1 = new K(this.q1, this.numOfLane);//流量 
            K K2 = new K(this.q2, this.numOfLane);

            this.k1 = K1.getK2();
            this.k2 = K2.getK1();
            calculateW();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="zcn">总车道数</param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        public W(int zcn, double q1, double q2)
        {
            this.numOfLane = zcn;
            this.q1 = q1;
            this.q2 = q2;
            K K1 = new K(this.q1, this.numOfLane);
            K K2 = new K(this.q2, this.numOfLane);
            this.k1 = K1.getK1();
            this.k2 = K2.getK1();
            calculateW();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <param name="k1"></param>
        /// <param name="k2"></param>
        public W(double q1, double q2, double k1, double k2, RoadSection road)
        {
            this.q1 = q1;
            this.q2 = q2;
            this.k1 = k1;
            this.k2 = k2;
            this.road = road;
            calculateW();
        }

        public double getW()
        {
            if (w<0)
            {
                return -w;
            } 
            else
            {
                return w;
            }
        }
        public void setW(double w)
        {
            this.w = -w;
        }

        public double getQ1()
        {
            return q1;
        }

        public void setQ1(double q1)
        {
            this.q1 = q1;
        }

        public double getQ2()
        {
            return q2;
        }

        public void setQ2(double q2)
        {
            this.q2 = q2;
        }

        public double getK1()
        {
            return k1;
        }

        public void setK1(double k1)
        {
            this.k1 = k1;
        }

        public double getK2()
        {
            return k2;
        }

        public void setK2(double k2)
        {
            this.k2 = k2;
        }

        public String toString()
        {
            return "q1:" + q1 + " q2:" + q2 + " k1:" + k1 + " k2:" + k2 + " w:" + w;
        }

        public static void main(String[] args) {
        ////W w = new W(1000D, 2000D, 120D, 342D);
        //Console.WriteLine("w: " + w.getW());
        //W w2 = new W(3,2,4000D);
        //Console.WriteLine("w2: " + w2.getW());
        //W w3 = new W(3,1600D,1200D);
        //Console.WriteLine("w2: " + w3.getW());
	}
    }
}
