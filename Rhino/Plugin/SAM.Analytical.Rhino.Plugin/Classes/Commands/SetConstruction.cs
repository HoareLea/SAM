//using Rhino;
//using Rhino.Commands;
//using Rhino.DocObjects;
//using Rhino.Geometry;
//using Rhino.Input;
//using SAM.Core;
//using System.Collections.Generic;
//using System.Linq;

//namespace SAM.Analytical.Rhino.Plugin
//{
//    public class SetConstruction : global::Rhino.Commands.Command
//    {
//        public SetConstruction()
//        {
//            // Rhino only creates one instance of each command class defined in a
//            // plug-in, so it is safe to store a refence in a static property.
//            Instance = this;
//        }

//        ///<summary>The only instance of this command.</summary>
//        public static SetConstruction Instance { get; private set; }

//        ///<returns>The command name as it appears on the Rhino command line.</returns>
//        public override string EnglishName => "SAM_SetConstruction";

//        protected override global::Rhino.Commands.Result RunCommand(RhinoDoc doc, RunMode mode)
//        {
//            global::Rhino.Commands.Result result = RhinoGet.GetMultipleObjects("Select Panels", false, ObjectType.Brep, out ObjRef[] obj_refs);
//            if (result != global::Rhino.Commands.Result.Success || obj_refs == null)
//                return result;

//            MaterialLibrary materialLibrary = Query.DefaultMaterialLibrary();

//            ConstructionLibrary constructionLibrary = Query.DefaultConstructionLibrary();

//            Construction construction = null;
//            using (Windows.Forms.ConstructionLibraryForm constructionLibraryForm = new Windows.Forms.ConstructionLibraryForm(materialLibrary, constructionLibrary))
//            {
//                if (constructionLibraryForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
//                {
//                    return global::Rhino.Commands.Result.Cancel;
//                }

//                construction = constructionLibraryForm.GetConstructions()?.FirstOrDefault();
//                if(construction != null)
//                {
//                    ConstructionLibrary constructionLibrary_New = constructionLibraryForm.ConstructionLibrary;
//                    if(constructionLibrary_New != null)
//                    {
//                        Core.Convert.ToFile(new IJSAMObject[] { constructionLibrary_New }, Query.DefaultConstructionLibraryPath());
//                    }
//                }
//            }

//            if (construction == null)
//            {
//                return global::Rhino.Commands.Result.Failure;
//            }

//            foreach (var obj_ref in obj_refs)
//            {
//                Brep brep = obj_ref.Brep();
//                if (brep != null)
//                {
//                    string @string = null;
//                    if (brep.HasUserData)
//                    {
//                        @string = brep.GetUserString("SAM");
//                        if (!string.IsNullOrWhiteSpace(@string))
//                        {
//                            List<Panel> panels_Temp = Core.Convert.ToSAM<Panel>(@string);
//                            if (panels_Temp != null && panels_Temp.Count != 0)
//                            {
//                                for (int i = 0; i < panels_Temp.Count; i++)
//                                {
//                                    panels_Temp[i] = Create.Panel(panels_Temp[i], construction);
//                                }
//                            }

//                            @string = Core.Convert.ToString(panels_Temp);
//                        }
//                    }

//                    if (string.IsNullOrWhiteSpace(@string))
//                    {
//                        List<Geometry.Spatial.ISAMGeometry3D> geometries = Geometry.Rhino.Convert.ToSAM(brep);
//                        if (geometries != null && geometries.Count != 0)
//                        {
//                            foreach (Geometry.Spatial.ISAMGeometry3D geometry in geometries)
//                            {
//                                List<Geometry.Spatial.Face3D> face3Ds = new List<Geometry.Spatial.Face3D>();

//                                if (geometry is Geometry.Spatial.Face3D)
//                                {
//                                    face3Ds.Add((Geometry.Spatial.Face3D)geometry);
//                                }
//                                else if (geometry is Geometry.Spatial.Shell)
//                                {
//                                    face3Ds.AddRange(((Geometry.Spatial.Shell)geometry).Face3Ds);
//                                }

//                                if (face3Ds == null || face3Ds.Count == 0)
//                                {
//                                    continue;
//                                }

//                                List<Panel> panels_Temp = new List<Panel>();
//                                foreach (Geometry.Spatial.Face3D face3D in face3Ds)
//                                {
//                                    if (face3D == null)
//                                    {
//                                        continue;
//                                    }

//                                    Panel panel = Create.Panel(construction, face3D.GetPlane().Normal.PanelType(), face3D);
//                                    if (panel == null)
//                                    {
//                                        continue;
//                                    }

//                                    panels_Temp.Add(panel);
//                                }

//                                if (panels_Temp != null && panels_Temp.Count != 0)
//                                {
//                                    @string = Core.Convert.ToString(panels_Temp);
//                                }
//                            }
//                        }
//                    }

//                    brep.SetUserString("SAM", @string);
//                }
//            }

//            return global::Rhino.Commands.Result.Success;
//        }
//    }
//}
