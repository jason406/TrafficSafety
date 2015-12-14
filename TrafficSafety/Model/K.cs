using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficSafety.Model
{
    class K //密度，根据流量和车道数计算交通波经过前后密度
    {
        private Double k1, k2;

        private Double q;//流量

        private double kj, vl;
        private double km;//最大流量对应的密度
        public double qm { get; private set; }

        //private const double w3 = 16 / 3.6;  //错了
        private const double w3 = 16;
        private int numOfLane;//车道数
        private int numOfLaneLeft;
        public K(double vl, double kj) //新newell模型，输入参数自由流速度vl,阻塞密度kj,启动波速w3 km/h
        {
            this.vl = vl;
            this.kj = kj;            
            this.km = w3 * kj / (vl + w3);
            this.qm = vl * km;
        }
        public double getK(double q, bool isCongestion)
        {
            if (q>qm)
            {
                q = qm;
            }
            if (!isCongestion)
            {
                return q / vl;
            } 
            else
            {
                return -q / w3 + kj;
            }
        }


        /// <summary>
        /// 密度构造函数
        /// </summary>
        /// <param name="q">流量</param>
        /// <param name="n">车道数</param>
        public K(Double q, int n)
        {
            this.q = q;
            this.numOfLane = n;
            
            this.k1 = 75 * n + Math.Sqrt(5625 * Math.Pow(n, 2) - 2.5 * n * q);
            this.k2 = 75 * n - Math.Sqrt(5625 * Math.Pow(n, 2) - 2.5 * n * q);
        }

        public Double getK1()
        {
            return k1;
        }

        public Double getK2()
        {
            return k2;
        }

        public static void main(String[] args) {
		K k = new K(3600D, 3);
		Console.WriteLine("k1: " + k.getK1());
		Console.WriteLine("k2: " + k.getK2());
	}
    }
}
