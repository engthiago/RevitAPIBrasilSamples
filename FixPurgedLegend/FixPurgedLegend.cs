using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

namespace FixPurgedLegend
{
    [Transaction(TransactionMode.Manual)]
    internal class FixPurgedLegend : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try 
			{
				UIApplication uiapp = commandData.Application;
				Document doc_source = uiapp.ActiveUIDocument.Document;
				Document doc_destiny = uiapp.Application.Documents.Cast<Document>().First(a=>a.Title!=doc_source.Title);
				CopyPasteOptions op = new CopyPasteOptions();
				FilteredElementCollector col = new FilteredElementCollector(doc_source);
				col = col.WhereElementIsNotElementType().OfClass(typeof(View));
				ElementId id = col.First(a=> ((View)a).ViewType == ViewType.Legend).Id;
				ICollection<ElementId> list_ids = new List<ElementId>();
				list_ids.Add(id);
				Transaction t = new Transaction(doc_destiny,"FixPurgedLegend");
				t.Start();
				ElementTransformUtils.CopyElements(doc_source,list_ids,doc_destiny,null,op);
				t.Commit();
			} 
			catch{}
        }
    }
}
