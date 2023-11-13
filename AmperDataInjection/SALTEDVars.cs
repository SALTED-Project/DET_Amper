using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;

namespace SALTED_DataInjection
{
    public enum ENTITY
    {
        AirQualityObserved,
        AgriParcel,
        AgriCrop
    }

    public class CO2Data
    {
        #region properties
        public string id { get; set; }
        public double co2 { get; set; }
        public string geometry { get; set; } //tipo de geometría: punto, polígono, multipolígono, etc.
        public List<Coordinates> coordinates { get; set; }


        #endregion
    }
    public class Coordinates
    {
        public decimal Lat { get; set; }
        public decimal Lon { get; set; }
    }
    public class ListMPCoords
    {
        public List<MPCoords> listMPCoords {get;set;}
        public ListMPCoords()
        {
            listMPCoords = new List<MPCoords>();
        }
    }
    public class MPCoords
    {
        public List<Coordinates> lcoord { get; set; }

        public MPCoords()
        {
            lcoord = new List<Coordinates>();
        }
    }

    public class SIGPACData
    {
        #region properties
        public string id { get; set; }
        public int provincia { get; set; }
        public int poligono { get; set; }
        public int parcela { get; set; }
        public int recinto { get; set; }
        public double area { get; set; }
        public double pend_media { get; set; }
        public double coef_rega { get; set; }
        public string uso_sigpac { get; set; }
        public string crop { get; set; }
        public int sigpac_n { get; set; }

        public string hashId { get; set; }

        public string geometry { get; set; } //tipo de geometría: punto, polígono, multipolígono, etc.
        //public List<Coordinates> coordinates { get; set; }

        public List<MPCoords> coordinates { get; set; }


        #endregion
        public SIGPACData()
        {
            id = string.Empty;
            provincia = 0; poligono = 0; parcela = 0; recinto = 0;
            area = 0; pend_media = 0; coef_rega = 0;
            uso_sigpac = string.Empty; crop = string.Empty;
            sigpac_n = 0;

            hashId = string.Empty;

            coordinates = new List<MPCoords>();
        }
    }

    //Smart Data Model Classes
    #region AirQuality
    public class AirQualityObserved
    {
        #region properties
        //identificador único de cada entidad
        public string id { get; set; }

        //tipo de la entidad
        public string tipo { get; set; }

        //dato de CO2
        public AirQualityValue co2 { get; set; }

        //área de observación
        public AirQualitySValue areaserved { get; set; }

        //fecha y hora de la medida
        public AirQualityDate dateObserved { get; set; }

        //datos de la parcela
        public Location location { get; set; }
        #endregion
        public AirQualityObserved()
        {
            id = "urn:ngsi-ld:AirQualityObserved";
            tipo = "AirQualityObserved";
            co2 = new AirQualityValue();
            areaserved = new AirQualitySValue();
            dateObserved = new AirQualityDate();
            location = new Location();
        }
    }
    public class AirQualityValue
    {
        public string type { get; set; }
        public double value { get; set; }
        public string unitCode { get; set; }

        public AirQualityValue()
        {
            type = "Property";
            value = 0;
            unitCode = string.Empty;
        }
    }
    public class AirQualitySValue
    {
        public string type { get; set; }
        public string value { get; set; }
        //public string unitCode { get; set; }

        public AirQualitySValue()
        {
            type = "Property";
            value = string.Empty;
            //unitCode = string.Empty;
        }
    }
    public class AirQualityDate
    {
        public string type { get; set; }
        public string value { get; set; }
        public AirQualityDate()
        {
            type = "Property";
            value = string.Empty;
        }
    }
    #endregion

    #region AgriParcel-AgriCrop
    public class AgriParcel
    {
        #region properties
        //identificador único de cada entidad
        public string id { get; set; }

        //tipo de la entidad
        public string tipo { get; set; }

        //dato de superficie de la parcela
        public AgriFoodValue area { get; set; }

        //fecha y hora de la creación
        public string dateCreated { get; set; } //formato ISO 6801

        //categoría el parcela
        public AgriFoodSValue categoria { get; set; }

        //datos de la parcela
        public MPLocation location { get; set; }

        //relationship AgriParcel with AgriCrop
        public AgriCrop hasAgriCrop { get; set; }
        //public AgriFoodRelation hasAirQualityObserved { get; set; }
        public AQORelation hasAirQualityObserved { get; set; }

