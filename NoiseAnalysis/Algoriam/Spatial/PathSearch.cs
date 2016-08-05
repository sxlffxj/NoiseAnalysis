using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using NoiseAnalysis.Algoriam.Spatial;
using System.Xml;
using NoiseAnalysis.Algorithm;
using GeoAPI.Geometries;


namespace NoiseAnalysis.Algoriam.Spatial
{

    /// <summary>
    /// 路径寻找
    /// </summary>
    class PathSearch
    {







        /// <summary>
        /// 寻找一组声线
        /// </summary>
        /// <param name="buildings">建筑物图层</param>
        /// <param name="sourcesPoint">声源点</param>
        /// <param name="receivePoint">接收点</param>
        /// <param name="range">影响范围</param>
        /// <returns>一组声线</returns>
        public Geometry[] getPath(Layer buildings, Geometry sourcesPoint, Geometry receivePoint, float range)
        {
            //构建传播路程的数组，分别为两点直射、上绕射、左绕射、右绕射、反射
            Geometry[] pathLength = new Geometry[5];

            //构建直达声线
            Geometry line = GeometryCreate.createLineString3D(sourcesPoint.GetX(0), sourcesPoint.GetY(0), sourcesPoint.GetZ(0), receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));
            //直达声线长度
           // double distance = Distance3D.getPointDistance(sourcesPoint, receivePoint);
            //直达声线
            pathLength[0] = line;

            buildings.SetSpatialFilter(line);//直达声线上受影响的建筑物

            //判断是否有直射路径，如果没有，求绕射路径

            if (buildings.GetFeatureCount(0) <= 0)
            {
                //存在直射则不存在绕射
                pathLength[1] = null;
                pathLength[2] = null;
                pathLength[3] = null;
                //计算反射
                pathLength[4] = line;
            }
            else
            {
                Geometry[] diffLine = diffractionLine(buildings, sourcesPoint, receivePoint, line);
                pathLength[1] = diffLine[0];
                pathLength[2] = diffLine[0];
                pathLength[3] = diffLine[0];

                //忽略反射
                pathLength[4] = null;
            }



            //计算反射路径









            return pathLength;
        }


