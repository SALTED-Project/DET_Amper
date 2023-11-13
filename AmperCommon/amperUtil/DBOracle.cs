using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using amperUtil.Log;

namespace amperUtil
{
    public class DBOracle : IDisposable
    {
        private OracleConnection oraConnection;
        public OracleDataReader oraDatareader;
        public OracleCommand oracommand;
        private OracleTransaction oraTransaction;

        public struct strConnDB
        {
            public string CadenaConexion;
            public string ErrorDesc;
            public int ErrorNum;
        }

        public strConnDB info;

        public int oraConnIntentos;

        public DBOracle(string server, string user, string passw)
        {
            //info.CadenaConexion = string.Format("Data Source={0};User Id={1};Password={2};", "CYRUS", "ParisDATA_v400_emersys", "nemesis");
            info.CadenaConexion = string.Format("Data Source={0};User Id={1};Password={2};", server, user, passw);
            oraConnection = new OracleConnection();
            oraConnection.ConnectionString = info.CadenaConexion;

            oraConnIntentos = 0;
        }

        public string ErrDesc
        {
            get { return info.ErrorDesc.ToString(); }
        }
        public string ErrNum
        {
            get { return info.ErrorNum.ToString(); }
        }

        public bool Conectar()
        {
            bool estado = false;
            try
            {
                if (oraConnection!= null)
                {
                    oraConnection.ConnectionString = info.CadenaConexion;
                    oraConnection.Open();
                    estado = true;
                    Log.Log.Write(string.Format("Connected to Oracle Database: {0}", info.CadenaConexion), LogLevel.Log_Info);
                }
            }
            catch(Exception ex)
            {
                Log.Log.Write(string.Format("Exception connecting to Oracle Database: {0}", ex.Message), LogLevel.Log_Debug);
                Desconectar();
                AsignarError(ref ex);
                estado = false;
            }

            return estado;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Liberamos objetos manejados.
            }

            try
            {
                // Liberamos los obtetos no manejados.
                if (oraDatareader != null)
                {
                    oraDatareader.Close();
                    oraDatareader.Dispose();
                }

                // Cerramos la conexión a DB.
                if (!Desconectar())
                {
                    // Grabamos Log de Error...
                }

            }
            catch (Exception ex)
            {
                // Asignamos error.
                AsignarError(ref ex);
            }
        }

        public bool Desconectar()
        {
            try
            {
                // Cerramos la conexion
                if (oraConnection != null)
                {
                    if (oraConnection.State != ConnectionState.Closed)
                    {
                        oraConnection.Close();
                    }
                }
                // Liberamos su memoria.
                oraConnection.Dispose();
                Log.Log.Write(string.Format("Disconnected from Oracle Database: {0}", info.CadenaConexion), LogLevel.Log_Info);
                return true;
            }
            catch (Exception ex)
            {
                Log.Log.Write(string.Format("Exception disconnecting from Oracle Database: {0}", ex.Message), LogLevel.Log_Debug);
                AsignarError(ref ex);
                return false;
            }
        }

        /// <summary>
        /// Ejecuta una sql que rellenar un DataReader (sentencia select).
        /// </summary>
        /// <param name="SqlQuery">sentencia sql a ejecutar</param>
        /// <returns></returns> 
        public bool EjecutaSQL(string SqlQuery)
        {

            bool ok = true;

            OracleCommand ora_Command = new OracleCommand();

            try
            {

                // Si no esta conectado, se conecta.
                if (!IsConected())
                {
                    ok = Conectar();
                }

                if (ok)
                {
                    // Cerramos cursores abiertos, para evitar el error ORA-1000
                    if ((oraDatareader != null))
                    {
                        oraDatareader.Close();
                        oraDatareader.Dispose();
                    }

                    ora_Command.Connection = oraConnection;
                    ora_Command.CommandType = CommandType.Text;
                    ora_Command.CommandText = SqlQuery;

                    // Ejecutamos sql.
                    oraDatareader = ora_Command.ExecuteReader();
                }

            }
            catch (Exception ex)
            {
                Log.Log.Write(string.Format("Error executing oracle command: {0}, error {1}", SqlQuery, ex.Message), LogLevel.Log_Debug);
                AsignarError(ref ex);
                ok = false;
            }
            finally
            {
                if (ora_Command != null)
                {
                    ora_Command.Dispose();
                }
            }

            return ok;

        }

        /// <summary>
        /// Ejecuta una sql que no devuelve datos (update, delete, insert).
        /// </summary>
        /// <param name="SqlQuery">sentencia sql a ejecutar</param>
        /// <param name="FilasAfectadas">Fila afectadas por la sentencia SQL</param>
        /// <returns></returns>
        public bool EjecutaSQL(string SqlQuery, ref int FilasAfectadas)
        {

            bool ok = true;
            OracleCommand ora_Command = new OracleCommand();

            try
            {

                // Si no esta conectado, se conecta.
                if (!IsConected())
                {
                    ok = Conectar();
                }

                if (ok)
                {
                    oraTransaction = oraConnection.BeginTransaction();
                    ora_Command = oraConnection.CreateCommand();
                    ora_Command.CommandType = CommandType.Text;
                    ora_Command.CommandText = SqlQuery;
                    FilasAfectadas = ora_Command.ExecuteNonQuery();
                    oraTransaction.Commit();
                }

            }
            catch (Exception ex)
            {
                // Hacemos rollback.
                Log.Log.Write(string.Format("Error executing oracle command: {0}, error {1}", SqlQuery, ex.Message), LogLevel.Log_Debug);
                oraTransaction.Rollback();
                AsignarError(ref ex);
                ok = false;
            }
            finally
            {
                // Recolectamos objetos para liberar su memoria.
                if (ora_Command != null)
                {
                    ora_Command.Dispose();
                }
            }

            return ok;

        }


        /// <summary>
        /// Captura Excepciones
        /// </summary>
        /// <param name="ex">Excepcion producida.</param>
        private void AsignarError(ref Exception ex)
        {
            // Si es una excepcion de Oracle.
            if (ex is OracleException)
            {
                info.ErrorNum = ((OracleException)ex).Number;
                info.ErrorDesc = ex.Message;
            }
            else
            {
                info.ErrorNum = 0;
                info.ErrorDesc = ex.Message;
            }
            // Grabamos Log de Error...
            Log.Log.Write(string.Format("Exception Oracle Database: {0} / {1}", info.ErrorNum.ToString(), info.ErrorDesc), LogLevel.Log_Error);
        }

        /// <summary>
        /// Devuelve el estado de la base de datos
        /// </summary>
        /// <returns>True si esta conectada.</returns>
        public bool IsConected()
        {

            bool ok = false;

            try
            {
                // Si el objeto conexion ha sido instanciado
                if (oraConnection != null)
                {
                    // Segun el estado de la Base de Datos.
                    switch (oraConnection.State)
                    {
                        case ConnectionState.Closed:
                        case ConnectionState.Broken:
                        case ConnectionState.Connecting:
                            ok = false;
                            break;
                        case ConnectionState.Open:
                        case ConnectionState.Fetching:
                        case ConnectionState.Executing:
                            ok = true;
                            break;
                    }
                }
                else
                {
                    ok = false;
                }

            }
            catch (Exception ex)
            {
                Log.Log.Write(string.Format("Exception getting Oracle Database State: {0}", ex.Message), LogLevel.Log_Debug);
                AsignarError(ref ex);
                ok = false;
            }

            return ok;

        }

    }
}

