using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace hsbSoft.Revit
{
  public class ModelLineCreator
  {
    Document _doc;

    public ModelLineCreator( Document doc )
    {
      _doc = doc;
    }

    /// <summary>
    /// Create the ModelLine
    /// </summary>
    public ModelLine CreateLine( 
      XYZ startPoint, 
      XYZ endPoint, 
      bool bound )
    {
      if( startPoint.DistanceTo( endPoint ) < 1.0e-9 ) 
        return null;

      // Create sketch plane; for non-vertical lines,
      // use Z-axis to span the plane, otherwise Y-axis:

      XYZ v = endPoint - startPoint;

      double dxy = Math.Abs( v.X ) + Math.Abs( v.Y );

      XYZ w = ( dxy > 1.0e-9 )
        ? XYZ.BasisZ
        : XYZ.BasisY;

      XYZ norm = v.CrossProduct( w ).Normalize();

      Plane plane = new Plane( norm, startPoint );

      SketchPlane sketchPlane = SketchPlane.Create( 
        _doc, plane );

      Line line = bound
        ? Line.CreateBound( startPoint, endPoint )
        : Line.CreateUnbound( startPoint, endPoint );

      return _doc.Create.NewModelCurve( 
        line, sketchPlane ) as ModelLine;
    }
  }
}
