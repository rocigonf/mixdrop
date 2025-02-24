namespace mixdrop_back.Models.DTOs;

// cuando giras la ruleta devuelve el array de las posiciones a eliminar y el nivel que ha eliminado
public class RouletteInfo
{
    public List<int> Positions { get; set; }
    public int Level { get; set; }
}
