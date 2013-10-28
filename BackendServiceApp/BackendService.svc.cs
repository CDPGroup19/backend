using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.ServiceModel.Activation;
using Json;
using System.IO;
using MongoDB.Driver.Linq;
using System.Web;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace BackendServiceApp
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class BackendService : IBackendService
    {
        static string _dbName = "uploaddata";
        static string _userName = "anhchi";
        static string _passWord = "12345";

        #region MongoDB services and middle services
        public MongoClient CreateMongoClient(string dbName, string userName, string passWord)
        {
            MongoClient result = null;
            try
            {
                var credential = MongoCredential.CreateMongoCRCredential(dbName, userName, passWord);
                var settings = new MongoClientSettings
                {
                    Credentials = new[] { credential }
                };
                var mongoClient = new MongoClient(settings);
                result = mongoClient;
            }
            catch (Exception e)
            {
                //Handle exception
            }
            return result;
        }

        public MongoDatabase GetMongoDatabase(string dbName, string userName, string passWord)
        {
            try
            {
                MongoClient client = CreateMongoClient(dbName, userName, passWord);
                if (client != null)
                {
                    MongoServer server = client.GetServer();
                    if (server == null)
                        throw new Exception();
                    MongoDatabase resultDB = server.GetDatabase(dbName);
                    return resultDB;
                }
            }
            catch (Exception e)
            {
                //Handle exception
            }
            return null;
        }
        
        /// <summary>
        /// Insert a new user into data storage
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="dbUserName"></param>
        /// <param name="dbPassWord"></param>
        /// <param name="userName"></param>
        /// <param name="pinCode"></param>
        /// <param name="gender"></param>
        /// <param name="birthYear"></param>
        /// <param name="occupation"></param>
        /// <param name="travelCard"></param>
        /// <param name="community"></param>
        /// <param name="city"></param>
        /// <param name="phoneNumber"></param>
        /// <returns>userID</returns>
        public string InsertUser(string dbName, string dbUserName, string dbPassWord, UserLegitimation user)
        {
            string userID;
            string result;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, dbUserName, dbPassWord);
                if (resultDB == null)
                    throw new Exception();
                MongoCollection<UserLegitimation> users = resultDB.GetCollection<UserLegitimation>("users");
                userID = MakeId(user.userName, user.pinCode);
                
                UserLegitimation newUser = new UserLegitimation
                {
                    userID = userID,
                    userName = user.userName,
                    pinCode = user.pinCode
                };
                users.Insert(newUser);                
                result = userID;
            }
            catch (Exception e)
            {
                //Handle exception
                result = null;
            }
            return result;
        }

        public bool UpdateUser(string dbName, string dbUserName, string dbPassWord, UserLegitimation user, Person person)
        {
            bool result = false;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, dbUserName, dbPassWord);
                if (resultDB == null)
                    throw new Exception();
                MongoCollection<Person> persons = persons = resultDB.GetCollection<Person>("persons");                
                if (persons.Count() > 0)
                {
                    var personQuery = Query<Person>.EQ(e => e.userID, user.userID);
                    //var temp = persons.FindOne(personQuery);
                    if (personQuery != null)
                    { 
                        persons.Remove(personQuery);                        
                    }                    
                }
                //File.WriteAllText(@"C:\Visual Studio 2013\JsonData\updateuser.txt", "(persons.Count()) " + persons.Count() + " (birthyear) "+person.birthYear + " (phonenumber) " + person.phoneNumber);
                Person newPerson = new Person
                {
                    userID = user.userID,
                    gender = person.gender,
                    birthYear = person.birthYear,
                    travelCard = person.travelCard,
                    occupation = person.occupation,
                    community = person.community,
                    city = person.city,
                    phoneNumber = person.phoneNumber
                };
                persons.Insert(newPerson);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
        }

        private string MakeId(string userName, string phoneNumber)
        {
            //GetHashCode() can return negative values.
            //You must not have any logic that works with GetHashCode() values.
            //GetHashCode() is not guaranteed to be unique and can change between builds.
            //So we use System.Security.Cryptography.SHA512Managed
            string result = null;
            try
            {
                if (!userName.Equals(null) && !phoneNumber.Equals(null))
                    result = CreateSHAHash(userName, phoneNumber);        
            }
            catch (Exception)
            {
                //Handle exception
                result = null;
            }
            return result;
        }

        public string CreateSHAHash(string Password, string Salt)
        {
            System.Security.Cryptography.SHA512Managed HashTool = new System.Security.Cryptography.SHA512Managed();
            Byte[] PasswordAsByte = System.Text.Encoding.UTF8.GetBytes(string.Concat(Password, Salt));
            Byte[] EncryptedBytes = HashTool.ComputeHash(PasswordAsByte);
            HashTool.Clear();
            return Convert.ToBase64String(EncryptedBytes);
        }

        public string InsertTripData(string dbName, string dbUserName, string dbPassWord, UserLegitimation user, Trip trip, List<TripChainJson> locations)
        {
            string result = null;
            string locID = null;
            try
            {                
                MongoDatabase resultDB = GetMongoDatabase(dbName, dbUserName, dbPassWord);
                if (resultDB == null)
                    throw new Exception();
                //Check locations exists. If not, add and return new locID. If exists, return locID.
                int i = 0;
                MongoCollection<TripChain> tripChains = resultDB.GetCollection<TripChain>("tripChains");
                MongoCollection<Location> mglocations = resultDB.GetCollection<Location>("locations");
                foreach(TripChainJson tj in locations)
                {
                    double latitude = double.Parse(tj.numbers[1].ToString());
                    double longitude = double.Parse(tj.numbers[2].ToString());
                    locID = CheckLocExists(latitude, longitude, mglocations);
                    if (locID == null)
                        locID = AddLocation(dbName, dbUserName, dbPassWord, tj);
                    if (i == 0)
                        trip.startLocationID = locID;
                    if (i == locations.Count - 1)
                        trip.endLocationID = locID;
                    i++;
                    TripChain tc = new TripChain 
                    {
                        tripID = trip.tripID, 
                        tripChainID = tj.numbers[0].ToString(), 
                        locationChainID = locID,
                        timeStamp = long.Parse(tj.numbers[0].ToString()),
                    };
                    tripChains.Insert(tc);                    
                }
                
                MongoCollection<Trip> trips = resultDB.GetCollection<Trip>("trips");
                File.WriteAllText(@"C:\Visual Studio 2013\JsonData\inserttrip.txt", "(trips.Count()) " + trips.Count() + " (tripID) " + trip.tripID + " (tripDate) " + trip.tripDate);
                //tripID will be made from the front end
                Trip newTrip = new Trip
                {
                    tripID = trip.tripID,
                    userID = user.userID,
                    startLocationID=trip.startLocationID,
                    endLocationID=trip.endLocationID,
                    tripDate=trip.tripDate,
                    tripPurpose=trip.tripPurpose                    
                };
                trips.Insert(newTrip);
                result = trip.tripID;
            }
            catch (Exception e)
            {
                result = null;
            }
            return result;
        }

        private string AddLocation(string dbName, string dbUserName, string dbPassWord, TripChainJson tj)
        {
            string result = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, dbUserName, dbPassWord);
                if (resultDB == null)
                    throw new Exception();
                MongoCollection<Location> locations = resultDB.GetCollection<Location>("locations");
                string locID = MakeId(tj.numbers[1].ToString(), tj.numbers[2].ToString());
                Location newLoc = new Location
                {
                    locationID = locID,
                    longitude = double.Parse(tj.numbers[2].ToString()),
                    latitude = double.Parse(tj.numbers[1].ToString())
                };
                locations.Insert(newLoc);
                result = locID;
            }
            catch (Exception e)
            {
                result = null;
            }
            return result;
        }

        private string CheckLocExists(double latitude, double longitude, MongoCollection<Location> locations)
        {
            string locationID = null;
            try
            {
                if (locations.Count() > 0)
                {
                    var locationQuery = Query.And(Query.EQ("longtitude", longitude), Query.EQ("latitude", latitude));
                    locationID = (locationQuery as Location).locationID;
                }
            }
            catch (Exception e)
            {
                locationID = null;
            }
            return locationID;
        }
               
        //private string LocationExists(string longitude, string latitude, MongoCollection<Location> locations)
        //{
        //    string locationID = null;
        //    try
        //    {
        //        if (locations.Count() > 0)
        //        {            
        //            var locationQuery = Query.And(Query.EQ("longtitude", longitude), Query.EQ("latitude", latitude));            
        //            locationID = (locationQuery as Location).locationID;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        locationID = null;
        //    }
        //    return locationID;
        //}

        public bool UpdateTripData(string dbName, string dbUserName, string dbPassWord, UserLegitimation user, Trip trip, TripChain tripChain)
        {
            bool result = false;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, dbUserName, dbPassWord);
                if (resultDB == null)
                    throw new Exception();
                //Get locationID 
                MongoCollection<TripChain> tripChains = resultDB.GetCollection<TripChain>("tripChains");
                //Add new trip chain
                //Edit trip chain
                if (tripChains.Count() > 0)
                {
                    //var tripChainsQuery = from tc in tripChains.AsQueryable<TripChain>() where tc..longitude == longitude && l.latitude == latitude select l;
                    //locationID = (locationQuery as Location).locationID;
                }
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Json object from front-end parse: userID
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="user">json object</param>
        /// <returns>the Person object of the given userID</returns>
        
        public Person GetPersonByUserId(string dbName, string userName, string passWord, UserLegitimation user)
        {
            MongoCollection<Person> persons = null;
            Person result = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, userName, passWord);
                if (resultDB == null)
                    throw new Exception();
                persons = resultDB.GetCollection<Person>("persons");                
                if (persons.Count() > 0)
                {
                    var personQuery = from p in persons.AsQueryable<Person>() where p.userID == user.userID select p;
                    result = personQuery as Person;
                }
            }
            catch (Exception e)
            {
                //Handle exception
            }
            return result;
        }

        #region For reference
        
        //must have an ID when using Save 
        //MongoCollection<BsonDocument> books;
        //var query = Query.And(
        //    Query.EQ("author", "Kurt Vonnegut"),
        //    Query.EQ("title", "Cats Craddle")
        //);
        //BsonDocument book = books.FindOne(query);
        //if (book != null)
        //{
        //    book["title"] = "Cat's Cradle";
        //    books.Save(book);
        //}

        //public List<Person> GetPersons(string dbName, string userName, string passWord)
        //{
        //    List<Person> result = new List<Person>();
        //    try
        //    {
        //        MongoCollection<Person> collection = GetPersonsMongoCollection(dbName, userName, passWord);
        //        MongoCursor<Person> cursor = collection.FindAllAs<Person>();
        //        result = cursor.ToList();
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        //Handle exception
        //    }
        //    return null;
        //}

        //public MongoCollection<UserLegitimation> GetUsersMongoCollection(string dbName, string userName, string passWord)
        //{
        //    MongoCollection<UserLegitimation> result = null;
        //    try
        //    {
        //        MongoDatabase resultDB = GetMongoDatabase(dbName, userName, passWord);
        //        result = resultDB.GetCollection<UserLegitimation>("users");
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        //Handle exception
        //    }
        //    return null;
        //}
        //public List<UserLegitimation> GetUsers(string dbName, string userName, string passWord)
        //{
        //    List<UserLegitimation> result = new List<UserLegitimation>();
        //    try
        //    {
        //        MongoCollection<UserLegitimation> collection = GetUsersMongoCollection(dbName, userName, passWord);
        //        MongoCursor<UserLegitimation> cursor = collection.FindAllAs<UserLegitimation>();
        //        result = cursor.ToList();
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        //Handle exception
        //    }
        //    return null;
        //}
        #endregion

        /// <summary>
        /// Json object from front-end parse: userID
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="user">json object</param>
        /// <returns>list of trips of this users</returns>
        public List<Trip> GetTripsByUserId(string dbName, string userName, string passWord, UserLegitimation user)
        {
            List<Trip> result = null;
            MongoCollection<Trip> trips = null;

            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, userName, passWord);
                if (resultDB == null)
                    throw new Exception();
                trips = resultDB.GetCollection<Trip>("trips");
                
                if(trips.Count() > 0)
                {
                    var tripsQuery = from t in trips.AsQueryable<Trip>() where t.userID == user.userID select t;
                    result = tripsQuery as List<Trip>;
               }
            }
            catch (Exception e)
            {
                //Handle exception
            }
            return result;
        }

        /// <summary>
        /// Json object from front-end parse: tripID
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="trip"></param>
        /// <returns>A list of trip chains in the given tripID</returns>
        public List<TripChain> GetTripChains(string dbName, string userName, string passWord, Trip trip)
        {
            List<TripChain> result = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, userName, passWord);
                if (resultDB == null)
                    throw new Exception();
                MongoCollection<TripChain> tripChains = resultDB.GetCollection<TripChain>("tripChains");

                if (tripChains.Count() > 0)
                {
                    var tripChainsQuery = from tc in tripChains.AsQueryable<TripChain>() where tc.tripID == trip.tripID select tc;
                    result = tripChainsQuery as List<TripChain>;
                }
            }
            catch (Exception e)
            {
                //Handle exception
            }
            return result;            
        }

        public List<Trip> GetTripsByDate(string dbName, string userName, string passWord, UserLegitimation user, DateTime date)
        {
            List<Trip> result = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, userName, passWord);
                if (resultDB == null)
                    throw new Exception();
                MongoCollection<Trip> trips = resultDB.GetCollection<Trip>("trips");

                if (trips.Count() > 0)
                {
                    var tripsQuery = from t in trips.AsQueryable<Trip>() where t.userID == user.userID && t.tripDate == date select t;
                    result = tripsQuery as List<Trip>;
                }
            }
            catch (Exception e)
            {
                //Handle exception
            }
            return result;
        }
        
        #endregion

        #region REST services
        public string InsertNewUserREST(UserLegitimation user)
        {            
            string  result = null;       
            try
            { 
                result = InsertUser(_dbName, _userName, _passWord, user);
                //File.WriteAllText(@"C:\Visual Studio 2013\JsonData\text.txt", "user is : (userID) = " + result + " (userName) = " + user.userName + " (pinCode) = " + user.pinCode +  DateTime.Now);          
            }
            catch (Exception e)
            {
                //Handle exception                
            }            
            return result;
        }
        public string InsertTripDataREST(long id, UserLegitimation user, TripJson trip)
        {
  //          "id":1382525646764,
  //"user":{"userName":"kitty","pinCode":"1234"},
  //"trip":{
  //  "meta":{
  //    "startTime":1382441890677,
  //    "distance":21,
  //    "purpose":0
  //  },
  //  "entries":[[1382525629947,63.4136106,10.4155071], [1382525635930,63.4136106,10.4155071],[1382525641932,63.4136106,10.4155071]]
  //}
            
            //TripJson tripjs = JsonConvert.DeserializeObject<TripJson>(trip);
            
            string result = null;
            Trip tripData = new Trip();
            List<TripChainJson> locations = new List<TripChainJson>();
            string locs = "";
            string tripstr = trip.ToJson();
            try
            {                
                //user.userID = AuthenticateUser(_dbName, _userName, _passWord, user);
                //if (user.userID == null)
                //    throw new Exception();
                tripData.tripID = id.ToString();
                if (trip.meta != null) locs = "trip meta not null"; else locs = "trip meta null";
                if (trip.entries != null) locs += " trip entries not null"; else locs += " trip entries null";
                //if (tripjs.meta != null)
                //{
                //    tripData.tripDate = new DateTime(1970, 1, 1) + new TimeSpan(tripjs.meta.startTime * 10000);                                 
                //    tripData.distance = tripjs.meta.distance;
                //    tripData.tripPurpose = tripjs.meta.purpose;
                //}
                //if (trip.entries != null)
                //{                    
                //    foreach (var e in trip.entries)
                //    {
                //        TripChainJson tcjs = new TripChainJson() { numbers = e.numbers };
                //        locations.Add(tcjs);
                //    }
                //}
                
                //if (locations.Count() > 0)
                //{
                //    foreach (TripChainJson tc in locations)
                //    {
                //        for (int i = 0; i < tc.numbers.Count(); i++)
                //        {
                //            if (tc.numbers[i] != null)
                //            {
                //                locs += tc.numbers[i].ToString();
                //            }
                //        }
                //        locs += "//";
                //    }
                //}
                //File.WriteAllText(@"C:\Visual Studio 2013\JsonData\inserttripdata.txt", " id " + id + " user has : (userName) = " + user.userName + " (pinCode) =  " + user.pinCode + " trip = " + trip.ToString() + " " + DateTime.Now);
                File.WriteAllText(@"C:\Visual Studio 2013\JsonData\inserttripdata.txt", " id " + id + " trip has (tripDate) = " + tripData.tripDate + " (distance) = " + tripData.distance + " (purpose) = " + tripData.tripPurpose + " " + locs + " tripstr = " + tripstr + " " + DateTime.Now);
                
                //result = InsertTripData(_dbName, _userName, _passWord, user, tripData, locations);
            }
            catch (Exception e)
            {
                result = null;
            }
            return result;
        }
              
        public bool UpdateTripDataREST(UserLegitimation user, Trip trip, TripChain tripChain)
        { 
            return false;
        }

        /// <summary>
        /// Json object from front-end parses: username, pincode
        /// if user exists returns userID
        /// if user not exists returns null
        /// </summary>
        /// <param name="user"></param>
        /// <returns>userID</returns>
        public string AuthenticateUser(string dbName, string userName, string passWord, UserLegitimation user)
        {
            string userID = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, userName, passWord);
                if (resultDB == null)
                    throw new Exception();
                MongoCollection<UserLegitimation> users = resultDB.GetCollection<UserLegitimation>("users");
                if (users.Count() > 0)
                {
                    var userQuery = Query.And(Query.EQ("userName", user.userName), Query.EQ("pinCode", user.pinCode));
                    userID = (userQuery as UserLegitimation).userID;
                }
            }
            catch (Exception e)
            {
                userID = null;
            }
            return userID;
        }      


        public List<Trip> GetTripsByDateREST(UserLegitimation user, DateTime date)
        {
            throw new NotImplementedException();
        }

        public List<TripChain> GetTripChainsOfATripREST(Trip trip)
        {
            throw new NotImplementedException();
        }

        public List<Trip> GetTripsByUserIdREST(UserLegitimation user)
        {
            throw new NotImplementedException();
        }


        public bool UpdateUserREST(UserLegitimation user, Person person)
        {
            bool result = false;
            try
            {
                result = UpdateUser(_dbName, _userName, _passWord, user, person);
                //File.WriteAllText(@"C:\Visual Studio 2013\JsonData\text.txt", "user is : (userID) = " + result + " (userName) = " + user.userName + " (pinCode) = " + user.pinCode +  DateTime.Now);          
            }
            catch (Exception e)
            {
                //Handle exception                
            }
            return result;
        }
        #endregion               
    

        public string AuthenticateUserREST(UserLegitimation user)
        {
            throw new NotImplementedException();
        }
    }

    //public class Utilities : IDispatchMessageInspector
    //{
    //    public string USER_HEADER1 = "Origin";
    //    public string USER_HEADER2 = "Access-Control-Allow-Origin";
    //    public static object sourceDomain = null;
    //    object IDispatchMessageInspector.AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
    //    {
    //        request.Headers.CopyHeaderFrom()
    //        HttpRequestMessageProperty httpRequestMessage;
    //        object httpRequestMessageObject;
    //        if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
    //        {
    //            httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
    //            sourceDomain = httpRequestMessage.Headers[USER_HEADER1];
    //            request.Headers.
    //            //Response.AppendHeader("Access-Control-Allow-Origin", sourceDomain );
    //        }
    //        return null;
    //    }

    //    void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
    //    {
    //        MessageHeader newMessageHeader = MessageHeader.CreateHeader(USER_HEADER2,)
    //        HttpResponseMessageProperty httpResponseMessage;
    //        reply.Headers.Add(Properties.TryGetValue
    //        responseMessageProperty.Headers.Add(USER_HEADER2, sourceDomain);
    //        responseMessageProperty.Headers.Add(USER_HEADER2)
    //            reply.Headers.CopyHeadersFrom(message.Headers);
    //    replacedMessage.Properties.CopyProperties(message.Properties);
    //    {
    //        // Inspect the reply, catch a possible validation error 
    //        try
    //        {
    //            ValidateMessageBody(ref reply, false);
    //        }
    //        catch ((ReplyValidationFault fault)
    //        {
    //            // if a validation error occurred, the message is replaced
    //            // with the validation fault.
    //            reply = Message.CreateMessage(reply.Version, 
    //                    fault.CreateMessageFault(), reply.Headers.Action);
    //        }
    //    }
        
    //    public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
    //    {
    //        HttpRequestMessageProperty httpRequestMessage;
    //        object httpRequestMessageObject;
    //        if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
    //        {
    //            httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
    //            sourceDomain = httpRequestMessage.Headers[USER_HEADER1];
    //            //Response.AppendHeader("Access-Control-Allow-Origin", sourceDomain );
    //        }
    //        return null;
    //    }

    //    public void AfterReceiveReply(ref Message reply, object correlationState)
    //    {
    //        HttpResponseMessageProperty httpResponseMessage = reply.Headers.GetHeader<;
    //        httpResponseMessage.Headers.Add(USER_HEADER2, sourceDomain.ToString());
    //        object httpResponseMessageObject = httpResponseMessage;
    //        reply.Headers.Add((MessageHeader)httpResponseMessageObject);
    //    }
    //}

}



