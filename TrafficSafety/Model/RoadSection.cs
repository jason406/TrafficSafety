using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.NetworkAnalyst;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;

namespace TrafficSafety.Model
{
    class RoadSection//路段，存储路段基本信息，寻找下一路段
    {
        public int RoadID { get;  set; }
        public int EID { get; private set; }
        public bool isPedestrian;
        public bool isAlongDigitized { get; set; }
        public string name;
        //路口处理
        public bool isTuring = false;//是否是路口的转弯道路
        public bool isCrossing; //是否是路口的接续道路
        private const double MAX_DEGREE = 20;//两条道路的夹角之差绝对值大于该值则认为是转弯
        public int linkCount;//如果是2，表明是丁字路口的衔接道路；3表明是十字路口衔接道路
        public int startNode; //开始节点
        public int endNode;
        public int passTurningCount = 0;
        public int passStraightCount = 0;
        public int spreadCase = 0;
        public int spreadCaseNext = 0;


        private int vf;
        public int Vf
        {
            get { return this.vf; }
            set
            {
                switch (value)
                {
                    case 1:
                        vf = 130;
                        break;
                    case 2:
                        vf = 130;
                        break;
                    case 3:
                        vf = 100;
                        break;
                    case 4:
                        vf = 90;
                        break;
                    case 5:
                        vf = 70;
                        break;
                    case 6:
                        vf = 50;
                        break;
                    case 7:
                        vf = 30;
                        break;
                    case 8:
                        vf = 11;
                        break;                        
                }
            }
        } //自由流速度，单位km/h
        public int Vl;   //限速，单位km/h
        private string _kind ; //道路种类
        private int _kindNum; //道路种类代码数
        public int kind { get; private set; } //道路种类 1快速路，2主干路，3次干路，4支路
        public double signalSplit { get; private set; } //平均绿信比
        public Double Length{get;set;} //长度
        public int numOfLane; //车道数
        private double direction; //方向
        public double qm_perLane;//每车道最大流量\
        public double qm;//总最大流量
        public double kj; //阻塞密度

        public W W1 { get; set; }//各列交通波的存根
        public W W2 { get; set; }
        public W W3 { get; set; }
        public W W1b { get; set; }
        public W W2b { get; set; }

        public double t0; //第一列交通波传入时刻

        public bool isFinished; //交通波是否传播结束
        public double timeOfFinish=0;//传播结束时间
        List<RoadSection> roadList = new List<RoadSection>();

        private List<Zone> _speedResultZone = new List<Zone>();
        public List<Zone> speedResultZone = new List<Zone>();
        public RoadSection(int roadID)
        {
            this.RoadID = roadID;
        }
        public RoadSection(int RoadID,ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset)
        {
            //构造查询变量
            ESRI.ArcGIS.Geodatabase.INetworkQuery networkQuery = ((ESRI.ArcGIS.Geodatabase.INetworkQuery)(networkDataset));
            
            ESRI.ArcGIS.Geodatabase.IEnumNetworkElement enumNetworkElement_OID = networkQuery.get_ElementsByOID(1, RoadID); //networksource 1是边，2是节点
            ESRI.ArcGIS.Geodatabase.INetworkElement networkElement_OID = enumNetworkElement_OID.Next();
            ESRI.ArcGIS.Geodatabase.INetworkEdge m_netWorkEdge = ((ESRI.ArcGIS.Geodatabase.INetworkEdge)(networkElement_OID));
            this.RoadID = RoadID;
            this.EID = m_netWorkEdge.EID;
            if (m_netWorkEdge.Direction == esriNetworkEdgeDirection.esriNEDAlongDigitized)
            {
                this.isAlongDigitized = true;
            } 
            else
            {
                this.isAlongDigitized = false;
            }
            this.Length = getRoadLength(m_netWorkEdge, networkDataset);
            getRoadInformation(m_netWorkEdge, networkDataset);

        }
        public RoadSection(int RoadID, double Length, int startNode, int endNode, ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset)
        {
            //构造查询变量
            ESRI.ArcGIS.Geodatabase.INetworkQuery networkQuery = ((ESRI.ArcGIS.Geodatabase.INetworkQuery)(networkDataset));
            ESRI.ArcGIS.Geodatabase.IEnumNetworkElement enumNetworkElement_OID = networkQuery.get_ElementsByOID(1, RoadID); //networksource 1是边，2是节点
            ESRI.ArcGIS.Geodatabase.INetworkElement networkElement_OID = enumNetworkElement_OID.Next();
            ESRI.ArcGIS.Geodatabase.INetworkEdge m_netWorkEdge = ((ESRI.ArcGIS.Geodatabase.INetworkEdge)(networkElement_OID));

            this.RoadID = RoadID;
            this.Length = Length;
            this.startNode = startNode;
            this.endNode = endNode;

            getRoadInformation(m_netWorkEdge, networkDataset);
        }
        public List<RoadSection> getRoadList()
        {
            RoadSection testRoad = new RoadSection(this.RoadID+1);
            testRoad.Length = 100;
            testRoad.Vl = 50;
            testRoad.kj = 150;
            testRoad.numOfLane = 3;
            testRoad.qm_perLane = 800;
            RoadSection testRoad2 = new RoadSection(this.RoadID * -1 - 1);
            testRoad2.Length = 10;
            roadList.Add(testRoad);            
            //roadList.Add(testRoad2);
            return roadList;
        }



