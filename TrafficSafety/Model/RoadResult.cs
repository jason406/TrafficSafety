using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;

namespace TrafficSafety.Model
{
    class RoadResult
    {
        struct zone//影响区域
        {
            public int roadID;
            public double start;//开始位置
            public double end;
            public double speed;
        }

        public double durationTime { get; private set; }
        public double currentTime {  get; private set; } //查看时间
        private List<zone> redZone = new List<zone>();
        private List<zone> yellowZone = new List<zone>();
        private List<zone> greenZone= new List<zone>();
        private MainRoadInfluence mainRoadInfluence;
        private List<RoadSection> specificResult = new List<RoadSection>();
        struct congestedLength
        {
            public int t;
            public int length;
        }
        public RoadResult(MainRoadInfluence mainRoadInfluence)
        {
            this.durationTime = 0;
            this.mainRoadInfluence=mainRoadInfluence;
        }

        public void processResult(double t)
        {
            List<RoadSection> result;
            result = this.mainRoadInfluence.roadResults;
            this.currentTime = t; 
            foreach (RoadSection roadSection in result)
            {
                //if (roadSection.RoadID == 73989)
                //{
                //    System.Diagnostics.Debug.WriteLine("rrr");
                //}
                if (roadSection.isFinished)
                {
                    if (roadSection.timeOfFinish > this.durationTime)
                    {
                        this.durationTime = roadSection.timeOfFinish;
                    }
                }
                if (roadSection.W1 != null && t > roadSection.W1.timeOfArrive)
                {
                    yellowZone.Add(saveInfluence(roadSection.RoadID, roadSection.W1, t));
                }
                if (roadSection.W2 != null && t > roadSection.W2.timeOfArrive)
                {
                    redZone.Add(saveInfluence(roadSection.RoadID, roadSection.W2, t));
                }
                if (roadSection.W3 != null && t > roadSection.W3.timeOfArrive)
                {
                    greenZone.Add(saveInfluence(roadSection.RoadID, roadSection.W3, t));
                }
                if (roadSection.W1b != null && t > roadSection.W1b.timeOfArrive)
                {
                    redZone.Add(saveInfluence(roadSection.RoadID, roadSection.W1b, t));
                }
                if (roadSection.W2b != null && t > roadSection.W2b.timeOfArrive)
                {
                    greenZone.Add(saveInfluence(roadSection.RoadID, roadSection.W2b, t));
                }
            }
        }
        private void removeRepeatedResult()
        {
            List<RoadSection> result;
            result = this.mainRoadInfluence.roadResults;
            for (int i = 0; i < result.Count; i++)
            {
                for (int j = result.Count - 1; j > i ;j-- )
                {

                    if (result[i].RoadID == result[j].RoadID)
                    {
                        if (result[i].RoadID == 109933)
                        {
                            int bbb;
                        }
                        if (result[i].t0 < result[j].t0)//剔除t0较大的
                        {
                            result.RemoveAt(j);
                        }
                        else
                        {
                            result[i] = result[j];
                            result.RemoveAt(j);
                        }
                    }

                    
                }
            }
            this.mainRoadInfluence.roadResults = result;

            //for (int i= 0 ; i <mainRoadInfluence.roadResults.Count; i++)
            //{
            //    System.Diagnostics.Debug.WriteLine(mainRoadInfluence.roadResults[i].RoadID);
            //}
        }
        private List<RoadSection> processSpeedResult(double t)
        {
            //removeRepeatedResult();
            List<RoadSection> result = new List<RoadSection>();
            //result = this.mainRoadInfluence.roadResults;
            this.mainRoadInfluence.roadResults.ForEach(i => result.Add(i));
            this.currentTime = t;
            double _max_t0=0;
            //System.IO.StreamWriter sw = new System.IO.StreamWriter("D:\roadResult.txt");
            foreach (RoadSection roadSection in result)
            {
                //sw.WriteLine("ROADID, ")
                if (roadSection.isFinished)
                //if (true)
                {
                    if (roadSection.timeOfFinish > this.durationTime)
                    {
                        this.durationTime = roadSection.timeOfFinish;
                    }
                    if (roadSection.t0 > _max_t0)
                    {
                        _max_t0 = roadSection.t0;
                    }
                }
                roadSection.getSpeedResult(t);

            }
            System.Diagnostics.Debug.WriteLine("查看时间：" + t + "结果数量：" + result.Count());
            System.Diagnostics.Debug.WriteLine("事故持续时间：" + this.durationTime);
            System.Diagnostics.Debug.WriteLine("MAX_t0：" + _max_t0);
            System.Diagnostics.Debug.WriteLine("check之前：" + result.Count());
            checkResult(result);
            System.Diagnostics.Debug.WriteLine("check之后：" + result.Count());
            return result;
        }

