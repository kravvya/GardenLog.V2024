﻿namespace GardenLog.SharedInfrastructure.MongoDB
{
    public  record MongoSettings
    {
        public const string SECTION = "MongoDB";
        public const string PASSWORD_SECRET = "mongodb-password";
        public const string DATABASE_NAME = "mongodb-databasename";
        public const string SERVER = "mongodb-server";
        public const string USERNAME = "mongodb-username";

        public string? Server { get; set; } 
        public string? UserName { get; set; }
        public string? Password { get; set; } 
        public string? DatabaseName { get; set; }
    }
}