        public List<RoadSection> searchRoad(int inputRoadID, bool isAlongDigitized, int startNodeID, ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset, ref RoadSearch roadSearch)
        {
            if (inputRoadID == 77164)
            {
                //System.Diagnostics.Debug.WriteLine("12121");
            }
            //Debug.WriteLine("当前路段：" + inputRoadID + " 开始节点：" + startNodeID);
            List<RoadSection> results = new List<RoadSection>();//输出结果

            //构造查询变量
            ESRI.ArcGIS.Geodatabase.INetworkQuery networkQuery = ((ESRI.ArcGIS.Geodatabase.INetworkQuery)(networkDataset));
            //构造forwardstar变量，并设置其查询属性
            var fStarEx = networkQuery.CreateForwardStar() as ESRI.ArcGIS.Geodatabase.INetworkForwardStarEx;
            fStarEx.IsForwardTraversal = false;
            fStarEx.BacktrackPolicy = esriNetworkForwardStarBacktrack.esriNFSBNoBacktrack;
            ESRI.ArcGIS.Geodatabase.INetworkAttribute restrictionAttribute = networkDataset.get_AttributeByName("Oneway");
            fStarEx.AddRestrictionAttribute(restrictionAttribute);

            ESRI.ArcGIS.Geodatabase.INetworkForwardStarAdjacencies fStarAdj = networkQuery.CreateForwardStarAdjacencies();

            ESRI.ArcGIS.Geodatabase.IEnumNetworkElement enumNetworkElement_OID = networkQuery.get_ElementsByOID(1, inputRoadID); //networksource 1是边，2是节点
            ESRI.ArcGIS.Geodatabase.INetworkElement networkElement_OID = enumNetworkElement_OID.Next();
            ESRI.ArcGIS.Geodatabase.INetworkEdge m_netWorkEdge = ((ESRI.ArcGIS.Geodatabase.INetworkEdge)(networkElement_OID));
            //各类变量初始化
            double fromPosition, toPosition;
            var junction = networkQuery.CreateNetworkElement(esriNetworkElementType.esriNETJunction) as INetworkJunction;
            var fromJunction = networkQuery.CreateNetworkElement(esriNetworkElementType.esriNETJunction) as INetworkJunction;
            var toJunction = networkQuery.CreateNetworkElement(esriNetworkElementType.esriNETJunction) as INetworkJunction;
            var toEdge = networkQuery.CreateNetworkElement(esriNetworkElementType.esriNETEdge) as INetworkEdge;
            var fromEdge = networkQuery.CreateNetworkElement(esriNetworkElementType.esriNETEdge) as INetworkEdge;

            int edgeEID = m_netWorkEdge.EID;
            roadSearch.isReached[edgeEID] = true;//标记当前路段为查询过的
            bool isFiltered;
            int j = 0;

            //fStarEx.QueryEdge(edgeEID, (isAlongDigitized)? esriNetworkEdgeDirection.esriNEDAlongDigitized:esriNetworkEdgeDirection.esriNEDAgainstDigitized, fromEdge);
            fStarEx.QueryEdge(edgeEID, esriNetworkEdgeDirection.esriNEDAlongDigitized, fromEdge);

            fromEdge.QueryJunctions(fromJunction, toJunction); //看路段的哪个节点是“开始节点”，要从开始节点往前搜寻新的路段 


            if (fromJunction.OID == startNodeID)
            {
                junction = fromJunction;
                this.startNode = fromJunction.OID;
                this.endNode = toJunction.OID;
            }
            else
            {
                fStarEx.QueryEdge(edgeEID, esriNetworkEdgeDirection.esriNEDAgainstDigitized, fromEdge);
                junction = toJunction;
                this.startNode = toJunction.OID;
                this.endNode = fromJunction.OID;
            }
            IPoint mPoint = new PointClass();
            junction.QueryPoint(mPoint);

            try //如果路段方向错误的话，换一个方向
            {
                fStarEx.QueryAdjacencies(junction, fromEdge, null, fStarAdj);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                //fStarEx.QueryEdge(edgeEID, ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection.esriNEDAgainstDigitized, fromEdge);
                //fStarEx.QueryAdjacencies(junction, fromEdge, null, fStarAdj);
            }

            double length = getRoadLength(fromEdge, networkDataset);


            for (j = 0; j < fStarAdj.Count; j++) //输出搜寻结果
            {
                fStarAdj.QueryEdge(j, toEdge, out fromPosition, out toPosition);
                //Debug.WriteLine(toEdge.Direction);
                fStarAdj.QueryToJunction(j, toJunction, out isFiltered);
                if (isFiltered)
                {
                    System.Diagnostics.Debug.WriteLine("交点" + toJunction.OID + "禁止进入！");
                    continue;
                }
                //if (roadSearch.isReached[toEdge.EID] && roadSearch.t0[toEdge.EID] > 0 && roadSearch.t0[toEdge.EID] < this.t0)
                ////if (roadSearch.isReached[toEdge.EID] && roadSearch.t0[toEdge.EID] > 0 )
                //{
                   
                //        //System.Diagnostics.Debug.WriteLine("边" + toEdge.OID + "已经传播过了，跳过");
                //    this.isFinished = true;
                    
                //    continue;
                //}
                //else if (roadSearch.isReached[toEdge.EID] && roadSearch.t0[toEdge.EID] > 0 && roadSearch.t0[toEdge.EID] > this.t0)
                //{

                //    //System.Diagnostics.Debug.WriteLine("覆盖路段" + toEdge.OID + ".");
                //    //System.Diagnostics.Debug.WriteLine("原先的t0是：" + roadSearch.t0[toEdge.EID] + "，当前t0是：" + this.t0);
                //    roadSearch.t0[toEdge.EID] = this.t0;
                //    roadSearch.coverCount[toEdge.EID]++;
                //}
                //else if (roadSearch.isReached[toEdge.EID] && roadSearch.t0[toEdge.EID] > 0 && (roadSearch.t0[toEdge.EID] - this.t0) < 0.001)
                //{
                //    System.Diagnostics.Debug.WriteLine("相等" + toEdge.OID + ".");
                //    continue;
                //}
                double nextLength = getRoadLength(toEdge, networkDataset);

                RoadSection newRoad = new RoadSection(toEdge.OID, nextLength, toJunction.OID, junction.OID,networkDataset);
                
                newRoad.EID = toEdge.EID;
                if (toEdge.Direction == esriNetworkEdgeDirection.esriNEDAlongDigitized)
                {
                    newRoad.isAlongDigitized = true;
                }
                else
                {
                    newRoad.isAlongDigitized = false;
                }
                if (fStarAdj.Count >=2)
                {
                    newRoad.isCrossing = true; //是路口的接续道路
                    newRoad.linkCount = fStarAdj.Count;
                    if (Math.Abs(this.direction - newRoad.direction) > MAX_DEGREE)
                    {
                        newRoad.isTuring = true;
                        
                    }
                }
                
                if (!newRoad.isPedestrian)//不是步行街
                {
                    results.Add(newRoad);
                }
                
                roadSearch.isReached[toEdge.EID] = true;
                string messageString = "Edge OID:" + toEdge.OID + " junction OID:" + toJunction.OID;
                //Debug.WriteLine(messageString);
            }

            return results;
        }
        private double getRoadLength(INetworkEdge road, ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset)
        {
            ESRI.ArcGIS.Geodatabase.INetworkSource netSource = networkDataset.get_SourceByID(road.SourceID);
            var fClassContainer = networkDataset as ESRI.ArcGIS.Geodatabase.IFeatureClassContainer;
            ESRI.ArcGIS.Geodatabase.IFeatureClass sourceFClass = fClassContainer.get_ClassByName(netSource.Name);
            ESRI.ArcGIS.Geodatabase.IFeature roadFeature = sourceFClass.GetFeature(road.OID);
            string aaa = roadFeature.get_Value(16).ToString();
            double length = Convert.ToDouble(aaa);
            if (length <= 0)
            {
                throw new Exception("路段长度错误！");
            }
            return 1000 * length; //米
        }

