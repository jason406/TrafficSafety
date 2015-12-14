using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficSafety.Model
{
    class Zone //影响区域
    {
        public RoadSection road;
        public double start;//开始位置
        public double end;
        public int congestionLevel; //拥堵等级，分为1--5，5为严重拥堵
        public System.Drawing.Color color; //颜色，表示拥堵等级
        public double speed;//车速，计算拥堵等级
        private double lookTime;//查看时间
        public double t0;
        public int waveKind;

        public Zone(RoadSection road, double speed,double start,double end,int waveKind, double t0,double lookTime)
        {
            this.road = road;
            this.speed = speed;
            this.start = start;
            this.end = end;
            this.waveKind = waveKind;
            this.t0 = t0;
            this.lookTime = lookTime;
            processCongestionLevel();
        }

        private void processCongestionLevel()
        {

            int Vl = road.Vl; //限速

            if (speed > 0.8 * Vl)
            {
                this.congestionLevel = 1;
            }
            else if (speed > 0.62 * Vl)
            {
                this.congestionLevel = 2;
            }
            else if (speed > 0.43 * Vl)
            {
                this.congestionLevel = 3;
            }
            else if (speed > 0.25 * Vl)
            {
                this.congestionLevel = 4;
            }
            else 
            {
                this.congestionLevel = 5;
            }

            //switch (road.kind)
            //{
            //    case 1:
            //        if (speed > 65)
            //        {
            //            this.congestionLevel = 1;
            //        }
            //        else if (speed > 50)
            //        {
            //            this.congestionLevel = 2;
            //        }
            //        else if (speed > 35)
            //        {
            //            this.congestionLevel = 3;
            //        }
            //        else if (speed > 20)
            //        {
            //            this.congestionLevel = 4;
            //        }
            //        else
            //        {
            //            this.congestionLevel = 5;
            //        }
            //        break;
            //    case 2:
            //        if (speed > 40)
            //        {
            //            this.congestionLevel = 1;
            //        }
            //        else if (speed > 30)
            //        {
            //            this.congestionLevel = 2;
            //        }
            //        else if (speed > 20)
            //        {
            //            this.congestionLevel = 3;
            //        }
            //        else if (speed > 15)
            //        {
            //            this.congestionLevel = 4;
            //        }
            //        else
            //        {
            //            this.congestionLevel = 5;
            //        }
            //        break;
            //    default: //3或4
            //        if (speed > 35)
            //        {
            //            this.congestionLevel = 1;
            //        }
            //        else if (speed > 25)
            //        {
            //            this.congestionLevel = 2;
            //        }
            //        else if (speed > 15)
            //        {
            //            this.congestionLevel = 3;
            //        }
            //        else if (speed > 10)
            //        {
            //            this.congestionLevel = 4;
            //        }
            //        else
            //        {
            //            this.congestionLevel = 5;
            //        }
            //        break;

            //}
        }
        
    }
}
