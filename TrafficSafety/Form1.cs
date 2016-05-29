using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Microsoft.VisualBasic;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.NetworkAnalyst;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesFile;
using TrafficSafety.Model;
namespace TrafficSafety
{
    public partial class Form1 : Form
    {
        private IWorkspace workspace;
        private INAContext m_NAContext;
        MainRoadInfluence mMainroad;
        public Form1()
        {
            InitializeComponent();
            
        }
#region 网络分析部分
        /// <summary>
        /// Geodatabase function: open work space
        /// </summary>
        /// <param name="strGDBName">Input file name</param>
        /// <returns>Workspace</returns>
        public IWorkspace OpenWorkspace(string strGDBName)
        {
            // As Workspace Factories are Singleton objects, they must be instantiated with the Activator
            var workspaceFactory = System.Activator.CreateInstance(System.Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory")) as ESRI.ArcGIS.Geodatabase.IWorkspaceFactory;

            if (!System.IO.Directory.Exists(strGDBName))
            {
                MessageBox.Show("The workspace: " + strGDBName + " does not exist", "Workspace Error");
                return null;
            }

            IWorkspace workspace = null;
            try
            {
                workspace = workspaceFactory.OpenFromFile(strGDBName, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Opening workspace failed: " + ex.Message, "Workspace Error");
            }

            return workspace;
            
        }

        /// <summary>
        /// Geodatabase function: open network dataset
        /// </summary>
        /// <param name="workspace">Input workspace</param>
        /// <param name="strNDSName">Input network dataset name</param>
        /// <returns>NetworkDataset</returns>
        public INetworkDataset OpenNetworkDataset(IWorkspace workspace, string featureDatasetName, string strNDSName)
        {
            // Obtain the dataset container from the workspace
            var featureWorkspace = workspace as IFeatureWorkspace;
            ESRI.ArcGIS.Geodatabase.IFeatureDataset featureDataset = featureWorkspace.OpenFeatureDataset(featureDatasetName);
            var featureDatasetExtensionContainer = featureDataset as ESRI.ArcGIS.Geodatabase.IFeatureDatasetExtensionContainer;
            ESRI.ArcGIS.Geodatabase.IFeatureDatasetExtension featureDatasetExtension = featureDatasetExtensionContainer.FindExtension(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTNetworkDataset);
            var datasetContainer3 = featureDatasetExtension as ESRI.ArcGIS.Geodatabase.IDatasetContainer3;
            
            // Use the container to open the network dataset.
            ESRI.ArcGIS.Geodatabase.IDataset dataset = datasetContainer3.get_DatasetByName(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTNetworkDataset, strNDSName);
            IFeatureClass fc = OpenFeatureClassFromGDB(workspace, featureDatasetName, "Nshanghai_Join");
            
            return dataset as ESRI.ArcGIS.Geodatabase.INetworkDataset;
            
        }
        public IFeatureClass OpenFeatureClassFromGDB(IWorkspace workspace, string featureDatasetName, string featureClassName)
        {
            // Obtain the dataset container from the workspace
            var featureWorkspace = workspace as IFeatureWorkspace;
            ESRI.ArcGIS.Geodatabase.IFeatureDataset pFeatureDataset = featureWorkspace.OpenFeatureDataset(featureDatasetName);
            IEnumDataset iEDS1=pFeatureDataset.Subsets;
            iEDS1.Reset();
            IDataset ids1=iEDS1.Next();
            while (ids1 is FeatureClass )
            {
                if (ids1.Name == featureClassName)
                {
                    IFeatureClass fc = featureWorkspace.OpenFeatureClass(ids1.Name);
                    return fc;
                }
                
                ids1 = iEDS1.Next();
            }
            return null;
        }
#endregion
#region 测试代码
        
        
        string txtWorkspacePath = "D:\\shanghai路网+POIshp\\shanghainet\\shanghainet.gdb";
        string txtNetworkDataset = "shanghainet_ND";
        string txtFeatureDataset = "shanghainet";
        ///<summary>Loop through a network dataset and display adjacencies for each junction in a message box.</summary>
        ///  
        ///<param name="networkDataset">An INetworkDataset interface.</param>
        ///   
        ///<remarks></remarks>
        public void DisplayNetworkAdjacencyInMessageBox(ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset)
        {
            if (networkDataset == null)
            {
                return;
            }
            ESRI.ArcGIS.Geodatabase.INetworkQuery networkQuery = ((ESRI.ArcGIS.Geodatabase.INetworkQuery)(networkDataset));


            ESRI.ArcGIS.Geodatabase.INetworkElement edgeNetworkElement = networkQuery.CreateNetworkElement(ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge);
            ESRI.ArcGIS.Geodatabase.INetworkEdge networkEdge = ((ESRI.ArcGIS.Geodatabase.INetworkEdge)(edgeNetworkElement)); // Explicit Cast 

            // Get the from network junction
            ESRI.ArcGIS.Geodatabase.INetworkElement fromJunctionNetworkElement = networkQuery.CreateNetworkElement(ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETJunction);
            ESRI.ArcGIS.Geodatabase.INetworkJunction fromNetworkJunction = ((ESRI.ArcGIS.Geodatabase.INetworkJunction)(fromJunctionNetworkElement)); // Explicit Cast

            // Get the to network junction
            ESRI.ArcGIS.Geodatabase.INetworkElement toJunctionNetworkElement = networkQuery.CreateNetworkElement(ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETJunction);
            ESRI.ArcGIS.Geodatabase.INetworkJunction toNetworkJunction = ((ESRI.ArcGIS.Geodatabase.INetworkJunction)(toJunctionNetworkElement)); // Explicit Cast

            ESRI.ArcGIS.Geodatabase.IEnumNetworkElement enumNetworkElement = networkQuery.get_Elements(ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETJunction); // Explicit Cast

            //测试代码
            ESRI.ArcGIS.Geodatabase.IEnumNetworkElement enumNetworkElement_OID = networkQuery.get_ElementsByOID(1, 100423); //networksource 1是边，2是节点
            ESRI.ArcGIS.Geodatabase.INetworkElement networkElement_OID = enumNetworkElement_OID.Next();
            ESRI.ArcGIS.Geodatabase.INetworkEdge m_netWorkEdge = ((ESRI.ArcGIS.Geodatabase.INetworkEdge)(networkElement_OID));

            
            int j = 0;
            while (!(networkElement_OID == null))
            {
                Debug.WriteLine("第" + j + "次while");
                Debug.WriteLine("类型：" + networkElement_OID.ElementType + "OID: " + networkElement_OID.OID);
                //networkElement_OID = enumNetworkElement_OID.Next();
                m_netWorkEdge.QueryJunctions(fromNetworkJunction, toNetworkJunction);
                string messageString = "从边" + m_netWorkEdge.OID + "的节点" + fromNetworkJunction.OID + "查找连接边。。。" + System.Environment.NewLine;
                for (System.Int32 i = 1; i <= fromNetworkJunction.EdgeCount - 1; i++)//i=1 避开本条边
                {
                    Debug.WriteLine("第" + i + "次for");
                    fromNetworkJunction.QueryEdge(i, false, m_netWorkEdge);
                    messageString =  "Edge OID:" + m_netWorkEdge.OID + System.Environment.NewLine;
                    Debug.WriteLine(messageString);
                }
                
                networkElement_OID = enumNetworkElement_OID.Next();
                j++;
            }
            //测试代码结束

            ESRI.ArcGIS.Geodatabase.INetworkElement networkElement = enumNetworkElement.Next();
            ESRI.ArcGIS.Geodatabase.INetworkJunction networkJunction = ((ESRI.ArcGIS.Geodatabase.INetworkJunction)(networkElement)); // Explicit Cast

            while (!(networkElement == null))
            {
                System.String messageString = "Junction: " + networkJunction.EID + " is adjacent to: " + networkJunction.EdgeCount + " junctions." + System.Environment.NewLine;
                for (System.Int32 i = 0; i <= networkJunction.EdgeCount - 1; i++)
                {
                    networkJunction.QueryEdge(i, true, networkEdge);
                    networkEdge.QueryJunctions(fromNetworkJunction, toNetworkJunction);
                    messageString = messageString + "From Junction OID: " + fromNetworkJunction.OID + System.Environment.NewLine; //From和source是一样的
                    messageString = messageString + "Source Junction OID: " + networkJunction.OID + System.Environment.NewLine;
                    messageString = messageString + "Adjacent Junction EID: " + toNetworkJunction.EID + System.Environment.NewLine;
                    messageString = messageString + "Adjacent Junction OID: " + toNetworkJunction.OID + System.Environment.NewLine;
                    //messageString = messageString + "Direction:" + networkEdge.Direction + System.Environment.NewLine;
                    messageString = messageString + "Edge OID:" + networkEdge.OID + System.Environment.NewLine;

                }
                System.Windows.Forms.MessageBox.Show(messageString, "Network Adjacency", System.Windows.Forms.MessageBoxButtons.OK);
                networkElement = enumNetworkElement.Next();
            }

        }

        //private List<RoadSection> searchRoad(int inputRoadID,int startNodeID, ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset)
        //{
        //    Debug.WriteLine("当前路段：" + inputRoadID + " 开始节点："+startNodeID);
        //    List<RoadSection> results = new List<RoadSection>();//输出结果
            
        //    //构造查询变量
        //    ESRI.ArcGIS.Geodatabase.INetworkQuery networkQuery = ((ESRI.ArcGIS.Geodatabase.INetworkQuery)(networkDataset));
        //    //构造forwardstar变量，并设置其查询属性
        //    var fStarEx = networkQuery.CreateForwardStar() as ESRI.ArcGIS.Geodatabase.INetworkForwardStarEx;
        //    fStarEx.IsForwardTraversal = false;
        //    fStarEx.BacktrackPolicy = esriNetworkForwardStarBacktrack.esriNFSBNoBacktrack;
        //    ESRI.ArcGIS.Geodatabase.INetworkAttribute restrictionAttribute = networkDataset.get_AttributeByName("Oneway");
        //    fStarEx.AddRestrictionAttribute(restrictionAttribute);

        //    ESRI.ArcGIS.Geodatabase.INetworkForwardStarAdjacencies fStarAdj = networkQuery.CreateForwardStarAdjacencies();

        //    ESRI.ArcGIS.Geodatabase.IEnumNetworkElement enumNetworkElement_OID = networkQuery.get_ElementsByOID(1, inputRoadID); //networksource 1是边，2是节点
        //    ESRI.ArcGIS.Geodatabase.INetworkElement networkElement_OID = enumNetworkElement_OID.Next();
        //    ESRI.ArcGIS.Geodatabase.INetworkEdge m_netWorkEdge = ((ESRI.ArcGIS.Geodatabase.INetworkEdge)(networkElement_OID));
        //    //各类变量初始化
        //    double fromPosition, toPosition;
        //    var junction = networkQuery.CreateNetworkElement(esriNetworkElementType.esriNETJunction) as INetworkJunction;
        //    var fromJunction = networkQuery.CreateNetworkElement(esriNetworkElementType.esriNETJunction) as INetworkJunction;
        //    var toJunction = networkQuery.CreateNetworkElement(esriNetworkElementType.esriNETJunction) as INetworkJunction;
        //    var toEdge = networkQuery.CreateNetworkElement(esriNetworkElementType.esriNETEdge) as INetworkEdge;
        //    var fromEdge = networkQuery.CreateNetworkElement(esriNetworkElementType.esriNETEdge) as INetworkEdge;
            
        //    int edgeEID = m_netWorkEdge.EID;
        //    bool isFiltered;
        //    int j = 0;
        //    fStarEx.QueryEdge(edgeEID, ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection.esriNEDAlongDigitized, fromEdge);


        //    fromEdge.QueryJunctions(fromJunction, toJunction); //看路段的哪个节点是“开始节点”，要从开始节点往前搜寻新的路段            
        //    if (fromJunction.OID==startNodeID)
        //    {
        //        junction = fromJunction;
        //    } 
        //    else
        //    {
        //        junction = toJunction;
        //    }
           
        //    try //如果路段方向错误的话，换一个方向
        //    {
        //        fStarEx.QueryAdjacencies(junction, fromEdge, null, fStarAdj);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        fStarEx.QueryEdge(edgeEID, ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection.esriNEDAgainstDigitized, fromEdge);
        //        fStarEx.QueryAdjacencies(junction, fromEdge, null, fStarAdj);
        //    }

        //    double length = getRoadLength(fromEdge, networkDataset);
            

        //    for (j = 0; j < fStarAdj.Count;j++ ) //输出搜寻结果
        //    {
        //        fStarAdj.QueryEdge(j, toEdge, out fromPosition, out toPosition);
        //        fStarAdj.QueryToJunction(j, toJunction, out isFiltered);
        //        double nextLength = getRoadLength(toEdge, networkDataset);
        //        RoadSection newRoad = new RoadSection(toEdge.OID,nextLength,toJunction.OID,junction.OID);
               
        //        results.Add(newRoad);
        //        string messageString = "Edge OID:" + toEdge.OID + " junction OID:" + toJunction.OID;
        //        Debug.WriteLine(messageString);
        //    }
            
        //    return results;
            
        //}

        //private double getRoadLength(INetworkEdge road, ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset)
        //{
        //    ESRI.ArcGIS.Geodatabase.INetworkSource netSource = networkDataset.get_SourceByID(road.SourceID);
        //    var fClassContainer = networkDataset as ESRI.ArcGIS.Geodatabase.IFeatureClassContainer;
        //    ESRI.ArcGIS.Geodatabase.IFeatureClass sourceFClass = fClassContainer.get_ClassByName(netSource.Name);
        //    ESRI.ArcGIS.Geodatabase.IFeature roadFeature = sourceFClass.GetFeature(road.OID);
        //    string aaa  = roadFeature.get_Value(16).ToString();
        //    double length = Convert.ToDouble(aaa);
        //    return 1000*length; //米
        //}
        
#endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        
#region Arcgis网络路径搜寻
        public struct SearchStruct
        {
            public int junctionEID;
            public int numberOfHops;
            public int fromEdgeEID;
            public ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection fromEdgeDirection;
            public int lastExteriorEdgeEID;
            public ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection
                lastExteriorEdgeDirection;
        } 

        public int BreadthFirstSearch(ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset,int startingJunctionEID, int destinationJunctionEID, string[]restrictionAttributeNames)
{
            // You are doing a breadth-first search to determine how many edges away a destination junction
            // is from an origin junction.
                    // Get the forward star and query adjacency objects.
        var networkQuery = networkDataset as ESRI.ArcGIS.Geodatabase.INetworkQuery;
        var fStarEx = networkQuery.CreateForwardStar()as
            ESRI.ArcGIS.Geodatabase.INetworkForwardStarEx;
        ESRI.ArcGIS.Geodatabase.INetworkForwardStarAdjacencies fStarAdj =
            networkQuery.CreateForwardStarAdjacencies();
        var searchQueue = new System.Collections.Queue();

        // Prepare the forward star settings.
        fStarEx.BacktrackPolicy =
            ESRI.ArcGIS.Geodatabase.esriNetworkForwardStarBacktrack.esriNFSBNoBacktrack; 
            // No u-turns allowed.
        
        fStarEx.IsForwardTraversal = true; // Searching in a forward direction.

        // The search will be an "exact" search and not use the hierarchy settings.
        // However, hierarchy settings can be set. See the following:
        // fStarEx.HierarchyAttribute = your hierarchy attribute.
        // fStarEx.MaxTraversableHierarchyValue = max value.

        // Add the restriction attributes that need to be honored.
        foreach (string restrictionAttributeName in restrictionAttributeNames)
        {
            ESRI.ArcGIS.Geodatabase.INetworkAttribute restrictionAttribute =
                networkDataset.get_AttributeByName(restrictionAttributeName);
            fStarEx.AddRestrictionAttribute(restrictionAttribute);
        }

        // Add attribute adjustments.
        // Some sample adjustments are shown as follows:
        /*
        fStarEx.AddEdgeRestriction(myEdge, 0.0, 0.5); // Restrict half an edge.
        fStarEx.AddJunctionRestriction(myJunction); // Restrict a junction.
        fStarEx.AddTurnRestriction(myTurn); // Restrict a single turn.
        fStarEx.AdjustEdgeAttributeValue(myEdge, 0.0, 1.0, myAttribute, esriNetworkAttributeAdjustmentType.esriNAATScale, 2.0); // Scale a whole edge attribute value by 2x.
        fStarEx.AdjustJunctionAttributeValue(myJunction, myAttribute, esriNetworkAttributeAdjustmentType.esriNAATReplace, 15.0); // Replace a junction attribute value.
        fStarEx.AdjustEdgeAttributeValue(myEdge, 0.5, 0.5, myAttribute, esriNetworkAttributeAdjustmentType.esriNAATAdd, 1.0); // Add 1.0 to the midway point of an edge.
              */
                    // Get the starting junction.
        var junction = networkQuery.CreateNetworkElement
            (ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETJunction)as
            ESRI.ArcGIS.Geodatabase.INetworkJunction;

        var fromEdge = networkQuery.CreateNetworkElement
            (ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge)as
            ESRI.ArcGIS.Geodatabase.INetworkEdge;
        var toEdge = networkQuery.CreateNetworkElement
            (ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge)as
            ESRI.ArcGIS.Geodatabase.INetworkEdge;
        var lastExteriorEdge = networkQuery.CreateNetworkElement
            (ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge)as
            ESRI.ArcGIS.Geodatabase.INetworkEdge;
        double fromPosition, toPosition;

        // Queue up the starting junction.
        searchQueue.Enqueue(new SearchStruct
        {
            junctionEID = startingJunctionEID, numberOfHops = 0, fromEdgeEID =  - 1,
                fromEdgeDirection =
                ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection.esriNEDNone,
                lastExteriorEdgeEID =  - 1, lastExteriorEdgeDirection =
                ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection.esriNEDNone
        }

        );

        // Use an edge based array to properly handle turns in the network dataset (see the following logic).
        bool[] isAlongReached = new bool[networkQuery.get_MaxEID
            (ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge) + 1];
        bool[] isAgainstReached = new bool[networkQuery.get_MaxEID
            (ESRI.ArcGIS.Geodatabase.esriNetworkElementType.esriNETEdge) + 1];
        // Iterate over the queue until the destination is found or the queue is empty.
        Debug.WriteLine("====================开始====================");
        var currentEdge = networkQuery.CreateNetworkElement(esriNetworkElementType.esriNETEdge) as INetworkEdge;
        while (searchQueue.Count > 0)
        {
            Debug.WriteLine("队列数量：" + searchQueue.Count);
            SearchStruct currentSearchStruct = (SearchStruct)searchQueue.Dequeue();
            if (currentSearchStruct.fromEdgeEID!=-1)
            {
                fStarEx.QueryEdge(currentSearchStruct.fromEdgeEID, esriNetworkEdgeDirection.esriNEDAlongDigitized, currentEdge);
                string message = currentEdge.OID.ToString();
                Debug.WriteLine("当前道路OID：" + message);
            }
            
            
            
            // Check if this is the destination.
            if (currentSearchStruct.junctionEID == destinationJunctionEID)
                return currentSearchStruct.numberOfHops;
            // Return the final hop count.

            // Populate the edges to indicate the path taken to reach this junction. This is 
            //  necessary for detecting and tracking turns.
            bool hasFromEdge = false;
            bool hasLastExteriorEdge = false;

            if (currentSearchStruct.fromEdgeEID != -1)
            {
                hasFromEdge = true;

                // If there was a previous edge in the search, then populate the fromEdge 
                //  with that edge's values.
                networkQuery.QueryEdge(currentSearchStruct.fromEdgeEID,
                    currentSearchStruct.fromEdgeDirection, fromEdge);
                if (currentSearchStruct.lastExteriorEdgeEID != -1)
                {
                    hasLastExteriorEdge = true;
                    networkQuery.QueryEdge(currentSearchStruct.lastExteriorEdgeEID,
                        currentSearchStruct.lastExteriorEdgeDirection, lastExteriorEdge);
                }
            }

            // Find the adjacencies from the current junction. The NetworkForwardStar relies on 
            //  the fromEdge and lastExteriorEdge objects to detect turns. Any restricted turns
            //   exclude the appropriate edges from being returned in the NetworkForwardStarAdjacencies object.
            fStarEx.QueryJunction(currentSearchStruct.junctionEID, junction);
            fStarEx.QueryAdjacencies(junction, (hasFromEdge) ? fromEdge : null,
                (hasLastExteriorEdge) ? lastExteriorEdge : null, fStarAdj);

            // Iterate the adjacencies adding them to the search queue.
            bool isJunctionFiltered;
            for (int index = 0; index < fStarAdj.Count; index++)
            {
                fStarAdj.QueryEdge(index, toEdge, out fromPosition, out toPosition);
                fStarAdj.QueryToJunction(index, junction, out isJunctionFiltered);

                // If this edge has already been marked as "reached" in the search, don't search it again.
                // If the junction is filtered (that is, inaccessible/restricted), skip this entry, since 
                //  it won't provide a valid path to the destination.               
                if ((toEdge.Direction ==
                    ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection.esriNEDAlongDigitized
                    && isAlongReached[toEdge.EID]) || (toEdge.Direction ==
                    ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection.esriNEDAgainstDigitized
                    && isAgainstReached[toEdge.EID]) || isJunctionFiltered)
                    continue;

                // Prepare the struct for the next junction.
                SearchStruct nextRecord = new SearchStruct();
                nextRecord.numberOfHops = currentSearchStruct.numberOfHops + 1;
                // The hop count!
                nextRecord.junctionEID = junction.EID;
                nextRecord.fromEdgeEID = toEdge.EID;
                nextRecord.fromEdgeDirection = toEdge.Direction;
                nextRecord.lastExteriorEdgeEID = -1;
                nextRecord.lastExteriorEdgeDirection =
                    ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection.esriNEDNone;

                // Establish the last exterior edge information based on the current edge's 
                //  turn participation type.
                if (toEdge.TurnParticipationType ==
                    ESRI.ArcGIS.Geodatabase.esriNetworkTurnParticipationType.esriNTPTExterior)
                {
                    nextRecord.lastExteriorEdgeEID = toEdge.EID;
                    nextRecord.lastExteriorEdgeDirection = toEdge.Direction;
                }
                else if (toEdge.TurnParticipationType ==
                    ESRI.ArcGIS.Geodatabase.esriNetworkTurnParticipationType.esriNTPTInterior)
                {
                    nextRecord.lastExteriorEdgeEID = currentSearchStruct.lastExteriorEdgeEID;
                    nextRecord.lastExteriorEdgeDirection =
                        currentSearchStruct.lastExteriorEdgeDirection;
                }

                // Queue the search element.
                searchQueue.Enqueue(nextRecord);

                // If this edge is not an interior edge, mark the edge as "reached." You do not 
                //  mark interior edges as "reached," since they might participate in a restricted turn
                // and you want to allow other (potentially unrestricted) search paths to reach
                // this edge, just in case.
                if (toEdge.TurnParticipationType !=
                    ESRI.ArcGIS.Geodatabase.esriNetworkTurnParticipationType.esriNTPTInterior)
                {
                    if (toEdge.Direction ==
                        ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection.esriNEDAlongDigitized)
                        isAlongReached[toEdge.EID] = true;
                    else if (toEdge.Direction ==
                        ESRI.ArcGIS.Geodatabase.esriNetworkEdgeDirection.esriNEDAgainstDigitized)
                        isAgainstReached[toEdge.EID] = true;
                    else
                        throw new System.Exception("Invalid edge direction");
                }
    }
}

// The search completed, and the destination was never found.
throw new System.Exception("Destination unreachable");
}
#endregion

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RoadResult mResult = new RoadResult(mMainroad);
             
            string _lookTime = InputBox.ShowInputBox("", "");
            double lookTime = Convert.ToDouble(_lookTime);

            //mResult.processResult(lookTime);
            //mResult.processSpeedResult(lookTime);
            mResult.saveSpeedResulttoFile(lookTime,@"D:\事故模拟\speedResult.txt");
            //mResult.saveResulttoFile(@"D:\事故模拟\result.txt");
            
        }

        private void infulenceLengthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RoadResult mResult = new RoadResult(mMainroad);
            int[] looktime = new int[6] { 150, 300, 450, 600, 750, 3000 };
            double[] influenceLength = mResult.getSpecificRoadResult("四平路", looktime);
            //mResult.getSpecificRoadResult("曲阳路", looktime);
            //mResult.getSpecificRoadResult("中山北一路", looktime);
            //mResult.getSpecificRoadResult("淮海中路", looktime);
            double durationTime=mResult.durationTime;
            toolStripStatusLabel1.Text = "traffic load:" + GlobalConst.FLOW_RATIO;
            toolStripStatusLabel1.Text += "，持续时间：" + durationTime;
            toolStripStatusLabel1.Text += "，影响长度：" + influenceLength[5];
        }

