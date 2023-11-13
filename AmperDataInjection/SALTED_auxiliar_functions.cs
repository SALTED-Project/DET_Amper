using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Timers;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using amperUtil.Log;
using GeoJSON.Net.Geometry;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SALTED_DataInjection
{
    public partial class Form1 : Form
    {
        public void ParseJSObjectToCO2(JObject jo)
        {
            Log.Write("Deserializing JObject: SALTED_DataInjection.ParseJSObjectToCO2", LogLevel.Log_Info);

            try
            {
                int nobj = jo.Count;
                //int numvar = 0;
                string name = string.Empty;
                string crs_name = string.Empty;



                CO2Data auxData = new CO2Data();
                auxData.coordinates = new List<Coordinates>();

                string vc = jo.ToString();
                //CO2 footprint data
                string vc1 = jo["properties"].ToString();
                vc1 = vc1.Replace("\r\n", "");
                vc1 = vc1.Replace("\"", "");
                vc1 = vc1.Replace(" ", "");
                vc1 = vc1.Replace("{", "");
                vc1 = vc1.Replace("}", "");
                string[] aux1 = vc1.Split(':');
                auxData.co2 = Convert.ToDouble(aux1[1]);

                //Location data
                string vc2 = jo["geometry"].ToString();
                JObject g = JObject.Parse(vc2);
                //Type
                string gt = g["type"].ToString();
                gt = gt.Replace("\r\n", "");
                gt = gt.Replace("\"", "");
                gt = gt.Replace(" ", "");
                auxData.geometry = gt;
                //Coordinates
                string gc = g["coordinates"].ToString();
                gc = gc.Replace("\r\n", "");
                gc = gc.Replace("\"", "");
                gc = gc.Replace(" ", "");
                gc = gc.Replace("],[", ";");
                gc = gc.Replace("[", "");
                gc = gc.Replace("]", "");
                string[] auxGC = gc.Split(';');
                for (int i = 0; i < auxGC.Length; i++)
                {
                    Coordinates xy = new Coordinates();
                    string[] auxLatLon = auxGC[i].Split(',');
                    xy.Lat = Convert.ToDecimal(auxLatLon[1], CultureInfo.GetCultureInfo("en-EN"));
                    xy.Lon = Convert.ToDecimal(auxLatLon[0], CultureInfo.GetCultureInfo("en-EN"));
                    auxData.coordinates.Add(xy);
                }
                listCO2Data.Add(auxData);
                //numvar++;

                Log.Write("Deserialized JObject: SALTED_DataInjection.ParseJSObjectToCO2", LogLevel.Log_Info);
                //btnCO2Map.BackColor = Color.Red;
                //btnCO2Map.ForeColor = Color.BlueViolet;
                //btnCO2Map.Text = "DATA MAPPEando";
            }
            catch(Exception ex)
            {
                Log.Write(string.Format("Exception deserializing JObject to CO2Datos data list: {0}", ex.Message), LogLevel.Log_Debug);
                picBox2.Visible = true;
            }
        }

        public void ParseJSObjectToParcel(JObject jo)
        {
            Log.Write("Deserializing JObject: SALTED_DataInjection.ParseJSObjectToParcel", LogLevel.Log_Info);

            string parcelId = string.Empty;

            try
            {
                int nobj = jo.Count;
                int numvar = 0;
                string name = string.Empty;
                string crs_name = string.Empty;
                SIGPACData auxParcel = new SIGPACData();
                //auxParcel.coordinates = new List<Coordinates>();

                string vc = jo.ToString();
                #region properties
                string vc1 = jo["properties"].ToString();
                vc1 = vc1.Replace("\r\n", "");
                vc1 = vc1.Replace("\"", "");
                vc1 = vc1.Replace(" ", "");
                vc1 = vc1.Replace("{", "");
                vc1 = vc1.Replace("}", "");
                string[] aux1 = vc1.Split(',');
                for (int i = 0; i < aux1.Length; i++)
                {
                    string[] auxProps = aux1[i].Split(':');
                    if (auxProps[1] == "null") continue;

                    switch(auxProps[0])
                    {
                        case SALTEDTags.ID:
                            auxParcel.id = auxProps[1];
                            parcelId = auxParcel.id;
                            break;
                        case SALTEDTags.PROVINCIA:
                            auxParcel.provincia = Convert.ToInt32(auxProps[1]);
                            break;
                        case SALTEDTags.POLIGONO:
                            auxParcel.poligono = Convert.ToInt32(auxProps[1]);
                            break;
                        case SALTEDTags.PARCELA:
                            auxParcel.parcela = Convert.ToInt32(auxProps[1]);
                            break;
                        case SALTEDTags.RECINTO:
                            auxParcel.recinto = Convert.ToInt32(auxProps[1]);
                            break;
                        case SALTEDTags.SIGPAC_NUM:
                            auxParcel.sigpac_n = Convert.ToInt32(auxProps[1]);
                            break;
                        case SALTEDTags.USO_SIGPAC:
                            auxParcel.uso_sigpac = auxProps[1];
                            break;
                        case SALTEDTags.COEF_REGA:
                            auxParcel.coef_rega = Convert.ToInt32(auxProps[1]);
                            break;
                        case SALTEDTags.AREA:
                            auxParcel.area = Convert.ToDouble(auxProps[1], CultureInfo.GetCultureInfo("en-EN"));
                            break;
                        case SALTEDTags.GRP_CULT:
                            auxParcel.crop = auxProps[1];
                            break;
                        case SALTEDTags.PEND_MEDIA:
                            auxParcel.pend_media = Convert.ToDouble(auxProps[1], CultureInfo.GetCultureInfo("en-EN"));
                            break;
                        default:
                            break;
                    }

                }
                #endregion

                #region coordinates
                //Location data
                string vc2 = jo["geometry"].ToString();
                JObject g = JObject.Parse(vc2);
                //Type
                string gt = g["type"].ToString();
                gt = gt.Replace("\r\n", "");
                gt = gt.Replace("\"", "");
                gt = gt.Replace(" ", "");
                auxParcel.geometry = gt;
                //Coordinates
                string gc = g["coordinates"].ToString();
                gc = gc.Replace(" ", "");
                gc = gc.Replace("\r\n", "");
                gc = gc.Replace("]],[[", "@");
                string[] mgc = gc.Split('@');
                if (mgc.Length == 1)
                {
                    //there is a single polygon area
                    auxParcel.geometry = "Polygon";
                }
                else
                {
                    auxParcel.geometry = "MultiPolygon";
                }

                for (int i = 0; i < mgc.Length; i++)
                {
                    mgc[i] = mgc[i].Replace("\r\n", "");
                    mgc[i] = mgc[i].Replace("\"", "");
                    mgc[i] = mgc[i].Replace(" ", "");
                    mgc[i] = mgc[i].Replace("],[", ";");
                    mgc[i] = mgc[i].Replace("[", "");
                    mgc[i] = mgc[i].Replace("]", "");
                    string[] auxGC = mgc[i].Split(';');
                    MPCoords mpc = new MPCoords();
                    for (int j = 0; j < auxGC.Length; j++)
                    {
                        Coordinates xy = new Coordinates();
                        string[] auxLatLon = auxGC[j].Split(',');
                        xy.Lat = Convert.ToDecimal(auxLatLon[1], CultureInfo.GetCultureInfo("en-EN"));
                        xy.Lon = Convert.ToDecimal(auxLatLon[0], CultureInfo.GetCultureInfo("en-EN"));
                        mpc.lcoord.Add(xy);
                        //auxParcel.coordinates.Add(xy);
                    }
                    auxParcel.coordinates.Add(mpc);
                }
                //gc = gc.Replace("],[", ";");
                //gc = gc.Replace("[", "");
                //gc = gc.Replace("]", "");
                //string[] auxGC = gc.Split(';');
                //for (int i = 0; i < auxGC.Length; i++)
                //{
                //    Coordinates xy = new Coordinates();
                //    string[] auxLatLon = auxGC[i].Split(',');
                //    xy.Lat = Convert.ToDecimal(auxLatLon[1], CultureInfo.GetCultureInfo("en-EN"));
                //    xy.Lon = Convert.ToDecimal(auxLatLon[0], CultureInfo.GetCultureInfo("en-EN"));
                //    auxParcel.coordinates.Add(xy);
                //}
                #endregion

                listParcelData.Add(auxParcel);
                numvar++;
                

                Log.Write("Deserialized JObject: SALTED_DataInjection.ParseJSObjectToParcel", LogLevel.Log_Info);
            }
            catch(Exception ex)
            {
                Log.Write(string.Format("Exception deserializing JObject to SIGPAC Datos data list: {0}. ID; {1}", ex.Message, parcelId), LogLevel.Log_Debug);
                picBox4.Visible = true;
            }
        }

        public void MapCO2ToAirQuality()
        {
            Log.Write("Map CO2 data to AirQualityObserved Smart Data Model: SALTED_DataInjection.MapCO2ToAirQuality", LogLevel.Log_Info);

            try
            {
                
                foreach (CO2Data co2 in listCO2Data)
                {
                    string sSourceData = string.Empty;
                    byte[] tmpSource;
                    byte[] tmpHash;

                    AirQualityObserved auxAQO = new AirQualityObserved();

                    foreach (Coordinates xy in co2.coordinates)
                    {
                        sSourceData += xy.Lat.ToString() + xy.Lon.ToString();
                        ////// LOCATION //////
                        ///
                        auxAQO.location.coordinates.Add(xy);
                    }
                    ////// ID //////
                    //Create a byte array from source data.
                    tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
                    //Compute hash based on source data.
                    tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
                    auxAQO.id += ":" + observedArea + ":" + ByteArrayToString(tmpHash);
                    
                    /////// CO2 //////
                    auxAQO.co2.unitCode = SALTEDTags.KG_M2;
                    auxAQO.co2.value = co2.co2 / 100; //convierte tn por ha a kg por m2

                    ////// DATE //////
                    ///
                    //auxAQO.dateObserved.value = Convert.ToDateTime(dateObservedInicio).ToString("s") + "/" + Convert.ToDateTime(dateObservedFinal).ToString("s");
                    //dateObserved solo permite una fecha, usaremos la del final del periodo
                    auxAQO.dateObserved.value = Convert.ToDateTime(dateObservedFinal).ToString("s");

                    ////// AREA SERVED //////
                    auxAQO.areaserved.value = observedArea.ToUpperInvariant();

                    listAirQualityObserved.Add(auxAQO);
                }

                Log.Write("Mapped CO2 data to smart data model: SALTED_DataInjection.MapCO2ToAirQuality", LogLevel.Log_Info);
                picBox1.Visible = true;
                btnCO2.Enabled = true;

            }
            catch(Exception ex)
            {
                picBox2.Visible = true;
                Log.Write(string.Format("Exception mapping CO2Datos data list to AirQualityObserved list: {0}", ex.Message), LogLevel.Log_Debug);
            }
        }

        public void MapParcelToAgriParcel()
        {
            Log.Write("Map SIGPAC data to AgriParcel Smart Data Model: SALTED_DataInjection.MapParcelToAgriParcel", LogLevel.Log_Info);

            bool hasAG = false, hasCA = false, hasCF = false, hasCI = false, hasCS = false, hasCV = false;
            bool hasED = false, hasEP = false, hasFF = false, hasFL = false, hasFO = false, hasFS = false;
            bool hasFV = false, hasFY = false, hasIM = false, hasIV = false, hasOC = false, hasOF = false;
            bool hasOV = false, hasPA = false, hasPR = false, hasPS = false, hasTA = false, hasTH = false;
            bool hasVF = false, hasVI = false, hasVO = false, hasZC = false, hasZU = false, hasZV = false;
            bool hasNOCROP = false;

            try
            {

                foreach (SIGPACData parcel in listParcelData)
                {
                    string sSourceData = string.Empty;
                    byte[] tmpSource;
                    byte[] tmpHash;

                    AgriParcel auxAParcel = new AgriParcel();
                    //MPCoords auxMPCoords = new MPCoords();
                    //AgriCrop auxACrop = new AgriCrop();

                    foreach (MPCoords mpc in parcel.coordinates)
                    {
                        ////// LOCATION //////
                        MPCoords auxMPCoords = new MPCoords();
                        foreach (Coordinates xy in mpc.lcoord)
                        {
                            auxMPCoords.lcoord.Add(xy);
                        }
                        auxAParcel.location.coordinates.Add(auxMPCoords);
                    }
                    auxAParcel.location.subtipo = parcel.geometry;
                    ////// ID //////
                    sSourceData = parcel.id.ToString() + parcel.provincia.ToString() + parcel.poligono.ToString() + parcel.parcela.ToString() + parcel.recinto.ToString();
                    //Create a byte array from source data.
                    tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
                    //Compute hash based on source data.
                    tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
                    auxAParcel.id += ":" + observedArea + ":" + sigpacName + ":" + ByteArrayToString(tmpHash);

                    listParcelData[listParcelData.IndexOf(parcel)].hashId = auxAParcel.id;
                    auxAParcel.poligono = parcel.poligono;
                    /////// SURFACE //////
                    auxAParcel.area.unitCode = "MTK"; //metros cuadrados
                    auxAParcel.area.value = parcel.area;

                    ////// DATE //////
                    ///
                    auxAParcel.dateCreated = DateTime.Now.ToString("s");

                    ////// CATEGORY //////
                    /// se crea una entidad de AgriCrop por cada tipo de uso sigac existente
                    #region category
                    switch (parcel.uso_sigpac)
                    {
                        case SALTEDTags.AG:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_AG;
                            SetAgriCropLink(SALTEDTags.CROP_NOCROP_AG, ref hasAG, ref auxAParcel);
                            break;
                        case SALTEDTags.CA:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_CA;
                            SetAgriCropLink(SALTEDTags.CROP_NOCROP_CA, ref hasCA, ref auxAParcel);
                            break;
                        case SALTEDTags.CF:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_CF;
                            SetAgriCropLink(SALTEDTags.CROP_CF, ref hasCF, ref auxAParcel);
                            break;
                        case SALTEDTags.CI:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_CI;
                            SetAgriCropLink(SALTEDTags.CROP_CI, ref hasCI, ref auxAParcel);
                            break;
                        case SALTEDTags.CS:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_CS;
                            SetAgriCropLink(SALTEDTags.CROP_CS, ref hasCS, ref auxAParcel);
                            break;
                        case SALTEDTags.CV:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_CV;
                            SetAgriCropLink(SALTEDTags.CROP_CV, ref hasCV, ref auxAParcel);
                            break;
                        case SALTEDTags.ED:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_ED;
                            SetAgriCropLink(SALTEDTags.CROP_NOCROP_ED, ref hasED, ref auxAParcel);
                            break;
                        case SALTEDTags.EP:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_EP;
                            SetAgriCropLink(SALTEDTags.CROP_NOCROP_EP, ref hasEP, ref auxAParcel);
                            break;
                        case SALTEDTags.FF:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_FF;
                            SetAgriCropLink(SALTEDTags.CROP_FF, ref hasFF, ref auxAParcel);
                            break;
                        case SALTEDTags.FL:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_FL;
                            SetAgriCropLink(SALTEDTags.CROP_FL, ref hasFL, ref auxAParcel);
                            break;
                        case SALTEDTags.FO:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_FO;
                            SetAgriCropLink(SALTEDTags.CROP_FO, ref hasFO, ref auxAParcel);
                            break;
                        case SALTEDTags.FS:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_FS;
                            SetAgriCropLink(SALTEDTags.CROP_FS, ref hasFS, ref auxAParcel);
                            break;
                        case SALTEDTags.FV:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_FV;
                            SetAgriCropLink(SALTEDTags.CROP_FV, ref hasFV, ref auxAParcel);
                            break;
                        case SALTEDTags.FY:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_FY;
                            SetAgriCropLink(SALTEDTags.CROP_FY, ref hasFY, ref auxAParcel);
                            break;
                        case SALTEDTags.IM:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_IM;
                            SetAgriCropLink(SALTEDTags.CROP_NOCROP_IM, ref hasIM, ref auxAParcel);
                            break;
                        case SALTEDTags.IV:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_IV;
                            SetAgriCropLink(SALTEDTags.CROP_IV, ref hasIV, ref auxAParcel);
                            break;
                        case SALTEDTags.OC:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_OC;
                            SetAgriCropLink(SALTEDTags.CROP_OC, ref hasOC, ref auxAParcel);
                            break;
                        case SALTEDTags.OF:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_OF;
                            SetAgriCropLink(SALTEDTags.CROP_OF, ref hasOF, ref auxAParcel);
                            break;
                        case SALTEDTags.OV:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_OV;
                            SetAgriCropLink(SALTEDTags.CROP_OV, ref hasOV, ref auxAParcel);
                            break;
                        case SALTEDTags.PA:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_PA;
                            SetAgriCropLink(SALTEDTags.CROP_PA, ref hasPA, ref auxAParcel);
                            break;
                        case SALTEDTags.PR:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_PR;
                            SetAgriCropLink(SALTEDTags.CROP_PR, ref hasPR, ref auxAParcel);
                            break;
                        case SALTEDTags.PS:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_PS;
                            SetAgriCropLink(SALTEDTags.CROP_PS, ref hasPS, ref auxAParcel);
                            break;
                        case SALTEDTags.TA:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_TA;
                            SetAgriCropLink(SALTEDTags.CROP_NOCROP_TA, ref hasTA, ref auxAParcel);
                            break;
                        case SALTEDTags.TH:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_TH;
                            SetAgriCropLink(SALTEDTags.CROP_TH, ref hasTH, ref auxAParcel);
                            break;
                        case SALTEDTags.VF:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_VF;
                            SetAgriCropLink(SALTEDTags.CROP_VF, ref hasVF, ref auxAParcel);
                            break;
                        case SALTEDTags.VI:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_VI;
                            SetAgriCropLink(SALTEDTags.CROP_VI, ref hasVI, ref auxAParcel);
                            break;
                        case SALTEDTags.VO:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_VO;
                            SetAgriCropLink(SALTEDTags.CROP_VO, ref hasVO, ref auxAParcel);
                            break;
                        case SALTEDTags.ZC:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_ZC;
                            SetAgriCropLink(SALTEDTags.CROP_NOCROP_ZC, ref hasZC, ref auxAParcel);
                            break;
                        case SALTEDTags.ZU:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_ZU;
                            SetAgriCropLink(SALTEDTags.CROP_NOCROP_ZU, ref hasZU, ref auxAParcel);
                            break;
                        case SALTEDTags.ZV:
                            auxAParcel.categoria.value = SALTEDTags.SIGPAC_ZV;
                            SetAgriCropLink(SALTEDTags.CROP_NOCROP_ZV, ref hasZV, ref auxAParcel);
                            break;
                        default:
                            break;
                    }
                    #endregion

                    listAgriParcel.Add(auxAParcel);
                }

                Log.Write("Mapped SIGPAC data to smart data model: SALTED_DataInjection.MapCO2ToAirQuality", LogLevel.Log_Info);
                picBox3.Visible = true;
                btnSIGPAC.Enabled = true;

            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception mapping CO2Datos data list to AirQualityObserved list: {0}", ex.Message), LogLevel.Log_Debug);
                picBox4.Visible = true;
            }
        }

        public static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

        public void SetAgriCropLink(string uso_sigpac, ref bool tipo, ref AgriParcel parcel)
        {
            Log.Write("Setting relationship AgriCrop to AgriParcel in SetAgriCropLink()", LogLevel.Log_Info);

            AgriCrop auxACrop = new AgriCrop();

            if (!tipo)
            {
                tipo = true;
                ////// ID //////
                //string sSourceData = uso_sigpac + "-" + auxACrop.dateCreated;
                string sSourceData = uso_sigpac;
                //Create a byte array from source data.
                byte [] tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
                //Compute hash based on source data.
                byte[] tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
                //auxACrop.id += ":" + observedArea + ":" + ByteArrayToString(tmpHash);
                auxACrop.id += ":" + ByteArrayToString(tmpHash);
                auxACrop.nombre.value = uso_sigpac;
                listAgriCrop.Add(auxACrop);
                parcel.hasAgriCrop = auxACrop;

                string[] auxCategory = parcel.categoria.value.Split(':');
                cmbSIGPAC.Items.Add(auxCategory[0]);
            }
            else
            {
                foreach (AgriCrop crop in listAgriCrop)
                {
                    if (crop.nombre.value == uso_sigpac)
                    {
                        parcel.hasAgriCrop = crop;
                        break;
                    }
                }
            }


        }

        //return true if the point is belongs to the polygon false anywhere
        public bool PolygonIntersection(IPosition pa0, IPosition pa1, IPosition pb0, IPosition pb1)
        {
            bool result = false;

            Log.Write("Polygon Intersection: calculates whether a polygon intersect another one", LogLevel.Log_Info);

            //Calculamos la ecuación de la recta de los polígonos de CO2 y de los polígonos de parcelas
            //Si se produce intersección entre alguno de los segmentos, relacionamos las dos entidades

            try
            {
                //ecuación de la recta de un polígono
                //(y-y0)/(y1-y0) = (x-x0)/(x1-x0) --> y = y0 + ((x-x0)(y1-y0))/(x1-x0)
                //Hay intersección si al igualar las ecuaciones obtenemos el punto de interserción 
                double x = 0;
                double ya = 0, yb = 0;
                double ma = 0, mb = 0;
                ///////////
                //y = xya0.Lon + (x - xya0.Lon) * ma;
                //y = xyb0.Lon + (x - xyb0.Lon) * mb;
                //
                // Se iguala y y se calcula x
                // xya0.Lon + ((x - xya0.Lon) * ma) = xyb0.Lon + ((x - xyb0.Lon) * mb)
                // (x * ma) - (xya0.Lon * ma) - (x * mb) + (xyb0.Lon * mb) = xyb0.Lon - xya0.Lon
                //
                // x = (xyb0.Lon - xya0.Lon)/(ma - mb)
                ////
                ///
                ma = (pa1.Longitude - pa0.Longitude) / (pa1.Latitude - pa0.Latitude);
                mb = (pb1.Longitude - pb0.Longitude) / (pb1.Latitude - pb0.Latitude);

                x = ((ma * pa0.Latitude) - pa0.Longitude - (mb * pb0.Latitude) + pb0.Longitude)/(ma - mb);
                ya = pa0.Longitude + ((x - pa0.Latitude) * ma);
                yb = pb0.Longitude + ((x - pb0.Latitude) * mb);

                if (ya == yb)
                {
                    double entreX = (pa0.Latitude - x) * (pa1.Latitude - x);
                    double entreY = (pa0.Longitude - ya) * (pa1.Longitude - ya);

                    if ((entreX <= 0) && (entreY <= 0))
                    { result = true; } //hay intersección
                }

            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception calculating polygons intersections. ERROR: {0}", ex.Message), LogLevel.Log_Debug);
            }

            return result;
        }

        //Actualiza la propiedad de relación de AgriParcel con la correspondientes del AirQualityObserved
        public void TestIntersections(AQOIntersects par)
        {
            try
            {
                HttpClient rclient = new HttpClient();
                rclient.DefaultRequestHeaders.Accept.Clear();
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/ld+json"));

                string auxParams = string.Empty;
                auxParams = "geoproperty=" + par.geoproperty;
                auxParams += "&georel=" + par.georel;
                auxParams += "&geometry=" + par.geometry;
                auxParams += "&coordinates=%5B%5B%5B";

                for (int i = 0; i < par.coordinates.Count; i++)
                {
                    auxParams += par.coordinates[i].Lon.ToString();
                    auxParams += "%2C";
                    auxParams += par.coordinates[i].Lat.ToString();
                    if (i < par.coordinates.Count - 1) auxParams += "%5D%2C%5B";
                }

                auxParams += "%5D%5D%5D&type=" + par.type;


                string url = string.Format("http://140.0.24.127:9090/ngsi-ld/v1/entities?{0}", auxParams);

                GetRequest(url, rclient);

                Log.Write("Tested get intersections");
            }
            catch(Exception ex)
            {
                Log.Write(string.Format("Exception testing intersections. ERROÇÇR: {0}", ex.Message), LogLevel.Log_Debug);
            }

            

        }
        public void UpdateAgriParcelData(List<Parcel> parcelas)
        {
            Log.Write("UpdateAgriParcelData with rlatioship Ids from airQualityObserved entities", LogLevel.Log_Info);

            List<Parcel> aux_parcelas = new List<Parcel>();
            List<CO2Parcel> aux_parcelasCO2 = new List<CO2Parcel>();

            string url = string.Empty;

            try
            {
                //Se recorre la lista de parcelas y se busca la parcela correspondiente en la lista de listAgriParcel
                foreach (Parcel p in parcelas)
                {
                    for (int i = 0; i < listAgriParcel.Count; i++)
                    {
                        if (listAgriParcel[i].id == p.Id)
                        {
                            foreach (string s in p.hasRelationship)
                            {
                                //listAgriParcel[i].hasAirQualityObserved.@object.Add(s); //pendiente de ver como construir esta propiedad
                                listAgriParcel[i].hasAirQualityObserved.@object.Add(s); //pendiente de ver como construir esta propiedad
                            }
                        }
                    }
                }

                //Se hace el PATCH para actualizar la entidad AgriParcel. Se envián los elementos de la lista cuya propiedad es distinta de string.Empty
                //PATCH entity
                HttpClient rclient = new HttpClient();
                rclient.DefaultRequestHeaders.Accept.Clear();
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/ld+json"));
                rclient.DefaultRequestHeaders.Add("Link", "<https://raw.githubusercontent.com/SALTED-Project/contexts/main/wrapped_contexts/agriparcel-context.jsonld>;rel=\"http://www.w3.org/ns/json-ld#context\";type=\"application/ld+json\"");

                for (int i = 0; i < listAgriParcel.Count; i++)
                {
                    if (listAgriParcel[i].poligono == Convert.ToInt32(txtNPolygon.Text))
                    {
                        if (listAgriParcel[i].hasAirQualityObserved.@object.Count >= 0)
                        {
                            //FASE 1. Borramos la entidad
                            url = string.Format(urlBase + "{0}", listAgriParcel[i].id); //internal
                            DeleteRequest(url, rclient);

                            ////FASE 2. Se crea de nuevo añadiendo el hasAirQualityObserved
                            //AgriPReqParameters rqAPParams = new AgriPReqParameters();
                            //rqAPParams.id = "urn:ngsi-ld:AgriParcel:lorca:SIGPAC_Lorca_20210104_4326:11F214C7A01D5B6C98E3F56A14CD18B39_113";
                            //rqAPParams.type = listAgriParcel[i].tipo;
                            //rqAPParams.createdAt = listAgriParcel[i].dateCreated;

                            //rqAPParams.area.value = listAgriParcel[i].area.value;
                            //rqAPParams.area.unitCode = listAgriParcel[i].area.unitCode;

                            //rqAPParams.category.value = listAgriParcel[i].categoria.value;

                            //rqAPParams.hasAgriCrop.@object = listAgriParcel[i].hasAgriCrop.id;

                            //if (listAgriParcel[i].location.subtipo == "Polygon")
                            //    continue;

                            //MPValLocation auxLocation = new MPValLocation();
                            //List<decimal[,]> listCoord = new List<decimal[,]>();
                            //for (int c = 0; c < listAgriParcel[i].location.coordinates.Count; c++)
                            //{
                            //    auxLocation.coordinates = new List<List<decimal[,]>>();

                            //    decimal[,] coord = new decimal[listAgriParcel[i].location.coordinates[c].lcoord.Count, 2];
                            //    for (int d = 0; d < listAgriParcel[i].location.coordinates[c].lcoord.Count; d++)
                            //    {

                            //        coord[d, 0] = listAgriParcel[i].location.coordinates[c].lcoord[d].Lon;
                            //        coord[d, 1] = listAgriParcel[i].location.coordinates[c].lcoord[d].Lat;
                            //    }
                            //    listCoord.Add(coord);
                            //}

                            //auxLocation.coordinates.Add(listCoord);
                            //auxLocation.type = listAgriParcel[i].location.subtipo;

                            //rqAPParams.location.value = auxLocation;
                            //rqAPParams.location.type = listAgriParcel[i].location.tipo;

                            //foreach (string obj in listAgriParcel[i].hasAirQualityObserved.@object)
                            //{
                            //    rqAPParams.hasAirQualityObserved.@object.Add(obj);
                            //}

                            //string jsonStringAP = JsonConvert.SerializeObject(rqAPParams);
                            //var jsonAPContent = new StringContent(JsonConvert.SerializeObject(rqAPParams), Encoding.UTF8, "application/json");

                            ////send async POST to the url for adding a entity
                            //PostRequest(url, rclient, jsonAPContent, ENTITY.AgriParcel, rqAPParams.id);
                        }

                        //FASE 2. Se crea de nuevo añadiendo el hasAirQualityObserved
                        //POST entity
                        //url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/"; //from outside
                        //url = "http://140.0.24.127:9090/ngsi-ld/v1/entities/"; //internal
                        url = urlBase;
                        AgriPReqParameters rqAPParams = new AgriPReqParameters();
                        //rqAPParams.id = "urn:ngsi-ld:AgriParcel:lorca:SIGPAC_Lorca_20210104_4326:11F214C7A01D5B6C98E3F56A14CD18B39_113";
                        rqAPParams.id = listAgriParcel[i].id;
                        rqAPParams.type = listAgriParcel[i].tipo;
                        rqAPParams.createdAt = listAgriParcel[i].dateCreated;

                        rqAPParams.area.value = listAgriParcel[i].area.value;
                        rqAPParams.area.unitCode = listAgriParcel[i].area.unitCode;

                        rqAPParams.category.value = listAgriParcel[i].categoria.value;

                        rqAPParams.hasAgriCrop.@object = listAgriParcel[i].hasAgriCrop.id;

                        if (listAgriParcel[i].location.subtipo == "Polygon")
                        {
                            //Se procesa como multipoligono
                            //continue;
                        }
                            

                        MPValLocation auxLocation = new MPValLocation();
                        List<decimal[,]> listCoord = new List<decimal[,]>();
                        for (int c = 0; c < listAgriParcel[i].location.coordinates.Count; c++)
                        {
                            auxLocation.coordinates = new List<List<decimal[,]>>();

                            decimal[,] coord = new decimal[listAgriParcel[i].location.coordinates[c].lcoord.Count, 2];
                            for (int d = 0; d < listAgriParcel[i].location.coordinates[c].lcoord.Count; d++)
                            {

                                coord[d, 0] = listAgriParcel[i].location.coordinates[c].lcoord[d].Lon;
                                coord[d, 1] = listAgriParcel[i].location.coordinates[c].lcoord[d].Lat;
                            }
                            listCoord.Add(coord);
                        }

                        auxLocation.coordinates.Add(listCoord);
                        auxLocation.type = listAgriParcel[i].location.subtipo;
                        auxLocation.type = "MultiPolygon";

                        rqAPParams.location.value = auxLocation;
                        rqAPParams.location.type = listAgriParcel[i].location.tipo;

                        foreach (string obj in listAgriParcel[i].hasAirQualityObserved.@object)
                        {
                            rqAPParams.hasAirQualityObserved.@object.Add(obj);
                        }
                        if (rqAPParams.hasAirQualityObserved.@object.Count == 0)
                        {
                            //Se indica no relationship
                            string none = "urn:ngsi-ld:AirQualityObserved:lorca:NONE";
                            rqAPParams.hasAirQualityObserved.@object.Add(none);
                        }
                        string jsonStringAP = JsonConvert.SerializeObject(rqAPParams);
                        var jsonAPContent = new StringContent(JsonConvert.SerializeObject(rqAPParams), Encoding.UTF8, "application/json");

                        //send async POST to the url for adding a entity
                        PostRequest(url, rclient, jsonAPContent, ENTITY.AgriParcel, rqAPParams.id);
                    }
                }



                ////url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/"; //from outside
                //url = "http://140.0.24.127:9090/ngsi-ld/v1/entities/"; //internal
                //url = string.Format("http://140.0.24.127:9090/ngsi-ld/v1/entities/{0}/attrs/", "urn:ngsi-ld:AgriParcel:lorca:SIGPAC_Lorca_20210104_4326:11F214C7A01D5B6C98E3F56A14CD18B39");
                ////2. Añadir las entidades AgriParcel correspondientes
                ////This method is used for injecting (creating) AgriParcel entities to Scorpio CB. Also AgriAgriCrop entities will be creted when needed
                //HttpClient rclient = new HttpClient();
                //rclient.DefaultRequestHeaders.Accept.Clear();
                ////rclient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                //rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                //rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/ld+json"));

                ////hasAirQualltyObserved (string array)

                ////AgriPReqParameters rqAPParams = new AgriPReqParameters();
                ////postAP = false;
                ////return;
                //foreach (AgriParcel aparcel in listAgriParcel)
                //{
                //    if (aparcel.hasAirQualityObserved.@object.Count > 0)
                //    {
                //        //int numRel = aparcel.hasAirQualityObserved.@object.Count;
                //        ////string[] rqAPParams = new string[aparcel.hasAirQualityObserved.@object.Count];
                //        ///
                //        //AQORelation1 rqAPParams = new AQORelation1();
                //        //rqAPParams.hasAirQualityObserved.Add("11:18_28-04-2023");
                //        ////for (int i = 0; i < aparcel.hasAirQualityObserved.@object.Count; i++)
                //        ////{
                //        ////    rqAPParams[i] = aparcel.hasAirQualityObserved.@object[i];
                //        ////}
                //        ////rqAPParams = new string[1];
                //        ////rqAPParams[0] = "11:18_28-04-2023";
                //        //string jsonStringAP = JsonConvert.SerializeObject(rqAPParams);
                //        //rqAPParams.location.value.type = "Polygon";
                //        //var jsonAPContent = new StringContent(JsonConvert.SerializeObject(rqAPParams), Encoding.UTF8, "application/json");

                //        //PatchRequest(url, rclient, jsonAPContent, ENTITY.AgriParcel);
                //    }


                //    //rqAPParams.id = aparcel.id;
                //    //rqAPParams.type = aparcel.tipo;
                //    //rqAPParams.createdAt = aparcel.dateCreated;

                //    //rqAPParams.area.value = aparcel.area.value;
                //    //rqAPParams.area.unitCode = aparcel.area.unitCode;

                //    //rqAPParams.category.value = aparcel.categoria.value;

                //    //rqAPParams.hasAgriCrop.@object = aparcel.hasAgriCrop.id;

                //    //if (aparcel.location.subtipo == "Polygon")
                //    //    continue;

                //    //MPValLocation auxLocation = new MPValLocation();
                //    //List<decimal[,]> listCoord = new List<decimal[,]>();
                //    //for (int c = 0; c < aparcel.location.coordinates.Count; c++)
                //    //{
                //    //    auxLocation.coordinates = new List<List<decimal[,]>>();

                //    //    decimal[,] coord = new decimal[aparcel.location.coordinates[c].lcoord.Count, 2];
                //    //    for (int d = 0; d < aparcel.location.coordinates[c].lcoord.Count; d++)
                //    //    {

                //    //        coord[d, 0] = aparcel.location.coordinates[c].lcoord[d].Lon;
                //    //        coord[d, 1] = aparcel.location.coordinates[c].lcoord[d].Lat;


                //    //    }
                //    //    listCoord.Add(coord);
                //    //}
                //    //auxLocation.coordinates.Add(listCoord);
                //    //auxLocation.type = aparcel.location.subtipo;

                //    //rqAPParams.location.value = auxLocation;
                //    //rqAPParams.location.type = aparcel.location.tipo;



                //    //string jsonStringAP = JsonConvert.SerializeObject(rqAPParams);
                //    ////rqAPParams.location.value.type = "Polygon";
                //    //var jsonAPContent = new StringContent(JsonConvert.SerializeObject(rqAPParams), Encoding.UTF8, "application/json");
                //    //send async POST to the url for adding a entity
                //    //PostRequest(url, rclient, jsonAPContent, ENTITY.AgriParcel);
                //    //break;
                //    //if (postAP) break;
                //}
                //string hasAirQualityObserved = "urn:ngsi-ld:AirQualityObserved:1234567890_2804_11111111111111111";
                //AQORelation1 rqParams = new AQORelation1();
                //rqParams.hasAirQualityObserved = hasAirQualityObserved;
                //AgriFoodSValue rqAPParams = new AgriFoodSValue();
                //rqAPParams.value = "11:18_28-04-2023";
                //rqAPParams.type = "Relationship";
                //string jsonStringAP = JsonConvert.SerializeObject(rqAPParams);
                //var jsonAPContent = new StringContent(JsonConvert.SerializeObject(jsonStringAP), Encoding.UTF8, "application/json");

                //PatchRequest(url, rclient, jsonAPContent, ENTITY.AgriParcel);
                string pepe = string.Empty;

                //Enable ENRICHMENT for the polygon calculated (linked)
                btnEnRich.Enabled = true;
                btnEnRich.Visible = true;
            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception updating AgriParcel entities. Error: {0}", ex.Message), LogLevel.Log_Debug);
            }
        }

        public void UpdateAgriParcelDataEnriched(AgriParcel parcel)
        {
            Log.Write("UpdateAgriParcelData enriched with with calculated relationship Id from airQualityObserved entity", LogLevel.Log_Info);

            List<Parcel> aux_parcelas = new List<Parcel>();
            List<CO2Parcel> aux_parcelasCO2 = new List<CO2Parcel>();

            string url = string.Empty;

            try
            {
                HttpClient rclient = new HttpClient();
                rclient.DefaultRequestHeaders.Accept.Clear();
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/ld+json"));
                rclient.DefaultRequestHeaders.Add("Link", "<https://raw.githubusercontent.com/SALTED-Project/contexts/main/wrapped_contexts/agriparcel-context.jsonld>;rel=\"http://www.w3.org/ns/json-ld#context\";type=\"application/ld+json\"");
                //FASE 1. Borramos la entidad
                //url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/"; //from outside
                //url = string.Format("http://140.0.24.127:9090/ngsi-ld/v1/entities/{0}", parcel.id); //internal
                url = string.Format(urlBase + "{0}", parcel.id); //internal
                DeleteRequest(url, rclient);

                //FASE 2. Se crea de nuevo añadiendo el hasAirQualityObserved
                //POST entity
                //url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/"; //from outside
                //url = "http://140.0.24.127:9090/ngsi-ld/v1/entities/"; //internal
                url = urlBase;
                AgriPReqParameters rqAPParams = new AgriPReqParameters();
                //rqAPParams.id = "urn:ngsi-ld:AgriParcel:lorca:SIGPAC_Lorca_20210104_4326:11F214C7A01D5B6C98E3F56A14CD18B39_113";
                rqAPParams.id = parcel.id;
                rqAPParams.type = parcel.tipo;
                rqAPParams.createdAt = parcel.dateCreated;

                rqAPParams.area.value = parcel.area.value;
                rqAPParams.area.unitCode = parcel.area.unitCode;

                rqAPParams.category.value = parcel.categoria.value;

                rqAPParams.hasAgriCrop.@object = parcel.hasAgriCrop.id;

                MPValLocation auxLocation = new MPValLocation();
                List<decimal[,]> listCoord = new List<decimal[,]>();
                for (int c = 0; c < parcel.location.coordinates.Count; c++)
                {
                    auxLocation.coordinates = new List<List<decimal[,]>>();

                    decimal[,] coord = new decimal[parcel.location.coordinates[c].lcoord.Count, 2];
                    for (int d = 0; d < parcel.location.coordinates[c].lcoord.Count; d++)
                    {

                        coord[d, 0] = parcel.location.coordinates[c].lcoord[d].Lon;
                        coord[d, 1] = parcel.location.coordinates[c].lcoord[d].Lat;
                    }
                    listCoord.Add(coord);
                }

                auxLocation.coordinates.Add(listCoord);
                auxLocation.type = parcel.location.subtipo;
                auxLocation.type = "MultiPolygon";

                rqAPParams.location.value = auxLocation;
                rqAPParams.location.type = parcel.location.tipo;

                foreach (string obj in parcel.hasAirQualityObserved.@object)
                {
                    rqAPParams.hasAirQualityObserved.@object.Add(obj);
                }
                if (rqAPParams.hasAirQualityObserved.@object.Count == 0)
                {
                    //Se indica no relationship
                    string none = "urn:ngsi-ld:AirQualityObserved:lorca:NONE";
                    rqAPParams.hasAirQualityObserved.@object.Add(none);
                }
                string jsonStringAP = JsonConvert.SerializeObject(rqAPParams);
                var jsonAPContent = new StringContent(JsonConvert.SerializeObject(rqAPParams), Encoding.UTF8, "application/json");

                //send async POST to the url for adding a entity
                PostRequest(url, rclient, jsonAPContent, ENTITY.AgriParcel, rqAPParams.id);
                //// ended FASE 2
                ///
                string pepe = string.Empty;

                //Disable ENRICHMENT for the polygon calculated (linked)
                btnEnRich.Enabled = false;
                btnEnRich.Visible = false;
            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception updating AgriParcel enriched entity. Error: {0}", ex.Message), LogLevel.Log_Debug);
            }
        }

        public void AddAirQualityObservedToSCORPIO(AirQualityObserved aqo)
        {
            Log.Write("CO2: Adding calculated AirQualityObserved entity to SCORPIO", LogLevel.Log_Info);

            string url = string.Empty;

            try
            {
                //This method is used for injecting (creating) AirQualitObserved entities to Scorpio CB
                HttpClient rclient = new HttpClient();
                rclient.DefaultRequestHeaders.Accept.Clear();
                //rclient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/ld+json"));
                rclient.DefaultRequestHeaders.Add("Link", "<https://raw.githubusercontent.com/SALTED-Project/contexts/main/wrapped_contexts/airqualityobserved-context.jsonld>;rel=\"http://www.w3.org/ns/json-ld#context\";type=\"application/ld+json\"");
                //rclient.DefaultRequestHeaders.Add("Link", "<https://smartdatamodels.org/context.jsonld>;rel=\"http://www.w3.org/ns/json-ld#context\";type=\"application/ld+json\"");
                //GET with link//rclient.DefaultRequestHeaders.Add("Link", "<https://smartdatamodels.org/context.jsonld>;rel=\"http://www.w3.org/ns/json-ld#context\";type=\"application/ld+json\"");
                // url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/?type=AirQualityObserved";
                // url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/types";
                //GetRequest(url, rclient);

                //POST entity
                //url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/"; //from outside
                url = "http://140.0.24.127:9090/ngsi-ld/v1/entities/"; //internal

                #region ejemplo
                ////////////////////////////////////////////////////////
                /////url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/?type=AirQualityObserved";
                ///{
                //    "@context": [
                //                  "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld",
                //	                "https://smartdatamodels.org/context.jsonld",  
                // 	                "https://raw.githubusercontent.com/smart-data-models/dataModel.Environment/master/context.jsonld"
                //    ]

                //      “id": "urn: ngsi - ld:AirQualityObserved: lorca: B3F76EA170D030BCD9E036DCC9BEA22B",
                //      "type": "AirQualityObserved",  
                //      “CO2": {

                //                  "type": "Property",  
                //	                "value":0.34,
                //	                “unitCode”:”28”
                //	            },
                //      "dateObserved": {
                //                      "type": "Property",  
                //	                    "value": "2016-09-30T00:00:00"

                //              },
                //      “areaServed”: { 
                //	                    “type”: “Property”.
                //	                    “value”: “LORCA”
                //	            },
                //      "location": {
                //                      "type": "GeoProperty",  
                //        	            "value": {
                //                                  "type": "Polygon",
                //	                                "coordinates": [ [ [ -1.82500101488498, 37.96917 ],
                //		                                               [ -1.82500101488498, 37.968336666666154 ],
                //			                                           [ -1.824167686062246, 37.968336666666154 ],
                // 			                                           [ -1.824167686062246, 37.96917 ],
                // 			                                           [ -1.82500101488498, 37.96917 ] ] ]
                //        		                }
                //                  }
                //   }

                ////////////////////////////////////////////////////////
                ///
                #endregion

                AQOReqParameters rqParams = new AQOReqParameters();
                postAQO = false;
                //@context
                //rqParams.context.Add("https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld");
                //rqParams.context.Add("https://smartdatamodels.org/context.jsonld");
                //rqParams.context.Add("https://raw.githubusercontent.com/smart-data-models/dataModel.Environment/master/context.jsonld");

                rqParams.co2 = aqo.co2;
                rqParams.areaserved = aqo.areaserved;
                rqParams.dateObserved = aqo.dateObserved;
                rqParams.id = aqo.id;
                rqParams.type = aqo.tipo;
                rqParams.location.value.type = aqo.location.subtipo;
                rqParams.location.value.coordinates = new List<decimal[,]>();
                decimal[,] coord = new decimal[aqo.location.coordinates.Count, 2];
                for (int i = 0; i < aqo.location.coordinates.Count; i++)
                {
                    coord[i, 0] = aqo.location.coordinates[i].Lon;
                    coord[i, 1] = aqo.location.coordinates[i].Lat;
                }
                rqParams.location.value.coordinates.Add(coord);
                rqParams.location.value.type = aqo.location.subtipo;

                string jsonString = JsonConvert.SerializeObject(rqParams);

                var jsonContent = new StringContent(JsonConvert.SerializeObject(rqParams), Encoding.UTF8, "application/json");
                //send async POST to the url for adding a entity
                PostRequest(url, rclient, jsonContent, ENTITY.AirQualityObserved, rqParams.id);

                Log.Write("CO2: Injected data to LOCAL SCORPIO", LogLevel.Log_Info);

            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception injecting CO2 AirQualityObserved data when enriching data. Error: {0}", ex.Message), LogLevel.Log_Debug);
            }
        }
        public double findMin(IReadOnlyCollection<IPosition> p, string coord)
        {
            if (p.Count == 0)
            {
                return 0;
            }

            double min = double.MaxValue;
            foreach (var i in p)
            {
                if (coord == "Lat")
                {
                    if (i.Latitude < min)
                    {
                        min = i.Latitude;
                    }
                }
                else if (coord == "Lon")
                {
                    if (i.Longitude < min)
                    {
                        min = i.Longitude;
                    }
                }
                else
                {
                    return 0;
                }
            }
            return min;
        }

        public double findMax(IReadOnlyCollection<IPosition> p, string coord)
        {
            if (p.Count == 0)
            {
                return 0;
            }

            double max = double.MinValue;
            foreach (var i in p)
            {
                if (coord == "Lat")
                {
                    if (i.Latitude > max)
                    {
                        max = i.Latitude;
                    }
                }
                else if (coord == "Lon")
                {
                    if (i.Longitude > max)
                    {
                        max = i.Longitude;
                    }
                }
                else
                {
                    return 0;
                }
            }
            return max;
        }

    }
}
