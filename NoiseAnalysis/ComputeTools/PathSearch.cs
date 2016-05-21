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
            Geometry sourceBuffer = null;
            double width=0;


            List<Geometry> geos = new List<Geometry>();
            while ((sFeature = sources.GetNextFeature()) != null)
            {
        

                sourcePoint = sFeature.GetGeometryRef();
                line = GeometryCreate.createLineString3D(sourcePoint.GetX(0), sourcePoint.GetY(0), sourcePoint.GetZ(0), receivePoint.GetX(0), receivePoint.GetY(0), receivePoint.GetZ(0));
                width =Math.Sqrt(Math.Pow(0.5 * distance,2) - Math.Pow(0.5 * line.Length(),2));
                
                bool isIntersect = false;
                buildings.ResetReading();


                sourceBuffer = line.Buffer(width, 0);


                buildings.SetSpatialFilter(sourceBuffer);
                while ((bFeature = buildings.GetNextFeature()) != null)
                {
                    //判断判断传播线是否与建筑物相交，如果相交，则进行绕射计算
                        if (bFeature.GetGeometryRef().Intersects(line))
                        {
                            isIntersect = true;
                            break;
                        }
                }
                if (isIntersect==false)
                {
                    geos.Add(line);
                    //length += line.Length();
                }
            }
            return geos;
           // return length;
        }




        public double diffraction(Geometry barrier, Geometry sourcePoint, Geometry receivePoint,Geometry line,double distance)
        {
   
        //获取影响范围


            Geometry upLine = line.Intersection(barrier);

            double length = Math.Sqrt(sourcePoint.Distance(upLine) * sourcePoint.Distance(upLine) + (upLine.GetZ(0) - sourcePoint.GetZ(0)) * (upLine.GetZ(0) - sourcePoint.GetZ(0)))
                + Math.Sqrt(receivePoint.Distance(upLine) * receivePoint.Distance(upLine) + (upLine.GetZ(0) - receivePoint.GetZ(0)) * (upLine.GetZ(0) - receivePoint.GetZ(0)))
                + upLine.Length();

            double areaWidth = Math.Sqrt(0.25 * distance * distance - 0.25 * line.Length()*line.Length());

            //获取影响范围内建筑物
            Geometry area = line.Buffer(areaWidth,0);
            barrierLayer.SetSpatialFilter(area);

            Feature barrierFearture = null;
            while ((barrierFearture = barrierLayer.GetNextFeature())!=null)
            {









            }







            return length;
        }








    }
}
