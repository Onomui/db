using System;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);
            var keys = Builders<UserEntity>.IndexKeys.Ascending(u => u.Login);
            var opts = new CreateIndexOptions { Unique = true, Name = "ux_login" };
            userCollection.Indexes.CreateOne(new CreateIndexModel<UserEntity>(keys, opts));

        }

        public UserEntity Insert(UserEntity user)
        {
            if (user.Id == Guid.Empty)
                user = new UserEntity(Guid.NewGuid(), user.Login, user.LastName, user.FirstName, user.GamesPlayed, user.CurrentGameId);

            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id)
        {
            return userCollection.Find(u => u.Id == id).FirstOrDefault();
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            var existing = userCollection.Find(u => u.Login == login).FirstOrDefault();
            if (existing != null)
                return existing;

            var created = new UserEntity(Guid.NewGuid(), login, "", "", 0, null);
            try
            {
                userCollection.InsertOne(created);
                return created;
            }
            catch (MongoWriteException ex) when (
                ex.WriteError?.Category == ServerErrorCategory.DuplicateKey || ex.Message.Contains("E11000"))
            {
                return userCollection.Find(u => u.Login == login).FirstOrDefault();
            }
        }

        public void Update(UserEntity user)
        {
            userCollection.ReplaceOne(u => u.Id == user.Id, user, new ReplaceOptions { IsUpsert = false });
        }

        public void Delete(Guid id)
        {
            userCollection.DeleteOne(u => u.Id == id);
        }

        // Для вывода списка всех пользователей (упорядоченных по логину)
        // страницы нумеруются с единицы
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var total = (int)userCollection.CountDocuments(_ => true);

            var items = userCollection.Find(_ => true)
                .SortBy(u => u.Login)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();

            return new PageList<UserEntity>(items, total, pageNumber, pageSize);
        }

        // Не нужно реализовывать этот метод
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}