        private void 参数设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmSettings settings = new FrmSettings();
            settings.ShowDialog();
            toolStripStatusLabel1.Text = "正在计算………………";
            运行ToolStripMenuItem_Click(sender, e);
        }

        private void 运行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "正在计算………………";
            workspace = OpenWorkspace(txtWorkspacePath);
            INetworkDataset networkDataset = OpenNetworkDataset(workspace, txtFeatureDataset, txtNetworkDataset); //读取网络数据集
            //DisplayNetworkAdjacencyInMessageBox(networkDataset);            
            string[] aaa = { "Oneway" };
            //int test = BreadthFirstSearch(networkDataset, 90, 91, aaa);

            //List<RoadSection> results = searchRoad(274032,5211, networkDataset);
            //List<RoadSection> newResults = new List<RoadSection>();
            //foreach (RoadSection road in results)
            //{
            //    newResults.AddRange(searchRoad(Convert.ToInt32(road.RoadID),road.startNode, networkDataset));
            //}

            //RoadSection mRoad = new RoadSection(72928,networkDataset);            
            //MainRoadInfluence mMainroad = new MainRoadInfluence(networkDataset, mRoad,155696, 300, 3, 1, 3000, 1800, 200, 300);
            ////曲阳路事故
            RoadSection mRoad = new RoadSection(72933, networkDataset);
            //(networkDataset, RoadSection road,int nodeOID, double accidentPoint, int numOfLane, int numOfLaneLeft, double q1, double T12, double T23, double t0)
            mMainroad = new MainRoadInfluence(networkDataset, mRoad, 155149, 5, 2, 1, 1000, 300, 600, 0); //车道数2 剩余1
            //泰梅路事故
            //RoadSection mRoad = new RoadSection(183778, networkDataset);
            //mMainroad = new MainRoadInfluence(networkDataset, mRoad, 234584, 10, 2, 1, 1000, 300, 600, 0);

            //中山北一路事故
            //RoadSection mRoad = new RoadSection(71428, networkDataset);
            ////(networkDataset, RoadSection road,int nodeOID, double accidentPoint, int numOfLane, int numOfLaneLeft, double q1, double T12, double T23, double t0)
            //mMainroad = new MainRoadInfluence(networkDataset, mRoad, 148740, 5, 2, 1, 500, 300, 420, 0); //车道数2 剩余1

            //淮海中路事故
            //RoadSection mRoad = new RoadSection(28755, networkDataset);
            ////(networkDataset, RoadSection road,int nodeOID, double accidentPoint, int numOfLane, int numOfLaneLeft, double q1, double T12, double T23, double t0)
            //mMainroad = new MainRoadInfluence(networkDataset, mRoad, 123541, 10, 2, 1, 1080, 300, 600, 0); //车道数2 剩余1
            toolStripStatusLabel1.Text = "计算完成！";
        }
    }
}
