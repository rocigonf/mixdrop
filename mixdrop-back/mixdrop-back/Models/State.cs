namespace mixdrop_back.Models;

public class State
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
}