        //permite filtrar para proceso de linkado y no forma parte del modelo
        public int poligono { get; set; }
        #endregion
        public AgriParcel()
        {
            id = "urn:ngsi-ld:AgriParcel";
            tipo = "AgriParcel";
            area = new AgriFoodValue();
            categoria = new AgriFoodSValue();
            dateCreated = string.Empty;
            location = new MPLocation();
            hasAgriCrop = new AgriCrop();
            hasAirQualityObserved = new AQORelation();
            poligono = 0;
        }
    }
    public class AgriCrop
    {
        #region properties
        //identificador único de cada entidad
        public string id { get; set; }

        //tipo de la entidad
        public string tipo { get; set; }

        //nombre de la entidad
        public AgriFoodSValue nombre { get; set; }

        //fecha de creación
        public string dateCreated { get; set; } //formato ISO 6801
        #endregion

        public AgriCrop()
        {
            id = "urn:ngsi-ld:AgriCrop";
            tipo = "AgriCrop";
            nombre = new AgriFoodSValue();
            dateCreated = DateTime.Now.ToString("s");
        }
    }
    public class AgriFoodValue
    {
        public string type { get; set; }
        public double value { get; set; }
        public string unitCode { get; set; }

        public AgriFoodValue()
        {
            type = "Property";
            value = 0;
            unitCode = string.Empty;
        }
    }
    public class AgriFoodSValue
    {
        public string type { get; set; }
        public string value { get; set; }

        public AgriFoodSValue()
        {
            type = "Property";
            value = string.Empty;
        }
    }
    public class AgriFoodRelation
    {
        public string type { get; set; }
        public string @object { get; set; }

        public AgriFoodRelation()
        {
            type = "Relationship";
            @object = string.Empty;
        }
    }
    #endregion
    public class Location
    {
        public string tipo { get; set; }
        public string subtipo { get; set; }
        public List<Coordinates> coordinates { get; set; }

        public Location()
        {
            tipo = "GeoProperty";
            subtipo = "Polygon";
            coordinates = new List<Coordinates>();
        }
    }
    public class MPLocation
    {
        public string tipo { get; set; }
        public string subtipo { get; set; }
        public List<MPCoords> coordinates { get; set; }

        public MPLocation()
        {
            tipo = "GeoProperty";
            subtipo = "Polygon";
            coordinates = new List<MPCoords>();
        }
    }

    public class JSLocation
    {
        public string type { get; set; }
        public ValLocation value { get; set; }

        public JSLocation()
        {
            type = "GeoProperty";
            value = new ValLocation();
        }
    }
    public class JSMPLocation
    {
        public string type { get; set; }
        public MPValLocation value { get; set; }

        public JSMPLocation()
        {
            type = "GeoProperty";
            value = new MPValLocation();
        }
    }
    public class MPValLocation
    {
        public string type { get; set; }
        public List<List<decimal[,]>> coordinates { get; set; }

        public MPValLocation()
        {
            type = string.Empty;
            coordinates = new List<List<decimal[,]>>();
        }
    }
    public class ValLocation
    {
        public string type { get; set; }
        public List<decimal[,]> coordinates { get; set; }
    }
    //public class MPCoordinates
    //{
    //    public List<decimal[,]> coordinates { get; set; }
    //}

    #region ReqParameters
    public class AQOReqParameters
    {
        //public List<string> @context;
        public string id;
        public string type;
        public AirQualityValue co2;
        public AirQualitySValue areaserved;
        public AirQualityDate dateObserved;
        public JSLocation location;

        public AQOReqParameters()
        {
            //@context = new List<string>();
            id = string.Empty;
            type = string.Empty;
            co2 = new AirQualityValue();
            areaserved = new AirQualitySValue();
            dateObserved = new AirQualityDate();
            location = new JSLocation();
        }
    }

    public class AgriPReqParameters
    {
        //class for injecting AgriParcel data to Scorpio CB
        public string id;
        public string type;
        public AgriFoodValue area;
        public AgriFoodSValue category;
        public string createdAt;
        public AgriFoodRelation hasAgriCrop;
        //public AgriFoodRelation hasAirQualityObserved;
        //public string hasAirQualityObserved;
        public AQORelation hasAirQualityObserved;
        public JSMPLocation location;