        /// <summary>
        /// 绕射声线
        /// </summary>
        /// <param name="buildingLayer">建筑物图层</param>
        /// <param name="sourcePoint">声源点</param>
        /// <param name="receivePoint">接收点</param>
        /// <param name="line">直达声线</param>
        /// <returns>绕射声线</returns>
        private Geometry[] diffractionLine(Layer buildingLayer, Geometry sourcePoint, Geometry receivePoint, Geometry line)
        {
            //左绕射路径点集
            SortedList<double, Geometry> leftPoints = new SortedList<double, Geometry>();
            //右绕射路径点集
            SortedList<double, Geometry> rightPoints = new SortedList<double, Geometry>();

            //直达路径方程参数
            double k = (sourcePoint.GetY(0) - receivePoint.GetY(0)) / (sourcePoint.GetX(0) - receivePoint.GetX(0));
            double b = sourcePoint.GetY(0) - k * sourcePoint.GetX(0);

            //最高建筑物
            double heighttemp = 0;
            Geometry building = null;

            Feature bFeature = null;
            try
            {
                //加入起始点
                rightPoints.Add(0, sourcePoint);
                leftPoints.Add(0, sourcePoint);

                while ((bFeature = buildingLayer.GetNextFeature()) != null)
                {

                    Geometry buildingring = bFeature.GetGeometryRef().ConvexHull().GetGeometryRef(0);
                    double height = bFeature.GetFieldAsDouble("HEIGHT_BU");
                    //选择最高的建筑物
                    if (heighttemp < height)
                    {
                        building = bFeature.GetGeometryRef();
                    }

                    //构建左右绕射点集
                    for (int i = 0; i < buildingring.GetPointCount() - 1; i++)
                    {
                        Geometry point = GeometryCreate.createPoint3D(buildingring.GetX(i), buildingring.GetY(i), height);
                        if (point.GetY(0) < k * point.GetX(0) + b)
                        {
                            rightPoints.Add(sourcePoint.Distance(point), point);
                        }
                        else if (buildingring.GetY(i) > k * buildingring.GetX(i) + b)
                        {
                            leftPoints.Add(sourcePoint.Distance(point), point);
                        }
                    }
                }
                //加入终点
                rightPoints.Add(sourcePoint.Distance(receivePoint), receivePoint);
                leftPoints.Add(sourcePoint.Distance(receivePoint), receivePoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            Geometry[] diffLine = new Geometry[3];
            if(heighttemp<=receivePoint.GetZ(0)||heighttemp<=sourcePoint.GetZ(0)){
                diffLine[0] = null;
                diffLine[1] = null;
                diffLine[2] = null;

            }
            else
            {
                diffLine[0] = getUpdiffLine(building, line, heighttemp);
                diffLine[1] = flankdiffLine(leftPoints, line);
                diffLine[2] = flankdiffLine(rightPoints, line);
            }
            return diffLine;

        }


        /// <summary>
        /// 获取直达路径
        /// </summary>
        /// <param name="height">建筑物高度</param>
        /// <param name="building">建筑物</param>
        /// <param name="line">直达路径</param>
        /// <returns>上绕射路径</returns>

        private Geometry getUpdiffLine(Geometry building, Geometry line, double height)
        {
            if (building == null)
            {
                return null;
            }
            //与建筑物的交线
            Geometry crossLine = line.Intersection(building);

            //上绕射路径
            Geometry updiffLine = new Geometry(wkbGeometryType.wkbLineString);



            updiffLine.AddPoint(line.GetX(0), line.GetY(0), line.GetZ(0));
            //按照与起点的距离依次加入
            if (line.GetGeometryRef(0).Distance(crossLine.GetGeometryRef(0)) <= line.GetGeometryRef(0).Distance(crossLine.GetGeometryRef(1)))
            {
                updiffLine.AddPoint(crossLine.GetX(0), crossLine.GetY(0), crossLine.GetZ(0));
                updiffLine.AddPoint(crossLine.GetX(1), crossLine.GetY(1), crossLine.GetZ(1));
            }

            else
            {
                updiffLine.AddPoint(crossLine.GetX(1), crossLine.GetY(1), crossLine.GetZ(1));
                updiffLine.AddPoint(crossLine.GetX(0), crossLine.GetY(0), crossLine.GetZ(0));

            }


            updiffLine.AddPoint(line.GetX(1), line.GetY(1), line.GetZ(1));


            return crossLine;
        }

        /// <summary>
        /// 绕射路径计算
        /// </summary>
        /// <param name="sList">绕射点集合</param>
        /// <param name="line">直达声线</param>
        /// <returns></returns>
        private Geometry flankdiffLine(SortedList<double, Geometry> sList, Geometry line)
        {

            //若是路径点集中点数量大于0则存在绕射路径,构建路径
            if (sList.Count > 0)
            {
                //侧绕射路径
                Geometry updiffLine = new Geometry(wkbGeometryType.wkbLinearRing);
                foreach (KeyValuePair<double, Geometry> items in sList)
                {
                    updiffLine.AddPoint(items.Value.GetX(0), items.Value.GetY(0), items.Value.GetZ(0));
                }
                //路径闭合
                updiffLine.CloseRings();
                //取其凸壳与直达声线的差
                return updiffLine.ConvexHull().Difference(line);
            }
            else
            {
                //无绕射路径则返回空值
                return null;
            }
        }



        /// <summary>
        /// 反射声线
        /// </summary>
        /// <param name="buildingLayer">建筑物图层</param>
        /// <param name="sourcePoint">声源点</param>
        /// <param name="receivePoint">接收点</param>
        /// <param name="range">影响范围</param>
        /// <param name="reflectCount">反射次数</param>
        /// <returns></returns>
        private Geometry reflectLine(Layer buildingLayer, Geometry sourcePoint, Geometry receivePoint,double range,int reflectCount)
        {
            //确定影响范围
            buildingLayer.SetSpatialFilter(receivePoint.Buffer(range,30));

            /* 1.根据声源点筛选可达到的反射面
             * 2.分别做虚声源
             * 3.判断虚声源是否直达接收点
             * 4.若能则加入声源线
             * 5.若不能则以该点为声源点，重复1
             * 6.递归直到筛选出一条声源线
            */
















            return null;







        }



        #region 废弃代码

        private Layer barrierLayer;
        private Layer sourceLayer;
        private Geometry receivePoint;

        private Layer BarrierLayer
        {
            set { barrierLayer = value; }
        }



        private Layer SourceLayer
        {
            set { sourceLayer = value; }
        }


        private struct topBuilding
        {
            public double width;
            public double high;
            public Geometry topwidth;
            public Geometry tophigh;
        }
        private List<Geometry> direct(Layer buildings, Layer sources, Geometry receivePoint, double distance)
        {
            sources.ResetReading();
            double length = 0;
            Geometry receiveBuffer = receivePoint.Buffer(distance, 30);
            sources.SetSpatialFilter(receiveBuffer);

            Feature sFeature = null;
            Geometry sourcePoint = null;
            Geometry line = null;
            Queue<Feature> diffbuildings = new Queue<Feature>();


            List<Geometry> geos = new List<Geometry>();




            while ((sFeature = sources.GetNextFeature()) != null)
            {
                sourcePoint = sFeature.GetGeometryRef();
                line = GeometryCreate.createLineString3D(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0), receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));
                buildings.SetSpatialFilter(line);



                //  List<Geometry> a = getWidth(buildings, sourcePoint, receivePoint, line);

                List<Geometry> a = diffractionLine2(buildings, sourcePoint, receivePoint, line);
                if (a != null)
                // if (buildings.GetFeaturesRead()>0)
                {
                    // geos.AddRange(getWidth(buildings, sourcePoint, receivePoint, line));
                    geos.AddRange(a);

                }
                else
                {
                    //计算绕射
                    // geos.Add(line);
                    // length += line.Length();
                    // break;
                }
            }

            //Console.WriteLine(length);
            return geos;

            // return length;
        }