        private void getRoadInformation(INetworkEdge road, ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset)
        {
            ESRI.ArcGIS.Geodatabase.INetworkSource netSource = networkDataset.get_SourceByID(road.SourceID);
            var fClassContainer = networkDataset as ESRI.ArcGIS.Geodatabase.IFeatureClassContainer;
            ESRI.ArcGIS.Geodatabase.IFeatureClass sourceFClass = fClassContainer.get_ClassByName(netSource.Name);
            ESRI.ArcGIS.Geodatabase.IFeature roadFeature = sourceFClass.GetFeature(road.OID);
            
            string _length = roadFeature.get_Value(16).ToString();
            string _speedClass = roadFeature.get_Value(28).ToString();
            string _kind = roadFeature.get_Value(7).ToString();
            string _kindNum = roadFeature.get_Value(6).ToString();
            string _laneNumS2E = roadFeature.get_Value(29).ToString();
            string _laneNumE2S = roadFeature.get_Value(30).ToString();
            string _speedLimitS2E = roadFeature.get_Value(37).ToString();//单位0.1km/h
            string _speedLimitE2S = roadFeature.get_Value(38).ToString();
            string _direction = roadFeature.get_Value(46).ToString();//方向
            this.name = roadFeature.get_Value(2).ToString();
            this.Length = Convert.ToDouble(_length)*1000;

            if (_laneNumE2S == " ") _laneNumE2S = "0";
            if (_laneNumS2E == " ") _laneNumS2E = "0";
            if (_speedLimitS2E == " ") _speedLimitS2E = "0";
            if (_speedLimitE2S == " ") _speedLimitE2S = "0";

            this.direction = Convert.ToDouble(_direction);
            this.Vl = Math.Max(Convert.ToInt16(_speedLimitE2S), Convert.ToInt16(_speedLimitS2E)) / 10; //限速km/h
            this.numOfLane = Math.Max(Convert.ToInt16(_laneNumE2S), Convert.ToInt16(_laneNumS2E)); //车道数
            //this.Vf = Convert.ToInt16(_speedClass); //自由流速度（不采用）

            this.kj = 150; //150veh/km



            
            this._kindNum = Convert.ToInt16(_kindNum);
            string kindCode = _kind.Substring(0, 2);
            switch (kindCode)
            {
                case "00":
                    this.kind = 1; //快速路                    
                    break;
                case "01":
                    this.kind = 1;
                    break;
                case "02":
                    this.kind = 2; 
                    break;
                case "03":
                    this.kind = 2;
                    break;
                case "04":
                    this.kind = 3;
                    break;
                case "06":
                    this.kind = 3;
                    break;
                case "08":
                    this.kind = 4;
                    break;
                case "09":
                    this.kind = 4;
                    break;
                default:
                    this.isPedestrian = true;
                    break;
            }
            switch (this.kind) //不用这个了
            {
                case 1:
                    this.qm_perLane = 1350;
                    this.signalSplit = 1;
                    break;
                case 2:
                    this.qm_perLane = 900  ;
                    this.signalSplit = 0.45;
                    break;
                case 3:
                    this.qm_perLane = 650  ;
                    this.signalSplit = 0.4;
                    break;
                case 4:
                    this.qm_perLane = 350  ;
                    this.signalSplit = 0.31;
                    break;                   

            }
            //新的qm_perlane，和newell模型一致
            double w3=16;
            double kj=150*this.numOfLane;
            this.qm = this.Vl * w3 * kj / (Vl + w3);
            this.qm_perLane = this.qm / numOfLane;
            //if (this.qm_perLane == 0) throw new Exception("0!");

        }