        public void checkResult(List<RoadSection> result)
        {
            List<int> noResultIdx= new List<int>(); //没有结果的Index
            for (int i = 0; i < result.Count;i++ )
            {
                RoadSection roadSection = result[i];


                if (roadSection.spreadCase == 0 || roadSection.spreadCaseNext == 0)
                {
                    System.Diagnostics.Debug.WriteLine("路段" + roadSection.RoadID + "传播情况有误");
                }
                if (roadSection.W1 != null && roadSection.W1.timeOfDeparture == 0)
                {
                    System.Diagnostics.Debug.WriteLine("W1 time有误");
                }
                if (roadSection.W2 != null && roadSection.W2.timeOfDeparture == 0)
                {
                    System.Diagnostics.Debug.WriteLine("W2 time有误");
                }
                if (roadSection.W3 != null && roadSection.W3.timeOfDeparture == 0)
                {
                    System.Diagnostics.Debug.WriteLine("W3 time有误");
                }
                if (roadSection.W1b != null && roadSection.W1b.timeOfDeparture == 0)
                {
                    System.Diagnostics.Debug.WriteLine("W1b time有误");
                }
                if (roadSection.W2b != null && roadSection.W2b.timeOfDeparture == 0)
                {
                    System.Diagnostics.Debug.WriteLine("W2b time有误");
                }
                string messageString = "";
                int count = 0;
                if (roadSection.W1 != null)
                {
                    messageString += "W1:" + roadSection.W1.v2;
                    count++;
                }
                if (roadSection.W2 != null)
                {
                    messageString += ",W2:" + roadSection.W2.v2;
                    count++;
                }
                if (roadSection.W3 != null)
                {
                    messageString += ",W3:" + roadSection.W3.v2;
                    count++;
                }
                if (roadSection.W1b != null)
                {
                    messageString += ",W1b:" + roadSection.W1b.v2;
                    count++;
                }
                if (roadSection.W2b != null)
                {
                    messageString += ",W2b:" + roadSection.W2b.v2;
                    count++;
                }
                //System.Diagnostics.Debug.WriteLine(messageString);
                if (count < 2) throw new Exception("COunt!");
            }

            for (int i = result.Count-1; i > 0;i-- )
            {
                RoadSection roadSection = result[i];
                if (roadSection.speedResultZone.Count == 0)
                {
                    result.RemoveAt(i);
                }
            }


        }

        public void saveSpeedResulttoFile(double t,string filePath)
        {
            List<RoadSection> result = processSpeedResult(t);
            //System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath);
            //string test = (System.IO.Path.GetDirectoryName(filePath) + System.IO.Path.GetFileNameWithoutExtension(filePath) +this.mainRoadInfluence.road.name+ this.currentTime + "s_speed.txt");
            System.IO.StreamWriter sw = new System.IO.StreamWriter(System.IO.Path.GetDirectoryName(filePath) + System.IO.Path.GetFileNameWithoutExtension(filePath) +this.mainRoadInfluence.road.name+ this.currentTime + "s_speed.txt");
            sw.WriteLine("ROADID,ROADNAME,CONGLEVEL,START,END,WAVEKIND,t0,passStraight,passTurning");
            string messageString;
            foreach (RoadSection roadSection in result)
            {
                foreach (Zone resultZones in roadSection.speedResultZone)
                {
                   
                    messageString = roadSection.RoadID.ToString() + "," + roadSection.name.ToString() + "," + resultZones.congestionLevel + "," + resultZones.start + "," + resultZones.end + "," + resultZones.waveKind + "," + resultZones.t0 + "," + roadSection.passStraightCount + "," + roadSection.passTurningCount;
                    //messageString = roadSection.RoadID.ToString() + "," + resultZones.congestionLevel + "," + resultZones.start + "," + resultZones.end + "," + resultZones.waveKind + "," + resultZones.t0;
                    sw.WriteLine(messageString);
                }
                //foreach (Zone resultZones in roadSection.speedResultZone)
                //{
                //    messageString = roadSection.RoadID.ToString() + "," + resultZones.congestionLevel + "," + resultZones.start + "," + resultZones.end + "," + resultZones.speed;
                //    sw.WriteLine(messageString);
                //}
                //测试代码！！！！！！！
                //if (roadSection.speedResultZone.Count>0)
                //{
                //    Zone resultZones = roadSection.speedResultZone[0];
                //    messageString = roadSection.RoadID.ToString() + "," + resultZones.congestionLevel + "," + resultZones.start + "," + resultZones.end + "," + resultZones.speed;
                //    sw.WriteLine(messageString);  
                //}
                 //测试代码结束             
            }
            sw.Close();
            
        }

