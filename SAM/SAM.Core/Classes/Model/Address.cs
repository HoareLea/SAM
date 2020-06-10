using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public class Address : SAMObject
    {
        private string street;
        private string city;
        private string postalCode;
        private CountryCode countryCode;

        public Address(string street, string city, string postalCode, CountryCode countryCode)
            : base()
        {
            this.street = street;
            this.city = city;
            this.postalCode = postalCode;
            this.countryCode = countryCode;
        }

        public Address(Guid guid, string name, string street, string city, string postalCode, CountryCode countryCode)
            : base(guid, name)
        {
            this.street = street;
            this.city = city;
            this.postalCode = postalCode;
            this.countryCode = countryCode;
        }

        public Address(JObject jObject)
            : base(jObject)
        {
        }

        public Address(Address address)
            :base(address)
        {

        }

        public string Street
        {
            get
            {
                return street;
            }
        }

        public string City
        {
            get
            {
                return city;
            }
        }

        public string PostalCode
        {
            get
            {
                return postalCode;
            }
        }

        public CountryCode CountryCode
        {
            get
            {
                return countryCode;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Street"))
                street = jObject.Value<string>("Street");

            if (jObject.ContainsKey("City"))
                city = jObject.Value<string>("City");

            if (jObject.ContainsKey("PostalCode"))
                postalCode = jObject.Value<string>("PostalCode");

            if (jObject.ContainsKey("CountryCode"))
                Enum.TryParse(jObject.Value<string>("CountryCode"), out countryCode);

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (street != null)
                jObject.Add("Street", street);

            if (city != null)
                jObject.Add("City", city);
            
            if (postalCode != null)
                jObject.Add("PostalCode", postalCode);

            if (countryCode != CountryCode.Undefined)
                jObject.Add("CountryCode", countryCode.ToString());

            return jObject;

        }
    }
}
