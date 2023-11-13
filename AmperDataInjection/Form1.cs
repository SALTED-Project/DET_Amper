using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Timers;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using amperUtil;
using amperUtil.Log;
using amperUtil.Http;
using AmperCore;
using RestSharp;
using GeoJSON.Net.Geometry;




namespace SALTED_DataInjection
{

    public delegate void OnLog(int level, string log);

    public partial class Form1 : Form
    {
        ExceptionSubscriber exceptionSubscriber;
        #region properties

        ThreadLogConfig m_threadLogConfig;
        ThreadLog m_threadLog;

        HttpSender httpSender;

        private static string projectName = "SALTED_DATA_INJECTION";
        public string co2FileName { get; set; }
        public string sigpacFileName { get; set; }

        public List<CO2Data> listCO2Data;
        public List<SIGPACData> listParcelData;

        public List<AirQualityObserved> listAirQualityObserved;
        public List<AgriParcel> listAgriParcel;
        public List<AgriCrop> listAgriCrop;

        public string urlBase { get; set; }

        public string observedArea { get; set; }
        public string sigpacName { get; set; }
        public string dateObservedInicio { get; set; }
        public string dateObservedFinal { get; set; }
        public string uriScorpio { get; set; }
        public int scbIntPort { get; set; }
        public int scbExtPort { get; set; }
        public string api { get; set; }

        public Int64 indIni { get; set; }
        public Int64 indFin { get; set; }

        public string usoSIGPAC { get; set; }

        #endregion

        public bool postAC = false;
        public bool postAP = false;
        public bool postAQO = false;

        System.Timers.Timer miTimer;

        protected static OnLog LogMethod;

        public Form1()
        {
            Log.Write("MPSProxy::MPSProxy1", LogLevel.Log_Debug);

            InitializeComponent();

            projectName = Assembly.GetExecutingAssembly().GetName().Name;

            AppDomain.CurrentDomain.UnhandledException += HandleException;
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directoryPath = System.IO.Path.GetDirectoryName(location);

            urlBase = string.Empty;

            ReadConfig();

            //entry data
            listCO2Data = new List<CO2Data>();
            listParcelData = new List<SIGPACData>();

            //smart data models
            listAirQualityObserved = new List<AirQualityObserved>();
            listAgriParcel = new List<AgriParcel>();
            listAgriCrop = new List<AgriCrop>();


            miTimer = new System.Timers.Timer(1000);
            miTimer.Elapsed += new ElapsedEventHandler(blinkingLabel);
            miTimer.Enabled = false;
            miTimer.AutoReset = true; //the event is launched each time the time is elapsed

            picBox1.Visible = false;
            picBox2.Visible = false;
            picBox3.Visible = false;
            picBox4.Visible = false;

            btnCO2.Enabled = false;
            btnSIGPAC.Enabled = false;

            btnLink.Enabled = false;

            //picBox1.VisibleChanged += PictureBox1OnVisibleChanged;
            httpSender = new HttpSender(uriScorpio + ":" + scbIntPort + "/" + api + "/");
            Launch_ThreadLog();
        }


        #region ThreadLog
        private void Launch_ThreadLog()
        {
            Stop_ThreadLog();

            m_threadLog = new ThreadLog(m_threadLogConfig, exceptionSubscriber);
            m_threadLog.Start(ThreadNames.ThreadName_Log, false);
        }
        private void RequestStop_ThreadLog()
        {
            if (m_threadLog != null)
            {
                m_threadLog.RequestStopThread();
                System.Threading.Thread.Yield();
            }
        }
        private void Stop_ThreadLog()
        {
            if (m_threadLog != null)
            {
                m_threadLog.StopThread();
                m_threadLog = null;
            }
        }
        #endregion


        public void ReadConfig()
        {
            #region threadLogConfig
            m_threadLogConfig = new ThreadLogConfig
            {
                m_logDirectory = ConfigurationManager.AppSettings["logDirectory"],
                m_logLevel = Convert.ToInt32(ConfigurationManager.AppSettings["logLevel"]),
                m_numberOfFiles = Convert.ToInt32(ConfigurationManager.AppSettings["numberOfFiles"]),
                m_numberOfEntriesPerFile = Convert.ToInt32(ConfigurationManager.AppSettings["numberOfEntriesPerFile"]),
                m_daysLogPersistence = Convert.ToInt32(ConfigurationManager.AppSettings["daysLogPersistence"])
            };
            #endregion

            observedArea = ConfigurationManager.AppSettings["observedArea"];
            dateObservedInicio = ConfigurationManager.AppSettings["dateObservedIni"];
            dateObservedFinal = ConfigurationManager.AppSettings["dateObservedEnd"];
            uriScorpio = ConfigurationManager.AppSettings["scorpioCBLocal"];
            scbIntPort = Convert.ToInt32(ConfigurationManager.AppSettings["scorpioInPort"]);
            scbExtPort = Convert.ToInt32(ConfigurationManager.AppSettings["scorpioExtPort"]);
            api = ConfigurationManager.AppSettings["api"];
            urlBase = ConfigurationManager.AppSettings["urlBase"];
            usoSIGPAC = ConfigurationManager.AppSettings["UsoSIGPAC"];
        }

        #region Logs
        public static void LogDebug(string log)
        {
            LogDebug(log, null);
        }

        public static void LogDebug(string log, Exception e)
        {
            if (LogMethod != null)
                LogMethod(0, projectName + " -> " + log + ExceptionToString(e));
        }

        public static void LogInfo(string log)
        {
            LogInfo(log, null);
        }

        public static void LogInfo(string log, Exception e)
        {
            if (LogMethod != null)
                LogMethod(1, projectName + " -> " + log + ExceptionToString(e));
        }

        public static void LogWarn(string log)
        {
            LogWarn(log, null);
        }

        public static void LogWarn(string log, Exception e)
        {
            if (LogMethod != null)
                LogMethod(2, projectName + " -> " + log + ExceptionToString(e));
        }

        public static void LogError(string log)
        {
            LogError(log, null);
        }

        public static void LogError(string log, Exception e)
        {
            if (LogMethod != null)
                LogMethod(3, projectName + " -> " + log + ExceptionToString(e));
        }

        public static void LogCritical(string log)
        {
            LogCritical(log, null);
        }

        public static void LogCritical(string log, Exception e)
        {
            if (LogMethod != null)
                LogMethod(4, projectName + " -> " + log + ExceptionToString(e));
        }

        private static string ExceptionToString(Exception e)
        {
            if (e != null)
            {
                return Environment.NewLine + e.GetType().Name + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace;
            }

            return string.Empty;
        }

        #endregion


        private static void HandleException(object sender, UnhandledExceptionEventArgs args)
        {
            if (args != null && args.ExceptionObject is Exception)
            {
                LogCritical("Unhandled exception, terminating = " + args.IsTerminating, (Exception)args.ExceptionObject);
            }
            else
            {
                LogCritical(string.Format("Unhandled exception, terminating = {0}", args != null ? args.IsTerminating.ToString() : "Unknown"));
            }
        }

        public class ThreadNames
        {
            //public static string ThreadName_Auth = "Thread_Auth";
            //public static string ThreadName_Back = "Thread_Back";
            public static string ThreadName_Log = "Thread_Log";
            //public static string ThreadName_Logic = "Thread_Logic";
        }

