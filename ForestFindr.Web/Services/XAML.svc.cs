using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using Microsoft.SqlServer.Types;
using ForestFindr.Web.ServicesContracts;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Configuration;
using System.Threading;

namespace ForestFindr.Web.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode=AspNetCompatibilityRequirementsMode.Allowed)]
    public class XAML : IXAML
    {
        public XAML()
        {
          //  Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }
    
        /* default global parameters */
        private string geomField = "the_geo"; // name of Geography DataType column in tables
        private string srid = "4326"; // name of SRID Constraint, EPSG:4326, of tables

        private int totalPoints = 0;


        /// <summary>
        /// GetSQLDataXAML
        ///     returns XAML MapLayer with results of SQL Server query
        /// </summary>
        /// <param name="Parameters">XAMLParameters required for query</param>
        public XAMLResponse GetSQLDataXAML(XAMLParameters Parameters)
        {
            DateTime queryStart = DateTime.Now;
            DateTime queryStop;


            XAMLResponse _XAMLResponse = new XAMLResponse();
            _XAMLResponse.ErrorCode = 0;
            _XAMLResponse.OutputMessage = "Success";
            _XAMLResponse.OutputFields = new List<XAMLFields>();

            StringBuilder xaml = new StringBuilder();
            string layerstr = "layer" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Parameters.table);

            //XML MapLayer header string for return XAML
            xaml.Append("<m:MapLayer x:Name=\"" + layerstr + "\"");
            xaml.Append(" xmlns:m=\"clr-namespace:Microsoft.Maps.MapControl;assembly=Microsoft.Maps.MapControl\"");
            xaml.Append(" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            xaml.Append(" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
            xaml.Append(">");
            string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connStr);
            SqlDataReader rdr = null;

            try
            {
                if (Parameters.querytype == null || Parameters.querytype.Length == 0) throw new ArgumentException("Invalid Parameters: querytype=\"" + Parameters.querytype + "\"");
                if (Parameters.querytype.ToLower().Equals("buffer") && Parameters.radius <= 0) throw new ArgumentException("Invalid Parameters: querytype buffer requires radius > 0 - \"" + Parameters.radius + "\"");
                int test = Parameters.points.Split(',').Length;
                if (Parameters.points.Length == 0 && Parameters.querytype.ToLower().Equals("buffer")) throw new ArgumentException("Invalid Parameters: points must contain at least one point for querytype buffer");
                if (Parameters.points.Split(',').Length != 5 && Parameters.querytype.ToLower().Equals("bbox")) throw new ArgumentException("Invalid Parameters: points must contain 5 points for a closed querytype bbox");

                StringBuilder query = new StringBuilder();
                if (Parameters.querytype.ToLower().Equals("bbox"))
                {   //BBox
                    query.Append("SELECT *," + geomField + ".Reduce(@reduce) as " + geomField + "XAML FROM [dbo].[" + Parameters.table + "] WITH(INDEX(the_geo_sidx)) WHERE ");
                    query.Append(geomField + ".STIntersects(geography::STGeomFromText('POLYGON(('+@points+'))', @srid))=1");
                }
                else
                {
                    if (Parameters.querytype.ToLower().Equals("buffer"))
                    {
                        query.Append("SELECT TOP 100 *," + geomField + ".Reduce(@reduce) as " + geomField + "XAML FROM [dbo].[" + Parameters.table + "] WITH(INDEX(the_geo_sidx)) WHERE marine = 0 and ");
                        if (Parameters.points.Split(',').Length > 1)
                        {   //Polyline Buffer
                            query.Append(geomField + ".STIntersects(geography::STGeomFromText('LINESTRING('+@points+')', @srid).STBuffer(@radius))=1  ORDER BY rep_area desc");
                        }
                        else
                        {   //Point Buffer
                            query.Append(geomField + ".STIntersects(geography::STGeomFromText('POINT('+@points+')', @srid).STBuffer(@radius))=1  ORDER BY rep_area desc");
                        }
                    }
                    else
                    {
                        if (Parameters.querytype.ToLower().Equals("name"))
                        {
                            query.Append("SELECT TOP 50 *," + geomField + ".Reduce(@reduce) as " + geomField + "XAML FROM [dbo].[" + Parameters.table + "] WHERE marine = 0 and ");
                            query.Append("country = @points ORDER BY rep_area desc");
                        }
                        else if (Parameters.querytype.ToLower().Equals("id"))
                        {
                            query.Append("SELECT *," + geomField + ".Reduce(@reduce) as " + geomField + "XAML FROM [dbo].[" + Parameters.table + "] WHERE marine = 0 and ");
                            query.Append("ID = @points");
                        }
                    }
                }
             

                queryStart = DateTime.Now;
                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.Add(new SqlParameter("reduce", Parameters.reduce));
                cmd.Parameters.Add(new SqlParameter("radius", Parameters.radius));
                cmd.Parameters.Add(new SqlParameter("srid", srid));
                cmd.Parameters.Add(new SqlParameter("points", Parameters.points));

                conn.Open();
                rdr = cmd.ExecuteReader();
                
                while (rdr.Read())
                {
                    
                    Parameters = GenerateXaml(Parameters, _XAMLResponse, xaml, rdr);
                }
                xaml.AppendLine("</m:MapLayer>");
                _XAMLResponse.XAML = xaml.ToString();
            }
            catch (ArithmeticException e)
            {
                ServiceException(_XAMLResponse, e.Message);
            }
            catch (ArgumentException e)
            {
                ServiceException(_XAMLResponse, e.Message);
            }
            catch (Exception e)
            {
                ServiceException(_XAMLResponse, e.Message);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (conn != null) conn.Close();
            }
            queryStop = DateTime.Now;
            _XAMLResponse.QueryTime = (queryStop - queryStart).TotalMilliseconds;
            _XAMLResponse.totalPoints = totalPoints;

            return _XAMLResponse;
        }


        public XAMLResponse GetSQLDataAreaXAML(XAMLParameters Parameters)
        {
            DateTime queryStart = DateTime.Now;
            DateTime queryStop;


            XAMLResponse _XAMLResponse = new XAMLResponse();
            _XAMLResponse.ErrorCode = 0;
            _XAMLResponse.OutputMessage = "Success";
            _XAMLResponse.OutputFields = new List<XAMLFields>();

            StringBuilder xaml = new StringBuilder();
            string layerstr = "layer" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Parameters.table);

            //XML MapLayer header string for return XAML
            xaml.Append("<m:MapLayer x:Name=\"" + layerstr + "\"");
            xaml.Append(" xmlns:m=\"clr-namespace:Microsoft.Maps.MapControl;assembly=Microsoft.Maps.MapControl\"");
            xaml.Append(" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            xaml.Append(" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
            xaml.Append(">");
            string connStr = ConfigurationManager.ConnectionStrings["DataConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(connStr);
            SqlDataReader rdr = null;

            StringBuilder query = new StringBuilder();

            try
            {
                switch (Parameters.querytype.ToLower())
                {
                    case "Buffer":
                        {
                            query.Append("SELECT TOP 100 *," + geomField + ".Reduce(@reduce) as " + geomField + "XAML FROM [dbo].[" + Parameters.table + "] where country = 'PRT'");

                            break;
                        }

                    default:
                        break;
                }

                queryStart = DateTime.Now;
                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.Add(new SqlParameter("reduce", Parameters.reduce));

                conn.Open();
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Parameters = GenerateXaml(Parameters, _XAMLResponse, xaml, rdr);
                }
                xaml.AppendLine("</m:MapLayer>");
                _XAMLResponse.XAML = xaml.ToString();
            }
            catch (ArithmeticException e)
            {
                ServiceException(_XAMLResponse, e.Message);
            }
            catch (ArgumentException e)
            {
                ServiceException(_XAMLResponse, e.Message);
            }
            catch (Exception e)
            {
                ServiceException(_XAMLResponse, e.Message);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (conn != null) conn.Close();
            }
            queryStop = DateTime.Now;
            _XAMLResponse.QueryTime = (queryStop - queryStart).TotalMilliseconds;
            _XAMLResponse.totalPoints = totalPoints;

            return _XAMLResponse;
        }

        private XAMLParameters GenerateXaml(XAMLParameters Parameters, XAMLResponse _XAMLResponse, StringBuilder xaml, SqlDataReader rdr)
        {
            XAMLFields shp = new XAMLFields();
            shp.Fields = new Dictionary<string, string>();
            for (int i = 0; i < rdr.FieldCount; i++)
            {
                if (rdr[i].GetType().Equals(typeof(SqlGeometry)) ||
                    rdr.GetName(i).Equals(geomField))
                { //skip
                }
                else
                {
                    string test2 = rdr.GetName(i) + "   " + geomField + "XAML";
                    if (rdr.GetName(i).Equals("ID"))
                    {
                        shp.ID = Parameters.table + "_" + rdr[i].ToString();
                    }
                    else if (rdr.GetName(i).Equals(geomField + "XAML"))
                    {

                        SqlGeography geo = (SqlGeography)rdr[geomField + "XAML"];
                        StringBuilder geoxaml = new StringBuilder();

                        switch (geo.STGeometryType().ToString().ToUpper())
                        {
                            case "POINT":
                                {
                                    geoxaml.Append(XamlPoint(geo, Parameters.table, shp.ID));
                                    break;
                                }
                            case "LINESTRING":
                                {
                                    geoxaml.Append(XamlLinestring(geo, Parameters.table, shp.ID));
                                    break;
                                }
                            case "POLYGON":
                                {
                                    geoxaml.Append(XamlPolygon(geo, Parameters.table, shp.ID));
                                    break;
                                }
                            case "MULTILINESTRING":
                                {
                                    geoxaml.Append(XamlMultiLinestring(geo, Parameters.table, shp.ID));
                                    break;
                                }
                            case "MULTIPOLYGON":
                                {
                                    geoxaml.Append(XamlMultiPolygon(geo, Parameters.table, shp.ID));
                                    break;
                                }
                            case "GEOMETRYCOLLECTION":
                                {
                                    geoxaml.Append(XamlGeometryCollection(geo, Parameters.table, shp.ID));
                                    break;
                                }
                        }
                        xaml.Append(geoxaml.ToString());
                    }
                    else
                    {
                        shp.Fields.Add(rdr.GetName(i), rdr[i].ToString().UF8toNormalString());
                    }
                }
            }
            _XAMLResponse.OutputFields.Add(shp);
            return Parameters;
        }

        /// <summary>
        /// ServiceException
        ///     sets response for exception
        /// </summary>
        /// <param name="response">XAMLResponse</param>
        /// <param name="message">string exception messsage</param>
        public void ServiceException(XAMLResponse response, string message)
        {
            response.ErrorCode = 1;
            response.OutputMessage = message;
            response.OutputFields.Clear();
            response.XAML = "";
        }

        /// <summary>
        /// XamlPoint
        ///     builds XAML string for a Pushpin point
        /// </summary>
        /// <param name="geo">SqlGeography geo</param>
        /// <param name="table">string layer table</param>
        /// <param name="ID">string ID of current record</param>
        /// <returns>string of XAML</returns>
        private string XamlPoint(SqlGeography geo, string table, string ID)
        {
            //<m:Pushpin PositionOrigin="Center" Content="Test" Location="39,-105"/>
            totalPoints++;
            StringBuilder sb = new StringBuilder();
            sb.Append("<m:Pushpin x:Name=\"" + ID + "\" PositionOrigin=\"BottomCenter\" Content=\"" + ID.Split('_')[1] + "\" Location=\"");
            sb.Append(String.Format("{0:0.#####},{1:0.#####}\"/>", (double)geo.Lat, (double)geo.Long));
            return sb.ToString();
        }

        /// <summary>
        /// XamlLinestring
        ///     builds XAML string for a MapPolyline
        /// </summary>
        /// <param name="geo">SqlGeography geo</param>
        /// <param name="table">string layer table</param>
        /// <param name="ID">string ID of current record</param>
        /// <returns>string of XAML</returns>
        private string XamlLinestring(SqlGeography geo, string table, string ID)
        {
            //<m:MapPolyline  Stroke="Red" Locations="40,-105 39,-104 39,-105"/>
            StringBuilder sb = new StringBuilder();
            if (geo.STNumPoints() > 1)
            {
                totalPoints += (int)geo.STNumPoints();
                sb.Append("<m:MapPolyline x:Name=\"" + ID + "\" Locations=\"");
                for (int j = 1; j <= geo.STNumPoints(); j++)
                {
                    if (j > 1) sb.Append(" ");
                    sb.Append(String.Format("{0:0.#####},{1:0.#####}", (double)geo.STPointN(j).Lat, (double)geo.STPointN(j).Long));
                }
                sb.AppendLine("\"/>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// XamlPolygon
        ///     builds XAML string for a MapPolygon
        /// </summary>
        /// <param name="geo">SqlGeography geo</param>
        /// <param name="table">string layer table</param>
        /// <param name="ID">string ID of current record</param>
        /// <returns>string of XAML</returns>
        private string XamlPolygon(SqlGeography geo, string table, string ID)
        {
            //<m:MapPolygon x:Name="test" Fill="Yellow" Locations="40,-105 40,-104 39,-104 39,-105 40,-105"/>
            StringBuilder sb = new StringBuilder();
            if (geo.NumRings() > 0)
            {
                totalPoints += (int)geo.STNumPoints();
                sb.AppendLine("<m:MapLayer x:Name=\"" + ID + "\">");
                for (int j = 1; j <= geo.NumRings(); j++)
                {
                    if (geo.RingN(j).STNumPoints() > 1)
                    {
                        sb.Append("<m:MapPolygon Fill=\"Red\" Locations=\"");
                        for (int k = 1; k <= geo.RingN(j).STNumPoints(); k++)
                        {
                            if (k > 1) sb.Append(" ");
                            double t1 = (double)geo.RingN(j).STPointN(k).Lat;
                            double t2 = (double)geo.RingN(j).STPointN(k).Long;
                            string test = String.Format("{0:0.#####},{1:0.#####}", (double)geo.RingN(j).STPointN(k).Lat, (double)geo.RingN(j).STPointN(k).Long);
                            sb.Append(String.Format("{0:0.#####},{1:0.#####}", (double)geo.RingN(j).STPointN(k).Lat, (double)geo.RingN(j).STPointN(k).Long));
                        }
                        sb.AppendLine("\"/>");
                    }
                }
                sb.AppendLine("</m:MapLayer>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// XamlMultiLinestring
        ///     builds XAML string for a MapLayer containing multiple MapPolylines
        /// </summary>
        /// <param name="geo">SqlGeography geo</param>
        /// <param name="table">string layer table</param>
        /// <param name="ID">string ID of current record</param>
        /// <returns>string of XAML</returns>
        private string XamlMultiLinestring(SqlGeography geo, string table, string ID)
        {
            //<m:MapPolygon x:Name="test" Fill="Yellow" Locations="40,-105 40,-104 39,-104 39,-105 40,-105"/>
            StringBuilder sb = new StringBuilder();
            if (geo.STNumGeometries() > 0)
            {
                totalPoints += (int)geo.STNumPoints();
                sb.AppendLine("<m:MapLayer x:Name=\"" + ID + "\">");
                for (int j = 1; j <= geo.STNumGeometries(); j++)
                {
                    if (geo.STGeometryN(j).NumRings() > 0)
                    {
                        for (int k = 1; k <= geo.STGeometryN(j).NumRings(); k++)
                        {
                            if (geo.STGeometryN(j).RingN(k).STNumPoints() > 1)
                            {
                                sb.Append("<m:MapPolyline Locations=\"");
                                for (int m = 1; m <= geo.STGeometryN(j).RingN(k).STNumPoints(); m++)
                                {
                                    if (m > 1) sb.Append(" ");
                                    sb.Append(String.Format("{0:0.#####},{1:0.#####}", (double)geo.STGeometryN(j).RingN(k).STPointN(m).Lat, (double)geo.STGeometryN(j).RingN(k).STPointN(m).Long));
                                }
                                sb.AppendLine("\"/>");
                            }
                        }
                    }
                }
                sb.AppendLine("</m:MapLayer>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// XamlMultiPolygon
        ///     builds XAML string for a MapLayer containing multiple MapPolygons
        /// </summary>
        /// <param name="geo">SqlGeography geo</param>
        /// <param name="table">string layer table</param>
        /// <param name="ID">string ID of current record</param>
        /// <returns>string of XAML</returns>
        private string XamlMultiPolygon(SqlGeography geo, string table, string ID)
        {
            //<m:MapPolygon x:Name="test" Fill="Yellow" Locations="40,-105 40,-104 39,-104 39,-105 40,-105"/>
            StringBuilder sb = new StringBuilder();
            if (geo.STNumGeometries() > 0)
            {
                totalPoints += (int)geo.STNumPoints();
                sb.AppendLine("<m:MapLayer x:Name=\"" + ID + "\">");
                for (int j = 1; j <= geo.STNumGeometries(); j++)
                {
                    if (geo.STGeometryN(j).NumRings() > 0)
                    {
                        for (int k = 1; k <= geo.STGeometryN(j).NumRings(); k++)
                        {
                            if (geo.STGeometryN(j).RingN(k).STNumPoints() > 1)
                            {
                                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                                sb.Append("<m:MapPolygon Fill=\"Red\" Locations=\"");
                                for (int m = 1; m <= geo.STGeometryN(j).RingN(k).STNumPoints(); m++)
                                {
                                    if (m > 1) sb.Append(" ");
                                    sb.Append(String.Format("{0:0.#####},{1:0.#####}", (double)geo.STGeometryN(j).RingN(k).STPointN(m).Lat, (double)geo.STGeometryN(j).RingN(k).STPointN(m).Long));
                                }
                                sb.AppendLine("\"/>");
                            }
                        }
                    }
                }
                sb.AppendLine("</m:MapLayer>");
            }
            return sb.ToString();
        }

        /// <summary>
        /// XamlGeometryCollection
        ///     builds XAML string for a MapLayer containing multiple MapPolygons and/or MapPolylines
        /// </summary>
        /// <param name="geo">SqlGeography geo</param>
        /// <param name="table">string layer table</param>
        /// <param name="ID">string ID of current record</param>
        /// <returns>string of XAML</returns>
        private string XamlGeometryCollection(SqlGeography geo, string table, string ID)
        {

            StringBuilder sb = new StringBuilder();
            if (geo.STNumGeometries() > 0)
            {
                totalPoints += (int)geo.STNumPoints();
                sb.AppendLine("<m:MapLayer x:Name=\"" + ID + "\">");
                for (int j = 1; j <= geo.STNumGeometries(); j++)
                {
                    if (geo.STGeometryN(j).NumRings() > 0)
                    {
                        for (int k = 1; k <= geo.STGeometryN(j).NumRings(); k++)
                        {
                            if (geo.STGeometryN(j).RingN(k).STNumPoints() > 1)
                            {
                                sb.Append("<m:MapPolygon Locations=\"");
                                for (int m = 1; m <= geo.STGeometryN(j).RingN(k).STNumPoints(); m++)
                                {
                                    if (m > 1) sb.Append(" ");
                                    sb.Append(String.Format("{0:0.#####},{1:0.#####}", (double)geo.STGeometryN(j).RingN(k).STPointN(m).Lat, (double)geo.STGeometryN(j).RingN(k).STPointN(m).Long));
                                }
                                sb.AppendLine("\"/>");
                            }
                        }
                    }
                }
                sb.AppendLine("</m:MapLayer>");
            }
            return sb.ToString();
        }

        // Add more operations here and mark them with [OperationContract]
    }

    public static class StringHelpers
    {
      

        public static string UF8toNormalString(this string s)
        {

            return Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(s));
            

        }

       
    }


}
