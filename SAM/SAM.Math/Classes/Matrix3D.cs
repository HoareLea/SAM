using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Math
{
    public class Matrix3D : Matrix
    {
        public Matrix3D(JObject jObject) 
            : base(jObject)
        {
        }

        public Matrix3D(Matrix3D matrix3D)
            : base(matrix3D)
        {

        }

        public Matrix3D()
            : base(3, 3)
        {

        }

        public override Matrix Clone()
        {
            return new Matrix3D(this);
        }


        public static Matrix3D GetIdentity()
        {
            Matrix3D matrix3D = new Matrix3D();
            for (int i = 0; i < 3; i++)
                matrix3D[i, i] = 1;

            return matrix3D;
        }


    }
}
