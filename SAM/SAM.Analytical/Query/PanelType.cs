using SAM.Architectural;
using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static PanelType PanelType(this object @object)
        {
            if (@object == null)
                return Analytical.PanelType.Undefined;
            
            if (@object is PanelType)
                return (PanelType)@object;

            if (@object is Geometry.Spatial.Vector3D)
                return PanelType((Geometry.Spatial.Vector3D)@object);

            if (@object is Panel)
                return ((Panel)@object).PanelType;

            PanelType result;

            if (@object is Construction)
            {
                string text;
                if (((Construction)@object).TryGetValue(ConstructionParameter.DefaultPanelType, out text) && !string.IsNullOrWhiteSpace(text))
                {
                    result = PanelType(text, false);
                    return result;
                }

                return Analytical.PanelType.Undefined;
            }
                

            if (@object is ApertureConstruction)
            {
                string text;
                if (((ApertureConstruction)@object).TryGetValue(ApertureConstructionParameter.DefaultPanelType, out text) && !string.IsNullOrWhiteSpace(text))
                {
                    result = PanelType(text, false);
                    return result;
                }

                return Analytical.PanelType.Undefined;
            }

            if (@object is string)
            {
                string value = (string)@object;

                if (Enum.TryParse(value, out result))
                    return result;

                value = value.Replace(" ", string.Empty).ToUpper();
                foreach (PanelType panelType in Enum.GetValues(typeof(PanelType)))
                {
                    string value_Type = panelType.ToString().ToUpper();
                    if (value_Type.Equals(value))
                        return panelType;

                    value_Type = Text(panelType)?.Replace(" ", string.Empty).ToUpper();
                    if (!string.IsNullOrEmpty(value_Type) && value_Type.Equals(value))
                        return panelType;
                }

                return Analytical.PanelType.Undefined;
            }

            if (@object is int)
                return (PanelType)(int)(@object);

            return Analytical.PanelType.Undefined;
        }

        public static PanelType PanelType(Geometry.Spatial.Vector3D normal, double tolerance = Core.Tolerance.Angle)
        {
            if (normal == null)
                return Analytical.PanelType.Undefined;

            double value = normal.Unit.DotProduct(Geometry.Spatial.Vector3D.WorldZ);
            if (System.Math.Abs(value) <= tolerance)
                return Analytical.PanelType.Wall;

            if (value < 0)
                return Analytical.PanelType.Floor;

            return Analytical.PanelType.Roof;
        }

        public static PanelType PanelType(string text, bool includeDefaultConstructionNames = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Analytical.PanelType.Undefined;

            PanelType result = Analytical.PanelType.Undefined;
            if (Enum.TryParse(text, out result))
                return result;

            foreach (PanelType panelType in Enum.GetValues(typeof(PanelType)))
            {
                string value = null;

                if(includeDefaultConstructionNames)
                {
                    value = DefaultConstruction(panelType)?.Name;
                    if (text.Equals(value))
                        return panelType;
                }

                value = Text(panelType);
                if (text.Equals(value))
                    return panelType;
            }

            return Analytical.PanelType.Undefined;
        }

        public static PanelType? PanelType(this BuildingModel buildingModel, IPartition partition)
        {
            if (partition is AirPartition)
            {
                return Analytical.PanelType.Air;
            }

            if (buildingModel == null || partition == null)
            {
                return null;
            }

            if (buildingModel.Shade(partition))
            {
                return Analytical.PanelType.Shade;
            }

            if (partition is IHostPartition)
            {
                IHostPartition hostPartition = partition as IHostPartition;
                if(hostPartition is Wall)
                {
                    Wall wall = (Wall)hostPartition;

                    if (buildingModel.Transparent(wall))
                    {
                        return Analytical.PanelType.CurtainWall;
                    }

                    if (buildingModel.Internal(wall))
                    {
                        return Analytical.PanelType.WallInternal;
                    }

                    if (buildingModel.External(wall))
                    {
                        ITerrain terrain = buildingModel.Terrain;
                        if (terrain != null)
                        {
                            if (terrain.Below(wall) || terrain.On(wall))
                            {
                                return Analytical.PanelType.UndergroundWall;
                            }
                        }

                        return Analytical.PanelType.WallExternal;
                    }

                    return Analytical.PanelType.Wall;
                }

                if(hostPartition is Floor)
                {
                    Floor floor = (Floor)hostPartition;

                    ITerrain terrain = buildingModel.Terrain;
                    if(terrain != null)
                    {
                        if(terrain.On(floor))
                        {
                            return Analytical.PanelType.SlabOnGrade;
                        }

                        if (terrain.Below(floor))
                        {
                            return Analytical.PanelType.UndergroundSlab;
                        }
                    }

                    if (buildingModel.Internal(floor))
                    {
                        return Analytical.PanelType.FloorInternal;
                    }

                    if (buildingModel.External(floor))
                    {
                        return Analytical.PanelType.FloorExposed;
                    }

                    return Analytical.PanelType.Floor;
                }

                if(hostPartition is Roof)
                {
                    Roof roof = (Roof)hostPartition;

                    return Analytical.PanelType.Roof;
                }
            }

            return null;
        }
    }
}