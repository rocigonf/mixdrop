namespace mixdrop_back.Models.Entities;

public class BattleRole
{
    public int Id { get; set; }
    public string Name { get; set; }

    // No le pongo la referencia a los usuarios y las batallas porque no creo que sea necesario
}