        public AgriPReqParameters()
        {
            id = string.Empty;
            type = string.Empty;
            area = new AgriFoodValue();
            category = new AgriFoodSValue();
            createdAt = string.Empty;
            hasAgriCrop = new AgriFoodRelation();
            //hasAirQualityObserved = new AgriFoodRelation();
            hasAirQualityObserved = new AQORelation();
            location = new JSMPLocation();
        }

    }
 
    public class AgriCReqParameters
    {
        //class for injecting AgriCrop data to Scorpio CB
        public string id;
        public string type;
        public AgriFoodSValue name;
        public string createdAt;

        public AgriCReqParameters()
        {
            id = string.Empty;
            type = string.Empty;
            name = new AgriFoodSValue();
            createdAt = string.Empty;
        }
    }

    public class AQORelation
    {
        public string type { get; set; }
        public List<string> @object { get; set; }

        public AQORelation()
        {
            type = "Relationship";
            @object = new List<string>();
        }
    }
    public class AQORelation1
    {
        public string hasAirQualityObserved { get; set; }

        public AQORelation1()
        {
            hasAirQualityObserved = string.Empty;
        }
    }

    public class AQOIntersects
    {
        public string geoproperty;
        public string georel;
        public string geometry;
        public string type;
        public List<Coordinates> coordinates;

        public AQOIntersects()
        {
            coordinates = new List<Coordinates>();
            geoproperty = "location";
            georel = "overlaps";
            geometry = "Polygon";
            type = string.Empty;
        }

    }
    #endregion

    #region Geometrías y polígonos
    public class Parcel
    {
        #region properties
        public string Id { get; set; }

        //Lista de polígonos que forman la parcela
        public List<GeoJSON.Net.Geometry.Polygon> listPolygon { get; set; }

        //Lista de id de las entidades (AirQualityObserved) cuya parcela está contenida totalmente o parte en alguno de los polígonos que forman la parcela
        public List<string> hasRelationship { get; set; }
        #endregion

        public Parcel()
        {
            Id = string.Empty;
            listPolygon = new List<GeoJSON.Net.Geometry.Polygon>();
            hasRelationship = new List<string>();
        }
        public Parcel(string uri)
        {
            Id = uri;
            listPolygon = new List<GeoJSON.Net.Geometry.Polygon>();
            hasRelationship = new List<string>();
        }
    }
    public class CO2Parcel
    {
        #region properties
        public string Id { get; set; }

        //Lista de polígonos que forman la parcela
        public List<GeoJSON.Net.Geometry.Polygon> listPolygon { get; set; }

        //Lista de id de las entidades (AgriParcel) que contienen este poligono de CO2
        public List<string> hasRelationship { get; set; }
        #endregion

        public CO2Parcel()
        {
            Id = string.Empty;
            listPolygon = new List<GeoJSON.Net.Geometry.Polygon>();
            hasRelationship = new List<string>();
        }
        public CO2Parcel(string uri)
        {
            Id = uri;
            listPolygon = new List<GeoJSON.Net.Geometry.Polygon>();
            hasRelationship = new List<string>();
        }
    }
    #endregion

    #region TAGS
    public class SALTEDTags
    {
        public static string KG_M2 = "28"; //kg por metro cuadrado
        public const string AREA = "dn_surface";
        public const string ID = "dn_oid";
        public const string PROVINCIA = "provincia";
        public const string POLIGONO = "poligono";
        public const string PARCELA = "parcela";
        public const string RECINTO = "recinto";
        public const string PEND_MEDIA = "pend_media";
        public const string COEF_REGA = "coef_rega";
        public const string USO_SIGPAC = "uso_sigpac";
        public const string GRP_CULT = "grp_cult";
        public const string SIGPAC_NUM = "_sigpac_n";
        public const string SIGPAC_NAME = "name";
        public const string SIGPAC_CRS = "crs";
        public const string GEOMETRY = "geometry";
 
        #region tipo cultivo 
        public const string AG = "AG";
        public const string CA = "CA";
        public const string CF = "CF";
        public const string CI = "CI";
        public const string CS = "CS";
        public const string CV = "CV";
        public const string ED = "ED";
        public const string EP = "EP";
        public const string FF = "FF";
        public const string FL = "FL";
        public const string FO = "FO";
        public const string FS = "FS";
        public const string FV = "FV";
        public const string FY = "FY";
        public const string IM = "IM";
        public const string IV = "IV";
        public const string OC = "OC";
        public const string OF = "OF";
        public const string OV = "OV";
        public const string PA = "PA";
        public const string PR = "PR";
        public const string PS = "PS";
        public const string TA = "TA";
        public const string TH = "TH";
        public const string VF = "VF";
        public const string VI = "VI";
        public const string VO = "VO";
        public const string ZC = "ZC";
        public const string ZU = "ZU";
        public const string ZV = "ZV";
        #endregion

