using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace hsbSoft.Revit
{
  public class WallSelectionFilter : ISelectionFilter
  {
    public bool AllowElement( Element e )
    {
      return e is Wall;
    }

    public bool AllowReference( Reference r, XYZ p )
    {
      return true;
    }
  }
}