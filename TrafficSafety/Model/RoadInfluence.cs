using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficSafety.Model
{
    class RoadInfluence
    {
        //protected string RoadID{ get; set; }//道路ID
        private double _q1, _q2, _q3;
        private double T12, T23,t0;
        private double q1; //事发流量
        private double q2; //民警处理后流量
        private double q3;
        private W W1 { get; set; }//各列交通波的存根
        private W W2 { get; set; }
        private W W3 { get; set; }
        private W W1b { get; set; }
        private W W2b { get; set; }
        private int spreadCase; //情况1,2,3
        private int numOfLaneLeft { get; set; }// 剩余车道数
        private int numOfLane { get; set; }// 总车道数
        private Duration duration;
        private RoadSection road;//当前路段
        private List<RoadSection> nextRoads;
        private List<RoadSection> roadResults = new List<RoadSection>();

        private int passTurningCount = 0;
        private int passStraightCount = 0;

        public const double TURNING_RATIO = 0.4;//0.3
        public const double STRAIGHT_RATIO = 1.1; //1.02
        public const double FLOW_RATIO = 0.9;//当前流量和最大流量的比率
        public const bool CONSIDER_TUNING = true;
        bool[] isReached; //查询标识，如果某条道路查询过了就为true
        RoadSearch roadSearch;

        private ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset;
        public RoadInfluence()
        {

        }


        public RoadInfluence(ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset, RoadSection road, int numOfLane, int numOfLaneLeft, double q1, double q2, double q3, double T12, double T23, double t0, int spreadCase, ref RoadSearch roadSearch, int passTurningCount, int passStraightCount)
        {
            this.networkDataset = networkDataset;
            this.roadSearch = roadSearch;
            //this.RoadID = RoadID;
            this.road = road;
            this.T12 = T12;
            this.T23 = T23;
            this.t0 = t0;
            roadSearch.t0[road.EID] = t0;
            this.spreadCase = spreadCase;
            this.road.spreadCase = spreadCase;
            this.numOfLane = numOfLane;
            this.numOfLaneLeft = numOfLaneLeft;
            this.passTurningCount = passTurningCount;
            this.passStraightCount = passStraightCount;


            if (road.isCrossing)//如果是交叉口，流量折减
            {
                //this.q1 = q1 * road.signalSplit;
                //this.q2 = q2 * road.signalSplit;
                //this.q3 = q3 * road.signalSplit;

                this._q1 = q1;
                
                this._q2 = q2  ;//流量折减
                this._q3 = q3 ;
                
            } 
            else
            {
                this._q1 = q1 ;
                //this.q1 = road.qm_perLane * road.numOfLane * 0.8;
                this._q2 = q2 ;
                this._q3 = q3 ;
            }


            

            if (q1 == 0) throw new Exception("WRONG");
            //测试代码
            //this.q1 = 3000;
            //this.q2 = 1800;
            //breadthFirstExecute();
            //execute();
        }

        public void execute()
        {
            //W1=new W(numOfLane,numOfLaneLeft,this.q1);
            //W2 = new W(numOfLane, W1.getQ2(), this.q2); //民警处理后第二道集结波
            //W1b = new W(W1.getQ1(), W2.getQ2(), W1.getK1(), W2.getK2());
            //W3 = new W(numOfLane, W2.getQ2(), 2000D * numOfLane);
            //W2b=new W(W2.getQ1(),W3.getQ2(),W2.getK1(),W3.getK2());
            makeW();

            System.Diagnostics.Debug.WriteLine("当前路段：" + "，情况" + this.spreadCase);

            //System.Diagnostics.Debug.Write("\"" + road.RoadID + "\",");
            switch (spreadCase)
            {
                case 1:
                    duration = new Duration(road.Length, W1,  W2,  W3,  W1b,  W2b, T12, T23, t0);
                    
                    break;
                case 2:
                    duration = new Duration(road.Length, W1b, W3, T23, t0, spreadCase);
                    
                    break;
                case 3:
                    duration = new Duration(road.Length, W1, W2b, T23, t0, spreadCase);
                    System.Diagnostics.Debug.WriteLine("W1：" + Convert.ToSingle(W1.getW()) + "W2b:" + Convert.ToSingle(W2b.getW()) + "T23:" + Convert.ToSingle(T23));
                    break;
            }
                       

            if (duration.spreadCaseNext==4)//传播结束
            {
                
                //System.Diagnostics.Debug.WriteLine("路段"+road.RoadID+"传播结束，t="+duration.t+" lm="+duration.lm+" t0="+ duration.t0);
                road.isFinished = true;
                road.timeOfFinish = duration.t0 + duration.t;
                //System.Diagnostics.Debug.WriteLine(road.RoadID + "," + road.timeOfFinish);
            } 
            else
            {

                //nextRoads = road.getRoadList();
                
                nextRoads = road.searchRoad(road.RoadID, road.isAlongDigitized, road.startNode, networkDataset, ref roadSearch);
                if (road.RoadID == 77164)
                {
                   // System.Diagnostics.Debug.WriteLine("当前路段" + road.RoadID + " nextroadsCount:" + nextRoads.Count);
                }

                if (nextRoads.Count==0)
                {
                    //System.Diagnostics.Debug.WriteLine("Dead end！");
                }
                else
                {
                    
                    //System.Diagnostics.Debug.WriteLine("下一段路" + "T12:" + duration.T12next + " T23:" + duration.T23next + " t0:" + duration.t0next + " 情况" + duration.spreadCaseNext);
                    foreach (RoadSection newRoad in nextRoads)
                    {
                        //roadSearch.t0[newRoad.RoadEID] = duration.t0next;
                        //newRoad.t0 = t0;
                        newRoad.t0 = duration.t0next;
                        RoadInfluence newRoadInfluence = new RoadInfluence(networkDataset, newRoad, numOfLane, numOfLaneLeft, this.q1, this.q2,this.q3, duration.T12next, duration.T23next, duration.t0next, duration.spreadCaseNext,ref roadSearch,this.passTurningCount,this.passStraightCount);
                        newRoadInfluence.execute();
                        roadResults.AddRange(newRoadInfluence.getRoadResults());
                    }
                }
                 
            }
            saveResults();//存储结果
            //roadResults.Add(road);
        }
        public List<RoadInfluence> breadthFirstExecute()
        {
            if (this.road.RoadID == 9008)
            {
                int aaa;
            }

            List<RoadInfluence> newRoadinfluences = new List<RoadInfluence>();
            makeW();
            //System.Diagnostics.Debug.WriteLine("当前路段：" + "，情况" + this.spreadCase);
            //System.Diagnostics.Debug.Write("\"" + road.RoadID + "\",");
            switch (spreadCase)
            {
                case 1:
                    duration = new Duration(road.Length, W1, W2, W3, W1b, W2b, T12, T23, t0);

                    break;
                case 2:
                    if (W1b.getW() > W3.getW()) throw new Exception("Wrong");
                    duration = new Duration(road.Length, W1b, W3, T23, t0, spreadCase);

                    break;
                case 3:
                    duration = new Duration(road.Length, W1, W2b, T23, t0, spreadCase);
                    //System.Diagnostics.Debug.WriteLine("W1：" + Convert.ToSingle(W1.getW()) + "W2b:" + Convert.ToSingle(W2b.getW()) + "T23:" + Convert.ToSingle(T23));
                    break;
            }

            this.road.spreadCaseNext = duration.spreadCaseNext;


            if (duration.spreadCaseNext == 4)//传播结束
            {

                //System.Diagnostics.Debug.WriteLine("路段"+road.RoadID+"传播结束，t="+duration.t+" lm="+duration.lm+" t0="+ duration.t0);
                road.isFinished = true;
                road.timeOfFinish = duration.t0 + duration.t;

                //System.Diagnostics.Debug.WriteLine(road.RoadID + "," + road.timeOfFinish);
            }
            else
            {

                ////nextRoads = road.getRoadList();

                nextRoads = road.searchRoad(road.RoadID, road.isAlongDigitized, road.startNode, networkDataset, ref roadSearch);
                
                //if (nextRoads.Count == 0)
                //{
                //    //System.Diagnostics.Debug.WriteLine("Dead end！");
                //}
                //else
                //{

                //    //System.Diagnostics.Debug.WriteLine("下一段路" + "T12:" + duration.T12next + " T23:" + duration.T23next + " t0:" + duration.t0next + " 情况" + duration.spreadCaseNext);
                //    foreach (RoadSection newRoad in nextRoads)
                //    {
                        
                //        newRoad.t0 = duration.t0next;
                //        RoadInfluence newRoadInfluence = new RoadInfluence(networkDataset, newRoad, numOfLane, numOfLaneLeft, this.q1, this.q2, this.q3, duration.T12next, duration.T23next, duration.t0next, duration.spreadCaseNext, ref roadSearch);
                //        newRoadinfluences.Add(newRoadInfluence);
                //        //newRoadInfluence.execute();
                //        //roadResults.AddRange(newRoadInfluence.getRoadResults());
                //    }
                //}
                newRoadinfluences = handleResult(nextRoads);

            }
            saveResults();//存储结果
            return newRoadinfluences;
        }

        private List<RoadInfluence> handleResult(List<RoadSection> nextRoads)
        {
            List<RoadInfluence> newRoadinfluences = new List<RoadInfluence>();
            if (nextRoads.Count>0)
            {
                foreach (RoadSection newRoad in nextRoads)
                {
                    newRoad.t0 = duration.t0next;
                    if (roadSearch.isReached[newRoad.EID] && roadSearch.t0[newRoad.EID] > 0 && roadSearch.t0[newRoad.EID] < this.t0)
                    {
                        road.isFinished = true;
                        continue;
                    }
                    else if (roadSearch.isReached[newRoad.EID] && roadSearch.t0[newRoad.EID] > 0 && roadSearch.t0[newRoad.EID] > this.t0)
                    {

                        //System.Diagnostics.Debug.WriteLine("覆盖路段" + toEdge.OID + ".");
                        //System.Diagnostics.Debug.WriteLine("原先的t0是：" + roadSearch.t0[toEdge.EID] + "，当前t0是：" + this.t0);
                        roadSearch.t0[newRoad.EID] = this.t0;
                        roadSearch.coverCount[newRoad.EID]++;
                    }
                    else if (roadSearch.isReached[newRoad.EID] && roadSearch.t0[newRoad.EID] > 0 && (roadSearch.t0[newRoad.EID] - this.t0) < 0.001)
                    {
                        System.Diagnostics.Debug.WriteLine("相等" + newRoad.RoadID + ".");
                        continue;
                    }

                    if (road.isTuring)
                    {
                        RoadInfluence newRoadInfluence = new RoadInfluence(networkDataset, newRoad, numOfLane, numOfLaneLeft, this.q1, this.q2, this.q3, duration.T12next, duration.T23next, duration.t0next, duration.spreadCaseNext, ref roadSearch,this.passTurningCount,this.passStraightCount);
                        newRoadinfluences.Add(newRoadInfluence);
                    }
                    else
                    {
                        RoadInfluence newRoadInfluence = new RoadInfluence(networkDataset, newRoad, numOfLane, numOfLaneLeft, this.q1, this.q2, this.q3, duration.T12next, duration.T23next, duration.t0next, duration.spreadCaseNext, ref roadSearch, this.passTurningCount,this.passStraightCount);
                        newRoadinfluences.Add(newRoadInfluence);
                    }


                    
                    


                }
            }
            return newRoadinfluences;
        }
        

        /// <summary>
        /// 构造交通波
        /// </summary>
        private void makeW()
        {
            K k = new K(this.road.Vl, this.road.kj * this.road.numOfLane);

            
            this.q1 = road.qm_perLane*road.numOfLane*FLOW_RATIO;//实时流量为最大流量的
            //System.Diagnostics.Debug.WriteLine(road.name + "," + this.q1);
            if (_q2>q1)//_q2为上一段路的q2
            //if (q2 > q1 || q3>q2)
            {
                double ratio1 = _q2 / _q1;
                double ratio2 = _q3 / _q2;                
                q2 = q1 * ratio1;
                q3 = q2 * ratio2;
            }
            else
            {
                q2 = _q2;
                q3 = _q3;
            }




            //System.Diagnostics.Debug.WriteLine(this.q1 + ",,," + this.q2 + ",,," + this.q3);
            double k1 = k.getK(this.q1, false);
            
            double k2 = k.getK(q2, true);
            W1 = new W(q1, q2, k1, k2,this.road);


            double k3 = k.getK(q3, true);
            W2 = new W(q1, q3, k1, k3,this.road);

            
            
            if (double.IsNaN(q1)||double.IsNaN(q3) || double.IsNaN(k1)||double.IsNaN(k3))
            {
                throw new Exception("NAN");
            }
            W1b = new W(q1, q3, k1, k3, this.road);


            //if (CONSIDER_TUNING && road.isTuring)//转弯，波速折减
            //{
            //    this.passTurningCount++;
                
                
            //}
            //W1.setW(W1.getW() * Math.Pow(TURNING_RATIO, this.passTurningCount));
            //W2.setW(W2.getW() * Math.Pow(TURNING_RATIO, this.passTurningCount));
            //W1b.setW(W1b.getW() * Math.Pow(TURNING_RATIO, this.passTurningCount));

            if (CONSIDER_TUNING )//转弯，波速折减
            {
                if (road.isTuring)
                {
                    this.passTurningCount++; //
                    
                    //System.Diagnostics.Debug.WriteLine("passTurn"+road.RoadID + "," + this.passTurningCount);
                } 
                else if((road.linkCount>=2 && road.isCrossing))//十字路口，
                {
                    this.passStraightCount++;
                }
                //测试代码
                //if(this.passTurningCount==0) System.Diagnostics.Debug.WriteLine("Straight" + road.RoadID + ","+road.name+"," + this.passStraightCount);
                //波速调整
                if (this.spreadCase == 2)
                {
                    W1b.setW(W1b.getW() * Math.Pow(STRAIGHT_RATIO, this.passStraightCount));
                }
                if (this.spreadCase == 3)
                {
                    W1.setW(W1.getW() * Math.Pow(STRAIGHT_RATIO, this.passStraightCount));
                }
                if (this.spreadCase == 1)
                {
                    W1.setW(W1.getW() * Math.Pow(STRAIGHT_RATIO, this.passStraightCount));
                    W2.setW(W2.getW() * Math.Pow(STRAIGHT_RATIO, this.passStraightCount));
                    W1b.setW(W1b.getW() * Math.Pow(STRAIGHT_RATIO, this.passStraightCount));
                }
                
                road.passTurningCount = this.passTurningCount;
                road.passStraightCount = this.passStraightCount;
                //测试代码
                bool isRoad;
                if (this.road.name == "四平路")
                {
                    isRoad = true;
                    System.Diagnostics.Debug.WriteLine("四平路" + road.RoadID + "," + this.passStraightCount);
                    if (this.road.RoadID == 72926)
                    {
                        System.Diagnostics.Debug.WriteLine("W1到达时间：" + this.t0);
                    }
                }
                

            }
            W1.setW(W1.getW() * Math.Pow(TURNING_RATIO, this.passTurningCount));
            W2.setW(W2.getW() * Math.Pow(TURNING_RATIO, this.passTurningCount));
            W1b.setW(W1b.getW() * Math.Pow(TURNING_RATIO, this.passTurningCount));

            W3 = new W(this.road.Vl);//！
            W2b = new W(this.road.Vl);//！
            //W3 = new W(q3, this.road.qm_perLane * this.road.numOfLane, k3, 0.5 * k3, this.road);
            //W3.v2 = this.road.Vl;
            //W2b = new W(q3, this.road.qm_perLane * this.road.numOfLane, k3, 0.5 * k3, this.road);
            //W2b.v2 = road.Vl;
            //System.Diagnostics.Debug.WriteLine("t0="+this.t0+"...W3="+this.W3.getW()+"...W2:"+this.W2.getW()+"...W1="+this.W1.getW()+"...W1b="+this.W1b.getW());
            checkWaveSpeed();
            if (W1.getW()<0 || W2.getW()<=0 || W3.getW()<=0 || W1b.getW()<=0 || W2b.getW()<=0 ) throw new Exception("WRONG");

            //if (W3.getW() < W1b.getW()) throw new Exception("WRONG");
            //if ( W2.getW() < W1.getW()) throw new Exception("WRONG");
            //if (W2b.getW() < W1.getW()) throw new Exception("WRONG");
            W1.kind = 1; W2.kind = 2; W3.kind = 4; W1b.kind = 3; W2b.kind = 5;
            //System.Diagnostics.Debug.WriteLine(this.road.RoadID + "," + passTurningCount);
        }

        private void checkWaveSpeed()//让集结波波速小于启动波速
        {

            double val=0.5;
            double vl = 16 / 3.6; //启动波速

            if (Math.Abs(W1.getW() -vl)< val)
            {
                W1.setW(vl - val);
            }
            if (Math.Abs(W2.getW() - vl) < val)
            {
                W2.setW(vl - val);
            }
            if (Math.Abs(W1b.getW() - vl) < val)
            {
                W1b.setW(vl - val);
            }

            //if (this.spreadCase == 1 && W2.getW() < W1.getW())
            //{
            //    W1.setW(W2.getW() * 0.99);

            //}
        }

        private void saveResults()
        {
            road.W1 = duration.W1;
            road.W2 = duration.W2;
            road.W3 = duration.W3;
            road.W1b = duration.W1b;
            road.W2b = duration.W2b;
            roadResults.Add(road);
        }

        public List<RoadSection> getRoadResults()
        {
            return roadResults;
        }
    }
}
