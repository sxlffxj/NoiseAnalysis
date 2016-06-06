using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using NoiseAnalysis.SpatialTools;
using System.Xml;
using NoiseAnalysis.Algorithm;
using GeoAPI.Geometries;


namespace NoiseAnalysis.ComputeTools
{
    class PathSearch
    {

        private Layer barrierLayer;
        private Layer sourceLayer;
        private Geometry receivePoint;

        public Layer BarrierLayer
        {
            set { barrierLayer = value; }
        }



        public Layer SourceLayer
        {
            set { sourceLayer = value; }
        }


        public struct topBuilding
        {
            public double width;
            public double high;
            public Geometry topwidth;
            public Geometry tophigh;
        }


















        public List<Geometry> direct(Layer buildings, Layer sources, Geometry receivePoint, double distance)
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



                List<Geometry> a = getWidth(buildings, sourcePoint, receivePoint, line);

           
               if (a!=null)
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


        public List<Geometry> getWidth(Layer buildingLayer, Geometry sourcePoint, Geometry receivePoint, Geometry line)
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


            List<Geometry> list = null ;
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


        public List<Geometry> getCrossLine(Geometry crossLine, Geometry line, Geometry sourcePoint, Geometry receivePoint)
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



        public List<Geometry> diffraction(Feature highf, Feature widthf, double width, Geometry sourcePoint, Geometry receivePoint, Geometry line, double distance)
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


        public List<Geometry> diffractionLine2(Layer buildingLayer, Geometry sourcePoint, Geometry receivePoint)
        {

            Geometry upLinePoint = new Geometry(wkbGeometryType.wkbMultiPoint);
            Geometry belowLinePoint = new Geometry(wkbGeometryType.wkbMultiPoint);

            double k = (sourcePoint.GetY(0) - receivePoint.GetY(0)) / (sourcePoint.GetX(0) - receivePoint.GetX(0));
            double b = sourcePoint.GetY(0) - k * sourcePoint.GetX(0);

            Feature bFeature = null;
            Geometry uoLine = new Geometry(wkbGeometryType.wkbLineString);
            Geometry belowLine = new Geometry(wkbGeometryType.wkbLineString);
            uoLine.AddPoint(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0));
            belowLine.AddPoint(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0));

            Geometry point;

            SortedList<double, Geometry> buildingList = new SortedList<double, Geometry>();
            while ((bFeature = buildingLayer.GetNextFeature()) != null)
            {
                buildingList.Add(sourcePoint.Distance(bFeature.GetGeometryRef()), bFeature.GetGeometryRef());
            }

            foreach (KeyValuePair<double, Geometry> item in buildingList)
            {
                Geometry buildingring = item.Value.Boundary();
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

                if (upLinePoint.GetGeometryCount() > 1)
                {

                    point = sourcePoint;

                    while (point.Distance(receivePoint) != receivePoint.Distance(upLinePoint))
                    {
                        sList.Clear();
                        for (int i = 0; i < upLinePoint.GetGeometryCount(); i++)
                        {
                            sList.Add(point.Distance(upLinePoint.GetGeometryRef(i)), upLinePoint.GetGeometryRef(i));
                        }
                        if (sList.Count != 0)
                        {
                            upLinePoint = upLinePoint.Difference(point);
                            point = sList.ElementAt(0).Value;
                            uoLine.AddPoint(point.GetX(0), point.GetY(0), point.GetZ(0));
                        }
                        else
                        {
                            point = upLinePoint;
                            uoLine.AddPoint(point.GetX(0), point.GetY(0), point.GetZ(0));
                        }
                    }

                }
                if (belowLinePoint.GetGeometryCount() > 1)
                {

                    point = sourcePoint;

                    while (point.Distance(receivePoint) != receivePoint.Distance(belowLinePoint))
                    {

                        sList.Clear();
                        for (int i = 0; i < belowLinePoint.GetGeometryCount(); i++)
                        {
                            sList.Add(point.Distance(belowLinePoint.GetGeometryRef(i)), belowLinePoint.GetGeometryRef(i));
                        }

                        if (sList.Count != 0)
                        {
                            point = sList.ElementAt(0).Value;
                            belowLinePoint = belowLinePoint.Difference(point);
                            belowLine.AddPoint(point.GetX(0), point.GetY(0), point.GetZ(0));
                        }
                        else
                        {
                            point = belowLinePoint;
                            belowLine.AddPoint(point.GetX(0), point.GetY(0), point.GetZ(0));
                        }

                    }


                }
            }
            uoLine.AddPoint(receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));
            belowLine.AddPoint(receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));
            List<Geometry> diffLineList = new List<Geometry>();
            if (uoLine.GetPointCount() > 0)
            {
                diffLineList.Add(uoLine);
            }

            if (belowLine.GetPointCount() > 0)
            {
                diffLineList.Add(belowLine);
            }
            return diffLineList;

        }

        public List<Geometry> diffractionLine(Layer buildingLayer, Geometry sourcePoint, Geometry receivePoint)
        {
            SortedDictionary<double, Geometry> upLinePoint = new SortedDictionary<double, Geometry>();
            SortedDictionary<double, Geometry> belowLinePoint = new SortedDictionary<double, Geometry>();

            double k = (sourcePoint.GetY(0) - receivePoint.GetY(0)) / (sourcePoint.GetX(0) - receivePoint.GetX(0));
            double b = sourcePoint.GetY(0) - k * sourcePoint.GetX(0);

            Feature bFeature = null;
            try
            {
                while ((bFeature = buildingLayer.GetNextFeature()) != null)
                {

                    Geometry buildingring = bFeature.GetGeometryRef().ConvexHull().GetGeometryRef(0);


                    for (int i = 0; i < buildingring.GetPointCount() - 1; i++)
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
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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



            List<Geometry> diffLineList = new List<Geometry>();
            if (uoLine.GetPointCount() > 0)
            {
                diffLineList.Add(uoLine);
            }

            if (belowLine.GetPointCount() > 0)
            {
                diffLineList.Add(belowLine);
            }




            return diffLineList;

        }






    }
}
