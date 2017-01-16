using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrafficSafety.Model
{
    class MainRoadInfluence 
    {
        private double T12, T23, t0;
        public double q1; //事发流量
        private double q2; //民警处理后流量
        private double q3;
        private AccidentInfo accidentInfo;

        private W W1 { get; set; }//各列交通波的存根
        private W W2 { get; set; }
        private W W3 { get; set; }
        private W W1b { get; set; }
        private W W2b { get; set; }
        private int numOfLaneLeft { get; set; }// 剩余车道数
        private int numOfLane { get; set; }// 总车道数
        private List<RoadSection> nextRoads;
        public List<RoadSection> roadResults = new List<RoadSection>();
        private ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset;
        bool[] isReached; //查询标识，如果某条道路查询过了就为true
        RoadSearch roadSearch;
        private Duration duration;
        public RoadSection road;//当前路段
        private double _accidentPoint;
        public double accidentPoint//事故点到道路起始点的距离米（线性参考）
        {
            get { return _accidentPoint; }
            set 
            { 
                if (value > road.Length && road.Length>0)
                {
                    _accidentPoint = road.Length;
                } 
                else
                {
                    _accidentPoint = value;
                }
            }
        }

        

        public MainRoadInfluence(ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset, RoadSection road,int nodeOID, double accidentPoint, int numOfLane, int numOfLaneLeft, double q1, double T12, double T23, double t0)
        //   numOfLane没用，会在道路信息里提供
        {
            this.networkDataset = networkDataset;
            var networkQuery = networkDataset as ESRI.ArcGIS.Geodatabase.INetworkQuery; //创建networkQuery，旨在初始化查询标记变量
            isReached = new bool[networkQuery.get_MaxEID(ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge) + 1];
            roadSearch = new RoadSearch();
            roadSearch.isReached = new bool[networkQuery.get_MaxEID(ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge) + 1];
            roadSearch.t0 = new double[networkQuery.get_MaxEID(ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge) + 1];
            roadSearch.coverCount = new int[networkQuery.get_MaxEID(ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge) + 1];
            roadSearch.passTurningCount = 0;
            this.road = road;
            this.road.startNode = nodeOID;
            this.accidentPoint = accidentPoint;
            this.numOfLane = numOfLane;
            this.numOfLaneLeft = numOfLaneLeft;
            this.q1 = q1;
            
            //this.q2 = q2;
            this.T12 = T12;
            this.T23 = T23;
            this.t0 = t0;
            roadSearch.t0[road.EID] = t0;

            breadthFirstExecute();
            //RoadResult mresult = new RoadResult();
            //mresult.checkResult(roadResults);
            //mresult.processResult(roadResults, 520);
            //mresult.saveResulttoFile(roadResults,@"D:\事故模拟\result.txt");
            int maxCoverCount=roadSearch.coverCount.Max();
            double max_t0 = roadSearch.t0.Max();
            System.Diagnostics.Debug.WriteLine(maxCoverCount);
            System.Diagnostics.Debug.WriteLine(max_t0);
        }
        public MainRoadInfluence(ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset, RoadSection road, int nodeOID, double accidentPoint, AccidentInfo accidentInfo)
        {
            this.networkDataset = networkDataset;
            var networkQuery = networkDataset as ESRI.ArcGIS.Geodatabase.INetworkQuery; //创建networkQuery，旨在初始化查询标记变量
            isReached = new bool[networkQuery.get_MaxEID(ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge) + 1];
            roadSearch = new RoadSearch();
            roadSearch.isReached = new bool[networkQuery.get_MaxEID(ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge) + 1];
            roadSearch.t0 = new double[networkQuery.get_MaxEID(ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge) + 1];
            this.road = road;
            this.road.startNode = nodeOID;
            this.accidentPoint = accidentPoint;
            
            this.accidentInfo = accidentInfo;
            this.q1 = accidentInfo.q1;
            //this.q2 = accidentInfo.q2;
            this.T12 = accidentInfo.T12;
            this.numOfLane = accidentInfo.numOfLane;
            this.numOfLaneLeft = accidentInfo.numOfLaneLeft;
            

            execute();
            //RoadResult mresult = new RoadResult();
            //mresult.checkResult(roadResults);
            //mresult.processResult(roadResults, 520);
            //mresult.saveResulttoFile(roadResults,@"D:\事故模拟\result.txt");
        }

        public  void execute()
        {
            //W1 = new W(numOfLane, numOfLaneLeft, this.q1);
            //W2 = new W(numOfLane, W1.getQ2(), this.q2); //民警处理后第二道集结波
            //W1b = new W(W1.getQ1(), W2.getQ2(), W1.getK1(), W2.getK2());
            //W3 = new W(numOfLane, W2.getQ2(), 2000D * numOfLane);
            //W2b = new W(W2.getQ1(), W3.getQ2(), W2.getK1(), W3.getK2());
            makeW();

            duration = new Duration(accidentPoint, W1, W2, W3, W1b, W2b, T12, T23, t0);

            System.Diagnostics.Debug.WriteLine("开始传播……");
            
            //if (duration.spreadCaseNext == 4)//传播结束
            //{

            //    System.Diagnostics.Debug.WriteLine("传播结束，t=" + duration.t + " lm=" + duration.lm + " t0=" + duration.t0);
            //    road.isFinished = true;
            //    road.timeOfFinish = duration.t0 + duration.t;
            //}
            //else
            //{                
                
            //    nextRoads = road.searchRoad(road.RoadID, road.isAlongDigitized, road.startNode, networkDataset, ref roadSearch);
            //    System.Diagnostics.Debug.WriteLine("下一段路" + "T12:" + duration.T12next + " T23:" + duration.T23next + " t0:" + duration.t0next + " 情况" + duration.spreadCaseNext);
            //    foreach (RoadSection newRoad in nextRoads)
            //    {
                    
                    
            //        RoadInfluence newRoadInfluence = new RoadInfluence(networkDataset, newRoad, numOfLane, numOfLaneLeft, this.q1, this.q2, duration.T12next, duration.T23next, duration.t0next, duration.spreadCaseNext, ref roadSearch);
            //        newRoadInfluence.execute();
            //        roadResults.AddRange(newRoadInfluence.getRoadResults());
            //    }
            //}
            if (duration.spreadCaseNext == 4)//传播结束
            {

                System.Diagnostics.Debug.WriteLine("路段" + road.RoadID + "传播结束，t=" + duration.t + " lm=" + duration.lm + " t0=" + duration.t0);
                road.isFinished = true;
                road.timeOfFinish = duration.t0 + duration.t;
            }
            else
            {

                //nextRoads = road.getRoadList();
                
                nextRoads = road.searchRoad(road.RoadID, road.isAlongDigitized, road.startNode, networkDataset, ref roadSearch);
                if (road.RoadID==109933)
                {
                    System.Diagnostics.Debug.WriteLine("当前路段" + road.RoadID + " nextroadsCount:" + nextRoads.Count);
                }
                
                if (nextRoads.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Dead end！");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("下一段路" + "T12:" + duration.T12next + " T23:" + duration.T23next + " t0:" + duration.t0next + " 情况" + duration.spreadCaseNext);
                    foreach (RoadSection newRoad in nextRoads)
                    {
                        //roadSearch.t0[newRoad.RoadEID] = duration.t0next;
                        //newRoad.t0 = t0;
                        newRoad.t0 = duration.t0next;
                        RoadInfluence newRoadInfluence = new RoadInfluence(networkDataset, newRoad, numOfLane, numOfLaneLeft, this.q1, this.q2,this.q3, duration.T12next, duration.T23next, duration.t0next, duration.spreadCaseNext, ref roadSearch,0,0);
                        newRoadInfluence.execute();
                        roadResults.AddRange(newRoadInfluence.getRoadResults());
                    }
                }

            }

            saveResults();//存储结果
            roadResults.Add(this.road);
        }
        public bool breadthFirstExecute()
        {
            var searchQueue = new System.Collections.Queue();
            //SearchStruct currentStruct = new SearchStruct(this.road,this.numOfLaneLeft,)
            makeW();
            
            duration = new Duration(accidentPoint, W1, W2, W3, W1b, W2b, T12, T23, t0);
            this.road.spreadCase = 1;
            this.road.spreadCaseNext = duration.spreadCaseNext;
            if (duration.spreadCaseNext == 4)//传播结束
            {

                System.Diagnostics.Debug.WriteLine("路段" + road.RoadID + "传播结束，t=" + duration.t + " lm=" + duration.lm + " t0=" + duration.t0);
                road.isFinished = true;
                road.timeOfFinish = duration.t0 + duration.t;
            }
            else
            {

                //nextRoads = road.getRoadList();

                nextRoads = road.searchRoad(road.RoadID, road.isAlongDigitized, road.startNode, networkDataset, ref roadSearch);
                foreach (RoadSection newRoad in nextRoads)
                {
                    //roadSearch.t0[newRoad.RoadEID] = duration.t0next;
                    //newRoad.t0 = t0;
                    newRoad.t0 = duration.t0next;
                    RoadInfluence newRoadInfluence = new RoadInfluence(networkDataset, newRoad, numOfLane, numOfLaneLeft, this.q1, this.q2, this.q3, duration.T12next, duration.T23next, duration.t0next, duration.spreadCaseNext, ref roadSearch,0,0);
                    //newRoadInfluence.execute();
                    //roadResults.AddRange(newRoadInfluence.getRoadResults());
                    searchQueue.Enqueue(newRoadInfluence);
                }
                saveResults();//存储结果
                roadResults.Add(this.road);

                while (searchQueue.Count > 0)
                {
                    //System.Diagnostics.Debug.WriteLine("队列长度：" + searchQueue.Count);
                    var newRoadInfluence = (RoadInfluence)searchQueue.Dequeue();
                    foreach (RoadInfluence _newRoadInfluence in newRoadInfluence.breadthFirstExecute())
                    {
                        
                        searchQueue.Enqueue(_newRoadInfluence);
                    }
                    
                    roadResults.AddRange(newRoadInfluence.getRoadResults());
                    //System.Diagnostics.Debug.WriteLine("ResultCount：" + roadResults.Count);

                }

            }

            


            return true;
        }


        private void makeW()
        {
            K k = new K(this.road.Vl, this.road.kj * this.road.numOfLane);
            this.q1 = road.qm_perLane * road.numOfLane * GlobalConst.FLOW_RATIO;
            this.q2 = this.numOfLaneLeft * this.road.qm_perLane;

            if (this.q2 > this.q1) q2 = q1 * 0.8;
            double k1 = k.getK(this.q1, false);
            double k2 = k.getK(q2, true);
            W1 = new W(q1, q2, k1, k2,this.road);
            
            this.q3 = (this.numOfLaneLeft - 1) * this.road.qm_perLane;
            if (this.q3 > this.q2) q3 = q2 * 0.8;
            double k3 = k.getK(q3, true);
            W2 = new W(q1, q3, k1, k3, this.road);

            
            W1b = new W(q1, q3, k1, k3, this.road);
            W3 = new W(this.road.Vl);//！
            W2b = new W(this.road.Vl);//！
            W1.kind = 1; W2.kind = 2; W3.kind = 4; W1b.kind = 3; W2b.kind = 5;
            
            System.Diagnostics.Debug.WriteLine("主路"+road.RoadID + "," + this.q1);
        }
        private void saveResults()
        {
            road.W1 = duration.W1;
            road.W2 = duration.W2;
            road.W3 = duration.W3;
            road.W1b = duration.W1b;
            road.W2b = duration.W2b;
        }
    }
}
