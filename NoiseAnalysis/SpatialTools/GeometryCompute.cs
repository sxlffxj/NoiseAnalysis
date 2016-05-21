using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.OGR;


namespace NoiseAnalysis.SpatialTools
{
  static  class GeometryCompute
    {
      public static Geometry getCentre(Geometry lineString)
        {
            Envelope env = new Envelope();
            lineString.GetEnvelope(env);
            double x = env.MinX + env.MaxX;
            double y = env.MinY + env.MaxY;
            return GeometryCreate.createPoint(x/2,y/2);

        }
    }
}