        public void getSpeedResult(double t)
        {
            this._speedResultZone.Clear();
            this.speedResultZone.Clear();
            //processSpeedResult(W1, t);
            //processSpeedResult(W2, t);
            //processSpeedResult(W3, t);
            //processSpeedResult(W1b, t);
            //processSpeedResult(W2b, t);

            processSpeedResult(t);
            //进行交通波传播结果的覆盖
            coverResult();
            speedResultZone = _speedResultZone;
        }

        private void coverResult()//交通波传播结果的覆盖
        {
            for (int i = 0; i < _speedResultZone.Count; i++)
            {
                for (int j = _speedResultZone.Count - 1; j > i; j--)
                {
                    int resultKind = resultCoverRelationship(_speedResultZone[i], _speedResultZone[j]);

                    switch (resultKind)
                    {
                        case 1://相等
                            if (_speedResultZone[i].waveKind > _speedResultZone[j].waveKind)
                            {
                                //_speedResultZone.Remove(_speedResultZone[j]);
                                _speedResultZone.RemoveAt(j);
                            }
                            else
                            {
                                _speedResultZone[i] = _speedResultZone[j];
                                _speedResultZone.RemoveAt(j);//相当于removeAt[i]
                            }
                            break;
                        case 2://1包含2
                            if (_speedResultZone[i].congestionLevel==_speedResultZone[j].congestionLevel)
                            {
                                _speedResultZone.RemoveAt(j);//拥堵等级相等的话就移除2
                            } 
                            else
                            {
                                if (_speedResultZone[i].waveKind > _speedResultZone[j].waveKind)
                                {
                                    //_speedResultZone.Remove(_speedResultZone[j]);
                                    _speedResultZone.RemoveAt(j);
                                }
                                else
                                {
                                    _speedResultZone[i] = _speedResultZone[j];
                                    _speedResultZone.RemoveAt(j);//相当于removeAt[i]
                                }
                            }
                            break;
                        case 3://2包含1
                            if (_speedResultZone[i].congestionLevel == _speedResultZone[j].congestionLevel)
                            {
                                _speedResultZone.RemoveAt(i);//拥堵等级相等的话就移除1
                            }
                            else
                            {
                                if (_speedResultZone[i].waveKind > _speedResultZone[j].waveKind)
                                {
                                    //_speedResultZone.Remove(_speedResultZone[j]);
                                    _speedResultZone.RemoveAt(j);
                                }
                                else
                                {
                                    _speedResultZone[i] = _speedResultZone[j];
                                    _speedResultZone.RemoveAt(j);//相当于removeAt[i]
                                }
                            }
                            break;
                        default:
                            //两个结果没有包含关系
                            break;
                    }
                }
            }
        }

