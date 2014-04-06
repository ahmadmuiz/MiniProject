using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using MiniTest.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MiniTest.Repository
{
    public class MongoDBRepository
    {
        static MongoServer server = MongoServer.Create(ConfigurationManager.ConnectionStrings["MongoConnection"].ConnectionString.ToString());
        MongoDatabase database = server.GetDatabase("MiniProject");

        List<UserModel> user;
        public List<UserModel> UserModels
        {
            get
            {
                var collection = database.GetCollection<UserModel>("UserModel");
                return collection.FindAllAs<UserModel>().ToList();
            }
            set { user = value; }
        }
        public void CreateUserModel(UserModel user)
        {
            try
            {
                MongoCollection<UserModel> MCollection = database.GetCollection<UserModel>("UserModel");
                BsonDocument doc = new BsonDocument { 
                    {"UserName",user.UserName},
                    {"Password",user.Password},
                    {"SecurityToken",user.SecurityToken},
                    {"url",user.url},
                    {"MD5Password",user.MD5Password},
                    {"MD5Password_UTF",user.MD5Password_UTF}
                };
                MCollection.Insert(doc);
            }
            catch (Exception e) { }
        }

        public void CreateMessageModel(MessageModel message)
        {
            try
            {
                MongoCollection<MessageModel> MCollection = database.GetCollection<MessageModel>("MessageModel");
                BsonDocument doc = new BsonDocument { 
                    {"thread_id",message.thread_id},
                    {"securitytoken",message.securitytoken},
                    {"title",message.title},
                    {"message",message.message},
                    {"ajaxhref",message.ajaxhref},
                    {"iconid",message.iconid},
                    {"humanverify_hash",message.humanverify_hash},
                    {"recaptcha_challenge_field",message.recaptcha_challenge_field},
                    {"recaptcha_response_field",message.recaptcha_response_field},
                    {"parseurl",message.parseurl}
                };
                MCollection.Insert(doc);
            }
            catch { }
        }
    }


}