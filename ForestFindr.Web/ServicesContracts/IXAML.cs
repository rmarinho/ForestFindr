using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.Runtime.Serialization;
using LinqToWikipedia;
using System.Collections.ObjectModel;

namespace ForestFindr.Web.ServicesContracts
{
  
    [ServiceContract]
    public interface IXAML
    {

        /// <summary>
        /// IXAML - XAML Interface
        /// </summary>
        /// <param name="Parameters">XAMLParameters Parameters</param>
        /// <returns>XAMLResponse</returns>
        /// <remarks>
        ///       input XAMLParameters
        ///      string table - DB table to query
        ///      string querytype - buffer || bbox
        ///      string radius - distance in meters from center of buffer
        ///      string points - buffer = Point LL || Polyline LL
        ///                    - bbox = SE.LL, NE.LL, NW.LL, SW.LL, SE.LL
        ///      string reduce - a node generalizing factor for Douglas-Peucker algorithm
        ///                      http://msdn.microsoft.com/en-us/library/cc627410.aspx
        ///                      
        ///   output XAMLResponse
        ///      int ErrorCode - success = 0, error = 1
        ///      string OutputMessage - default "Success" 
        ///      double QueryTime in miliseconds
        ///      int totalPoints
        ///      List<XAMLShape> OutputShapes
        ///              string Shape.ID - "ID" column from table
        ///              Dictionary<string, string> Fields - FieldName: FieldValue
        ///      string XAML result
        /// </remarks>
        [OperationContract]
        XAMLResponse GetSQLDataXAML(XAMLParameters Parameters);

        [OperationContract]
        XAMLResponse GetSQLDataAreaXAML(XAMLParameters Parameters);
    }

    [DataContract]
    public class XAMLResponse
    {
        [DataMember]
        public int ErrorCode;

        [DataMember]
        public string OutputMessage;

        [DataMember]
        public List<XAMLFields> OutputFields;

        [DataMember]
        public string XAML;

        [DataMember]
        public double QueryTime;

        [DataMember]
        public int totalPoints;
    }

    [DataContract]
    public struct XAMLParameters
    {
        [DataMember]
        public string table;

        [DataMember]
        public string querytype;

        [DataMember]
        public double radius;

        [DataMember]
        public string points;

        [DataMember]
        public double reduce;
        [DataMember]
        public string value;
    }

    [DataContract]
    public struct XAMLFields
    {
        [DataMember]
        public string ID;

        [DataMember]
        public Dictionary<string, string> Fields;
    }
}