        private void processSpeedResult(W W, double t)
        {
            if (W!=null)
            {
                if (t > W.timeOfArrive && t < W.timeOfDeparture)
                {
                    _speedResultZone.Add(new Zone(this,W.v2, W.locationOfArrive, W.getW() * (t - W.timeOfArrive),W.kind,W.timeOfArrive,t));
                }
                else if (t >= W.timeOfDeparture)
                {
                    _speedResultZone.Add(new Zone(this,W.v2, W.locationOfArrive, W.locationOfDeparture,W.kind, W.timeOfArrive,t));
                }
            }
            else
            {
                //throw new Exception("W=null");
            }

            //for (int i = 0; i < _speedResultZone.Count; i++)
            //{
            //    if (!hasResult2(i))
            //    {
            //        speedResultZone.Add(_speedResultZone[i]);
            //    }
            //}

            //if (_speedResultZone.Count > 0)
            //{
            //    //speedResultZone.Add(_speedResultZone[0]);
            //    foreach (Zone speedResult in _speedResultZone)
            //    {
            //        if (!hasResult(speedResult))
            //        {
            //            speedResultZone.Add(speedResult);
            //        }
            //    }
            //}

            

        }

        private void processSpeedResult(double t)
    {
        switch (this.spreadCase)
        {
            case 1:
                if (this.spreadCaseNext == 1)
                {
                    processSpeedResult(W1, t);
                    processSpeedResult(W2, t);
                    processSpeedResult(W3, t);
                }
                if (this.spreadCaseNext == 2)
                {
                    processSpeedResult(W1, t);
                    processSpeedResult(W2, t);
                    processSpeedResult(W3, t);
                    processSpeedResult(W1b, t);
                }
                if (this.spreadCaseNext == 3)
                {
                    processSpeedResult(W1, t);
                    processSpeedResult(W2, t);
                    processSpeedResult(W3, t);
                    processSpeedResult(W2b, t);
                }
                if (this.spreadCaseNext == 4)
                {
                    processSpeedResult(W1, t);
                    processSpeedResult(W2, t);
                    processSpeedResult(W3, t);
                    processSpeedResult(W1b, t);
                    processSpeedResult(W2b, t);
                }
                break;
            case 2:
                if (this.spreadCaseNext == 2)
                {                    
                    processSpeedResult(W3, t);
                    processSpeedResult(W1b, t);
                }
                else if (this.spreadCaseNext == 4)
                {                    
                    processSpeedResult(W3, t);
                    processSpeedResult(W1b, t);                    
                }
                else
                {
                    throw new Exception("下个路段传播状况错误");
                }
                break;
            case 3:
                if (this.spreadCaseNext == 3)
                {                    
                    processSpeedResult(W1, t);
                    processSpeedResult(W2b, t);
                }
                else if (this.spreadCaseNext == 4)
                {                    
                    processSpeedResult(W1, t);
                    processSpeedResult(W2b, t);                    
                }
                else
                {
                    throw new Exception("下个路段传播状况错误");
                }
                break;
            default:
                throw new Exception("case错误");

        }
    }

