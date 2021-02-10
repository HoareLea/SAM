using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class GooDelimitedFileTable: GH_Goo<DelimitedFileTable>
    {
        public GooDelimitedFileTable()
            : base()
        {
        }

        public GooDelimitedFileTable(DelimitedFileTable delimitedFileTable)
            : base(delimitedFileTable)
        {
        }

        public override bool IsValid => Value != null;

        public override string TypeName
        {
            get
            {
                Type type = null;

                if (Value == null)
                    type = typeof(DelimitedFileTable);
                else
                    type = Value.GetType();

                if (type == null)
                    return null;

                return type.Name;
            }
        }

        public override string TypeDescription
        {
            get
            {
                Type type = null;

                if (Value == null)
                    type = typeof(DelimitedFileTable);
                else
                    type = Value.GetType();

                if (type == null)
                    return null;

                return type.FullName.Replace(".", " ");
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooDelimitedFileTable(Value);
        }

        public override string ToString()
        {
            return TypeDescription;
        }

        public override bool CastFrom(object source)
        {
            return base.CastFrom(source);
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if(typeof(Q).IsAssignableFrom(Value?.GetType()))
            {
                target = (Q)(object)Value;
                return true;
            }
            
            return base.CastTo(ref target);
        }
    }

    public class GooDelimitedFileTableParam : GH_PersistentParam<GooDelimitedFileTable>
    {
        public override Guid ComponentGuid => new Guid("abcaf868-db1b-4407-92f3-f8bca25a5e79");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public GooDelimitedFileTableParam()
            : base(typeof(DelimitedFileTable).Name, typeof(DelimitedFileTable).Name, typeof(DelimitedFileTable).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooDelimitedFileTable> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooDelimitedFileTable value)
        {
            throw new NotImplementedException();
        }
    }
}