using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Mechanical;

namespace InsulateDucts
{
    [Transaction(TransactionMode.Manual)]
    class InsulateDucts : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Gets all insulation types id in current document
            IList<ElementId> insTypeFilt = new FilteredElementCollector(doc).OfClass(typeof(DuctInsulationType)).ToElementIds().ToList();

            //If no types were found return failed
            if (insTypeFilt == null || insTypeFilt.Count == 0)
            {
                message = "No insulation type were found in current document.";
                return Result.Failed;
            }

            //Gets the first insulation type
            ElementId insTypeId = insTypeFilt.FirstOrDefault();


            //Gets all Rigid Ducts in current document
            IList<Element> ductsInDoc = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctCurves)
                .OfClass(typeof(Duct)).WhereElementIsNotElementType().ToElements();

            int prevAppliedIns = 0;
            int appliedIns = 0;

            //Open the Revit database for editing
            using (Transaction t = new Transaction(doc, "Create Duct Insulation"))
            {
                t.Start();
                //Loops through all ducts in the document and apply insulation to ducts that don't have it applied
                foreach (Element currentDuctElement in ductsInDoc)
                {
                    if (InsulationLiningBase.GetInsulationIds(doc, currentDuctElement.Id).Count == 0)
                    {
                        DuctInsulation.Create(doc, currentDuctElement.Id, insTypeId, 0.2);
                        appliedIns++;
                    }
                    else
                        prevAppliedIns++;       
                }
                t.Commit();
            }

            //Results message
            string finalMessage = "";
            if (appliedIns > 0)
                finalMessage = "Duct(s) insulated: " + appliedIns + ".";
            else
                finalMessage = "No ducts were insulated!";
            
            if (prevAppliedIns > 0)
                finalMessage += "\nDuct(s) insulated before running the command: " + prevAppliedIns + ".";
            
            TaskDialog.Show("Done!", finalMessage);


            return Result.Succeeded;
        }
    }
}