        private bool hasResult(Zone speedResult)
        {
            foreach (Zone speedResult0 in this.speedResultZone)
            {
                if (speedResult0.start == speedResult.start && speedResult0.end == speedResult.end && speedResult0.waveKind > speedResult.waveKind)
                {
                    return true;
                }
            }
            return false;
        }

        private bool isResultEqual(Zone Result1, Zone Result2)
        {
            if (
                ((Result1.start - Result2.start<1E-4) && (Result1.end - Result2.end<1E-4)) || 
                ((Result2.start <= Result1.start) && (Result2.end >=Result1.end)) ||
                ((Result1.start <= Result2.start) && (Result1.end >= Result2.end))
                )
            {
                return true;
            } 
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 判断两个结果的包含关系
        /// </summary>
        /// <param name="result1"></param>
        /// <param name="result2"></param>
        /// <returns>1为相等，2为结果1包含结果2,3为结果2包含结果1,0为没有包含关系</returns>
        private int resultCoverRelationship(Zone Result1, Zone Result2)
        {
            //int resultKind = 0;
            if ((Result1.start - Result2.start < 1E-4) && (Result1.end - Result2.end < 1E-4))
            {
                return 1;
            }
            else if ((Result2.start <= Result1.start) && (Result2.end >= Result1.end))
            {
                return 3;
            }
            else if ((Result1.start <= Result2.start) && (Result1.end >= Result2.end))
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        private bool hasResult2(int i)
        {
            for (int j = i + 1; j < _speedResultZone.Count; j++)
            {

                if (_speedResultZone[i].start == _speedResultZone[j].start && _speedResultZone[i].end == _speedResultZone[j].end)
                {
                    if (_speedResultZone[i].waveKind > _speedResultZone[j].waveKind)
                    {
                        speedResultZone.Add(_speedResultZone[i]);
                        
                    }
                    else
                    {
                        speedResultZone.Add(_speedResultZone[j]);
                    }
                    return true;
                }

            }
            return false;
        }
        
    }
}
