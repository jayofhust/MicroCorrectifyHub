using RecALLDemo.Infrastructure.Ddd.Domain.SeedWork;

namespace RecALLDemo.Core.List.Domain.AggregateModels.ItemAggregate;

public interface IItemRepository : IRepository<Item> {
    Item Add(Item item);

    Task<Item> GetAsync(int itemId, string userIdentityGuid);

    Task<IEnumerable<Item>> GetItemsAsync(IEnumerable<int> itemIds,
        string userIdentityGuid);
}
