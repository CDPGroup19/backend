using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace BackendServiceApp
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IBackendService" in both code and config file together.
    [ServiceContract]
    public interface IBackendService
    {        
        [OperationContract]       
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.InsertNewUserRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        string InsertNewUserREST(UserLegitimation user);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.UpdateUserRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        bool UpdateUserREST(UserLegitimation user, Person person);
                
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.GetTripsByDateRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]

        List<Trip> GetTripsByDateREST(UserLegitimation user, DateTime date);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.GetTripChainsRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        List<TripChain> GetTripChainsOfATripREST(Trip trip);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.GetTripsByUserIdRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        List<Trip> GetTripsByUserIdREST(UserLegitimation user);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.InsertTripDataRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        string InsertTripDataREST(long id, UserLegitimation user, TripJson trip);
        
        [OperationContract]  
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.UpdateTripDataRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        bool UpdateTripDataREST(UserLegitimation user, Trip trip, TripChain tripChain);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.AuthenticateUserRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        string AuthenticateUserREST(UserLegitimation user);
    }
  
    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    
    public enum Gender
    { 
        Male = 0,
        Female
    }
    
    public enum TransportMode
    { 
        Walk = 0, 
        Bike,
        Bus,
        Tram,
        Car
    }
    
    public enum ResidenceCommunity
    { 
        Oslo = 0,
        Bærum,
        Asker,
        Oppegård,
        Nesodden,
        Ski,
        Frogn,
        Vestby,
        Ås,
        Enebakk,
        AurskogHøland,
        Fet,
        Sørum,
        Ullensaker,
        Eidsvoll,
        Hurdal,
        Nannestad,
        Nittedal,
        Gjerdrum,
        Skedsmo,
        Lørenskog,
        Rælingen,
        Nes,
        Other
    }
    
    public enum City
    {
        Alna = 0,
        Bjerke,
        Frogner,
        GamleOslo,
        Grorud,
        Grünerløkka,
        NordreAker,
        Nordstrand,
        Sagene,
        StHanshaugen,
        Stovner,
        SøndreNordstrand,
        Ullern,
        VestreAker,
        Østensjø
    }
    
    public enum Occupation
    {
        Employed = 0,
        Retired,
        Student,
        MilitaryService,
        WorkingAtHome,
        NotWorking,
        Other
    }
    
    public enum TripPurpose
    {
        Work = 0,
        BusinessService,
        School,
        AccompanyOthers,
        ShoppingService,
        Leisure,
        Home
    }
    
    //tracking info for location is saved in this class structure
    [DataContract]
    public class Location
    {
        [BsonId]
        [DataMember]
        public string locationID { get; set; }//MongoDB purpose
        [DataMember]
        public double longitude { get; set; }
        [DataMember]
        public double latitude { get; set; }
        [DataMember]
        public string name { get; set; }
    }
    
    [DataContract]
    public class Person
    {
        [BsonId]
        [DataMember]
        public string userID { get; set; }//MongoDB purpose        
        [DataMember]
        public int gender { get; set; }
        [DataMember]
        public int birthYear { get; set; }
        [DataMember]
        public int occupation { get; set; }
        [DataMember]
        public bool travelCard { get; set; }
        [DataMember]
        public int community { get; set; }
        [DataMember]
        public int city { get; set; }
        [DataMember]
        public string phoneNumber { get; set; }
    }    
        
    [DataContract]
    public class UserLegitimation 
    {
        [BsonId]
        [DataMember]
        public string userID { get; set; }//MongoDB purpose
        [DataMember]
        public string userName { get; set; }
        [DataMember]
        public string pinCode { get; set; } 
    }
    //[DataContract]
    //public class TripHistory 
    //{
    //    [DataMember]
    //    public int tripHistoryID { get; set; }
    //    [DataMember]
    //    public int userID { get; set; }//MongoDB purpose
    //    [DataMember]
    //    public int tripID { get; set; }//MongoDB purpose
    //}
    [DataContract]
    public class Trip
    {
        [BsonId]
        [DataMember]
        public string tripID { get; set; }//MongoDB purpose
        [DataMember]
        public string userID { get; set; }//MongoDB purpose
        [DataMember]
        public string startLocationID { get; set; }
        [DataMember]
        public string endLocationID { get; set; }
        [DataMember]
        public double distance { get; set; }
        [DataMember]
        public DateTime tripDate { get; set; }
        [DataMember]
        public int tripPurpose { get; set; }
    }
        
    [DataContract]
    public class TripChain
    {
        [BsonId]
        [DataMember]
        public string tripChainID { get; set; }//MongoDB purpose
        [DataMember]
        public string tripID { get; set; }//MongoDB purpose
        [DataMember]
        public string locationChainID { get; set; }//MongoDB purpose
        [DataMember]
        public long timeStamp { get; set; }
        [DataMember]
        public float timeAmount { get; set; } //in minutes
        [DataMember]
        public float speed { get; set; } //in km/h
        [DataMember]
        public int mode { get; set; }
        [DataMember]
        public bool questionable { get; set; }
        [DataMember]
        public string tripChainPurpose { get; set; }
    }

    [DataContract]
    public class TripJson
    {
        public TripDetailJson meta { get; set; }
        public List<TripChainJson> entries { get; set; }
    }

    [DataContract]
    public class TripChainJson
    {
        [DataMember]
        public List<object> numbers { get; set; }
    //    [DataMember]
    //    public long timeStamp { get; set; }
    //    [DataMember]
    //    public double latitude { get; set; }
    //    [DataMember]
    //    public double longitude { get; set; }
    }
    [DataContract]
    public class TripDetailJson
    {
        [DataMember]
        public long startTime { get; set; }
        [DataMember]
        public double distance { get; set; }
        [DataMember]
        public int purpose { get; set; }
    }
}

public static class Routing
{
    public const string InsertNewUserRoute = "/InsertNewUserREST";
    public const string UpdateUserRoute = "/UpdateUserREST";
    public const string InsertTripDataRoute = "/InsertTripDataREST";
    public const string UpdateTripDataRoute = "/UpdateTripDataREST";
    public const string AuthenticateUserRoute = "/AuthenticateUserREST";
    public const string GetTripsByDateRoute = "/GetTripsByDateREST";
    public const string GetTripChainsRoute = "/GetTripChainsREST";
    public const string GetTripsByUserIdRoute = "/GetTripsByUserIdREST";
}
