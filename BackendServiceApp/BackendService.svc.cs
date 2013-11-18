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
        public void InsertUser(string dbName, string dbUserName, string dbPassWord, UserLegitimation user, bool isInsert, out string statusMessage)
        {
            statusMessage = null;
            string userID;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, dbUserName, dbPassWord);
                if (resultDB == null)
                {
                    statusMessage = "Database is null";
                    return;
                }
                MongoCollection<UserLegitimation> users = resultDB.GetCollection<UserLegitimation>("users");
                userID = CheckUserExists(user.userName, users, out statusMessage);
                if (userID != null)
                {
                    if (isInsert)
                    {
                        statusMessage = "Username is taken.";
                        return;
                    }
                    else //remove the current user and insert new row with same userID and userName, only different pincode
                    {
                        var userQuery = Query.EQ("_id", userID);
                        if (userQuery != null)
                            users.Remove(userQuery);
                    }
                }
                if (isInsert)
                    userID = MakeId(user.userName, user.pinCode);                

                UserLegitimation newUser = new UserLegitimation
                {
                    userID = userID,
                    userName = user.userName,
                    pinCode = user.pinCode
                };
                users.Insert(newUser);
            }
            catch (Exception e)
            {
                //Handle exception
                statusMessage = "An exception has occured in InsertUser method. " + e.Message;
            }
        }

        private string CheckUserExists(string userName, MongoCollection<UserLegitimation> users, out string statusMessage)
        {
            statusMessage = null;
            string userID = null;
            try
            {
                if (users.Count() > 0)
                {
                    var userQuery = Query.EQ("userName", userName);
                    var existedUser = users.FindOne(userQuery);
                    if (existedUser != null)
                        userID = (existedUser as UserLegitimation).userID;
                }
            }
            catch (Exception e)
            {
                userID = null;
                statusMessage = "An exception has occured in CheckUserExists method. " + e.Message;
            }
            return userID;
        }

        public void UpdateUser(string dbName, string dbUserName, string dbPassWord, UserLegitimation user, Person person, out string statusMessage)
        {
            statusMessage = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, dbUserName, dbPassWord);
                if (resultDB == null)
                {
                    statusMessage = "Database is null";
                    return;
                }
                user.userID = AuthenticateUser(_dbName, _userName, _passWord, user, out statusMessage);
                if (statusMessage != null)
                    return;
                MongoCollection<Person> persons = persons = resultDB.GetCollection<Person>("persons");
                if (persons.Count() > 0)
                {
                    var personQuery = Query<Person>.EQ(e => e.userID, user.userID);
                    if (personQuery != null)
                    {
                        persons.Remove(personQuery);
                    }
                }

                Person newPerson = new Person
                {
                    userID = user.userID,
                    GenderId = person.GenderId,
                    birthyearId = person.birthyearId,
                    occupationId = person.occupationId,
                    residenceId = person.residenceId,
                    areaId = person.areaId,
                    numChildrenId = person.numChildrenId,
                    maritalStatusId = person.maritalStatusId,
                    subscriptionId = person.subscriptionId
                };
                                
                if (newPerson.birthyearId == 0)
                    newPerson.birthyearName = "NotProvided";
                else if (newPerson.birthyearId == 1)
                    newPerson.birthyearName = "Under18";
                else newPerson.birthyearName = newPerson.birthyearId.ToString();

                newPerson.GenderName = ((Gender)newPerson.GenderId).ToString();
                newPerson.occupationName = ((Occupation)newPerson.occupationId).ToString();
                newPerson.residenceName = ((Residence)newPerson.residenceId).ToString();
                newPerson.areaName = ((Area)newPerson.areaId).ToString();
                newPerson.numChildrenName = ((NumberOfChildren)newPerson.numChildrenId).ToString();
                newPerson.maritalStatusName = ((MaritalStatus)newPerson.maritalStatusId).ToString();
                newPerson.subscriptionName = ((Subscription)newPerson.subscriptionId).ToString();
                persons.Insert(newPerson);
            }
            catch (Exception e)
            {
                statusMessage += "An exception has occured in UpdateUser method. " + e.Message;
            }
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

        public void InsertTripData(string dbName, string dbUserName, string dbPassWord, UserLegitimation user, Person person, Trip trip, List<TripChainJson> locations, List<TransportModeJson> modes, bool isInsert, out string statusMessage)
        {
            statusMessage = null;
            string locID = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, dbUserName, dbPassWord);
                if (resultDB == null)
                {
                    statusMessage = "Database is null";
                    return;
                }

                UpdateUser(_dbName, _userName, _passWord, user, person, out statusMessage);
                if (statusMessage != null) return;
                //Check locations exists. If not, add and return new locID. If exists, return locID.

                MongoCollection<TripChain> tripChains = resultDB.GetCollection<TripChain>("tripChains");
                MongoCollection<Location> mgLocations = resultDB.GetCollection<Location>("locations");
                MongoCollection<Mode> mgModes = resultDB.GetCollection<Mode>("modes");
                foreach (TripChainJson tj in locations)
                {
                    locID = CheckLocExists(tj.latitude, tj.longitude, mgLocations, out statusMessage);
                    if (locID == null && statusMessage == null)
                        locID = AddLocation(dbName, dbUserName, dbPassWord, tj, out statusMessage);
                    if (statusMessage != null) return;
                    TripChain tc = new TripChain
                    {
                        tripID = trip.tripID,
                        tripChainID = tj.timestamp.ToString(),
                        locationChainID = locID,
                        timeStamp = tj.timestamp,
                        accuracy = tj.accuracy,
                        altitude = tj.altitude,
                        altitudeAccuracy = tj.altitudeAccuracy,
                        heading = tj.heading,
                        speed = tj.speed
                    };
                    if (isInsert == false) //this trip chain is existed -> remove it -> insert new info with same id (= existed id)
                    {
                        var tripChainQuery = Query<TripChain>.EQ(e => e.tripChainID, tc.tripChainID);
                        if (tripChainQuery != null)                                      
                            tripChains.Remove(tripChainQuery);
                    }
                    tripChains.Insert(tc);                    
                }

                foreach (TransportModeJson tm in modes)
                {
                    Mode m = new Mode
                    {
                        modeID = tm.time.ToString(),
                        tripID = trip.tripID,
                        timestamp = tm.time,
                        mode = tm.mode
                    };
                    if (isInsert == false) //this trip chain is existed -> remove it -> insert new info with same id (= existed id)
                    {
                        var modeQuery = Query<Mode>.EQ(e => e.timestamp, m.timestamp);
                        if (modeQuery != null)
                            mgModes.Remove(modeQuery);
                    }
                    mgModes.Insert(m);
                }

                MongoCollection<Trip> trips = resultDB.GetCollection<Trip>("trips");
                //tripID will be made from the front end
                Trip newTrip = new Trip
                {
                    tripID = trip.tripID,
                    userID = user.userID,
                    distance = trip.distance,
                    tripDate = trip.tripDate,
                    tripPurposeId = trip.tripPurposeId,
                    tripPurposeName = ((TripPurpose)trip.tripPurposeId).ToString()
                };
                if (isInsert == false) //this trip chain is existed -> remove it -> insert new info with same id (= existed id)
                {
                    var tripQuery = Query<Trip>.EQ(e => e.tripID, newTrip.tripID);
                    if (tripQuery != null)
                        trips.Remove(tripQuery);
                }
                trips.Insert(newTrip);
            }
            catch (Exception e)
            {
                statusMessage += "An exception has occured in InsertTripData method. " + e.Message;
            }
        }

        private string AddLocation(string dbName, string dbUserName, string dbPassWord, TripChainJson tj, out string statusMessage)
        {
            statusMessage = null;
            string result = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, dbUserName, dbPassWord);
                if (resultDB == null)
                {
                    statusMessage = "Database is null";
                    return result;
                }
                MongoCollection<Location> locations = resultDB.GetCollection<Location>("locations");
                string locID = MakeId(tj.latitude.ToString(), tj.longitude.ToString());
                Location newLoc = new Location
                {
                    locationID = locID,
                    longitude = tj.longitude,
                    latitude = tj.latitude
                };
                locations.Insert(newLoc);
                result = locID;
            }
            catch (Exception e)
            {
                result = null;
                statusMessage = "An exception has occured in AddLocation method. " + e.Message;
            }
            return result;
        }

        private string CheckLocExists(double latitude, double longitude, MongoCollection<Location> locations, out string statusMessage)
        {
            statusMessage = null;
            string locationID = null;
            try
            {
                if (locations.Count() > 0)
                {
                    var locationQuery = Query.And(Query.EQ("longitude", longitude), Query.EQ("latitude", latitude));
                    var existedLocation = locations.FindOne(locationQuery);
                    if (existedLocation != null)
                        locationID = (existedLocation as Location).locationID;
                    //File.WriteAllText(@"C:\locations.txt", "(current locationQuery) = " + locationQuery.ToJson() + " (current existedLocation) = " + existedLocation.ToJson() + " locationID = " + locationID + " " + DateTime.Now);
                }
            }
            catch (Exception e)
            {
                locationID = null;
                statusMessage = "An exception has occured in CheckLocExists method. " + e.Message;
            }
            return locationID;
        }

        /// <summary>
        /// NOT TESTED
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
                    var personQuery = Query.EQ("userID", user.userID);
                    var existedPerson = persons.FindOne(personQuery);
                    //var personQuery = from p in persons.AsQueryable<Person>() where p.userID == user.userID select p;
                    if (existedPerson != null)
                        result = existedPerson as Person;
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
        /// NOT TESTED
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

                if (trips.Count() > 0)
                {
                    //var tripsQuery = from t in trips.AsQueryable<Trip>() where t.userID == user.userID select t;
                    var tripsQuery = Query.EQ("userID", user.userID);
                    var existedTrips = trips.Find(tripsQuery);
                    if (existedTrips != null)
                        result = existedTrips.ToList<Trip>();
                }
            }
            catch (Exception e)
            {
                //Handle exception
            }
            return result;
        }

        /// <summary>
        /// NOT TESTED
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
                    var tripChainsQuery = Query.EQ("tripID", trip.tripID);
                    var existedtripChains = tripChains.Find(tripChainsQuery);
                    //var tripChainsQuery = from tc in tripChains.AsQueryable<TripChain>() where tc.tripID == trip.tripID select tc;
                    if (existedtripChains != null)
                        result = existedtripChains.ToList<TripChain>();
                }
            }
            catch (Exception e)
            {
                //Handle exception
            }
            return result;
        }

        /// <summary>
        /// NOT TESTED
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="user"></param>
        /// <param name="date"></param>
        /// <returns></returns>
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
                    var tripsQuery = Query.And(Query.EQ("userID", user.userID), Query.EQ("tripDate", date));
                    var existedTrips = trips.Find(tripsQuery);
                    //var tripsQuery = from t in trips.AsQueryable<Trip>() where t.userID == user.userID && t.tripDate == date select t;
                    if (existedTrips != null)
                        result = existedTrips.ToList<Trip>();
                }
            }
            catch (Exception e)
            {
                //Handle exception
            }
            return result;
        }

        public string ExtractTripData(long id, UserLegitimation user, Person person, TripJson trip, out Trip tripData, out List<TripChainJson> locations, out List<TransportModeJson> modes, out string statusMessage)
        {
            statusMessage = null;
            tripData = new Trip();
            locations = new List<TripChainJson>();
            modes = new List<TransportModeJson>();
            try
            {
                tripData.tripID = id.ToString();
                if (trip.meta != null)
                {
                    tripData.tripDate = new DateTime(1970, 1, 1) + new TimeSpan(trip.meta.startTime * 10000);
                    tripData.distance = trip.meta.distance;
                    tripData.tripPurposeId = trip.meta.purpose;
                }
                if (trip.entries != null)
                {
                    foreach (var e in trip.entries)
                    {
                        TripChainJson tcjs = new TripChainJson()
                        {
                            timestamp = e.timestamp,
                            latitude = e.latitude,
                            longitude = e.longitude,
                            altitude = e.altitude,
                            accuracy = e.accuracy,
                            altitudeAccuracy = e.altitudeAccuracy,
                            heading = e.heading,
                            speed = e.speed
                        };
                        locations.Add(tcjs);
                    }
                }

                if (trip.modes != null)
                {
                    foreach (var m in trip.modes)
                    {
                        TransportModeJson tmjs = new TransportModeJson()
                        {
                            time = m.time,
                            mode = m.mode
                        };
                        modes.Add(tmjs);
                    }
                }
            }
            catch (Exception e)
            {
                statusMessage = "An exception has occured in ExtractTripData method. " + e.Message;
            }
            return statusMessage;
        }

        //random string generator         
        public string CreateActivationCode(int size)
        {
            Random _rng = new Random();
            const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }    
            return new string(buffer);
        }

        public string CreateAndSaveActivationCode(string dbName, string dbUserName, string dbPassWord, string userName, out string activationCode)
        {
            string statusMessage = null;
            activationCode = null;
            try 
            {
                activationCode = CreateActivationCode(6);
                MongoDatabase resultDB = GetMongoDatabase(dbName, dbUserName, dbPassWord);
                if (resultDB == null)
                {
                    statusMessage = "Database is null";
                    return statusMessage;
                }

                MongoCollection<ActivationCode> activationCodes = resultDB.GetCollection<ActivationCode>("activationCodes");
                if (activationCodes != null) //check for activation code existence to overwrite, i.e. one user has only one activation code
                {
                    var query = Query<ActivationCode>.EQ(e => e.userName, userName);
                    if (query != null)
                        activationCodes.Remove(query);
                }
                activationCodes.Insert(new ActivationCode() {acID = userName + activationCode, userName = userName, activationCode = activationCode });
                statusMessage = "ok";
            }
            catch (Exception e) 
            {
                //Handle exception                
                statusMessage += "An exception has occured in CreateAndSaveActivationCode method. " + e.Message;
            }
            return statusMessage;
        }

        /// <summary>
        /// Json object from front-end parses: username, pincode
        /// if user exists returns userID
        /// if user not exists returns null
        /// </summary>
        /// <param name="user"></param>
        /// <returns>userID</returns>
        public string AuthenticateUser(string dbName, string userName, string passWord, UserLegitimation user, out string statusMessage)
        {
            statusMessage = null;
            string result = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(dbName, userName, passWord);
                if (resultDB == null)
                {
                    statusMessage = "Database is null";
                    return result;
                }
                MongoCollection<UserLegitimation> users = resultDB.GetCollection<UserLegitimation>("users");
                if (users.Count() > 0)
                {
                    var userQuery = Query.And(Query.EQ("userName", user.userName), Query.EQ("pinCode", user.pinCode));
                    var existedUser = users.FindOne(userQuery);
                    if (existedUser != null)
                        result = (existedUser as UserLegitimation).userID;
                }
                if (result == null)
                {
                    statusMessage = "Cannot authenticate this user.";
                }
            }
            catch (Exception e)
            {
                statusMessage += Environment.NewLine + "An exception has occured in AuthenticateUser method. " + e.Message;
            }
            return result;
        }

   
        #endregion

        #region REST services
        public string InsertNewUserREST(UserLegitimation user)
        {
            string statusMessage = null;
            try
            {
                InsertUser(_dbName, _userName, _passWord, user, true, out statusMessage);
                if (statusMessage == null)
                    statusMessage = "ok";
            }
            catch (Exception e)
            {
                //Handle exception                
                statusMessage += "An exception has occured in InsertNewUserREST method. " + e.Message;
            }            
            return statusMessage;
        }

        public string InsertTripDataREST(long id, UserLegitimation user, Person person, TripJson trip)
        {
            string statusMessage = null;
            Trip tripData = new Trip();
            List<TripChainJson> locations = new List<TripChainJson>();
            List<TransportModeJson> modes = new List<TransportModeJson>();

            try
            {
                statusMessage = ExtractTripData(id, user, person, trip, out tripData, out locations, out modes, out statusMessage);
                if(statusMessage == null)
                { 
                    InsertTripData(_dbName, _userName, _passWord, user, person, tripData, locations, modes, true, out statusMessage);
                    if (statusMessage == null)
                        statusMessage = "ok";
                }
            }
            catch (Exception e)
            {
                statusMessage += "An exception has occured in InsertTripDataREST method. " + e.Message;
            }
            //if (statusMessage != null)
            //    File.AppendAllText(@"C:\logs.txt", Environment.NewLine + "<statusMessage - " + DateTime.Now + ">: " + statusMessage);
            return statusMessage;
        }
        /// <summary>        
        /// </summary>
        /// <param name="user"></param>
        /// <param name="trip"></param>
        /// <param name="tripChain"></param>
        /// <returns></returns>
        public string UpdateTripDataREST(long id, UserLegitimation user, Person person, TripJson trip)
        {
            string statusMessage = null;
            Trip tripData = new Trip();
            List<TripChainJson> locations = new List<TripChainJson>();
            List<TransportModeJson> modes = new List<TransportModeJson>();

            try
            {
                statusMessage = ExtractTripData(id, user, person, trip, out tripData, out locations, out modes, out statusMessage);
                if (statusMessage == null)
                {
                    InsertTripData(_dbName, _userName, _passWord, user, person, tripData, locations, modes, false, out statusMessage);
                    if (statusMessage == null)
                        statusMessage = "ok";
                }
            }
            catch (Exception e)
            {
                statusMessage += "An exception has occured in UpdateTripDataREST method. " + e.Message;
            }
            //if (statusMessage != null)
            //    File.AppendAllText(@"C:\logs.txt", Environment.NewLine + "<statusMessage - " + DateTime.Now + ">: " + statusMessage);
            return statusMessage;
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


        public string UpdateUserREST(UserLegitimation user, Person person)
        {
            string statusMessage = null;
            try
            {
                UpdateUser(_dbName, _userName, _passWord, user, person, out statusMessage);
                if (statusMessage == null)
                    statusMessage = "ok";
            }
            catch (Exception e)
            {
                //Handle exception    
                statusMessage += "An exception has occured in UpdateUserREST method. " + e.Message;
            }
            //if (statusMessage != null)
            //    File.AppendAllText(@"C:\logs.txt", Environment.NewLine + "<statusMessage - " + DateTime.Now + ">: " + statusMessage);
            return statusMessage;
        }

        public string AuthenticateUserREST(UserLegitimation user)
        {
            string statusMessage = null;
            try
            {
                AuthenticateUser(_dbName, _userName, _passWord, user, out statusMessage);
                if (statusMessage == null)
                    statusMessage = "ok";
            }
            catch (Exception e)
            {
                statusMessage += "An exception has occured in AuthenticateUserREST method. " + e.Message;
            }
            //if(statusMessage != null)
            //    File.AppendAllText(@"C:\logs.txt", Environment.NewLine + "<statusMessage - " + DateTime.Now + ">: "+ statusMessage);
            return statusMessage;
        }

        public string SendEmailForNewPasswordREST(UserLegitimation user)
        {
            string statusMessage = null;
            try
            {
                //Create activation code and save to database
                string activationCode = null;
                statusMessage = CreateAndSaveActivationCode(_dbName, _userName, _passWord, user.userName, out activationCode);
                if (statusMessage == "ok")
                {
                    string from = "smio19noreply@gmail.com";
                    string to = user.userName;
                    System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(from, to);
                    msg.Subject = "Reset password";
                    msg.Body = "Please use this code to activate reset password process: " + activationCode;
                    System.Net.Mail.SmtpClient oSmtpClient = new System.Net.Mail.SmtpClient();
                    oSmtpClient.Send(msg);
                    statusMessage = "ok";
                }
            }
            catch (Exception e)
            {
                statusMessage = "An exception has occured in SendEmailForNewPasswordREST method. " + e.Message;
            }
            return statusMessage;
        }

        public string AuthenticateActivationCodeREST(UserLegitimation user, string activationCode)
        {
            string statusMessage = null;            
            try 
            { 
                MongoDatabase resultDB = GetMongoDatabase(_dbName, _userName, _passWord);
                if (resultDB == null)
                {
                    statusMessage = "Database is null";
                    return statusMessage;
                }

                MongoCollection<ActivationCode> activationCodes = resultDB.GetCollection<ActivationCode>("activationCodes");
                if (activationCodes != null)
                {
                    var activationCodeQuery = Query.And(Query.EQ("userName", user.userName), Query.EQ("activationCode", activationCode));
                    var existedCode = activationCodes.FindOne(activationCodeQuery);
                    if (existedCode == null)
                        statusMessage = "Invalid activation code for this user.";    
                    else
                        statusMessage = "ok";
                     
                }
            }
            catch (Exception e)
            {
                statusMessage += "An exception has occured in AuthenticateActivationCodeREST method. " + e.Message;
            }
            return statusMessage;
        }

        public string ResetPasswordREST(UserLegitimation user, string activationCode)
        {
            string statusMessage = null;
            try
            {
                statusMessage = AuthenticateActivationCodeREST(user, activationCode);
                if (statusMessage == "ok")
                {
                    InsertUser(_dbName, _userName, _passWord, user, false, out statusMessage);
                    if (statusMessage == null)
                        statusMessage = "ok";
                }
            }
            catch (Exception e)
            {
                //Handle exception                
                statusMessage += "An exception has occured in ResetPasswordREST method. " + e.Message;
            }
            return statusMessage;
        }
        
        public string DeleteUserREST(UserLegitimation user)
        {            
            //delete userlegitimation, person, trips, tripChains, modes, activationcode
            string statusMessage = null;
            UserLegitimation target = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(_dbName, _userName, _passWord);
                if (resultDB == null)
                {
                    statusMessage = "Database is null";
                    return statusMessage;
                }
                AuthenticateUser(_dbName, _userName, _passWord, user, out statusMessage);
                if (statusMessage != null)
                    return statusMessage;
                MongoCollection<UserLegitimation> users = resultDB.GetCollection<UserLegitimation>("users");
                if (users != null)
                {
                    var userQuery = Query.EQ("userName", user.userName);
                    var existedUser = users.FindOne(userQuery);
                    if (existedUser != null)
                    { 
                        target = (existedUser as UserLegitimation);
                        MongoCollection<Person> persons = resultDB.GetCollection<Person>("persons");
                        var personQuery = Query<Person>.EQ(e => e.userID, target.userID);
                        if (personQuery != null)
                            persons.Remove(personQuery);

                        MongoCollection<ActivationCode> activationCodes = resultDB.GetCollection<ActivationCode>("activationCodes");
                        var codeQuery = Query<ActivationCode>.EQ(e => e.userName, user.userName);
                        if (codeQuery != null)
                            activationCodes.Remove(codeQuery);

                        DeleteTrip(target, resultDB);
                        userQuery = Query<UserLegitimation>.EQ(e => e.userName, user.userName);  
                        users.Remove(userQuery);
                        statusMessage = "ok";
                    }
                    else
                        statusMessage += "Invalid user.";                    
                }
            }
            catch (Exception e)
            {
                statusMessage += "An exception has occured in DeleteUserREST method. " + e.Message;
            }
            return statusMessage;
        }

        public string DeleteAllTripsREST(UserLegitimation user)
        {
            string statusMessage = null;
            UserLegitimation target = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(_dbName, _userName, _passWord);
                if (resultDB == null)
                {
                    statusMessage = "Database is null";
                    return statusMessage;
                }
                AuthenticateUser(_dbName, _userName, _passWord, user, out statusMessage);
                if (statusMessage != null)
                    return statusMessage;
                MongoCollection<UserLegitimation> users = resultDB.GetCollection<UserLegitimation>("users");
                if (users != null)
                {
                    var userQuery = Query.EQ("userName", user.userName);
                    var existedUser = users.FindOne(userQuery);
                    if (existedUser != null)
                    {
                        target = (existedUser as UserLegitimation);
                        DeleteTrip(target, resultDB);                        
                        statusMessage = "ok";
                    }
                    else
                        statusMessage += "Invalid user.";
                }
            }
            catch (Exception e)
            {
                statusMessage += "An exception has occured in DeleteAllTripsREST method. " + e.Message;
            }
            return statusMessage;
        }

        public void DeleteTrip(UserLegitimation target, MongoDatabase resultDB)
        {
            MongoCollection<Trip> trips = resultDB.GetCollection<Trip>("trips");
            MongoCollection<TripChain> tripChains = resultDB.GetCollection<TripChain>("tripChains");
            MongoCollection<Mode> modes = resultDB.GetCollection<Mode>("modes");
            var tripsQuery = Query.EQ("userID", target.userID);
            var existedTrips = trips.Find(tripsQuery);
            if (existedTrips != null)
            {
                List<Trip> userTrips = existedTrips.ToList<Trip>();
                if (userTrips != null && userTrips.Count > 0)
                {
                    foreach (Trip t in userTrips)
                    {
                        var tripChainsQuery = Query<TripChain>.EQ(e => e.tripID, t.tripID);
                        if (tripChainsQuery != null)
                            tripChains.Remove(tripChainsQuery);


                        var modeQuery = Query<Mode>.EQ(e => e.tripID, t.tripID);
                        if (modeQuery != null)
                            modes.Remove(modeQuery);
                    }
                }
                tripsQuery = Query<Trip>.EQ(e => e.userID, target.userID);
                trips.Remove(tripsQuery);
            }
        }

        public string DeleteTripByIdREST(UserLegitimation user, long id)
        {
            string statusMessage = null;
            try
            {
                MongoDatabase resultDB = GetMongoDatabase(_dbName, _userName, _passWord);
                if (resultDB == null)
                {
                    statusMessage = "Database is null";
                    return statusMessage;
                }

                AuthenticateUser(_dbName, _userName, _passWord, user, out statusMessage);
                if (statusMessage != null)
                    return statusMessage;

                MongoCollection<Trip> trips = resultDB.GetCollection<Trip>("trips");
                MongoCollection<TripChain> tripChains = resultDB.GetCollection<TripChain>("tripChains");
                MongoCollection<Mode> modes = resultDB.GetCollection<Mode>("modes");
                if (trips != null)
                {                    
                        var tripChainsQuery = Query<TripChain>.EQ(e => e.tripID, id.ToString());
                        if (tripChainsQuery != null)
                           tripChains.Remove(tripChainsQuery);

                        var modeQuery = Query<Mode>.EQ(e => e.tripID, id.ToString());
                        if (modeQuery != null)
                            modes.Remove(modeQuery);
                    
                        var tripQuery = Query<Trip>.EQ(e => e.tripID, id.ToString());
                        if (tripQuery != null)
                        {
                            trips.Remove(tripQuery);
                            statusMessage = "ok";
                        }                
                        else statusMessage = "Cannot find this trip.";
                }                
            }
            catch (Exception e)
            {
                statusMessage += "An exception has occured in DeleteTripByIdREST method. " + e.Message;
            }
            return statusMessage;
        }

        #endregion
    }
}



