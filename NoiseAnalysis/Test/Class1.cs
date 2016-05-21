using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;

namespace NoiseAnalysis
{

    public class Class1
    {
        public void readLayerTest(string strVectorFile)
        {
            Gdal.AllRegister();
            //为了支持中文路径，请添加下面这句代码  
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            //为了使属性表字段支持中文，请添加下面这句  
            Gdal.SetConfigOption("SHAPE_ENCODING", "");


            // 注册所有的驱动  
            Ogr.RegisterAll();
            Gdal.SetConfigOption("GDAL_DATA", "E://lib//gdal//gdal1.9//data");
    
            //打开数据  
            DataSource ds = Ogr.Open(strVectorFile, 0);


           
            if (ds == null)
            {
                Console.WriteLine("打开文件【{0}】失败！", strVectorFile);
                return;
            }
            Console.WriteLine("打开文件【{0}】成功！", strVectorFile);

            
            // 获取该数据源中的图层个数，一般shp数据图层只有一个，如果是mdb、dxf等图层就会有多个  
            int iLayerCount = ds.GetLayerCount();

            // 获取第一个图层  
            Layer oLayer = ds.GetLayerByIndex(0);


            String c = "";
            oLayer.GetSpatialRef().AutoIdentifyEPSG();

            Console.WriteLine(oLayer.GetSpatialRef().AutoIdentifyEPSG());


            OSGeo.OSR.SpatialReference spa = oLayer.GetSpatialRef();
           // if (spa==null)
           // {
                spa = new OSGeo.OSR.SpatialReference(null);
                spa.ImportFromEPSG(3395);

              
                spa.ExportToWkt(out c);
                Console.WriteLine(c );
              
                return;


           // }
          //  String a = "";
           // spa.EPSGTreatsAsLatLong();
           // Console.WriteLine(spa.ExportToWkt(out a));
           
           // oLayer.GetSpatialRef().ExportToWkt(out a);

           // Console.WriteLine(spa.GetProjParm(out a));

           // Console.WriteLine(oLayer.GetSpatialRef().GetLinearUnitsName());
            if (oLayer == null)
            {
                Console.WriteLine("获取第{0}个图层失败！\n", 0);
                return;
            }

            // 对图层进行初始化，如果对图层进行了过滤操作，执行这句后，之前的过滤全部清空  
            oLayer.ResetReading();

            // 通过属性表的SQL语句对图层中的要素进行筛选，这部分详细参考SQL查询章节内容  
            //  oLayer.SetAttributeFilter("\"NAME99\"LIKE \"北京市市辖区\"");  

            // 通过指定的几何对象对图层中的要素进行筛选  
            //oLayer.SetSpatialFilter();  

            // 通过指定的四至范围对图层中的要素进行筛选  
            //oLayer.SetSpatialFilterRect();  
            
            // 获取图层中的属性表表头并输出  
           // Console.WriteLine("属性表结构信息：");
            FeatureDefn oDefn = oLayer.GetLayerDefn();
            int iFieldCount = oDefn.GetFieldCount();
            for (int iAttr = 0; iAttr < iFieldCount; iAttr++)
            {
                FieldDefn oField = oDefn.GetFieldDefn(iAttr);

                Console.WriteLine("{0}:{1} ({2}.{3})", oField.GetNameRef(),
                         oField.GetFieldTypeName(oField.GetFieldType()),
                         oField.GetWidth(), oField.GetPrecision());
            }

            // 输出图层中的要素个数  
            Console.WriteLine("要素个数 = {0}", oLayer.GetFeatureCount(0));

            Feature oFeature = null;
            // 下面开始遍历图层中的要素  
            while ((oFeature = oLayer.GetNextFeature()) != null)
            {
                Console.WriteLine("当前处理第{0}个: \n属性值：", oFeature.GetFID());
                // 获取要素中的属性表内容  
        
                for (int iField = 0; iField < iFieldCount; iField++)
                {
                    FieldDefn oFieldDefn = oDefn.GetFieldDefn(iField);
                    FieldType type = oFieldDefn.GetFieldType();

                    switch (type)
                    {
                        case FieldType.OFTString:
                            Console.WriteLine("{0}\t", oFeature.GetFieldAsString(iField));
                            break;
                        case FieldType.OFTReal:
                            Console.WriteLine("{0}\t", oFeature.GetFieldAsDouble(iField));
                            break;
                        case FieldType.OFTInteger:
                            Console.WriteLine("{0}\t", oFeature.GetFieldAsInteger(iField));
                            break;
                        default:
                            Console.WriteLine("{0}\t", oFeature.GetFieldAsString(iField));
                            break;
                    }
                }
       
                // 获取要素中的几何体  
                Geometry oGeometry = oFeature.GetGeometryRef();
   
              //  String a=oGeometry.GetGeometryName();
              //  String b = "";
              //  oGeometry.ExportToWkt(out b);
                Console.WriteLine(oGeometry.GetGeometryName());
                // 为了演示，只输出一个要素信息  
                break;
            }
    
            Console.WriteLine("数据集关闭！");
            Console.ReadLine();
        }
    }
}
