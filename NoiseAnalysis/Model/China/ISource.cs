using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;

namespace NoiseAnalysis.Model.China
{
    internal interface ISource
    {
        public Layer getSource(Layer sourceLayer, Layer resultLayer, float splitLength, String timeType, int frequency);

    }
}
