using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DNBase.DataLayer.EF.Mongo.Entities
{
    public abstract class Entity : IEntity<string>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }

    public abstract class Entity<TKey> : IEntity<TKey> where TKey : IEquatable<TKey>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public TKey Id { get; set; }
    }

    public interface IEntity<TKey> where TKey : IEquatable<TKey>
    {
        TKey Id { get; set; }
    }

}