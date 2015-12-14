using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficSafety.Model
{
    class Duration //计算波的追赶、叠加时间
    {
        private RoadSection road;

        public double t0{get; private set;}  //第一列波传入当前路段时刻
        private Double T12; //W1与W2时间差
        private Double T23; //W2与W3时间差
        private Double t12; //W2追上W1时刻
        private Double t23; //W3追上W2时刻，或者W3追上W1b时刻，或者W2b追上W1时刻
        private double l12;//W2追上W1的位置
        private double l23;//W3追上W2的位置
        private Double w1;
        private Double w1b;
        private Double w2;
        private Double w2b;
        private Double w3;

        public W W1 { get; set; }//各列交通波的存根
        public W W2 { get; set; }
        public W W3 { get; set; }
        public W W1b { get; set; }
        public W W2b { get; set; }

        private double l { get; set; } //当前路段长度
        public double lm { get; private set; }//最大影响长度
        public double t { get; private set; }//传播结束时间
        public Double T12next { get;  private set; } //下一路段W1与W2时间差
        public Double T23next { get; private set; } //下一路段W2与W3时间差
        public double t0next { get; private set; }  //下一路段t0

        private int spreadCase; //情况1,2,3
        public int spreadCaseNext { get; private set; } //下一路段情况1,2,3

        /// <summary>
        /// 构造函数，情况一
        /// </summary>
        /// <param name="W1"></param>
        /// <param name="W2"></param>
        /// <param name="W3"></param>
        /// <param name="t0">第一列波传入时刻</param>
        /// <param name="T12">W1与W2间隔时间</param>
        /// <param name="T23">W2与W3间隔时间</param>
        public Duration( double roadLength,W W1,  W W2,  W W3,  W W1b,  W W2b, double T12, double T23, double t0)
        {
            this.W1 = W1;
            this.W2 = W2;
            this.W3 = W3;
            this.W1b = W1b;
            this.W2b = W2b;
            this.w1 = W1.getW();
            this.w2 = W2.getW();
            this.w3 = W3.getW();
            this.w1b = W1b.getW();
            this.w2b = W2b.getW();
            this.T12 = T12;
            this.T23 = T23;
            this.t0 = t0;
            this.spreadCase = 1;
            this.l = roadLength;
            
            //测试代码
            //this.w1b = (W1 + W2) / 2;
            //this.w2b = (W2 + W3) / 2;
            
            execute();
        }
        /// <summary>
        /// 构造函数，情况二、三
        /// </summary>
        /// <param name="W1b">W1b或W1</param>
        /// <param name="W3">W3或W2b</param>
        /// <param name="t0">第一列波传入时刻</param>
        /// <param name="T23">W1b与W3间隔时间或W1与W2b间隔时间</param>
        /// <param name="spreadCase">2为情况2,3为情况3</param>
        public Duration(double roadLength, W W1,  W W3, double T23, double t0, int spreadCase)
        {
            switch (spreadCase)
            {
                case 2:
                    this.W1b = W1;
                    this.W3 = W3;
                    this.w1b = W1.getW();
                    
                    this.w3 = W3.getW();            
                    this.T23 = T23;
                    this.t0 = t0;
                    this.spreadCase = 2;
                    this.l = roadLength;
                    break;
                case 3:
                    this.W1 = W1;
                    this.W2b = W3;
                    this.w1 = W1.getW();
                    this.w2b = W3.getW();
                    this.T23 = T23;
                    this.t0 = t0;
                    this.spreadCase = 3;
                    this.l = roadLength;
                    break;
            }
            execute();

            
        }
        

        private void execute() 
        {
            if (double.IsNaN(w1b)) throw new Exception("WRONG");
            switch (spreadCase)
            {
                case 1://情况一  w1=1.329 w2=1.303 w3=4.44 w1b=1.303 w2b=4.44 l=53 T12=213 T23=98  t0=1116
                    t12 = w2 * T12 / (w2 - w1);                    
                    t23 = (w3 * (T12 + T23) - w2 * T12) / (w3 - w2);                    
                    if ((t12 < t23 && t12>0) || t23<=0)//即W2先追上W1，或者W3追不上W2
                    {
                        l12 = w1 * t12;
                        t = ((w1 - w1b) * t12 + w3 * (T12 + T23)) / (w3 - w1b);
                        lm = w3 * (t - T12 - T23);
                        if (l<=l12) //本路段未发生追赶，下一路段为情况一
                        {
                            t0next = t0 + l / w1;
                            T12next = l / w2 - l / w1 + T12;
                            T23next = l / w3 - l / w2 + T23;
                            spreadCaseNext = 1;

                            W1.timeOfArrive = t0;
                            W1.locationOfArrive = 0;
                            W1.timeOfDeparture = t0next;
                            W1.locationOfDeparture = l;

                            W2.timeOfArrive = t0 + T12;
                            W2.locationOfArrive = 0;
                            W2.timeOfDeparture = t0next + T12next;
                            W2.locationOfDeparture = l;

                            W3.timeOfArrive = t0+T12+T23;
                            W3.locationOfArrive = 0;
                            W3.timeOfDeparture = t0next + T12next + T23next;
                            W3.locationOfDeparture = l;

                            W1b = null; //清除不存在的交通波
                            W2b = null;
                        }
                        else if (l <= lm)//即在本路段W2追上W1形成W1b，那么下一路段为情况二
                        {
                            t0next = (l - l12) / w1b + t12 + t0;
                            T23next = l / w3 - (l - l12) / w1b + T12 + T23 - t12;
                            spreadCaseNext = 2;

                            W1.timeOfArrive = t0;
                            W1.locationOfArrive = 0;
                            W1.timeOfDeparture = t0+t12;
                            W1.locationOfDeparture = l12;
                            W1.isFinished = true;

                            W2.timeOfArrive = t0 + T12;
                            W2.locationOfArrive = 0;
                            W2.timeOfDeparture = t0+t12;
                            W2.locationOfDeparture = l12;
                            W2.isFinished = true;

                            W3.timeOfArrive = t0 + T12 + T23;
                            W3.locationOfArrive = 0;
                            W3.timeOfDeparture = t0next + T23next;
                            W3.locationOfDeparture = l;

                            W1b.timeOfArrive = t0+t12;
                            W1b.locationOfArrive = l12;
                            W1b.timeOfDeparture = t0next;
                            W1b.locationOfDeparture = l;

                            //清除不存在的交通波
                            W2b = null;
                        }
                        else
                        {
                            spreadCaseNext = 4;
                            
                            //传播结束，输出t和lm
                            W1.timeOfArrive = t0;
                            W1.locationOfArrive = 0;
                            W1.timeOfDeparture = t0 + t12;
                            W1.locationOfDeparture = l12;
                            W1.isFinished = true;

                            W2.timeOfArrive = t0 + T12;
                            W2.locationOfArrive = 0;
                            W2.timeOfDeparture = t0 + t12;
                            W2.locationOfDeparture = l12;
                            W2.isFinished = true;

                            W3.timeOfArrive = t0 + T12 + T23;
                            W3.locationOfArrive = 0;
                            W3.timeOfDeparture = t0+t;
                            W3.locationOfDeparture = lm;
                            W3.isFinished = true;

                            W1b.timeOfArrive = t0+t12;
                            W1b.locationOfArrive = l12;
                            W1b.timeOfDeparture = t0+t;
                            W1b.locationOfDeparture = lm;
                            W1b.isFinished = true;

                            //清除不存在的交通波
                            W2b = null;
                        }
                    }
                    else//即W3先追上W2
                    {
                        l23 = w2 * (t23 - T12);
                        t = (w3 * (t23 - T12 - T23) - w2b * t23) / (w1 - w2b);
                        lm = w1 * t;
                        if (l <= l23)//即在本路段未发生交通波的追赶，下一路段为情况一
                        {
                            t0next = l / w1 + t0;
                            T12next = l / w2 - l / w1 + T12;
                            T23next = l / w3 - l / w2 + T23;
                            spreadCaseNext = 1;

                            W1.timeOfArrive = t0;
                            W1.locationOfArrive = 0;
                            W1.timeOfDeparture = t0next;
                            W1.locationOfDeparture = l;

                            W2.timeOfArrive = t0 + T12;
                            W2.locationOfArrive = 0;
                            W2.timeOfDeparture = t0next + T12next;
                            W2.locationOfDeparture = l;

                            W3.timeOfArrive = t0 + T12 + T23;
                            W3.locationOfArrive = 0;
                            W3.timeOfDeparture = t0next + T12next + T23next;
                            W3.locationOfDeparture = l;

                            W1b = null; //清除不存在的交通波
                            W2b = null;
                        }
                        else if (l < lm)//即在本路段W3追上W2形成W2b，那么下一路段为情况三
                        {
                            t0next = l / w1 + t0;
                            T23next = (l - l23) / w2b + t23;
                            spreadCaseNext = 3;

                            W1.timeOfArrive = t0;
                            W1.locationOfArrive = 0;
                            W1.timeOfDeparture = t0next;
                            W1.locationOfDeparture = l;
                           
                            W2.timeOfArrive = t0 + T12;
                            W2.locationOfArrive = 0;
                            W2.timeOfDeparture = t0 + t23;
                            W2.locationOfDeparture = l23;
                            W2.isFinished = true;

                            W3.timeOfArrive = t0 + T12 + T23;
                            W3.locationOfArrive = 0;
                            W3.timeOfDeparture = t0 + t23;
                            W3.locationOfDeparture = l23;
                            W3.isFinished = true;

                            W2b.timeOfArrive = t0 + t23;
                            W2b.locationOfArrive = l23;
                            W2b.timeOfDeparture = t0next+T23next;
                            W2b.locationOfDeparture = l;

                            W1b = null; //清除不存在的交通波
                           
                        }
                        else
                        {
                            spreadCaseNext = 4;
                            //传播结束，输出t和lm
                            W1.timeOfArrive = t0;
                            W1.locationOfArrive = 0;
                            W1.timeOfDeparture = t0 + t;
                            W1.locationOfDeparture = lm;
                            W1.isFinished = true;

                            W2.timeOfArrive = t0 + T12;
                            W2.locationOfArrive = 0;
                            W2.timeOfDeparture = t0 + t23;
                            W2.locationOfDeparture = l23;
                            W2.isFinished = true;

                            W3.timeOfArrive = t0 + T12 + T23;
                            W3.locationOfArrive = 0;
                            W3.timeOfDeparture = t0 + t23;
                            W3.locationOfDeparture = l23;
                            W3.isFinished = true;

                            W2b.timeOfArrive = t0 + t23;
                            W2b.locationOfArrive = l23;
                            W2b.timeOfDeparture = t0 + t;
                            W2b.locationOfDeparture = lm;
                            W2b.isFinished = true;

                            W1b = null; //清除不存在的交通波
                            
                        }
                    }
                    break;
                case 2://情况二
                    t23 = w3 * T23 / (w3 - w1b);
                    l23 = w1b * t23;
                    if (l>l23)
                    {
                        spreadCaseNext = 4;
                        //传播结束
                        t = t23;
                        lm = l23;

                        W3.timeOfArrive = t0  + T23;
                        W3.locationOfArrive = 0;
                        W3.timeOfDeparture = t0 + t;
                        W3.locationOfDeparture = lm;
                        W3.isFinished = true;

                        W1b.timeOfArrive = t0;
                        W1b.locationOfArrive = 0;
                        W1b.timeOfDeparture = t0 + t;
                        W1b.locationOfDeparture = lm;
                        W1b.isFinished = true;
                    }
                    else//即在本路段未发生交通波追赶，那么下一路段为情况二
                    {
                        t0next = l / w1b + t0;
                        T23next = l / w3 - l / w1b + T23;
                        spreadCaseNext = 2;

                        W3.timeOfArrive = t0 + T23;
                        W3.locationOfArrive = 0;
                        W3.timeOfDeparture = t0next + T23next;
                        W3.locationOfDeparture = l;


                        W1b.timeOfArrive = t0;
                        W1b.locationOfArrive = 0;
                        W1b.timeOfDeparture = t0next;
                        W1b.locationOfDeparture = l;

                    }
                    break;
                case 3://情况三
                    t23 = w2b * T23 / (w2b - w1);
                    l23 = w1 * t23;
                    if (l > l23)
                    {
                        spreadCaseNext = 4;
                        //传播结束
                        t = t23;
                        lm = l23;

                        W1.timeOfArrive = t0 ;
                        W1.locationOfArrive = 0;
                        W1.timeOfDeparture = t0 + t;
                        W1.locationOfDeparture = lm;
                        W1.isFinished = true;

                        W2b.timeOfArrive = t0+T23;
                        W2b.locationOfArrive = 0;
                        W2b.timeOfDeparture = t0 + t;
                        W2b.locationOfDeparture = lm;
                        W2b.isFinished = true;
                    }
                    else//即在本路段未发生交通波追赶，那么下一路段为情况三
                    {
                        t0next = l / w1 + t0;
                        T23next = l / w2b - l / w1 + T23;
                        if (T23next>T23)
                        {
                            throw new Exception("Wrong");
                        }
                        spreadCaseNext = 3;

                        W1.timeOfArrive = t0;
                        W1.locationOfArrive = 0;
                        W1.timeOfDeparture = t0next;
                        W1.locationOfDeparture = l;
                        
                        W2b.timeOfArrive = t0 + T23;
                        W2b.locationOfArrive = 0;
                        W2b.timeOfDeparture = t0next+T23next;
                        W2b.locationOfDeparture = l;
                        
                    }
                    break;
            }
            check();
        }

        private void check()
        {
            switch (this.spreadCase)
            {
                case 1:
                    if (w1 > w3)
                    {
                        System.Diagnostics.Debug.WriteLine("情况1  w1>w3!");
                    }
                    //if (w2 > w3)
                    //{
                    //    System.Diagnostics.Debug.WriteLine("情况1  w2>w3!");
                    //}
                    if (w1b > w3)
                    {
                        System.Diagnostics.Debug.WriteLine("情况1  w1b>w3!");
                    }
                    break;
                case 2:
                    if (w1b > w3)
                    {
                        System.Diagnostics.Debug.WriteLine("情况2  w1b>w3!");
                    }
                    break;
                case 3:
                    if (w1 > w2b)
                    {
                        System.Diagnostics.Debug.WriteLine("情况2  w1>w2b!");
                    }
                    break;
            }


            if (t0<0 ||t12<0||t<0||t0next<0||T12<0||T12next<0||T23<0||T23next<0)
            {
                System.Diagnostics.Debug.WriteLine("时间有误！小于0");
            }
            if (double.IsNaN(t0) || double.IsNaN(t12) || double.IsNaN(t) || double.IsNaN(t0next) || double.IsNaN(T12) || double.IsNaN(T23) || double.IsNaN(T12next) || double.IsNaN(T23next))
            {
                System.Diagnostics.Debug.WriteLine("时间有误！NaN");
            }
            if (w1<0||w1b<0||w2<0||w2b<0||w3<0)
            {
                System.Diagnostics.Debug.WriteLine("波速有误！小于0");
            }
            if (l < 0 || l12 < 0 || l23 < 0 || lm < 0)
            {
                System.Diagnostics.Debug.WriteLine("长度有误！小于0");
            }

            if (W1 != null && (W1.timeOfArrive < 0 || W1.timeOfDeparture < W1.timeOfArrive || W1.locationOfArrive < 0 || W1.locationOfDeparture < 0))
            {
                System.Diagnostics.Debug.WriteLine("W1有误！到离时间或位置不对");
            }

            if (W2 != null && (W2.timeOfArrive < 0 || W2.timeOfDeparture < W2.timeOfArrive || W2.locationOfArrive < 0 || W2.locationOfDeparture < 0))
            {
                System.Diagnostics.Debug.WriteLine("W2有误！到离时间或位置不对");
            }

            if (W3 != null && (W3.timeOfArrive < 0 || W3.timeOfDeparture < W3.timeOfArrive || W3.locationOfArrive < 0 || W3.locationOfDeparture < 0))
            {
                System.Diagnostics.Debug.WriteLine("W3有误！到离时间或位置不对" + this.spreadCase);
            }

            if (W1b != null && (W1b.timeOfArrive <= 0 || W1b.timeOfDeparture < W1b.timeOfArrive || W1b.locationOfArrive < 0 || W1b.locationOfDeparture < 0))
            {
                System.Diagnostics.Debug.WriteLine("W1b有误！到离时间或位置不对" + this.spreadCase);
            }

            if (W2b != null && (W2b.timeOfArrive <= 0 || W2b.timeOfDeparture < W2b.timeOfArrive || W2b.locationOfArrive < 0 || W2b.locationOfDeparture < 0))
            {
                System.Diagnostics.Debug.WriteLine("W2b有误！到离时间或位置不对");
            }

            
        }
    }
}
