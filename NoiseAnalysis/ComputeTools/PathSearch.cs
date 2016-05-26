using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using NoiseAnalysis.SpatialTools;
using System.Xml;


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


    public struct map
        {
         public double width;
         public Geometry geo;
        }


















        public List<Geometry> direct(Layer buildings, Layer sources, Geometry receivePoint, double distance)
        {
            sources.ResetReading();
            double length = 0;
            Geometry receiveBuffer = receivePoint.Buffer(distance, 30);
            sources.SetSpatialFilter(receiveBuffer);

            Feature sFeature = null;
            Feature bFeature = null;
            Geometry sourcePoint = null;
            Geometry line = null;
            Queue<Feature> diffbuildings = new Queue<Feature>();


            List<Geometry> geos = new List<Geometry>();

            double hightemp = 0;

            Geometry highgeotemp = null;
            double width = 0;

            Geometry crossLine = null;
            map st = new map();


          



            while ((sFeature = sources.GetNextFeature()) != null)
            {


                sourcePoint = sFeature.GetGeometryRef();
                line = GeometryCreate.createLineString3D(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0), receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));
                buildings.SetSpatialFilter(line);
                // Console.WriteLine(buildings.GetFeatureCount(0));
                bool isIntersect = false;
                buildings.ResetReading();







               
                while ((bFeature = buildings.GetNextFeature()) != null)
                {
                    //判断判断传播线是否与建筑物相交，如果相交，则进行绕射计算
                    if (bFeature.GetGeometryRef().Intersect(line))
                    {
                        // 计算绕射
                        isIntersect = true;
                        if (hightemp < bFeature.GetFieldAsDouble("B_HI"))
                        {
                            hightemp = bFeature.GetFieldAsDouble("B_HI");
                            highgeotemp = bFeature.GetGeometryRef();
                        }
           
                        //获取最大宽度和对应的宽度线
                      st=  getWidth(bFeature.GetGeometryRef(), line, width, crossLine);
                    }
                }

                if (!isIntersect)
                {
                   // geos.Add(line);
                    // length += line.Length();
                }
                else
                {
                    //计算绕射
                    geos.AddRange(getCrossLine(st.geo, line, sourcePoint, receivePoint));
                }
               

            }

            //Console.WriteLine(length);
            return geos;

            // return length;
        }


        public map getWidth(Geometry building, Geometry line,double width,Geometry crossLine)
        {
            Geometry buildingring = building.GetGeometryRef(0);
            Geometry intersectLine = null;
            map st = new map();
            for (int i = 0; i < buildingring.GetPointCount() - 1; i++)
            {
                intersectLine = new Geometry(wkbGeometryType.wkbLineString);

                intersectLine.AddPoint(buildingring.GetX(i), buildingring.GetY(i), buildingring.GetZ(i));
                intersectLine.AddPoint(buildingring.GetX(i + 1), buildingring.GetY(i + 1), buildingring.GetZ(i + 1));

                if (intersectLine.Intersect(line) && width < intersectLine.Length())
                {
                    st.width = intersectLine.Length();
                   
                    st.geo = intersectLine;
                }
            }
            return st;
        }

        public Geometry getDirect(Geometry building, Geometry line, Geometry sourcePoint, Geometry receivePoint,double high)
        {
            Geometry crossLine = line.Intersection(building);

            Geometry up = new Geometry(wkbGeometryType.wkbLineString);












            return up;
        }











        public List<Geometry> getCrossLine(Geometry crossLine, Geometry line, Geometry sourcePoint, Geometry receivePoint)
        {

            String a = "";
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

















        public List<Geometry> diffraction(Feature highf,Feature widthf,double width, Geometry sourcePoint, Geometry receivePoint, Geometry line, double distance)
        {

            List<Geometry> geos = new List<Geometry>();

            double high=highf.GetFieldAsDouble("B_HI");

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








    }
}