        private List<Geometry> getWidth(Layer buildingLayer, Geometry sourcePoint, Geometry receivePoint, Geometry line)
        {
            topBuilding st = new topBuilding();
            st.width = 0;
            st.high = 0;
            st.tophigh = null;
            st.topwidth = null;
            // buildingLayer.ResetReading();
            Feature bFeature = null;
            OSGeo.OGR.Envelope env = new OSGeo.OGR.Envelope();
            while ((bFeature = buildingLayer.GetNextFeature()) != null)
            {

                bFeature.GetGeometryRef().GetEnvelope(env);

                double width = Math.Sqrt((env.MaxX - env.MinX) * (env.MaxX - env.MinX) + (env.MaxY - env.MinY) * (env.MaxY - env.MinY));


                if (st.width < width)
                {
                    st.width = width;
                    st.topwidth = bFeature.GetGeometryRef();
                }


                if (st.high < bFeature.GetFieldAsDouble("HEIGHT_G"))
                {
                    st.high = bFeature.GetFieldAsDouble("HEIGHT_G");
                    st.tophigh = bFeature.GetGeometryRef();
                }
            }


            List<Geometry> list = null;
            //list.Add(line);



            if (st.topwidth != null)
            {
                list = new List<Geometry>();
                SortedDictionary<double, Geometry> upLinePoint = new SortedDictionary<double, Geometry>();
                SortedDictionary<double, Geometry> belowLinePoint = new SortedDictionary<double, Geometry>();
                double k = (sourcePoint.GetY(0) - receivePoint.GetY(0)) / (sourcePoint.GetX(0) - receivePoint.GetX(0));
                double b = sourcePoint.GetY(0) - k * sourcePoint.GetX(0);

                Geometry buildingring = new Geometry(wkbGeometryType.wkbLinearRing);
                st.topwidth.GetEnvelope(env);
                buildingring.AddPoint(env.MinX, env.MinY, 0);
                buildingring.AddPoint(env.MaxX, env.MinY, 0);

                buildingring.AddPoint(env.MaxX, env.MaxY, 0);
                buildingring.AddPoint(env.MinX, env.MaxY, 0);


                for (int i = 0; i < buildingring.GetPointCount(); i++)
                {
                    Geometry point = GeometryCreate.createPoint3D(buildingring.GetX(i), buildingring.GetY(i), buildingring.GetZ(i));
                    if (point.GetY(0) < k * point.GetX(0) + b)
                    {
                        belowLinePoint.Add(sourcePoint.Distance(point), point);
                    }
                    else if (buildingring.GetY(i) > k * buildingring.GetX(i) + b)
                    {
                        upLinePoint.Add(sourcePoint.Distance(point), point);
                    }
                }

                Geometry uoLine = new Geometry(wkbGeometryType.wkbLineString);
                Geometry belowLine = new Geometry(wkbGeometryType.wkbLineString);

                if (upLinePoint.Count > 0)
                {
                    uoLine.AddPoint(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0));
                    foreach (KeyValuePair<double, Geometry> item in upLinePoint)
                    {
                        uoLine.AddPoint(item.Value.GetX(0), item.Value.GetY(0), item.Value.GetZ(0));
                    }
                    uoLine.AddPoint(receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));
                }

