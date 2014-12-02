using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace hsbSoft.Revit
{
  [Transaction( TransactionMode.Manual )]
  public class CmdGeometryParts : IExternalCommand
  {
    public Result Execute( 
      ExternalCommandData cmdData, 
      ref string msg, 
      ElementSet elems )
    {
      Result result = Result.Failed;

      UIApplication uiApp = cmdData.Application;
      UIDocument uiDoc = uiApp.ActiveUIDocument;
      Document doc = uiDoc.Document;
      
      Transaction transaction = new Transaction( doc );

      try
      {
        string strMsg = "Select walls";

        ISelectionFilter filter 
          = new WallSelectionFilter();

        IList<Reference> walls 
          = uiDoc.Selection.PickObjects( 
            ObjectType.Element, filter, strMsg );

        if( walls.Count == 0 ) { return result; }

        List<ElementId> ids = new List<ElementId>();

        foreach( Reference reference in walls )
          ids.Add( reference.ElementId );

        if( !PartUtils.AreElementsValidForCreateParts(
          doc, ids ) )
        {
          return result;
        }

        transaction.Start( "parts" );
        
        // Split walls into parts

        PartUtils.CreateParts( doc, ids );

        // Regenerate document to get the part geometry

        doc.Regenerate();

        // Retrieve points from bottom faces of parts

        List<List<XYZ>> bottomFacesPts 
          = new List<List<XYZ>>();

        foreach( ElementId id in ids )
        {
          if( !PartUtils.HasAssociatedParts( doc, id ) )
          { 
            continue; 
          }

          ICollection<ElementId> partIds 
            = PartUtils.GetAssociatedParts( 
              doc, id, true, true );

          foreach( ElementId partId in partIds )
          {
            Element part = doc.GetElement( partId );

            bottomFacesPts.Add( 
              GetBottomFacePoints( part ) );
          }
        }

        // Do not affect the original state of walls

        transaction.RollBack();

        // Draw lines to show the bottom faces of parts

        transaction.Start();

        ModelLineCreator model 
          = new ModelLineCreator( doc );

        foreach( List<XYZ> bottomFacePts in 
          bottomFacesPts )
        {
          for( int i = 1; i < bottomFacePts.Count; ++i )
          {
            model.CreateLine( bottomFacePts[i - 1], 
              bottomFacePts[i], true );
          }

          if( bottomFacePts.Count > 3 )
          {
            model.CreateLine( bottomFacePts[0], 
              bottomFacePts[bottomFacePts.Count - 1], 
              true );
          }
        }
        transaction.Commit();

        result = Result.Succeeded;
      }
      catch( System.Exception e )
      {
        msg = e.Message;
        result = Result.Failed;
      }
      return result;
    }

    /// <summary>
    /// Return a list of points representing 
    /// the bottom face of the given Revit element.
    /// </summary>
    public List<XYZ> GetBottomFacePoints( Element e )
    {
      List<XYZ> resultingPts = new List<XYZ>();

      FaceExtractor faceExtractor 
        = new FaceExtractor( e );

      FaceArray faces = faceExtractor.Faces;

      if( faces.Size == 0 ) { return resultingPts; }

      foreach( Face face in faces )
      {
        PlanarFace pf = face as PlanarFace;
        
        if( pf == null ) { continue; }

        if( pf.Normal.IsAlmostEqualTo( -XYZ.BasisZ ) )
        {
          EdgeArrayArray edgeLoops = face.EdgeLoops;

          foreach( EdgeArray edgeArray in edgeLoops )
          {
            foreach( Edge edge in edgeArray )
            {
              List<XYZ> points 
                = edge.Tessellate() as List<XYZ>;

              resultingPts.AddRange( points );
            }
          }
        }
      }
      return resultingPts;
    }
  }
}
