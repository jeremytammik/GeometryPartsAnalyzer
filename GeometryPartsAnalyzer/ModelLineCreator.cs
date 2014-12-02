using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace hsbSoft.Revit
{
  public class ModelLineCreator
  {
    Document _doc;
    //private Application _app;
    //private Autodesk.Revit.Creation.Application _createApp;
    //private Autodesk.Revit.Creation.Document _createDoc;

    public ModelLineCreator( Document doc )
    {
      _doc = doc;
      //_app = doc.Application;
      //_createApp = _app.Create;
      //_createDoc = doc.Create;
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

      //Plane plane = _createApp.NewPlane( norm, startPoint );
      Plane plane = new Plane( norm, startPoint );

      //SketchPlane sketchPlane = SketchPlane.Create( _createDoc, plane );
      SketchPlane sketchPlane = SketchPlane.Create( 
        _doc, plane );

      //_createApp.NewLine( startPoint, endPoint, bound ),
      Line line = bound
        ? Line.CreateBound( startPoint, endPoint )
        : Line.CreateUnbound( startPoint, endPoint );

      return _doc.Create.NewModelCurve( 
        line, sketchPlane ) as ModelLine;
    }
  }
}
