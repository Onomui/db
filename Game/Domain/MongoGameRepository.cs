using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Game.Domain
{
    // TODO Сделать по аналогии с MongoUserRepository
    public class MongoGameRepository : IGameRepository
    {
        public const string CollectionName = "games";

        private readonly IMongoCollection<GameEntity> gameEntities;
        public MongoGameRepository(IMongoDatabase db)
        {
            gameEntities = db.GetCollection<GameEntity>(CollectionName);
        }

        public GameEntity Insert(GameEntity game)
        {
            gameEntities.InsertOne(game);
            return game;
        }

        public GameEntity FindById(Guid gameId)
        {
            return gameEntities.Find(entity => entity.Id == gameId).SingleOrDefault();
        }

        public void Update(GameEntity game)
        {
            gameEntities.UpdateOne(
                entity => entity.Id == game.Id,
                new BsonDocumentUpdateDefinition<GameEntity>(new BsonDocument("$set", game.ToBsonDocument()))
            );
        }

        // Возвращает не более чем limit игр со статусом GameStatus.WaitingToStart
        public IList<GameEntity> FindWaitingToStart(int limit)
        {
            //TODO: Используй Find и Limit
            return gameEntities.Find(entity => entity.Status == GameStatus.WaitingToStart).Limit(limit).ToList();
        }

        // Обновляет игру, если она находится в статусе GameStatus.WaitingToStart
        public bool TryUpdateWaitingToStart(GameEntity game)
        {
            //TODO: Для проверки успешности используй IsAcknowledged и ModifiedCount из результата
            var result = gameEntities.UpdateOne(
                x => x.Status == GameStatus.WaitingToStart && x.Id == game.Id,
                new BsonDocumentUpdateDefinition<GameEntity>(new BsonDocument("$set", game.ToBsonDocument()))
            );

            return result.ModifiedCount > 0;
        }
    }
}