        public double getMaxInfluenceTime()
        {

            List<RoadSection> results = processSpeedResult(mainRoadInfluence.T12);
            
            List<congestedLength> congestedLengthTime = new List<congestedLength>();
            congestedLength m_ongestedLengthTime = new congestedLength();
            int t = (int)(mainRoadInfluence.T12);
            int ans = t;
            double maxLength = 0;
            while (t < this.durationTime)
            {
                results = processSpeedResult(t);
                m_ongestedLengthTime.t = t;
                m_ongestedLengthTime.length = 0;
                foreach (var roadResult in results)
                {
                    foreach (var resultZone in roadResult.speedResultZone)
                    {
                        if (resultZone.congestionLevel > 3)
                        {                            
                            m_ongestedLengthTime.length +=(int) (resultZone.end - resultZone.start);
                        }
                    }
                }
                congestedLengthTime.Add(m_ongestedLengthTime);
                if (m_ongestedLengthTime.length > maxLength)
                {
                    maxLength = m_ongestedLengthTime.length;
                    ans = m_ongestedLengthTime.t;
                }
                t = t + 10;
            }

            
            return ans;
        }
        public void saveResultToDatabase(double t)
        {
            List<RoadSection> result = processSpeedResult(t);

            //创建表格
            string connstr = "Provider=Microsoft.Jet.OLEDB.4.0 ;Data Source=D:\\事故模拟\\陆家浜路\\lujiabang.mdb";
            string tableName = this.mainRoadInfluence.road.name + this.currentTime;
            OleDbConnection myConn = new OleDbConnection(connstr);
            myConn.Open();
            string SQL_createTable = "CREATE TABLE " + tableName + " (";
            SQL_createTable += "ROADID  INT,";
            SQL_createTable += "CONGLEVEL   INT,";
            SQL_createTable += "STARTPOS   decimal(10,2),";
            SQL_createTable += "ENDPOS     decimal(10,2)";
            SQL_createTable += " )";
            
            OleDbCommand myCommand = new OleDbCommand(SQL_createTable,myConn);
            try
            {
                myCommand.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            //清除原有数据
            string SQL_truncate = "DELETE * FROM " + tableName;
            myCommand.CommandText = SQL_truncate;
            try
            {
                myCommand.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }




            
            OleDbCommand insertCommand = new OleDbCommand();
            insertCommand.Connection = myConn;
            insertCommand.Transaction = myConn.BeginTransaction();
            List<string> cmdStrings = new List<string>();
            foreach (RoadSection roadSection in result)
            {
                foreach (Zone resultZones in roadSection.speedResultZone)
                {
                    string SQL_insert = "INSERT INTO " + tableName + " VALUES ";
                    //messageString = roadSection.RoadID.ToString() + "," + roadSection.name.ToString() + "," + resultZones.congestionLevel + "," + resultZones.start + "," + resultZones.end + "," + resultZones.waveKind + "," + resultZones.t0 + "," + roadSection.passStraightCount + "," + roadSection.passTurningCount;
                    //messageString = roadSection.RoadID.ToString() + "," + resultZones.congestionLevel + "," + resultZones.start + "," + resultZones.end + "," + resultZones.waveKind + "," + resultZones.t0;
                    SQL_insert += " ( ";
                    SQL_insert += roadSection.RoadID + ",";
                    SQL_insert += resultZones.congestionLevel + ",";
                    SQL_insert += resultZones.start + ",";
                    SQL_insert += resultZones.end;
                    SQL_insert += " )";
                    cmdStrings.Add(SQL_insert);
                }
        
            }
            try
            {
                foreach (var insertString in cmdStrings)
                {
                    insertCommand.CommandText = insertString;
                    insertCommand.ExecuteNonQuery();
                }
                insertCommand.Transaction.Commit();
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                insertCommand.Transaction.Rollback();
            }            

            myConn.Close();
            
        }

        public void saveResulttoFile(string filePath)
        {
            List<RoadSection> result = this.mainRoadInfluence.roadResults;
            //System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath);
            //foreach (RoadSection roadSection in result)
            //{
            //    string messageString =  roadSection.RoadID.ToString()  ;
            //    sw.WriteLine(messageString);
            //}
            //sw.Close();
            System.IO.StreamWriter sw_Red = new System.IO.StreamWriter(System.IO.Path.GetDirectoryName(filePath) + System.IO.Path.GetFileNameWithoutExtension(filePath) + this.currentTime+"s_RedZone.txt");
            sw_Red.WriteLine("OBJECTID, STARTJD, ENDJD, KIND");
            foreach (zone redZones in redZone)
            {
                string messageString = redZones.roadID + "," + redZones.start + "," + redZones.end + ",red";
                sw_Red.WriteLine(messageString);

            }
            sw_Red.Close();
            System.IO.StreamWriter sw_Yellow = new System.IO.StreamWriter(System.IO.Path.GetDirectoryName(filePath) + System.IO.Path.GetFileNameWithoutExtension(filePath) + this.currentTime+ "s_YellowZone.txt");
            sw_Yellow.WriteLine("OBJECTID, STARTJD, ENDJD, KIND");
            foreach (zone yellowZones in yellowZone)
            {
                string messageString = yellowZones.roadID + "," + yellowZones.start + "," + yellowZones.end + ",yellow";
                sw_Yellow.WriteLine(messageString);

            }
            sw_Yellow.Close();
            System.IO.StreamWriter sw_Green = new System.IO.StreamWriter(System.IO.Path.GetDirectoryName(filePath) + System.IO.Path.GetFileNameWithoutExtension(filePath) + this.currentTime + "s_GreenZone.txt");
            sw_Green.WriteLine("OBJECTID, STARTJD, ENDJD, KIND");
            foreach (zone greenZones in greenZone)
            {
                string messageString = greenZones.roadID + "," + greenZones.start + "," + greenZones.end + ",green";
                sw_Green.WriteLine(messageString);

            }
            sw_Green.Close();
        }

        private zone saveInfluence(int roadID, W W,double t)
        {
            zone influence = new zone();
            
            influence.roadID = roadID;
            if (t > W.timeOfArrive && t < W.timeOfDeparture)
            {
                influence.start = W.locationOfArrive;
                influence.end = W.getW() * (t - W.timeOfArrive);//速度*时间

            }
            else if (t > W.timeOfDeparture)
            {
                influence.start = W.locationOfArrive;
                influence.end = W.locationOfDeparture;
            }
            return influence;   
            
            
        }

        private zone saveSpeed(int roadID, W W, double t)
        {
            zone influence = new zone();

            influence.roadID = roadID;
            if (t > W.timeOfArrive && t < W.timeOfDeparture)
            {
                influence.start = W.locationOfArrive; 
                influence.end = W.getW() * (t - W.timeOfArrive);//速度*时间
                influence.speed = W.v2;
            }
            else if (t > W.timeOfDeparture)
            {
                influence.start = W.locationOfArrive;
                influence.end = W.locationOfDeparture;
                influence.speed = W.v2;
            }
            return influence;


        }

        public double[] getSpecificRoadResult(string roadName, int[] lookTime)
        {
            double influenceLength = 0;
            double[] results=new double[lookTime.Length];
            for (int i = 0; i < lookTime.Length;i++ )
            {
                influenceLength = 0;
                List<RoadSection> result = new List<RoadSection>();
                result = processSpeedResult(lookTime[i]);
                List<int> specificResultRoadIDs = new List<int>();
                System.Diagnostics.Debug.WriteLine("时间:" + lookTime[i]);


                foreach (RoadSection roadSection in result)
                {
                    if (roadSection.passTurningCount == 0 && roadSection.passStraightCount<=100 && roadSection.speedResultZone.Count>0) //passturningcount=1为曲阳路 =0是四平路
                    //((roadSection.speedResultZone.Count > 0 && roadSection.passTurningCount == 0 && roadSection.name==roadName)) //曲阳路
                    //if ((roadSection.speedResultZone.Count > 0 && roadSection.name == "环湖西一路") || (roadSection.speedResultZone.Count > 0 && roadSection.name == "申港大道"))
                    //if ((roadSection.speedResultZone.Count > 0 && roadSection.passTurningCount == 0 && roadSection.speedResultZone.Count > 0) || (roadSection.speedResultZone.Count > 0 && roadSection.name == "水芸路") || (roadSection.speedResultZone.Count > 0 && roadSection.name == "环湖西一路")) //passturningcount=1为曲阳路 =0是四平路
                    {
                        if (!isResultExist(roadSection)) specificResult.Add(roadSection);
                        //specificResult.Add(roadSection);
                        specificResultRoadIDs.Add(roadSection.RoadID);
                        if (roadSection.name=="邯郸路")
                        {
                            int bbb;
                        }
                        foreach (Zone resultZones in roadSection.speedResultZone)
                        {
                            
                            //System.Diagnostics.Debug.WriteLine("曲阳路"+roadSection.RoadID+"，长度"+(resultZones.end - resultZones.start)+"，交通波"+resultZones.waveKind+"，W1波速"+roadSection.W1.getW());
                        }
                    }
                }

                specificResultRoadIDs = specificResultRoadIDs.Distinct().ToList();
                System.Diagnostics.Debug.WriteLine("结果数：" + specificResult.Count);


                for (int j = 0; j < specificResult.Count; j++)
                {
                    RoadSection roadSection = specificResult[j];
                    System.Diagnostics.Debug.WriteLine("ID:"+roadSection.RoadID+" 路段名字："+roadSection.name+",passturning:"+roadSection.passTurningCount+",passStraight:"+roadSection.passStraightCount);
                    if (true)
                    {
                        if (j==specificResult.Count-1)
                        {
                            if (roadSection.speedResultZone.Count > 1)
                            {
                                //throw new Exception("count >1");

                            }
                            foreach (Zone resultZones in roadSection.speedResultZone)
                            {
                                //if (Math.Abs(resultZones.end - resultZones.start-roadSection.Length)>1)
                                //{
                                //    throw new Exception("length variance >1");

                                //}
                                
                                influenceLength += resultZones.end - resultZones.start;
                                System.Diagnostics.Debug.WriteLine("时间：" + lookTime[i] + "，拥堵最长到:" + roadSection.name);
                            }
                        } 
                        else
                        {

                            foreach (Zone resultZones in roadSection.speedResultZone)
                            {

                                influenceLength += resultZones.end - resultZones.start;
                                
                            }
                        }
                        
                    }
                }
                    
                    //foreach (RoadSection roadSection in result)
                    //{
                    //    if (roadSection.name == roadName && roadSection.passTurningCount == 0)
                    //    {

                    //        //测试代码
                    //        if (roadSection.speedResultZone.Count > 1)
                    //        {
                    //            int aaa;
                    //        }
                    //        //结束



                    //        foreach (Zone resultZones in roadSection.speedResultZone)
                    //        {
                    //            if (resultZones.congestionLevel <= 5)
                    //            {
                    //                if (Math.Abs(resultZones.end - resultZones.start - roadSection.Length) < 0.01)
                    //                {
                    //                    influenceLength += roadSection.Length;
                    //                }
                    //                else
                    //                {
                    //                    influenceLength += resultZones.end - resultZones.start;
                    //                    System.Diagnostics.Debug.WriteLine("时间：" + lookTime[i] + "，拥堵最长到:" + roadSection.RoadID);

                    //                }


                    //            }

                    //        }
                    //    }
                    //}
                results[i] = influenceLength;
            } 
             
            return results;
        }

        private bool isResultExist(RoadSection roadSection)
        {
            bool isExist = false;
            foreach (RoadSection rs in specificResult)
            {
                if (roadSection.RoadID == rs.RoadID)
                {
                    isExist = true;
                }
            }
            return isExist;
        }
    }
}