        #region Injection
        /// <summary>
        /// Read CO2 dataset
        /// Map dataset to Smart Data Model AirQualityObserved
        /// Inject data to Scorpio Broker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCO2_Click(object sender, EventArgs e)
        {
            Log.Write("CO2: Reading data mapped and injecting them to LOCAL SCORPIO", LogLevel.Log_Info);

            string nombreFichero = string.Empty;
            string nombreFicheroCompleto = string.Empty;
            string nombre = string.Empty;

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

                //rclient.DefaultRequestHeaders.Add("Link", "<https://smartdatamodels.org/context.jsonld>;rel=\"http://www.w3.org/ns/json-ld#context\";type=\"application/ld+json\"");
                //GET with link//rclient.DefaultRequestHeaders.Add("Link", "<https://smartdatamodels.org/context.jsonld>;rel=\"http://www.w3.org/ns/json-ld#context\";type=\"application/ld+json\"");
                // url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/?type=AirQualityObserved";
                // url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/types";
                //GetRequest(url, rclient);

                //POST entity
                //url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/"; //from outside
                //url = "http://140.0.24.127:9090/ngsi-ld/v1/entities/"; //internal
                url = urlBase;

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
                //rqParams.context.Add("https://raw.githubusercontent.com/SALTED-Project/contexts/main/wrapped_contexts/airqualityobserved-context.jsonld");
                Int64 ini = Convert.ToInt64(textBox1.Text);
                Int64 fin = Convert.ToInt64(textBox2.Text);
                Int64 index = 0;
                foreach (AirQualityObserved aqo in listAirQualityObserved)
                {
                    if (index >= ini && index <= fin)
                    {
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
                        //index++;
                    }
                    index++;
                    if (postAQO) break;
                }
                //string jsonString = JsonConvert.SerializeObject(rqParams);

                Log.Write("CO2: Injected data to LOCAL SCORPIO", LogLevel.Log_Info);
                
                //var jsonContent = new StringContent(JsonConvert.SerializeObject(rqParams), Encoding.UTF8, "application/json");
                ////send async POST to the url for adding a entity
                //PostRequest(url, rclient, jsonContent, ENTITY.AirQualityObserved);

            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception injecting CO2 AirQualityObserved data in btnCO2_Click. Error: {0}", ex.Message), LogLevel.Log_Debug);
            }
   
        }
        private async void GetTesting(string requestUrl, HttpClient client)
        {
            string result = string.Empty;
            try
            {
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string payload = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void GetTestingRest(RestRequest requestUrl, RestClient client)
        {
            string result = string.Empty;
            try
            {
                RestResponse response = await client.ExecuteGetAsync(requestUrl);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string payload = response.ToString();
                }
                else
                {
                    string error = "ERROR";
                }
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// Read SIGPAC dataset
        /// Map dataset to Smart Data Model AgriParcel
        /// Create an AgriCrop entity for each AgriParcel entity
        /// Inject data to Scorpio Broker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSIGPAC_Click(object sender, EventArgs e)
        {
            Log.Write("SIGPAC: Reading data mapped and inject them to LOCAL SCORPIO", LogLevel.Log_Info);

            string url = string.Empty;

            try
            {
                //For each AgriParcel entity created, it is mandatory to create an AgriCrop entity for the type of AgriParcel
                //This method is used for injecting (creating) AgriParcel entities to Scorpio CB. Also AgriAgriCrop entities will be creted when needed
                HttpClient rclient = new HttpClient();
                rclient.DefaultRequestHeaders.Accept.Clear();
                //rclient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/ld+json"));
                rclient.DefaultRequestHeaders.Add("Link", "<https://raw.githubusercontent.com/SALTED-Project/contexts/main/wrapped_contexts/agricrop-context.jsonld>;rel=\"http://www.w3.org/ns/json-ld#context\";type=\"application/ld+json\"");
                //rclient.DefaultRequestHeaders.Add("Link", "<https://smartdatamodels.org/context.jsonld>;rel=\"http://www.w3.org/ns/json-ld#context\";type=\"application/ld+json\"");

                //GET with link//rclient.DefaultRequestHeaders.Add("Link", "<https://smartdatamodels.org/context.jsonld>;rel=\"http://www.w3.org/ns/json-ld#context\";type=\"application/ld+json\"");
                // url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/?type=AirQualityObserved";
                // url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/types";
                //GetRequest(url, rclient);

                //POST entity
                //url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/"; //from outside
                //url = "http://140.0.24.127:9090/ngsi-ld/v1/entities/"; //internal
                url = urlBase;

                //1. Añadir las entidades AgriCrop necesarias para las AgriParcel nuevas
                AgriCReqParameters rqACParams = new AgriCReqParameters();
                postAC = false;
                Int64 ini = Convert.ToInt64(textBox1.Text);
                Int64 fin = Convert.ToInt64(textBox2.Text);
                Int64 index = 0;

                foreach (AgriCrop acrop in listAgriCrop)
                {
                    rqACParams.id = acrop.id;
                    rqACParams.name = acrop.nombre;
                    rqACParams.createdAt = acrop.dateCreated;
                    rqACParams.type = acrop.tipo;

                    string jsonStringAC = JsonConvert.SerializeObject(rqACParams);
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(rqACParams), Encoding.UTF8, "application/json");
                    //send async POST to the url for adding a entity
                    PostRequest(url, rclient, jsonContent, ENTITY.AgriCrop, rqACParams.id);

                    if (postAC) break;
                }

                Log.Write("AgriCrop: Injected data to LOCAL SCORPIO", LogLevel.Log_Info);

                //string jsonStringAC = JsonConvert.SerializeObject(rqACParams);
                //var jsonContent = new StringContent(JsonConvert.SerializeObject(rqACParams), Encoding.UTF8, "application/json");
                ////send async POST to the url for adding a entity
                //PostRequest(url, rclient, jsonContent);

                //2. Añadir las entidades AgriParcel correspondientes
                //AgriPReqParameters rqAPParams = new AgriPReqParameters();
                postAP = false;
                rclient.DefaultRequestHeaders.Remove("Link");
                rclient.DefaultRequestHeaders.Add("Link", "<https://raw.githubusercontent.com/SALTED-Project/contexts/main/wrapped_contexts/agriparcel-context.jsonld>;rel=\"http://www.w3.org/ns/json-ld#context\";type=\"application/ld+json\"");
                //return;
                index = 0;
                foreach (AgriParcel aparcel in listAgriParcel)
                {
                    string[] auxCategory = aparcel.categoria.value.Split(':');
                    if (usoSIGPAC.Contains(auxCategory[0]))
                        continue;

                    if (index >= ini && index <= fin)
                    {
                        AgriPReqParameters rqAPParams = new AgriPReqParameters();
                        rqAPParams.id = aparcel.id;
                        rqAPParams.type = aparcel.tipo;
                        rqAPParams.createdAt = aparcel.dateCreated;

                        rqAPParams.area.value = aparcel.area.value;
                        rqAPParams.area.unitCode = aparcel.area.unitCode;

                        rqAPParams.category.value = aparcel.categoria.value;

                        rqAPParams.hasAgriCrop.@object = aparcel.hasAgriCrop.id;

                        //if (aparcel.location.subtipo == "Polygon")
                        //{
                        //    //string poligono = "Es un poligono";
                        //    continue;
                        //}

                        MPValLocation auxLocation = new MPValLocation();
                        List<decimal[,]> listCoord = new List<decimal[,]>();
                        for (int c = 0; c < aparcel.location.coordinates.Count; c++)
                        {
                            //MPValLocation auxLocation = new MPValLocation();
                            auxLocation.coordinates = new List<List<decimal[,]>>();
                            //List<decimal[,]> listCoord = new List<decimal[,]>();

                            decimal[,] coord = new decimal[aparcel.location.coordinates[c].lcoord.Count, 2];
                            for (int d = 0; d < aparcel.location.coordinates[c].lcoord.Count; d++)
                            {

                                coord[d, 0] = aparcel.location.coordinates[c].lcoord[d].Lon;
                                coord[d, 1] = aparcel.location.coordinates[c].lcoord[d].Lat;


                            }
                            listCoord.Add(coord);
                            //auxLocation.coordinates.Add(listCoord);
                            //auxLocation.type = aparcel.location.subtipo;

                            //rqAPParams.location.value.Add(auxLocation);
                            //rqAPParams.location.type = aparcel.location.tipo;
                        }
                        auxLocation.coordinates.Add(listCoord);
                        auxLocation.type = aparcel.location.subtipo;
                        auxLocation.type = "MultiPolygon";
                        rqAPParams.location.value = auxLocation;
                        rqAPParams.location.type = aparcel.location.tipo;

                        //rqAPParams.location.value.coordinates = new List<decimal[,]>();
                        ////aparcel.location.coordinates.Count
                        //for (int l = 0; l < aparcel.location.coordinates.Count; l++)
                        //{
                        //    decimal[,] coord = new decimal[aparcel.location.coordinates.Count, 2];
                        //    for (int i = 0; i < aparcel.location.coordinates[l].lcoord.Count; i++)
                        //    {
                        //        coord[i, 0] = aparcel.location.coordinates[l].lcoord[i].Lon;
                        //        coord[i, 1] = aparcel.location.coordinates[l].lcoord[i].Lat;
                        //    }
                        //    rqAPParams.location.value.coordinates.Add(coord);
                        //    rqAPParams.location.value.type = aparcel.location.subtipo;
                        //}

                        rqAPParams.hasAirQualityObserved.@object.Add("urn:ngsi-ld:AirQualityObserved:lorca:NONE");

                        string jsonStringAP = JsonConvert.SerializeObject(rqAPParams);
                        //rqAPParams.location.value.type = "Polygon";
                        var jsonAPContent = new StringContent(JsonConvert.SerializeObject(rqAPParams), Encoding.UTF8, "application/json");
                        //send async POST to the url for adding a entity
                        PostRequest(url, rclient, jsonAPContent, ENTITY.AgriParcel, rqAPParams.id);
                        index++;
                    }
                    else index++;

                    if (postAP) break;
                }
                //string jsonStringAP = JsonConvert.SerializeObject(rqAPParams);
                //var jsonAPContent = new StringContent(JsonConvert.SerializeObject(rqAPParams), Encoding.UTF8, "application/json");
                ////send async POST to the url for adding a entity
                //PostRequest(url, rclient, jsonAPContent, ENTITY.AgriParcel);
                Log.Write("SIGPAC: Injected data to LOCAL SCORPIO", LogLevel.Log_Info);

            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception injecting SIGPAC AgriParcel data in btnSIGPAC_Click. Error: {0}", ex.Message), LogLevel.Log_Debug);
            }
        }
        #endregion

        private void txtCO2_TextChanged(object sender, EventArgs e)
        {
            //openFileDialog1.ShowDialog();
        }

        #region Mapping
        private void txtCO2_Click(object sender, EventArgs e)
        {
            //Select the file for reading CO2 footprint dataset
            openFileDialog1.ShowDialog();
            txtCO2.Text = openFileDialog1.FileName;
        }

        private void txtSIGPAC_Click(object sender, EventArgs e)
        {
            //Select file for reading parcel information from SIGPAC
            openFileDialog1.ShowDialog();
            txtSIGPAC.Text = openFileDialog1.FileName;
        }

        private void btnCO2Map_Click(object sender, EventArgs e)
        {
            //Map data from footprint CO2 file to AirQualityObserved smart data model
            Log.Write("btnCO2Map::Map data form footprint CO2 file to AirQualityObserved smart data model", LogLevel.Log_Info);

            string nombreFichero = string.Empty;
            string nombreFicheroCompleto = string.Empty;
            string nombre = string.Empty;

            //button1.Focus();

            btnCO2Map.BackColor = System.Drawing.Color.LightGray;
            btnCO2Map.ForeColor = System.Drawing.Color.Black;
            btnCO2Map.Text = "DATA MAPPING";
            //picBox1.Hide();
            //picBox2.Hide();

            //lblAlarm.Visible = false;
            //lblAlarm.Hide();

            ////Coordinates pa0 = new Coordinates();
            ////pa0.Lat = -2; pa0.Lon = 5;
            ////Coordinates pa1 = new Coordinates();
            ////pa1.Lat = 1; pa1.Lon = -1;
            ////Coordinates pb0 = new Coordinates();
            ////pb0.Lat = -2; pb0.Lon = 0;
            ////Coordinates pb1 = new Coordinates();
            ////pb1.Lat = -1; pb1.Lon = 3;

            ////bool pi = false;
            ////pi = PolygonIntersection(pa0, pa1, pb0, pb1);

            List<CO2Data> co2Data = new List<CO2Data>();

            try
            {
                if (!File.Exists(txtCO2.Text))
                {
                    lblAlarm.Text = string.Format("Selected file {0} doesn`t exists", txtCO2.Text);
                    lblAlarm.Visible = true;
                    miTimer.Enabled = true;
                    miTimer.Start();
                    return; //No se encuentra el fichero y salimos. se muestra mensaje de error
                }

                //Se procesa el archivo y se guarda la información en una lista de variables del tipo del fichero
                string fileName = txtCO2.Text;
                foreach (string fl in File.ReadLines(fileName))
                {
                    nombre = fl;
                    if (fl.Contains(SALTEDTags.GEOMETRY))
                    {
                        string auxLine = string.Empty;
                        auxLine = fl.Substring(0, fl.Length);
                        if (fl.EndsWith(",")) auxLine = fl.Substring(0, fl.Length - 1);

                        JObject jo = JObject.Parse(auxLine);
                        //Extrae datos del fichero a la lista de datos de CO2
                        ParseJSObjectToCO2(jo);
                    }
                }
                //string jsonString = File.ReadAllText(fileName);

                //JObject jo = JObject.Parse(jsonString);

                //Extrae datos del fichero a la lista de datos de CO2
                //ParseJSObjectToCO2(jo);

                label7.Text = listCO2Data.Count.ToString();
                //Rellena los datos en el modelo AirQualityObserved
                MapCO2ToAirQuality();
            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception leyendo fichero {} en btnCO2Map_Click. Error: {1}", txtCO2.Text, ex.Message), LogLevel.Log_Debug);
            }
        }


        private void btnSIGPACMap_Click(object sender, EventArgs e)
        {
            //Map data from SIGPAC file to AgriParcel smart data model
            Log.Write("btnSIGPACMap::Map data form SIGPAC file to AgriParcel smart data model", LogLevel.Log_Info);

            string nombreFichero = string.Empty;
            string nombreFicheroCompleto = string.Empty;
            string nombre = string.Empty;

            lblAlarm.Visible = false;
            lblAlarm.Hide();

            try
            {
                if (!File.Exists(txtSIGPAC.Text))
                {
                    lblAlarm.Text =string.Format("Selected file {0} doesn`t exists", txtSIGPAC.Text);
                    lblAlarm.Visible = true;
                    miTimer.Enabled = true;
                    miTimer.Start();
                    return; //No se encuentra el fichero y salimos. se muestra mensaje de error
                }

                //Se procesa el archivo y se guarda la información en una lista de variables del tipo del fichero
                string fileName = txtSIGPAC.Text;
                foreach(string fl in File.ReadLines(fileName))
                {
                    nombre = fl;
                    if (fl.Contains(SALTEDTags.AREA))
                    {
                        string auxLine = string.Empty;
                        auxLine = fl.Substring(0, fl.Length);
                        if (fl.EndsWith(",")) auxLine = fl.Substring(0, fl.Length - 1);
                        
                        JObject jo = JObject.Parse(auxLine);
                        //Extrae datos del fichero a la lista de datos de SIGPAC
                        ParseJSObjectToParcel(jo);
                    }
                    else if (fl.Contains(SALTEDTags.SIGPAC_CRS))
                    {
                        continue;
                    }
                    else if(fl.Contains(SALTEDTags.SIGPAC_NAME))
                    {
                        string auxLine = string.Empty;
                        auxLine = fl.Substring(0, fl.Length);
                        string[] auxName = auxLine.Split(':');
                        sigpacName = auxName[1];
                        sigpacName = sigpacName.Replace(" ", "");
                        sigpacName = sigpacName.Replace("\"", "");
                        sigpacName = sigpacName.Replace(",", "");
                        string [] name = sigpacName.Split('_');
                        sigpacName = string.Empty;
                        for (int i = 0; i < name.Length - 2; i++)
                        {
                            sigpacName += name[i] + "_";
                        }
                        sigpacName = sigpacName.Substring(0, sigpacName.Length - 1);
                    }
                }

                label8.Text = listParcelData.Count.ToString();

                //Rellena los datos en el modelo AgriParcel
                MapParcelToAgriParcel();

                btnLink.Enabled = true;
            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception leyendo fichero {} en btnSIGPACMap_Click. Error: {1}; Dato: {2}", txtSIGPAC.Text, ex.Message, nombre), LogLevel.Log_Debug);
            }
        }

        private void blinkingLabel(object source, ElapsedEventArgs e)
        {
            if (lblAlarm.ForeColor == System.Drawing.Color.Red) lblAlarm.ForeColor = System.Drawing.Color.BlueViolet;
            else lblAlarm.ForeColor = System.Drawing.Color.Red;
        }
        #endregion

        private void lblAlarm_Click(object sender, EventArgs e)
        {
            miTimer.Enabled = false;
            miTimer.Stop();
            lblAlarm.Text = "";
        }

        #region Linking and Enrichment
        //Para el linkado, se deben leer las entidades de tipo AirQualityObserved y AgriParcel. Una vez en memoria se calcula la intersección
        //de los polígonos para determinar la relación entre ambas entidades
        #endregion

        //private void PictureBox1OnVisibleChanged(object sender, EventArgs e)
        //{
        //    picBox1.VisibleChanged -= PictureBox1OnVisibleChanged;
        //    picBox1.Visible = false;
        //    picBox1.VisibleChanged += PictureBox1OnVisibleChanged;
        //}


        #region API NGSI-LD
        //Funciones para lectura y escritura de datos de/en la plataforma SCORPIO CB 
        private async void GetRequest(string requestUrl, HttpClient rclient)
        {
            string result = string.Empty;

            try
            {
                HttpResponseMessage response = await rclient.GetAsync(requestUrl);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string payload = await response.Content.ReadAsStringAsync();
                    string res = payload;

                    //get var list from JSON message
                    JObject jo = JObject.Parse(payload);
                }
                else
                {
                    //AGCInizialzated = false;
                    Log.Write(string.Format("NOK Reading varlues from {0}: StausCode {1}: Reason {2}", "SCADA SHERPA S6000", response.StatusCode, response.ReasonPhrase), LogLevel.Log_Error);
                    
                }
            }
            catch (Exception ex)
            {
                string fh = DateTime.Now.ToString();
                result = "Exception getting variable list from SCADA: \n\"" + ex.Message + "\"";
                Log.Write(string.Format("{2}: Failed reading variable list from SCADA: server  [{0}]. / Message:  {1}", requestUrl, result, fh), LogLevel.Log_Debug);
            }
        }
        private async void PostRequest(string requestUrl, HttpClient rclient, HttpContent content, ENTITY ent, string id)
        {
            Log.Write("PostRequest method. Used to add a entity to SCORPIO CB", LogLevel.Log_Info);

            string result = string.Empty;

            try
            {
                HttpResponseMessage response = await rclient.PostAsync(requestUrl, content);
                if ((response.StatusCode == System.Net.HttpStatusCode.OK) || (response.StatusCode == System.Net.HttpStatusCode.Created) ||
                    (response.StatusCode == System.Net.HttpStatusCode.Accepted))
                {
                    string payload = await response.Content.ReadAsStringAsync();
                    string res = payload;

                    Log.Write(string.Format("POST method {0}: StatusCode {1}: Reason {2}: Entity: {3}: Id {4}",
                        "SALTED Amper data injection.PostRequest()", response.StatusCode, response.ReasonPhrase, ent.ToString(), id), LogLevel.Log_Error);
                }
                else
                {
                    //Request failed
                    //if (ent == ENTITY.AirQualityObserved) postAQO = true;
                    //else if (ent == ENTITY.AgriCrop) postAC = true;
                    //else if (ent == ENTITY.AgriParcel) postAP = true;

                    Log.Write(string.Format("Error in POST method {0}: StatusCode {1}: Reason {2}: Entity: {3}: Id {4}",
                        "SALTED Amper data injection.PostRequest()", response.StatusCode, response.ReasonPhrase, ent.ToString(), id), LogLevel.Log_Error);

                }
                //Log.Write(string.Format("Post request perfomed OK: StatusCode {0}: Reason {1}", response.StatusCode, response.ReasonPhrase),LogLevel.Log_Info);
            }
            catch (Exception ex)
            {
                //if (ent == ENTITY.AirQualityObserved) postAQO = true;
                //else if (ent == ENTITY.AgriCrop) postAC = true;
                //else if (ent == ENTITY.AgriParcel) postAP = true;

                string fh = DateTime.Now.ToString();
                result = "Exception in POST method to Scorpio CB: \n\"" + ex.Message + "\"";
                Log.Write(string.Format("{2}: Failed injecting data to SALTED platform  [{0}]. / Message:  {1}", requestUrl, result, fh), LogLevel.Log_Debug);
            }
        }
        private async void DeleteRequest(string requestUrl, HttpClient rclient)
        {
            Log.Write("DeleteRequest method. Used to delete a entity from SCORPIO CB", LogLevel.Log_Info);

            string result = string.Empty;

            try
            {
                HttpResponseMessage response = await rclient.DeleteAsync(requestUrl);
                if ((response.StatusCode == System.Net.HttpStatusCode.OK) || (response.StatusCode == System.Net.HttpStatusCode.Created) ||
                    (response.StatusCode == System.Net.HttpStatusCode.Accepted))
                {
                    string payload = await response.Content.ReadAsStringAsync();
                    string res = payload;

                    Log.Write(string.Format("DELETE method: StausCode {0}: Reason {1}", "SALTED Amper data injection.PostRequest()", response.StatusCode, response.ReasonPhrase), LogLevel.Log_Error);
                }
                else
                {
                    //Request failed

                    Log.Write(string.Format("Error in DELETE method: StausCode {1}: Reason {2}", "SALTED Amper data injection.PostRequest()", response.StatusCode, response.ReasonPhrase), LogLevel.Log_Error);

                }

                Log.Write(string.Format("Delete request perfomed OK: StausCode {0}: Reason {1}", response.StatusCode, response.ReasonPhrase), LogLevel.Log_Info);
            }
            catch (Exception ex)
            {
                string fh = DateTime.Now.ToString();
                result = "Exception in DEELTE method to Scorpio CB: \n\"" + ex.Message + "\"";
                Log.Write(string.Format("{2}: Failed deleting data from SALTED platform  [{0}]. / Message:  {1}", requestUrl, result, fh), LogLevel.Log_Debug);
            }
        }
        private async void PatchRequest(string requestUrl, HttpClient rclient, HttpContent content, ENTITY ent)
        {
            Log.Write("PatchRequest method. Used to update an AgriParcel entity to SCORPIO CB", LogLevel.Log_Info);

            string result = string.Empty;
            HttpResponseMessage responseMessage;
            try
            {
                var root = new
                {
                    fields = new Dictionary<string, string>
                    {
                        {"hasAirQualityObserved", "TRIPLEMMM" }
                    }
                };
                var s = new JsonSerializerSettings { DateFormatHandling = DateFormatHandling.MicrosoftDateFormat };
                var mi_content = JsonConvert.SerializeObject(root, s);
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUrl);

                request.Content = new StringContent(mi_content, System.Text.Encoding.UTF8, "application/json;odata=verbose");
                responseMessage = await rclient.SendAsync(request);
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                var method = "PATCH";
                var httpVerb = new HttpMethod(method);
                var httpRequestMessage =
                    new HttpRequestMessage(httpVerb, requestUrl)
                    {
                        Content = content
                    };

                var response = await rclient.SendAsync(httpRequestMessage);
                if (!response.IsSuccessStatusCode)
                {
                    var responseCode = response.StatusCode;
                    var responseJson = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Log.Write(string.Format("Error in PATCH method {0}: StausCode {1}: Reason {2}", "SALTED Amper data linking.PatchRequest()",
                        response.StatusCode, response.ReasonPhrase), LogLevel.Log_Error);
                }
                Log.Write(string.Format("Post request perfomed OK: StausCode {0}: Reason {1}", response.StatusCode, response.ReasonPhrase), LogLevel.Log_Info);
            }
            catch (Exception ex)
            {
                string fh = DateTime.Now.ToString();
                result = "Exception in POST method to Scorpio CB: \n\"" + ex.Message + "\"";
                Log.Write(string.Format("{2}: Failed linking data to SALTED platform  [{0}]. / Message:  {1}", requestUrl, result, fh), LogLevel.Log_Debug);
            }
        }
        private async void PatchTesting(string requestUrl, HttpClient rclient, HttpContent content)
        {
            try
            {
                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(method, requestUrl)
                {
                    Content = content
                };

                var response = await rclient.SendAsync(request);
                if ((response.StatusCode == System.Net.HttpStatusCode.OK) || (response.StatusCode == System.Net.HttpStatusCode.Created) ||
                    (response.StatusCode == System.Net.HttpStatusCode.Accepted))
                {
                    int i = 200;
                }
                else
                {
                    int j = 500;
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
            }
        }

        #endregion



        private void btnLink_Click(object sender, EventArgs e)
        {
            //Activa la opción para el enlace de datos de parcelas de SIGPAC con los de CO2

            Log.Write("Start link data action: btnLink_CLick()", LogLevel.Log_Info);

            UInt64 np = 0, nps = 0;
            UInt64 nco = 0, npc = 0;
            bool intersection = false;

            try
            {
                List<Parcel> parcelas = new List<Parcel>();
                List<CO2Parcel> parcelasCO2 = new List<CO2Parcel>();

                int poligono = Convert.ToInt32(txtNPolygon.Text);

                foreach (AgriParcel ap in listAgriParcel)
                {
                    string[] auxCategory = ap.categoria.value.Split(':');
                    if (usoSIGPAC.Contains(auxCategory[0])) 
                        continue;

                    if (ap.poligono == poligono)
                    {
                        Parcel auxParcel = new Parcel(ap.id);
                        var lPoligonos = new List<Polygon>();

                        foreach (MPCoords mPCoords in ap.location.coordinates)
                        {
                            var lines = new List<LineString>();
                            var coordenadas = new List<IPosition>();
                            foreach (Coordinates coords in mPCoords.lcoord)
                            {
                                coordenadas.Add(new Position(Convert.ToDouble(coords.Lat), Convert.ToDouble(coords.Lon)));
                            }
                            lines.Add(new LineString(coordenadas));
                            Polygon mipol = new Polygon(lines);
                            lPoligonos.Add(mipol);
                        }

                        auxParcel.listPolygon = lPoligonos;
                        parcelas.Add(auxParcel);
                        np++;
                    }
                    //foreach (SIGPACData sd in listParcelData)
                    //{
                    //    if (sd.poligono == poligono && sd.hashId == ap.id)
                    //    {
                    //        Parcel auxParcel = new Parcel(ap.id);
                    //        var lPoligonos = new List<Polygon>();

                    //        foreach (MPCoords mPCoords in ap.location.coordinates)
                    //        {
                    //            var lines = new List<LineString>();
                    //            var coordenadas = new List<IPosition>();
                    //            foreach (Coordinates coords in mPCoords.lcoord)
                    //            {
                    //                coordenadas.Add(new Position(Convert.ToDouble(coords.Lat), Convert.ToDouble(coords.Lon)));
                    //            }
                    //            lines.Add(new LineString(coordenadas));
                    //            Polygon mipol = new Polygon(lines);
                    //            lPoligonos.Add(mipol);
                    //        }

                    //        auxParcel.listPolygon = lPoligonos;
                    //        parcelas.Add(auxParcel);
                    //        np++;
                    //        break;
                    //    }
                    //}
                    //Parcel auxParcel = new Parcel(ap.id);
                    //var lPoligonos = new List<Polygon>();

                    //foreach (MPCoords mPCoords in ap.location.coordinates)
                    //{
                    //    var lines = new List<LineString>();
                    //    var coordenadas = new List<IPosition>();
                    //    foreach (Coordinates coords in mPCoords.lcoord)
                    //    {
                    //        coordenadas.Add(new Position(Convert.ToDouble(coords.Lat), Convert.ToDouble(coords.Lon)));
                    //    }
                    //    lines.Add(new LineString(coordenadas));
                    //    Polygon mipol = new Polygon(lines);
                    //    lPoligonos.Add(mipol);
                    //}

                    //auxParcel.listPolygon = lPoligonos;
                    //parcelas.Add(auxParcel);
                    //np++;
                }

                foreach (AirQualityObserved aqo in listAirQualityObserved)
                {
                    CO2Parcel auxParcel = new CO2Parcel(aqo.id);
                    var lPoligonosCO2 = new List<Polygon>();
                    var lines = new List<LineString>();
                    var coordenadas = new List<IPosition>();

                    if ((aqo.location.coordinates[0].Lat != aqo.location.coordinates[aqo.location.coordinates.Count - 1].Lat) ||
                        (aqo.location.coordinates[0].Lon != aqo.location.coordinates[aqo.location.coordinates.Count - 1].Lon))
                    {
                        //el polígono es abierto y no se añade por la excepción
                        Log.Write(string.Format("El polígono {0} es un poligono abierto y no se añade a la lista", aqo.id), LogLevel.Log_Debug);
                        continue;
                    }

                    foreach (Coordinates coords in aqo.location.coordinates)
                    {
                        coordenadas.Add(new Position(Convert.ToDouble(coords.Lat), Convert.ToDouble(coords.Lon)));
                    }
                    lines.Add(new LineString(coordenadas));
                    Polygon mipol = new Polygon(lines);
                    lPoligonosCO2.Add(mipol);

                    auxParcel.listPolygon = lPoligonosCO2;
                    parcelasCO2.Add(auxParcel);
                    nco++;
                }

                ////////////// TEST //////////////////
                /// se hace llamada a API
                /// 
                
                //foreach (AirQualityObserved a in listAirQualityObserved)
                //{

                //    AQOIntersects reqParams = new AQOIntersects();

                //    for (int i = 0; i < a.location.coordinates.Count; i++)
                //    {
                //        Coordinates c = new Coordinates();
                //        c.Lat = a.location.coordinates[i].Lat;
                //        c.Lon = a.location.coordinates[i].Lon;

                //        reqParams.coordinates.Add(c);
                //    }

                //    reqParams.type = "AgriParcel";

                //    TestIntersections(reqParams);

                //    break;
                //}

                //return;

                //2. Se recorre la lista de parcelas y se busca intersección con la lista de parcelas de CO2
                List<Parcel> parcelasOut = new List<Parcel>();
                List<CO2Parcel> parcelasCO2Out = new List<CO2Parcel>();
                #region calculo de intersección de rectangulos
                
                foreach (Parcel pSigpac in parcelas)
                {
                    foreach (Polygon pol in pSigpac.listPolygon)
                    {
                        double x1 = 0, x2 = 0, y1 = 0, y2 = 0;
                        //Se construye el rectángulo que contiene el polígono
                        foreach (LineString ls in pol.Coordinates)
                        {
                            x2 = findMax(ls.Coordinates, "Lat");
                            y2 = findMax(ls.Coordinates, "Lon");
                            x1 = findMin(ls.Coordinates, "Lat");
                            y1 = findMin(ls.Coordinates, "Lon");
                        }
                        Rect rp = new Rect(x2, y1, x2 - x1, y2 - y1);
                        List<CO2Parcel> parcelasCO2Aux = new List<CO2Parcel>();
                        //Rectangulo de las parcelas CO2
                        foreach (CO2Parcel co2parcel in parcelasCO2)
                        {
                            foreach (Polygon cpol in co2parcel.listPolygon)
                            {
                                double xc1 = 0, xc2 = 0, yc1 = 0, yc2 = 0;
                                //Se construye el rectángulo que contiene el polígono
                                foreach (LineString ls in cpol.Coordinates)
                                {
                                    xc2 = findMax(ls.Coordinates, "Lat");
                                    yc2 = findMax(ls.Coordinates, "Lon");
                                    xc1 = findMin(ls.Coordinates, "Lat");
                                    yc1 = findMin(ls.Coordinates, "Lon");
                                }
                                Rect rcp = new Rect(xc2, yc1, xc2 - xc1, yc2 - yc1);

                                if (rp.IntersectsWith(rcp))
                                {
                                    //Se añadea a la lista de polígonos de CO2 a revisar
                                    parcelasCO2Aux.Add(co2parcel);
                                }
                            }
                        }
                        
                        //Calculamos las intersecciones por poligono con los posibles de CO2
                        foreach (LineString ls in pol.Coordinates)
                        {
                            //Para cada conjunto de coordenadas de las parcelas e SIGPAC se revisa si hay
                            //intersección con los polígonos de CO2
                            for (int c = 0; c < ls.Coordinates.Count; c++)
                            {
                                //Aqui construimos los segmentos de los poligonos de las parcelas
                                int indexB = 0;
                                if (c == ls.Coordinates.Count - 1) indexB = 0;
                                else indexB = c + 1;

                                ////////////////////////////////////////////////////////////////////////////
                                ///Aquí se construyen los puntos de los poligonos de CO2
                                ///
                                npc = 0;
                                bool linked = false;
                                foreach (CO2Parcel co2parcel in parcelasCO2Aux)
                                {
                                    //Si el polígono de CO2 ya está en la parcela SIGPAC continua al siguiente
                                    for (int i = 0; i < pSigpac.hasRelationship.Count; i++)
                                    {
                                        if (co2parcel.Id == pSigpac.hasRelationship[i])
                                        {
                                            linked = true;
                                            break;
                                        }
                                    }
                                    if (linked)
                                    {
                                        //El polígono de CO2 ya está en la parcela SIGPAC y se pasa al siguiente polígon de CO2
                                        linked = false;
                                        continue;
                                    }
                                    
                                    foreach (Polygon cpol in co2parcel.listPolygon)
                                    {
                                        foreach (LineString lsc in cpol.Coordinates)
                                        {
                                            for (int d = 0; d < lsc.Coordinates.Count; d++)
                                            {
                                                //Aqui construimos los segmentos de los poligonos de las parcelas
                                                int indexA = 0;
                                                if (d == lsc.Coordinates.Count - 1) indexA = 0;
                                                else indexA = d + 1;

                                                bool pi = PolygonIntersection(lsc.Coordinates[d], lsc.Coordinates[indexA], ls.Coordinates[c], ls.Coordinates[indexB]);
                                                if (pi)
                                                {
                                                    //Se va saliendo de los bucles para ver si el siguiente poligono de CO2 está relacionado con la parcela
                                                    intersection = true;
                                                    break;
                                                }
                                            }
                                            if (intersection)
                                            {
                                                Log.Write(string.Format("El poligono de CO2 id: {0} esté relacionado con la parcela id: {1}",
                                                    co2parcel.Id.ToString(), pSigpac.Id.ToString()), LogLevel.Log_Debug);
                                                break;
                                            }
                                        }
                                        if (intersection)
                                        {
                                            Log.Write(string.Format("El poligono de CO2 id: {0} esté relacionado con la parcela id: {1}",
                                                co2parcel.Id.ToString(), pSigpac.Id.ToString()), LogLevel.Log_Debug);
                                            break;
                                        }
                                        Log.Write("Fin de caluclo de intersección poligonos a parcelas");
                                    }
                                    
                                    if (intersection)
                                    {
                                        //Se actualiza la relación de linkado y seguimos al siguiente polígono de CO2
                                        string fechaIntersection = DateTime.Now.ToString();
                                        intersection = false;
                                        pSigpac.hasRelationship.Add(co2parcel.Id);
                                        co2parcel.hasRelationship.Add(pSigpac.Id);
                                    }

                                    npc++;
                                }
                            }
                        }
                    }

                    parcelasOut.Add(pSigpac);
                    nps++;

                    Log.Write("Finalizado el análisis de la intersección entre polígonos y parcelas", LogLevel.Log_Debug);
                }

                #endregion

                #region old
                //foreach (Parcel pSigpac in parcelas)
                //{
                //    foreach(Polygon pol in pSigpac.listPolygon)
                //    {
                //        foreach (LineString ls in pol.Coordinates)
                //        {
                //            for (int c = 0; c < ls.Coordinates.Count; c++)
                //            {
                //                //Aqui construimos los segmentos de los poligonos de las parcelas
                //                int indexB = 0;
                //                if (c == ls.Coordinates.Count - 1) indexB = 0;
                //                else indexB = c + 1;

                //                ////////////////////////////////////////////////////////////////////////////
                //                ///Aquí se construyen los puntos de los poligonos de CO2
                //                ///
                //                npc = 0;
                //                bool linked = false;
                //                foreach (CO2Parcel co2parcel in parcelasCO2)
                //                {
                //                    //Si el polígono de CO2 ya está en la parcela SIGPAC continua al siguiente
                //                    for (int i = 0; i < pSigpac.hasRelationship.Count; i++)
                //                    {
                //                        if (co2parcel.Id == pSigpac.hasRelationship[i])
                //                        {
                //                            linked = true;
                //                            break;
                //                        }
                //                    }
                //                    if (linked)
                //                    {
                //                        linked = false;
                //                        continue;
                //                    }
                //                    //npc = 0;
                //                    foreach (Polygon cpol in co2parcel.listPolygon)
                //                    {
                //                        foreach (LineString lsc in cpol.Coordinates)
                //                        {
                //                            for (int d = 0; d < lsc.Coordinates.Count; d++)
                //                            {
                //                                //Aqui construimos los segmentos de los poligonos de las parcelas
                //                                int indexA = 0;
                //                                if (d == lsc.Coordinates.Count - 1) indexA = 0;
                //                                else indexA = d + 1;

                //                                bool pi = PolygonIntersection(lsc.Coordinates[d], lsc.Coordinates[indexA], ls.Coordinates[c], ls.Coordinates[indexB]);
                //                                if (pi)
                //                                {
                //                                    //Se va saliendo de los bucles para ver si el siguiente poligono de CO2 está relacionado con la parcela
                //                                    intersection = true;
                //                                    break;
                //                                }
                //                            }
                //                            if (intersection)
                //                            {
                //                                Log.Write(string.Format("El poligono de CO2 id: {0} esté relacionado con la parcela id: {1}",
                //                                    co2parcel.Id.ToString(), pSigpac.Id.ToString()), LogLevel.Log_Debug);
                //                                break;
                //                            }
                //                        }
                //                        if (intersection)
                //                        {
                //                            Log.Write(string.Format("El poligono de CO2 id: {0} esté relacionado con la parcela id: {1}",
                //                                co2parcel.Id.ToString(), pSigpac.Id.ToString()), LogLevel.Log_Debug);
                //                            break;
                //                        }
                //                        Log.Write("Fin de caluclo de intersección poligonos a parcelas");
                //                    }
                //                    if (intersection)
                //                    {
                //                        string fechaIntersection = DateTime.Now.ToString();
                //                        intersection = false;
                //                        pSigpac.hasRelationship.Add(co2parcel.Id);
                //                        co2parcel.hasRelationship.Add(pSigpac.Id);
                //                        parcelasCO2Out.Add(co2parcel);
                //                        //break;
                //                    }

                //                    npc++;
                //                }


                //                if (intersection)
                //                {
                //                    string fechaNuevaCO2Parcel = DateTime.Now.ToString();
                //                    //intersection = false;
                //                    //Se actualiza la relación en la parcela / poligono C02
                //                    break;
                //                }
                //            }
                //            if (intersection)
                //            {
                //                intersection = false;
                //                break;
                //            }
                //        }
                //    }

                //    parcelasOut.Add(pSigpac);
                //    nps++;
                //}






                //int parcelCount = 0;
                //foreach (Parcel parcel in parcelas)
                //{
                //    //Por cada parcela vemos las corrdenadas de su/sus poligonos
                //    var lPoligonos = new List<Polygon>();
                //    foreach (Polygon pol in parcel.listPolygon)
                //    {
                //        Polygon mipol = new Polygon(pol.Coordinates);
                //        lPoligonos.Add(mipol);
                //        //Poligonos CO2
                //        int pCO2Count = 0;
                //        foreach (CO2Parcel co2parcel in parcelasCO2)
                //        {
                //            foreach (Polygon copol in co2parcel.listPolygon)
                //            {
                //                //Comparo las coordenadas de poligonos de CO2 con las de la parcel acada vez
                //                foreach (LineString ls in pol.Coordinates)
                //                {
                //                    for (int c = 0; c < ls.Coordinates.Count; c++)
                //                    {
                //                        //Aqui construimos los segmentos de los poligonos de las parcelas
                //                        int indexB = 0;
                //                        if (c == ls.Coordinates.Count - 1) indexB = 0;
                //                        else indexB = c + 1;
                //                        //Recorremos los poligonos de los datos de CO2
                //                        string fechaIni = DateTime.Now.ToString();
                //                        foreach (LineString lsc in copol.Coordinates)
                //                        {
                //                            for (int k = 0; k < lsc.Coordinates.Count; k++)
                //                            {
                //                                //se van tomando los puntos de 2 en 2 para completar el segmento.
                //                                //hay que ver que en el último punyo del polígono se debe poner el punto inicial para cerrrar el polígono
                //                                int indexA = 0;

                //                                if (k == lsc.Coordinates.Count - 1) indexA = 0;
                //                                else indexA = k + 1;
                //                                bool pi = PolygonIntersection(lsc.Coordinates[k], lsc.Coordinates[indexA], ls.Coordinates[c], ls.Coordinates[indexB]);
                //                                if (pi)
                //                                {
                //                                    //Se va saliendo de los bucles para ver si el siguiente poligono de CO2 está relacionado con la parcela
                //                                    intersection = true;
                //                                    break;
                //                                }
                //                            }
                //                            if (intersection)
                //                            {
                //                                Log.Write(string.Format("El poligono de CO2 id: {0} esté relacionado con la parcela id: {1}",
                //                                    co2parcel.Id.ToString(), parcel.Id.ToString()), LogLevel.Log_Debug);
                //                                break;
                //                            }
                //                        }
                //                        if (intersection)
                //                        {
                //                            string fechaIntersection = DateTime.Now.ToString();
                //                            break;
                //                        }

                //                        string fechaFin = DateTime.Now.ToString();
                //                        Log.Write("Fin de caluclo de intersección poligonos a parcelas");
                //                    }
                //                    if (intersection)
                //                    {
                //                        string fechaIntersection = DateTime.Now.ToString();
                //                        break;
                //                    }
                //                }
                //                if (intersection)
                //                {
                //                    string fechaNuevaCO2Parcel = DateTime.Now.ToString();
                //                    intersection = false;
                //                    //Se actualiza la relación en la parcela / poligono C02
                //                    parcelas[parcelCount].hasRelationship.Add(co2parcel.Id);
                //                    parcelasCO2[pCO2Count].hasRelationship.Add(parcel.Id);
                //                    break;
                //                }
                //            }
                //            pCO2Count++;

                //        }

                //        Log.Write(string.Format("Test for recovering multipolygon list OK"), LogLevel.Log_Info);



                //        Log.Write("Finalizado el análisis de la intersección entre polígonos y parcelas", LogLevel.Log_Debug);
                //    }

                //    parcelCount++;
                //}
                #endregion

                //Se preparan los datos para actualizar las entidades AgriParcel en SCORPIO
                UpdateAgriParcelData(parcelasOut);
            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception on data linking task: {0}. Parcela: {1}, CO2: {2}", ex.Message, np, nco), LogLevel.Log_Debug);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = string.Format("http://140.0.24.127:9090/ngsi-ld/v1/entities/{0}/attrs/",
                "urn:ngsi-ld:AgriParcel:lorca:SIGPAC_Lorca_20210104_4326:11F214C7A01D5B6C98E3F56A14CD18B39");

            HttpClient rclient = new HttpClient();
            rclient.DefaultRequestHeaders.Accept.Clear();
            rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/ld+json"));

            foreach(SIGPACData p in listParcelData)
            {
                url = string.Format("http://140.0.24.127:9090/ngsi-ld/v1/entities/{0}", p.hashId); //internal
                DeleteRequest(url, rclient);
            }

            return;

            AQORelation hasAirQualityObserved = new AQORelation();
            hasAirQualityObserved.@object.Add("urn:ngsi-ld:AirQualityObserved:1234567890_2804_888888888888888888");
            string jsonStringAPr = JsonConvert.SerializeObject(hasAirQualityObserved);
            var jsonAPContent1 = new StringContent(JsonConvert.SerializeObject(hasAirQualityObserved), Encoding.UTF8, "application/json");
            PatchTesting(url, rclient, jsonAPContent1);

            //string url = string.Format("https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/{0}/attrs/",
            //    "urn:ngsi-ld:AgriParcel:lorca:SIGPAC_Lorca_20210104_4326:11F214C7A01D5B6C98E3F56A14CD18B39");

            //FASE 1. Borramos la entidad
            url = "http://140.0.24.127:9090/ngsi-ld/v1/entities/urn:ngsi-ld:AgriParcel:lorca:SIGPAC_Lorca_20210104_4326:11F214C7A01D5B6C98E3F56A14CD18B39_113"; //internal
            DeleteRequest(url, rclient);

            //FASE 2. Se crea de nuevo añadiendo el hasAirQualityObserved
            AgriPReqParameters rqAPParams = new AgriPReqParameters();
            postAP = false;
            //POST entity
            //url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/"; //from outside
            url = "http://140.0.24.127:9090/ngsi-ld/v1/entities/"; //internal
            foreach (AgriParcel aparcel in listAgriParcel)
            {
                rqAPParams.id = "urn:ngsi-ld:AgriParcel:lorca:SIGPAC_Lorca_20210104_4326:11F214C7A01D5B6C98E3F56A14CD18B39_113";
                rqAPParams.type = aparcel.tipo;
                rqAPParams.createdAt = aparcel.dateCreated;

                rqAPParams.area.value = aparcel.area.value;
                rqAPParams.area.unitCode = aparcel.area.unitCode;

                rqAPParams.category.value = aparcel.categoria.value;

                rqAPParams.hasAgriCrop.@object = aparcel.hasAgriCrop.id;

                if (aparcel.location.subtipo == "Polygon")
                    continue;

                MPValLocation auxLocation = new MPValLocation();
                List<decimal[,]> listCoord = new List<decimal[,]>();
                for (int c = 0; c < aparcel.location.coordinates.Count; c++)
                {
                    auxLocation.coordinates = new List<List<decimal[,]>>();

                    decimal[,] coord = new decimal[aparcel.location.coordinates[c].lcoord.Count, 2];
                    for (int d = 0; d < aparcel.location.coordinates[c].lcoord.Count; d++)
                    {

                        coord[d, 0] = aparcel.location.coordinates[c].lcoord[d].Lon;
                        coord[d, 1] = aparcel.location.coordinates[c].lcoord[d].Lat;


                    }
                    listCoord.Add(coord);
                }
                auxLocation.coordinates.Add(listCoord);
                auxLocation.type = aparcel.location.subtipo;

                rqAPParams.location.value = auxLocation;
                rqAPParams.location.type = aparcel.location.tipo;

                rqAPParams.hasAirQualityObserved.@object.Add("urn:ngsi-ld:AirQualityObserved:1234567890_2804_11111111111111111");
                rqAPParams.hasAirQualityObserved.@object.Add("urn:ngsi-ld:AirQualityObserved:1234567890_2804_22222222222222222");

                string jsonStringAP = JsonConvert.SerializeObject(rqAPParams);
                var jsonAPContent = new StringContent(JsonConvert.SerializeObject(rqAPParams), Encoding.UTF8, "application/json");
                //send async POST to the url for adding a entity
                PostRequest(url, rclient, jsonAPContent, ENTITY.AgriParcel, rqAPParams.id);
                break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            indIni = Convert.ToInt64(textBox1.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            indFin = Convert.ToInt64(textBox2.Text);
        }

        private void btnEnRich_Click(object sender, EventArgs e)
        {
            //This method calculates the airqualityobserved entity for enrichment of SIGPAC parcel when there is not any CO2 parcel linked
            Log.Write(string.Format("Starting enrichment for parcels belonging to polygon {0}", txtNPolygon.Text), LogLevel.Log_Info);

            int nPolygon = Convert.ToInt32(txtNPolygon.Text);

            try
            {
                for (int i = 0; i < listAgriParcel.Count; i++)
                {
                    string[] auxCategory = listAgriParcel[i].categoria.value.Split(':');
                    if (usoSIGPAC.Contains(auxCategory[0])) continue;

                    if (listAgriParcel[i].poligono == nPolygon && listAgriParcel[i].hasAirQualityObserved.@object.Count == 0)
                    {
                        //There is not any AirQualityObserved linked. Starting enrichment process

                        //1. Checking the kind of crop for continuing
                        string[] auxTipo = listAgriParcel[i].categoria.value.Split(':');
                        if (listAgriParcel[i].categoria.value == SALTEDTags.SIGPAC_AG || listAgriParcel[i].categoria.value == SALTEDTags.SIGPAC_CA ||
                            listAgriParcel[i].categoria.value == SALTEDTags.SIGPAC_ED || listAgriParcel[i].categoria.value == SALTEDTags.SIGPAC_ZC ||
                            listAgriParcel[i].categoria.value == SALTEDTags.SIGPAC_ZU || listAgriParcel[i].categoria.value == SALTEDTags.SIGPAC_ZV)
                        {
                            continue;
                        }

                        //2. Searching AirQualityObserved entities for the same polygon or neighborhood and the same kind of crop
                        //
                        List<string> auxAirQO = new List<string>();
                        foreach(AgriParcel ap in listAgriParcel)
                        {
                            if ((ap.poligono == nPolygon || ap.poligono == nPolygon+1 || ap.poligono == nPolygon-1) && ap.hasAirQualityObserved.@object.Count > 0)
                            {
                                foreach(string s in ap.hasAirQualityObserved.@object)
                                {
                                    //"s" contains the id of the AirQualityObserved entity
                                    auxAirQO.Add(s);
                                }
                            }
                        }

                        //3. Creating an "virtual" AirQualityObserved entitiy with CO2 calculated value and linked to the AgriParcel entity
                        //3.1 Virtual AirQualityObserved entity
                        double CO2Val = 0;
                        int numAOQ = 0;

                        for (int c = 0; c < listAirQualityObserved.Count; c++)
                        {
                            foreach(string s in auxAirQO)
                            {
                                if (s == listAirQualityObserved[c].id)
                                {
                                    //Gets CO2 value to calculate medium value
                                    CO2Val += listAirQualityObserved[c].co2.value;
                                    numAOQ++;
                                }
                            }
                        }
                        AirQualityObserved aqo = new AirQualityObserved();
                        string sSourceData = string.Empty;
                        byte[] tmpSource;
                        byte[] tmpHash;

                        aqo.co2.value = CO2Val / numAOQ;
                        aqo.dateObserved.value = DateTime.Now.ToString();
                        aqo.areaserved.value = "";
                        aqo.id = "";
                        //Same location as the AgriCrop parcel
                        foreach (Coordinates xy in listAgriParcel[i].location.coordinates[0].lcoord)
                        {
                            sSourceData += xy.Lat.ToString() + xy.Lon.ToString();
                            ////// LOCATION //////
                            ///
                            aqo.location.coordinates.Add(xy);
                        }
                        ////// ID //////
                        //Create a byte array from source data.
                        tmpSource = ASCIIEncoding.ASCII.GetBytes(sSourceData);
                        //Compute hash based on source data.
                        tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
                        aqo.id = "urn:ngsi-ld:AirQualityObserved";
                        aqo.id += ":" + observedArea + ":" + ByteArrayToString(tmpHash);

                        /////// CO2 //////
                        aqo.co2.unitCode = SALTEDTags.KG_M2;
                        aqo.co2.value = CO2Val / numAOQ;

                        ////// DATE //////
                        ///
                        //auxAQO.dateObserved.value = Convert.ToDateTime(dateObservedInicio).ToString("s") + "/" + Convert.ToDateTime(dateObservedFinal).ToString("s");
                        //dateObserved solo permite una fecha, usaremos la del final del periodo
                        aqo.dateObserved.value = Convert.ToDateTime(dateObservedFinal).ToString("s");

                        ////// AREA SERVED //////
                        aqo.areaserved.value = observedArea.ToUpperInvariant();
                        listAirQualityObserved.Add(aqo);

                        //3.2 Linking new AirQualityObserved entity to AgriParcel entity
                        listAgriParcel[i].hasAirQualityObserved.@object.Add(aqo.id);

                        //3.3 Upload the entities to SCORPIO
                        //3.3.1 adding AirQualityObserved entity
                        AddAirQualityObservedToSCORPIO(aqo);
                        //3.3.2 updating AgriParcel entity
                        UpdateAgriParcelDataEnriched(listAgriParcel[i]);
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Write(string.Format("Exception on enrichment process for polygon {0}: {1}", txtNPolygon.Text, ex.Message), LogLevel.Log_Debug);
            }
            finally
            {
                btnEnRich.Enabled = false;
                btnEnRich.Visible = false;
            }
        }

        private void btnCO2Delete_Click(object sender, EventArgs e)
        {
            //Se eliminan todas las entidades de CO2 de tipo AirQualityObserved
            Log.Write("AirQualityObserved delete entities function", LogLevel.Log_Info);

            string url = string.Empty;

            try
            {
                //This method is used for deleting AirQualitObserved entities to Scorpio CB
                HttpClient rclient = new HttpClient();
                rclient.DefaultRequestHeaders.Accept.Clear();
                //rclient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/ld+json"));

                //url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/"; //from outside
                //url = "http://140.0.24.127:9090/ngsi-ld/v1/entities/"; //internal
                url = urlBase;

                for (int i = 0; i < listAirQualityObserved.Count; i++)
                {
                    url = string.Format(urlBase + "{0}", listAirQualityObserved[i].id);
                    DeleteRequest(url, rclient);
                }
            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception deleting CO2 AirQualityObserved data in btnCO2Delete_Click. Error: {0}", ex.Message), LogLevel.Log_Debug);
            }
        }

        private void btnSIGPACDelete_Click(object sender, EventArgs e)
        {
            //Se eliminan todas las entidades de parelas SIGPAC de tipo AgriParcel
            Log.Write("AgriParcel delete entities function", LogLevel.Log_Info);

            string url = string.Empty;
            string uso_sigpac = string.Empty;

            try
            {
                //This method is used for deleting AirQualitObserved entities to Scorpio CB
                HttpClient rclient = new HttpClient();
                rclient.DefaultRequestHeaders.Accept.Clear();
                //rclient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/ld+json"));

                //url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/"; //from outside
                //url = "http://140.0.24.127:9090/ngsi-ld/v1/entities/"; //internal
                url = urlBase;
                uso_sigpac = txtUsoSIGPAC.Text;

                for (int i = 0; i < listAgriParcel.Count; i++)
                {
                    if (uso_sigpac == string.Empty)
                    {
                        url = string.Format(urlBase + "{0}", listAgriParcel[i].id);
                        DeleteRequest(url, rclient);
                    }
                    else
                    {
                        string[] auxCategory = listAgriParcel[i].categoria.value.Split(':');
                        if (uso_sigpac == auxCategory[0])
                        {
                            url = string.Format(urlBase + "{0}", listAgriParcel[i].id);
                            DeleteRequest(url, rclient);
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception deleting SIGPAC parcels for AgriParcel data in btnSIGPACDelete_Click. Error: {0}", ex.Message), LogLevel.Log_Debug);
            }
        }

        private void btnCropsDelete_Click(object sender, EventArgs e)
        {
            //Se eliminan todas las entidades de cultivos de tipo AgriCrop
            Log.Write("AgriCrop delete entities function", LogLevel.Log_Info);

            string url = string.Empty;

            try
            {
                //This method is used for deleting AirQualitObserved entities to Scorpio CB
                HttpClient rclient = new HttpClient();
                rclient.DefaultRequestHeaders.Accept.Clear();
                //rclient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                rclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/ld+json"));

                //url = "https://salted.grupoamper.com:9000/ngsi-ld/v1/entities/"; //from outside
                //url = "http://140.0.24.127:9090/ngsi-ld/v1/entities/"; //internal
                url = urlBase;

                for (int i = 0; i < listAgriCrop.Count; i++)
                {
                    url = string.Format(urlBase + "{0}", listAgriCrop[i].id);
                    DeleteRequest(url, rclient);
                }
            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Exception deleting Crops for AgriCrop data in btnCropsDelete_Click. Error: {0}", ex.Message), LogLevel.Log_Debug);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtUsoSIGPAC.Text = cmbSIGPAC.SelectedItem.ToString();
        }
    }

}