        #region descripción tipos de cultivos
        public const string SIGPAC_AG = "AG: corrientes y superficies de agua";
        public const string SIGPAC_CA = "CA: viales";
        public const string SIGPAC_CF = "CF: asociación cítricos - frutales";
        public const string SIGPAC_CI = "CI: cítricos";
        public const string SIGPAC_CS = "CS: asociación cítricos - frutales de cáscara";
        public const string SIGPAC_CV = "CV: asociación cítricos - viñedos";
        public const string SIGPAC_ED = "ED: eificaciones";
        public const string SIGPAC_EP = "EP: elementos del paisaje";
        public const string SIGPAC_FF = "FF: asociación frutales - frutales de cáscara";
        public const string SIGPAC_FL = "FL: frutos secos y olivar";
        public const string SIGPAC_FO = "FO: forestal";
        public const string SIGPAC_FS = "FS: frutos secos";
        public const string SIGPAC_FV = "FV: frutos secos y viñedo";
        public const string SIGPAC_FY = "FY: frutales";
        public const string SIGPAC_IM = "IM: improductivos";
        public const string SIGPAC_IV = "IV: invernaderos y cultivos bajo plástico";
        public const string SIGPAC_OC = "OC: olivar- cítricos";
        public const string SIGPAC_OF = "OF: olivar - frutal";
        public const string SIGPAC_OV = "OV: olivar";
        public const string SIGPAC_PA = "PA: pasto con arbolado";
        public const string SIGPAC_PR = "PR: pasto arbustivo";
        public const string SIGPAC_PS = "PS: pastizal";
        public const string SIGPAC_TA = "TA: tierras arables";
        public const string SIGPAC_TH = "TH: huerta";
        public const string SIGPAC_VF = "VF: viñedo - frutal";
        public const string SIGPAC_VI = "VI: viñedo";
        public const string SIGPAC_VO = "VO: viñedo - olivar";
        public const string SIGPAC_ZC = "ZC: zona concentrada no incluida en la ortofoto";
        public const string SIGPAC_ZU = "ZU: zona urbana";
        public const string SIGPAC_ZV = "ZV: zona censurada";

        public const string CROP_NOCROP = "No crop";
        public const string CROP_NOCROP_AG = "Sin cultivo: corrientes de agua";
        public const string CROP_NOCROP_CA = "Sin cultivo: viales";
        public const string CROP_NOCROP_ED = "Sin cultivo: edificaciones";
        public const string CROP_NOCROP_EP = "Sin cultivo: elementos del paisaje";
        public const string CROP_NOCROP_IM = "Sin cultivo: improductivos";
        public const string CROP_NOCROP_TA = "Sin cultivo: tierras arables";
        public const string CROP_NOCROP_ZC = "Sin cultivo: zona concentrada";
        public const string CROP_NOCROP_ZU = "Sin cultivo: zona urbana";
        public const string CROP_NOCROP_ZV = "Sin cultivo: zona censurada";
        public const string CROP_CF = "cítricos - frutales";
        public const string CROP_CI = "cítricos";
        public const string CROP_CS = "cítricos - frutales de cáscara";
        public const string CROP_CV = "cítricos - viñedos";
        public const string CROP_FF = "frutales - frutales de cáscara";
        public const string CROP_FL = "frutos secos y olivar";
        public const string CROP_FO = "forestal";
        public const string CROP_FS = "frutos secos";
        public const string CROP_FV = "frutos secos y viñedo";
        public const string CROP_FY = "frutales";
        public const string CROP_IV = "invernaderos y cultivos bajo plástico";
        public const string CROP_OC = "olivar- cítricos";
        public const string CROP_OF = "olivar - frutal";
        public const string CROP_OV = "olivar";
        public const string CROP_PA = "pasto con arbolado";
        public const string CROP_PR = "pasto arbustivo";
        public const string CROP_PS = "pastizal";
        public const string CROP_TH = "huerta";
        public const string CROP_VF = "viñedo - frutal";
        public const string CROP_VI = "viñedo";
        public const string CROP_VO = "viñedo - olivar";
        #endregion
    }
    #endregion
}
