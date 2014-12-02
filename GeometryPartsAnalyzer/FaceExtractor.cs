using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace hsbSoft.Revit
{
  public class FaceExtractor
  {
    private Element _element;
    private Document _doc;
    private Application _app;
    private Autodesk.Revit.Creation.Application _appCreator;
    private Transform _matrix;
    private FaceArray _faces;

    /// <summary>
    /// Gets the transform for element
    /// </summary>
    public Transform Matrix
    {
      get { return _matrix; }
    }

    /// <summary>
    /// Gets faces
    /// </summary>
    public FaceArray Faces
    {
      get { return _faces; }
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="element">element</param>
    public FaceExtractor( Element element )
    {
      _element = element;
      _doc = element.Document;
      _app = _doc.Application;
      _appCreator = _app.Create;
      GetFaces();
    }

    /// <summary>
    /// Retrieve all faces from the 
    /// given element's geometry solid.
    /// </summary>
    private void GetFaces()
    {
      Options geoOptions = _appCreator.NewGeometryOptions();
      geoOptions.ComputeReferences = true;
      GeometryElement geoElem = _element.get_Geometry( geoOptions );
      _faces = GetFacesFrom( geoElem );
    }

    /// <summary>
    /// Retrieve all faces from the first solid 
    /// encountered in the given geometry element.
    /// </summary>
    /// <param name="geoElement">geometry element</param>
    /// <returns>faces</returns>
    public FaceArray GetFacesFrom( GeometryElement geoElement )
    {
      GeometryObjectArray geoElems = geoElement.Objects;

      foreach( object o in geoElems )
      {
        Solid geoSolid = o as Solid;
        if( null == geoSolid )
        {
          GeometryInstance instance = o as GeometryInstance;
          if( null == instance )
            continue;
          GeometryElement geoElement2 = instance.SymbolGeometry;
          _matrix = instance.Transform;
          GeometryObjectArray geoElems2 = geoElement2.Objects;
          if( geoElems2 == null )
            continue;
          if( geoElems2.Size == 0 )
            continue;
          return GetFacesFrom( geoElement2 );
        }
        FaceArray faces = geoSolid.Faces;
        if( faces == null )
          continue;
        if( faces.Size == 0 )
          continue;
        return geoSolid.Faces;
      }
      return null;
    }
  }
}
