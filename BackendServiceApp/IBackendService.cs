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
        string UpdateUserREST(UserLegitimation user, Person person);
                
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
        string InsertTripDataREST(long id, UserLegitimation user, Person person, TripJson trip);
        
        [OperationContract]  
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.UpdateTripDataRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        string UpdateTripDataREST(long id, UserLegitimation user, Person person, TripJson trip);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.AuthenticateUserRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        string AuthenticateUserREST(UserLegitimation user);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.SendEmailForNewPasswordRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        string SendEmailForNewPasswordREST(UserLegitimation user);

        //[OperationContract]
        //[WebInvoke(Method = "POST",
        //    UriTemplate = Routing.AuthenticateActivationCodeRoute,
        //    BodyStyle = WebMessageBodyStyle.Wrapped,
        //    ResponseFormat = WebMessageFormat.Json,
        //    RequestFormat = WebMessageFormat.Json
        //  )]
        //string AuthenticateActivationCodeREST(UserLegitimation user, string activationCode);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.ResetPasswordRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        string ResetPasswordREST(UserLegitimation user, string activationCode);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.DeleteUserRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        string DeleteUserREST(UserLegitimation user);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.DeleteTripByIdRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        string DeleteTripByIdREST(UserLegitimation user, long id);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = Routing.DeleteAllTripsRoute,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            ResponseFormat = WebMessageFormat.Json,
            RequestFormat = WebMessageFormat.Json
          )]
        string DeleteAllTripsREST(UserLegitimation user);
    }
  
    // Use a data contract as illustrated in the sample below to add composite types to service operations.

    //default value is 0 (user does not set anything)
    public enum NumberOfChildren
    { 
        NotProvided = 0,
        NoChildren,
        One,
        Two,
        Three,  
        Four, 
        FiveOrMore
    }

    public enum Subscription
    {
        NotProvided = 0,
        TravelCard,
        MonthlyCard,
        SingleTickets
    }
    public enum Gender
    {
        NotProvided = 0,
        Male,
        Female
    }
    
    public enum TransportMode
    {
        NotProvided = 0,
        Bus, 
        Tram,
        Subway,
        Walk,
        Bike
    }

    public enum MaritalStatus
    {
        NotProvided = 0,
        Single,
        Married,
        Partner,
        Divorced,
        Widowed
    }

    public enum Residence
    {
        NotProvided = 0,
        Oslo,
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
    
    public enum Area
    {
        NotProvided = 0,
        Alna,
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
        NotProvided = 0,
        Employed,
        Retired,
        Student,
        MilitaryService,
        WorkingAtHome,
        NotWorking,
        Other
    }
    
    public enum TripPurpose
    {   
        NotProvided = 0,
        Work,
        BusinessService,
        School,
        AccompanyOthers,
        ShoppingService,
        Leisure,
        Home
    }

    //tracking info for location is saved in this class structure
    [DataContract]
    public class ActivationCode
    {
        [BsonId]
        [DataMember]
        public string acID { get; set; }//MongoDB purpose
        [DataMember]
        public string userName { get; set; }

        [DataMember]
        public string activationCode { get; set; }
    }

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
        public int GenderId { get; set; }
        [DataMember]
        public string GenderName { get; set; }
        [DataMember]
        public int maritalStatusId { get; set; }
        [DataMember]
        public string maritalStatusName { get; set; }        
        [DataMember]
        public int numChildrenId { get; set; }
        [DataMember]
        public string numChildrenName { get; set; }
        [DataMember]
        public int birthyearId { get; set; }
        [DataMember]
        public string birthyearName { get; set; }//0: Not tell, 1: Under 18        
        [DataMember]
        public int occupationId { get; set; }
        [DataMember]
        public string occupationName { get; set; }
        [DataMember]
        public int subscriptionId { get; set; }
        [DataMember]
        public string subscriptionName { get; set; }
        [DataMember]
        public int residenceId { get; set; }
        [DataMember]
        public string residenceName { get; set; }
        [DataMember]
        public int areaId { get; set; }
        [DataMember]
        public string areaName { get; set; }
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
    
    [DataContract]
    public class Trip
    {
        [BsonId]
        [DataMember]
        public string tripID { get; set; }//MongoDB purpose
        [DataMember]
        public string userID { get; set; }//MongoDB purpose
        //[DataMember]
        //public string startLocationID { get; set; }
        //[DataMember]
        //public string endLocationID { get; set; }
        [DataMember]
        public double distance { get; set; }
        [DataMember]
        public DateTime tripDate { get; set; }
        [DataMember]
        public int tripPurposeId { get; set; }
        [DataMember]
        public string tripPurposeName { get; set; }
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
        public double altitude { get; set; }
        [DataMember]
        public double accuracy { get; set; }
        [DataMember]
        public double altitudeAccuracy { get; set; }
        [DataMember]
        public double heading { get; set; }
        [DataMember]
        public double speed { get; set; } //in km/h
        //[DataMember] //For now, mode relates to trip chain by comparing timestamps to decide which mode is used in the specific trip chain.
        //public int mode { get; set; }
        [DataMember]
        public bool questionable { get; set; }
        [DataMember]
        public string tripChainPurpose { get; set; }
    }

    [DataContract]
    public class Mode
    {
        [BsonId]
        [DataMember]
        public string modeID { get; set; }
        [DataMember]
        public string tripID { get; set; }
        [DataMember]
        public int mode { get; set; }
        [DataMember]
        public long timestamp { get; set; }
        
    }

    [DataContract]
    public class TripJson
    {
        [DataMember]
        public TripDetailJson meta { get; set; }
        [DataMember]
        public List<TransportModeJson> modes { get; set; }
        [DataMember]
        public List<TripChainJson> entries { get; set; }
    }

    [DataContract]
    public class TripChainJson
    {
        [DataMember]
        public long timestamp { get; set; }
        [DataMember]
        public double latitude { get; set; }
        [DataMember]
        public double longitude { get; set; }
        [DataMember]
        public double altitude { get; set; }
        [DataMember]
        public double accuracy { get; set; }
        [DataMember]
        public double altitudeAccuracy { get; set; }
        [DataMember]
        public double heading { get; set; }
        [DataMember]
        public double speed { get; set; }
    }

    [DataContract]
    public class TransportModeJson
    {
        [DataMember]
        public int mode { get; set; }
        [DataMember]
        public long time { get; set; }
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
    public const string ResetPasswordRoute = "/ResetPasswordREST";
    public const string SendEmailForNewPasswordRoute = "/SendEmailForNewPasswordREST";
    //public const string AuthenticateActivationCodeRoute = "/AuthenticateActivationCodeREST";
    public const string DeleteUserRoute = "/DeleteUserREST";
    public const string DeleteTripByIdRoute = "/DeleteTripByIdREST";
    public const string DeleteAllTripsRoute = "/DeleteAllTripsREST";
}
