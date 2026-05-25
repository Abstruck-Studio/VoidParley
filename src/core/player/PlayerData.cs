namespace VoidParley.core.player;

public class PlayerData(long uniqueId, string id)
{
    public long UniqueId { get; set; } = uniqueId;
    public string Id { get; set; } = id;
}