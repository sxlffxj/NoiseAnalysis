using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;

namespace NoiseAnalysis.SourcePartition
{

 public class LayerCreate
    {
       const string POINTSOURCE = "NAME,HEIGHT_G,PWLs,FB_TYPE,PWLm,TIME";
       const string LINESOURCE = "NAME,HEIGHT_G,PWLs,FB_TYPE,PWLm,TIME";
       const string POLYGONSOURCE = "NAME,HEIGHT_G,PWLs,FB_TYPE,PWLm,TIME";
       const string ROADSOURCE = "NAME,HEIGHT_G,ROAD_T,ROAD_W,ROAD_G,LANE_N,SPEED_SD,SPEED_MD,SPEED_LD,SPEED_SE,SPEED_ME,SPEED_LE,SPEED_SN,SPEED_MN,SPEED_LN,FLOW_SD,FLOW_MD,FLOW_LD,FLOW_SE,FLOW_ME,FLOW_LE,FLOW_SN,FLOW_MN,FLOW_LN,TD_TYPE,Leq_D,Leq_E,Leq_N";
       const string RAILWAYSOURCE = "NAME,HEIGHT_G,RAILBED_WIDTH,REF_HEIGHT,REF_DIS,IF_jumble,IF_bridge,IF_seamed,FBs,Leq_D,Leq_E,Leq_N,Leq_Da,Leq_Ea,Leq_Na,TR _NAME,TR_T_LIST,CAL_TYPE,E_TIME,TR_NUM,TR_V,TR_L_LIST,TR_L,TR_TIME";















       public bool createFidle(Layer receiveLayer, string fieldName)
       {
           string[] fieldList = fieldName.Split(',');
           foreach (String name in fieldList)
           {
               receiveLayer.CreateField(new FieldDefn(name, FieldType.OFTString), 1);//高度
           }
           return true;
       }
    }
}