                if (belowLinePoint.Count > 0)
                {
                    belowLine.AddPoint(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0));
                    foreach (KeyValuePair<double, Geometry> item in belowLinePoint)
                    {
                        belowLine.AddPoint(item.Value.GetX(0), item.Value.GetY(0), item.Value.GetZ(0));
                    }
                    belowLine.AddPoint(receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));
                }

                list.Add(uoLine);
                list.Add(belowLine);

            }



            return list;

        }


        private List<Geometry> getCrossLine(Geometry crossLine, Geometry line, Geometry sourcePoint, Geometry receivePoint)
        {
            List<Geometry> geos = new List<Geometry>();
            geos.Add(line);


            Geometry right = new Geometry(wkbGeometryType.wkbLineString);

            right.AddPoint(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0));
            right.AddPoint(crossLine.GetX(0), crossLine.GetY(0), crossLine.GetZ(0));
            right.AddPoint(receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));
            geos.Add(right);



            Geometry left = new Geometry(wkbGeometryType.wkbLineString);
            left.AddPoint(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0));
            left.AddPoint(crossLine.GetX(1), crossLine.GetY(1), crossLine.GetZ(1));
            left.AddPoint(receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));
            geos.Add(left);


            return geos;
        }



        private List<Geometry> diffractionx(Feature highf, Feature widthf, double width, Geometry sourcePoint, Geometry receivePoint, Geometry line, double distance)
        {

            List<Geometry> geos = new List<Geometry>();

            double high = highf.GetFieldAsDouble("B_HI");

            Geometry cross = highf.GetGeometryRef();
            cross.CloseRings();

            Geometry upline = new Geometry(wkbGeometryType.wkbLineString);

            upline.AddPoint(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0));


            upline.AddPoint(cross.Intersection(line).GetX(0), cross.Intersection(line).GetY(0), high);

            upline.AddPoint(cross.Intersection(line).GetX(1), cross.Intersection(line).GetY(1), high);

            upline.AddPoint(receivePoint.GetX(0), receivePoint.GetX(0), receivePoint.GetX(0));



            Geometry widthg = widthf.GetGeometryRef().Centroid();
            Geometry highg = highf.GetGeometryRef();


            Geometry left = new Geometry(wkbGeometryType.wkbLineString);

            left.AddPoint(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0));


            left.AddPoint(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0));



            left.AddPoint(receivePoint.GetX(0), receivePoint.GetX(0), receivePoint.GetX(0));

            Geometry right = new Geometry(wkbGeometryType.wkbLineString);

            geos.Add(upline);
            geos.Add(left);
            geos.Add(right);



            //  length = Math.Sqrt(sourcePoint.Distance(highgeotemp) * sourcePoint.Distance(highgeotemp) + (hightemp - sourcePoint.GetZ(0)) * (hightemp - sourcePoint.GetZ(0)))
            //     + Math.Sqrt(receivePoint.Distance(highgeotemp) * receivePoint.Distance(highgeotemp) + (hightemp - receivePoint.GetZ(0)) * (hightemp - receivePoint.GetZ(0)))
            //     + line.Length() - sourcePoint.Distance(highgeotemp) - receivePoint.Distance(highgeotemp);


            return geos;

            // return length;
        }






        /// <summary>
        /// 计算绕射路径长度
        /// </summary>
        /// <param name="buildingLayer">建筑物图层</param>
        /// <param name="sourcePoint">声源点</param>
        /// <param name="receivePoint">接收点</param>
        /// <param name="line">直达路径</param>
        /// <returns>绕射路径长度</returns>
        private List<Geometry> diffractionLine2(Layer buildingLayer, Geometry sourcePoint, Geometry receivePoint, Geometry line)
        {
            List<Geometry> diffLineList = new List<Geometry>();
            //绕射声线
            Geometry[] diffractionLine = new Geometry[3];


            Feature bFeature = null;

            //直达声线上的建筑物,并选取最高建筑物
            List<Geometry> buildingList = new List<Geometry>();
            double buildingheigh = 0;
            Geometry buildingtop = null;
            while ((bFeature = buildingLayer.GetNextFeature()) != null)
            {
                if (buildingheigh < bFeature.GetFieldAsDouble("HEIGHT_BU"))
                {
                    buildingtop = bFeature.GetGeometryRef();
                }
                buildingList.Add(bFeature.GetGeometryRef());
            }





            if (buildingList.Count > 0)
            {
                Geometry upLinePoint = new Geometry(wkbGeometryType.wkbMultiPoint);
                Geometry belowLinePoint = new Geometry(wkbGeometryType.wkbMultiPoint);

                double k = (sourcePoint.GetY(0) - receivePoint.GetY(0)) / (sourcePoint.GetX(0) - receivePoint.GetX(0));
                double b = sourcePoint.GetY(0) - k * sourcePoint.GetX(0);
                Geometry upLine = new Geometry(wkbGeometryType.wkbLinearRing);
                Geometry belowLine = new Geometry(wkbGeometryType.wkbLinearRing);

                foreach (Geometry item in buildingList)
                {
                    Geometry buildingring = item.Boundary();
                    for (int i = 0; i < buildingring.GetPointCount() - 1; i++)
                    {
                        Geometry pointb = GeometryCreate.createPoint3D(buildingring.GetX(i), buildingring.GetY(i), buildingring.GetZ(i));
                        if (pointb.GetY(0) < k * pointb.GetX(0) + b)
                        {
                            belowLinePoint.AddGeometry(pointb);
                        }
                        else if (buildingring.GetY(i) > k * buildingring.GetX(i) + b)
                        {
                            upLinePoint.AddGeometry(pointb);
                        }
                    }

                    SortedList<double, Geometry> sList = new SortedList<double, Geometry>();

                    if (upLinePoint.GetGeometryCount() > 0)
                    {
                        upLine.AddPoint(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0));

                        sList.Clear();
                        for (int i = 0; i < upLinePoint.GetGeometryCount(); i++)
                        {
                            sList.Add(sourcePoint.Distance(upLinePoint.GetGeometryRef(i)), upLinePoint.GetGeometryRef(i));
                        }

                        foreach (KeyValuePair<double, Geometry> items in sList)
                        {
                            upLine.AddPoint(items.Value.GetX(0), items.Value.GetY(0), items.Value.GetZ(0));
                        }

                        upLine.AddPoint(receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));

                        upLine.CloseRings();

                    }
                    if (belowLinePoint.GetGeometryCount() > 0)
                    {
                        belowLine.AddPoint(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0));


                        sList.Clear();
                        for (int i = 0; i < belowLinePoint.GetGeometryCount(); i++)
                        {
                            sList.Add(sourcePoint.Distance(belowLinePoint.GetGeometryRef(i)), belowLinePoint.GetGeometryRef(i));
                        }
                        foreach (KeyValuePair<double, Geometry> items in sList)
                        {
                            belowLine.AddPoint(items.Value.GetX(0), items.Value.GetY(0), items.Value.GetZ(0));
                        }

                        belowLine.AddPoint(receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));
                        belowLine.CloseRings();


                    }

                }




                Geometry area;
                Geometry diffline;
                Geometry convexHull;
                if (upLine.GetPointCount() > 3)
                {
                    diffline = new Geometry(wkbGeometryType.wkbLineString);
                    area = new Geometry(wkbGeometryType.wkbPolygon);

                    area.AddGeometry(upLine);
                    convexHull = area.ConvexHull().Boundary();
                    for (int i = 0; i < convexHull.GetPointCount(); i++)
                    {
                        diffline.AddPoint(convexHull.GetX(i), convexHull.GetY(i), convexHull.GetZ(i));
                    }




                    diffLineList.Add(diffline.Difference(line));

                }

                if (belowLine.GetPointCount() > 3)
                {
                    area = new Geometry(wkbGeometryType.wkbPolygon);
                    area.AddGeometry(belowLine);
                    convexHull = area.ConvexHull().Boundary();
                    diffline = new Geometry(wkbGeometryType.wkbLineString);


                    for (int i = 0; i < convexHull.GetPointCount(); i++)
                    {
                        diffline.AddPoint(convexHull.GetX(i), convexHull.GetY(i), convexHull.GetZ(i));
                    }





                    diffLineList.Add(diffline.Difference(line));

                }
            }
            return diffLineList;

        }

        #endregion


    }
}
